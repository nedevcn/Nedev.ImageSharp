// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nedev.ImageSharp.Formats.Png;
using Nedev.ImageSharp.PixelFormats;

namespace Nedev.ImageSharp.Formats.Ico
{
    /// <summary>
    /// Encoder for Windows icon files.
    /// </summary>
    public sealed class IcoEncoder : IImageEncoder
    {
        /// <inheritdoc/>
        public void Encode<TPixel>(Image<TPixel> image, Stream stream)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Guard.NotNull(image, nameof(image));
            Guard.NotNull(stream, nameof(stream));

            // Encode the actual image data as a PNG and embed it inside the ICO container.
            using var iconDataStream = new MemoryStream();
            new PngEncoder().Encode(image, iconDataStream);
            byte[] iconData = iconDataStream.ToArray();

            // ICONDIR header
            // Reserved (2 bytes), Type (2 bytes), Count (2 bytes)
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(1);
            stream.WriteByte(0);
            stream.WriteByte(1);
            stream.WriteByte(0);

            // Directory entry for a single image
            // Width, Height, ColorCount, Reserved
            int width = image.Width;
            int height = image.Height;

            stream.WriteByte((byte)(width >= 256 ? 0 : width));
            stream.WriteByte((byte)(height >= 256 ? 0 : height));
            stream.WriteByte(0);
            stream.WriteByte(0);

            // Planes (2 bytes) and BitCount (2 bytes)
            stream.WriteByte(1);
            stream.WriteByte(0);
            stream.WriteByte(32);
            stream.WriteByte(0);

            // BytesInRes (4 bytes)
            stream.Write(BitConverter.GetBytes((uint)iconData.Length), 0, 4);

            // ImageOffset (4 bytes): header (6) + directory (16)
            const uint imageOffset = 6 + 16;
            stream.Write(BitConverter.GetBytes(imageOffset), 0, 4);

            // Image data
            stream.Write(iconData, 0, iconData.Length);
        }

        /// <inheritdoc/>
        public Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            // The simple implementation just delegates to the synchronous version.
            // ICO is small enough that this should be fine for typical use.
            this.Encode(image, stream);
            return Task.CompletedTask;
        }
    }
}
