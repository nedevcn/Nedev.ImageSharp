// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nedev.ImageSharp.PixelFormats;

namespace Nedev.ImageSharp.Formats.Avif
{
    /// <summary>
    /// Placeholder encoder for AVIF format.
    /// </summary>
    public sealed class AvifEncoder : IImageEncoder
    {
        /// <inheritdoc/>
        public void Encode<TPixel>(Image<TPixel> image, Stream stream)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Guard.NotNull(image, nameof(image));
            Guard.NotNull(stream, nameof(stream));
            throw new NotSupportedException("AVIF encoding is not supported in this build. Please use an external AV1/AVIF encoder or enable AVIF support via a separate package.");
        }

        /// <inheritdoc/>
        public Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            // Delegate to the synchronous implementation to ensure the same exception is thrown.
            this.Encode(image, stream);
            return Task.CompletedTask;
        }
    }
}
