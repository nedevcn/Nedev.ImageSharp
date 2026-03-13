// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Nedev.ImageSharp.Formats.Heif
{
    /// <summary>
    /// Registers the image encoder, decoder and mime type detector for the HEIF/HEIC format.
    /// </summary>
    public sealed class HeifConfigurationModule : IConfigurationModule
    {
        /// <inheritdoc/>
        public void Configure(Configuration configuration)
        {
            configuration.ImageFormatsManager.SetEncoder(HeifFormat.Instance, new HeifEncoder());
            configuration.ImageFormatsManager.SetDecoder(HeifFormat.Instance, new HeifDecoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new HeifImageFormatDetector());
        }
    }
}
