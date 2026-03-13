// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Nedev.ImageSharp.Formats.Ico
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for the ico format.
    /// </summary>
    public sealed class IcoConfigurationModule : IConfigurationModule
    {
        /// <inheritdoc/>
        public void Configure(Configuration configuration)
        {
            configuration.ImageFormatsManager.SetEncoder(IcoFormat.Instance, new IcoEncoder());
            configuration.ImageFormatsManager.SetDecoder(IcoFormat.Instance, new IcoDecoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new IcoImageFormatDetector());
        }
    }
}
