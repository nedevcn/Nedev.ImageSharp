// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Nedev.ImageSharp.Memory;
using Nedev.ImageSharp.PixelFormats;
using Nedev.ImageSharp.Processing.Processors.Quantization;

namespace Nedev.ImageSharp.Formats.Gif
{
    /// <summary>
    /// The configuration options used for encoding gifs.
    /// </summary>
    internal interface IGifEncoderOptions
    {
        /// <summary>
        /// Gets the quantizer used to generate the color palette.
        /// </summary>
        IQuantizer Quantizer { get; }

        /// <summary>
        /// Gets the color table mode: Global or local.
        /// </summary>
        GifColorTableMode? ColorTableMode { get; }

        /// <summary>
        /// Gets the <see cref="IPixelSamplingStrategy"/> used for quantization when building a global color table.
        /// </summary>
        IPixelSamplingStrategy GlobalPixelSamplingStrategy { get; }
    }
}

