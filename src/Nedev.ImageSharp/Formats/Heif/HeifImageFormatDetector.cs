// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers.Binary;

namespace Nedev.ImageSharp.Formats.Heif
{
    /// <summary>
    /// Detects HEIF/HEIC files by inspecting the file-type box.
    /// </summary>
    public sealed class HeifImageFormatDetector : IImageFormatDetector
    {
        /// <inheritdoc/>
        public int HeaderSize => HeifConstants.HeaderSize;

        /// <inheritdoc/>
        public IImageFormat DetectFormat(ReadOnlySpan<byte> header)
        {
            if (header.Length < this.HeaderSize)
            {
                return null;
            }

            // HEIF/HEIC uses an 'ftyp' box and the major_brand is commonly "heic", "mif1", "msf1" or "heix".
            if (BinaryPrimitives.ReadUInt32BigEndian(header.Slice(4)) != HeifConstants.FtypBox)
            {
                return null;
            }

            uint majorBrand = BinaryPrimitives.ReadUInt32BigEndian(header.Slice(8));
            foreach (uint brand in HeifConstants.ValidBrands)
            {
                if (majorBrand == brand)
                {
                    return HeifFormat.Instance;
                }
            }

            return null;
        }
    }
}
