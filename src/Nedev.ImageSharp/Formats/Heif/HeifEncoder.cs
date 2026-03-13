// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nedev.ImageSharp.PixelFormats;

namespace Nedev.ImageSharp.Formats.Heif
{
    /// <summary>
    /// Placeholder encoder for HEIF/HEIC format.
    /// </summary>
    public sealed class HeifEncoder : IImageEncoder
    {
        /// <inheritdoc/>
        public void Encode<TPixel>(Image<TPixel> image, Stream stream)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Guard.NotNull(image, nameof(image));
            Guard.NotNull(stream, nameof(stream));
            throw new NotSupportedException("HEIF/HEIC encoding is not supported in this build. Please use an external HEVC/HEIF encoder or enable HEIF support via a separate package.");
        }

        /// <inheritdoc/>
        public Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            this.Encode(image, stream);
            return Task.CompletedTask;
        }
    }
}
