// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using Nedev.ImageSharp.Formats.Bmp;
using Nedev.ImageSharp.Formats.Png;
using Nedev.ImageSharp.PixelFormats;

namespace Nedev.ImageSharp.Formats.Ico
{
    /// <summary>
    /// Image decoder for reading Windows icon files.
    /// </summary>
    public sealed class IcoDecoder : IImageDecoder
    {
        /// <inheritdoc/>
        public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Guard.NotNull(stream, nameof(stream));

            var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);

            // ICONDIR
            ushort reserved = reader.ReadUInt16();
            ushort type = reader.ReadUInt16();
            ushort count = reader.ReadUInt16();

            if (reserved != 0 || (type != 1 && type != 2) || count == 0)
            {
                throw new ImageFormatException("Invalid ICO header.");
            }

            // Pick the largest entry by byte size.
            IcoDirectoryEntry bestEntry = null;
            for (int i = 0; i < count; i++)
            {
                IcoDirectoryEntry entry = IcoDirectoryEntry.Read(reader);
                if (entry.BytesInRes == 0)
                {
                    continue;
                }

                if (bestEntry == null || entry.BytesInRes > bestEntry.BytesInRes)
                {
                    bestEntry = entry;
                }
            }

            if (bestEntry == null)
            {
                throw new ImageFormatException("ICO does not contain any image data.");
            }

            stream.Seek(bestEntry.ImageOffset, SeekOrigin.Begin);

            // Read the image bytes for that entry.
            byte[] imageData = new byte[bestEntry.BytesInRes];
            int read = stream.Read(imageData, 0, imageData.Length);
            if (read != imageData.Length)
            {
                throw new ImageFormatException("Failed to read ICO image entry.");
            }

            // Most modern ICOs store images as PNG.
            if (imageData.Length >= 8 && BinaryPrimitives.ReadUInt32LittleEndian(imageData) == IcoConstants.PngSignature)
            {
                using var ms = new MemoryStream(imageData);
                return new PngDecoder().Decode<TPixel>(configuration, ms, cancellationToken);
            }

            // Older ICOs store BMP/DIB data. Wrap it so the BMP decoder can read it with an ICO-like file header.
            byte[] bmpData = this.CreateBmpStream(imageData);
            using var bmpStream = new MemoryStream(bmpData);
            return new BmpDecoder().Decode<TPixel>(configuration, bmpStream, cancellationToken);
        }

        /// <inheritdoc/>
        public Image Decode(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            => this.Decode<Rgb24>(configuration, stream, cancellationToken);

        private byte[] CreateBmpStream(byte[] dibData)
        {
            // The ICO/BMP stored inside ICO is a DIB with BITMAPINFOHEADER height = 2*realHeight.
            // Fix the height before handing off to the BMP decoder.
            if (dibData.Length < 40)
            {
                throw new ImageFormatException("Unsupported ICO entry format.");
            }

            // Copy header so we can patch it.
            byte[] patched = new byte[dibData.Length];
            Array.Copy(dibData, patched, dibData.Length);

            int headerSize = (int)BinaryPrimitives.ReadUInt32LittleEndian(patched);
            if (headerSize < 40 || headerSize > patched.Length)
            {
                throw new ImageFormatException("Unsupported ICO DIB header.");
            }

            int height = BinaryPrimitives.ReadInt32LittleEndian(patched.AsSpan(8));
            if (height > 0)
            {
                // ICO stores height as imageHeight * 2 (XOR + AND masks).
                BinaryPrimitives.WriteInt32LittleEndian(patched.AsSpan(8), height / 2);
            }

            ushort bitCount = BinaryPrimitives.ReadUInt16LittleEndian(patched.AsSpan(14));
            uint colorsUsed = BinaryPrimitives.ReadUInt32LittleEndian(patched.AsSpan(32));
            if (colorsUsed == 0 && bitCount <= 8)
            {
                colorsUsed = 1u << bitCount;
            }

            int paletteSize = (int)(colorsUsed * 4);
            int offset = 14 + headerSize + paletteSize;

            int fileSize = 14 + patched.Length;
            byte[] fileData = new byte[fileSize];

            // Build BMP file header with ICO marker (IC).
            // 2 bytes: type
            fileData[0] = (byte)'I';
            fileData[1] = (byte)'C';
            BinaryPrimitives.WriteUInt32LittleEndian(fileData.AsSpan(2), (uint)fileSize);
            // reserved1, reserved2 are 0 by default
            BinaryPrimitives.WriteUInt32LittleEndian(fileData.AsSpan(10), (uint)offset);

            Array.Copy(patched, 0, fileData, 14, patched.Length);
            return fileData;
        }

        private sealed class IcoDirectoryEntry
        {
            public byte Width { get; private set; }
            public byte Height { get; private set; }
            public byte ColorCount { get; private set; }
            public byte Reserved { get; private set; }
            public ushort Planes { get; private set; }
            public ushort BitCount { get; private set; }
            public uint BytesInRes { get; private set; }
            public uint ImageOffset { get; private set; }

            public static IcoDirectoryEntry Read(BinaryReader reader)
            {
                IcoDirectoryEntry entry = new IcoDirectoryEntry
                {
                    Width = reader.ReadByte(),
                    Height = reader.ReadByte(),
                    ColorCount = reader.ReadByte(),
                    Reserved = reader.ReadByte(),
                    Planes = reader.ReadUInt16(),
                    BitCount = reader.ReadUInt16(),
                    BytesInRes = reader.ReadUInt32(),
                    ImageOffset = reader.ReadUInt32(),
                };

                return entry;
            }
        }
    }
}
