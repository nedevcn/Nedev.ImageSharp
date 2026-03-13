// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using Nedev.ImageSharp.Formats.Av1;
using Nedev.ImageSharp.PixelFormats;

namespace Nedev.ImageSharp.Formats.Heif
{
    /// <summary>
    /// Placeholder decoder for HEIF/HEIC format.
    /// </summary>
    public sealed class HeifDecoder : IImageDecoder
    {
        /// <inheritdoc/>
        public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Guard.NotNull(stream, nameof(stream));

            // HEIF/HEIC typically wraps AV1 bitstreams, but some files carry embedded JPEG or PNG images.
            // We support those cases in pure managed code by scanning for embedded image signatures and using existing decoders.
            if (TryDecodeEmbeddedJpeg(configuration, stream, cancellationToken, out Image<TPixel> image)
                || TryDecodeEmbeddedPng(configuration, stream, cancellationToken, out image))
            {
                return image;
            }

            if (Av1Parser.DetectAv1Bitstream(stream))
            {
                throw new NotSupportedException("HEIF/HEIC decoding is not supported in this build. AV1 bitstreams are detected but not yet implemented in this pure-managed build.");
            }

            throw new NotSupportedException("HEIF/HEIC decoding is not supported in this build. Only HEIF files containing embedded JPEG/PNG items are supported.");
        }

        /// <inheritdoc/>
        public Image Decode(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            => this.Decode<Rgb24>(configuration, stream, cancellationToken);

        private static bool TryDecodeEmbeddedJpeg<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken, out Image<TPixel> image)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            image = null;
            if (!stream.CanSeek)
            {
                // Read into memory to allow scanning.
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                stream = ms;
            }

            long originalPosition = stream.Position;
            stream.Position = 0;

            // Find a JPEG start marker (0xFF 0xD8) and attempt decode.
            // This is a heuristic for HEIF files carrying a JPEG item.
            const byte JpegStart0 = 0xFF;
            const byte JpegStart1 = 0xD8;
            const byte JpegStart2 = 0xFF;

            int b0;
            while ((b0 = stream.ReadByte()) >= 0)
            {
                if ((byte)b0 != JpegStart0)
                {
                    continue;
                }

                int b1 = stream.ReadByte();
                if (b1 < 0) break;
                if ((byte)b1 != JpegStart1)
                {
                    stream.Position -= 1;
                    continue;
                }

                int b2 = stream.ReadByte();
                if (b2 < 0) break;
                if ((byte)b2 != JpegStart2)
                {
                    stream.Position -= 2;
                    continue;
                }

                // Found a probable JPEG start; attempt decode from this offset.
                stream.Position -= 3;
                using var jpegStream = new NonClosingStreamWrapper(stream);
                try
                {
                    var decoder = new Nedev.ImageSharp.Formats.Jpeg.JpegDecoder();
                    image = decoder.Decode<TPixel>(configuration, jpegStream, cancellationToken);
                    return true;
                }
                catch
                {
                    // Not a valid JPEG or decode failed; keep searching.
                    stream.Position += 1;
                }
            }

            stream.Position = originalPosition;
            return false;
        }

        private static bool TryDecodeEmbeddedPng<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken, out Image<TPixel> image)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            image = null;
            if (!stream.CanSeek)
            {
                // Read into memory to allow scanning.
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                stream = ms;
            }

            long originalPosition = stream.Position;
            stream.Position = 0;

            byte[] pngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            int b0;
            while ((b0 = stream.ReadByte()) >= 0)
            {
                if ((byte)b0 != pngSignature[0])
                {
                    continue;
                }

                byte[] buffer = new byte[pngSignature.Length - 1];
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read != buffer.Length)
                {
                    break;
                }

                bool matches = true;
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] != pngSignature[i + 1])
                    {
                        matches = false;
                        break;
                    }
                }

                if (!matches)
                {
                    stream.Position -= buffer.Length;
                    continue;
                }

                stream.Position -= pngSignature.Length;
                using var pngStream = new NonClosingStreamWrapper(stream);
                try
                {
                    var decoder = new Nedev.ImageSharp.Formats.Png.PngDecoder();
                    image = decoder.Decode<TPixel>(configuration, pngStream, cancellationToken);
                    return true;
                }
                catch
                {
                    stream.Position += 1;
                }
            }

            stream.Position = originalPosition;
            return false;
        }

        /// <summary>
        /// A wrapper that prevents disposing the underlying stream when the inner decoder disposes it.
        /// </summary>
        private sealed class NonClosingStreamWrapper : Stream
        {
            private readonly Stream inner;

            public NonClosingStreamWrapper(Stream inner)
            {
                this.inner = inner;
            }

            public override bool CanRead => this.inner.CanRead;
            public override bool CanSeek => this.inner.CanSeek;
            public override bool CanWrite => this.inner.CanWrite;
            public override long Length => this.inner.Length;
            public override long Position { get => this.inner.Position; set => this.inner.Position = value; }

            public override void Flush() => this.inner.Flush();
            public override int Read(byte[] buffer, int offset, int count) => this.inner.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => this.inner.Seek(offset, origin);
            public override void SetLength(long value) => this.inner.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => this.inner.Write(buffer, offset, count);

            protected override void Dispose(bool disposing)
            {
                // Do not dispose inner stream.
            }
        }
    }
}

