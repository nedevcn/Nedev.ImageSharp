// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers.Binary;

namespace Nedev.ImageSharp.Formats.Ico
{
    /// <summary>
    /// Detects ICO file headers.
    /// </summary>
    public sealed class IcoImageFormatDetector : IImageFormatDetector
    {
        /// <inheritdoc/>
        public int HeaderSize => IcoConstants.HeaderSize;

        /// <inheritdoc/>
        public IImageFormat DetectFormat(ReadOnlySpan<byte> header)
        {
            if (header.Length < this.HeaderSize)
            {
                return null;
            }

            ushort reserved = BinaryPrimitives.ReadUInt16LittleEndian(header);
            ushort type = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(2));

            // Reserved should be 0. Type 1 means ICO, 2 means CUR (cursor).
            if (reserved == 0 && (type == 1 || type == 2))
            {
                return IcoFormat.Instance;
            }

            return null;
        }
    }
}
