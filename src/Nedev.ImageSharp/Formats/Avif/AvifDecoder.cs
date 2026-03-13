// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using Nedev.ImageSharp.PixelFormats;

namespace Nedev.ImageSharp.Formats.Avif
{
    /// <summary>
    /// Placeholder decoder for AVIF format.
    /// </summary>
    public sealed class AvifDecoder : IImageDecoder
    {
        /// <inheritdoc/>
        public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Guard.NotNull(stream, nameof(stream));
            throw new NotSupportedException("AVIF decoding is not supported in this build. Please use an external AV1/AVIF decoder or enable AVIF support via a separate package.");
        }

        /// <inheritdoc/>
        public Image Decode(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            => this.Decode<Rgb24>(configuration, stream, cancellationToken);
    }
}
