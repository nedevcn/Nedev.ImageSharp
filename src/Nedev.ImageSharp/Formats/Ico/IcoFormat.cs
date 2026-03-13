// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace Nedev.ImageSharp.Formats.Ico
{
    /// <summary>
    /// Image format for Windows icon files.
    /// </summary>
    public sealed class IcoFormat : IImageFormat<IcoMetadata>
    {
        private IcoFormat()
        {
        }

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        public static IcoFormat Instance { get; } = new IcoFormat();

        /// <inheritdoc/>
        public string Name => "ICO";

        /// <inheritdoc/>
        public string DefaultMimeType => IcoConstants.MimeTypes[0];

        /// <inheritdoc/>
        public IEnumerable<string> MimeTypes => IcoConstants.MimeTypes;

        /// <inheritdoc/>
        public IEnumerable<string> FileExtensions => IcoConstants.FileExtensions;

        /// <inheritdoc/>
        public IcoMetadata CreateDefaultFormatMetadata() => new IcoMetadata();
    }
}
