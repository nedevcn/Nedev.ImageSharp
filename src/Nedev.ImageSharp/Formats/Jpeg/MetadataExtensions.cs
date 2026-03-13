// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using Nedev.ImageSharp.Formats.Jpeg;
using Nedev.ImageSharp.Metadata;

namespace Nedev.ImageSharp
{
    /// <summary>
    /// Extension methods for the <see cref="ImageMetadata"/> type.
    /// </summary>
    public static partial class MetadataExtensions
    {
        /// <summary>
        /// Gets the jpeg format specific metadata for the image.
        /// </summary>
        /// <param name="metadata">The metadata this method extends.</param>
        /// <returns>The <see cref="JpegMetadata"/>.</returns>
        public static JpegMetadata GetJpegMetadata(this ImageMetadata metadata) => metadata.GetFormatMetadata(JpegFormat.Instance);
    }
}

