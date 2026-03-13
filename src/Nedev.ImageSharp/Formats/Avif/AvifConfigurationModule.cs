// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Nedev.ImageSharp.Formats.Avif
{
    /// <summary>
    /// Registers the image encoder, decoder and mime type detector for the AVIF format.
    /// </summary>
    public sealed class AvifConfigurationModule : IConfigurationModule
    {
        /// <inheritdoc/>
        public void Configure(Configuration configuration)
        {
            configuration.ImageFormatsManager.SetEncoder(AvifFormat.Instance, new AvifEncoder());
            configuration.ImageFormatsManager.SetDecoder(AvifFormat.Instance, new AvifDecoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new AvifImageFormatDetector());
        }
    }
}
