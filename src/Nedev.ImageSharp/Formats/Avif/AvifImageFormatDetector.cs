// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers.Binary;

namespace Nedev.ImageSharp.Formats.Avif
{
    /// <summary>
    /// Detects AVIF files by inspecting the file-type box.
    /// </summary>
    public sealed class AvifImageFormatDetector : IImageFormatDetector
    {
        /// <inheritdoc/>
        public int HeaderSize => AvifConstants.HeaderSize;

        /// <inheritdoc/>
        public IImageFormat DetectFormat(ReadOnlySpan<byte> header)
        {
            if (header.Length < this.HeaderSize)
            {
                return null;
            }

            // AVIF files start with a ftyp box, and the major_brand should be "avif".
            // [0..3]: size, [4..7]: "ftyp", [8..11]: major_brand
            if (BinaryPrimitives.ReadUInt32BigEndian(header.Slice(4)) != AvifConstants.FtypBox)
            {
                return null;
            }

            if (BinaryPrimitives.ReadUInt32BigEndian(header.Slice(8)) != AvifConstants.AvifBrand)
            {
                return null;
            }

            return AvifFormat.Instance;
        }
    }
}
