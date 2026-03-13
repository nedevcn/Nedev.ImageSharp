// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace Nedev.ImageSharp.Formats.Heif
{
    /// <summary>
    /// Defines the HEIF/HEIC image format.
    /// </summary>
    public sealed class HeifFormat : IImageFormat
    {
        private HeifFormat()
        {
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static HeifFormat Instance { get; } = new HeifFormat();

        /// <inheritdoc/>
        public string Name => "HEIC";

        /// <inheritdoc/>
        public string DefaultMimeType => HeifConstants.MimeTypes[0];

        /// <inheritdoc/>
        public IEnumerable<string> MimeTypes => HeifConstants.MimeTypes;

        /// <inheritdoc/>
        public IEnumerable<string> FileExtensions => HeifConstants.FileExtensions;
    }
}
