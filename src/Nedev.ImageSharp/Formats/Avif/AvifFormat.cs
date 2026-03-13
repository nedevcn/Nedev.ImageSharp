// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace Nedev.ImageSharp.Formats.Avif
{
    /// <summary>
    /// Defines the AVIF image format.
    /// </summary>
    public sealed class AvifFormat : IImageFormat
    {
        private AvifFormat()
        {
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static AvifFormat Instance { get; } = new AvifFormat();

        /// <inheritdoc/>
        public string Name => "AVIF";

        /// <inheritdoc/>
        public string DefaultMimeType => AvifConstants.MimeTypes[0];

        /// <inheritdoc/>
        public IEnumerable<string> MimeTypes => AvifConstants.MimeTypes;

        /// <inheritdoc/>
        public IEnumerable<string> FileExtensions => AvifConstants.FileExtensions;
    }
}
