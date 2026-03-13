// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Nedev.ImageSharp.Formats.Avif
{
    internal static class AvifConstants
    {
        public static readonly string[] MimeTypes = { "image/avif" };
        public static readonly string[] FileExtensions = { "avif" };

        // Most AVIF files start with a file-type box containing "ftyp" + brand "avif".
        public const int HeaderSize = 12;
        public const uint FtypBox = 0x66747970; // 'ftyp'
        public const uint AvifBrand = 0x61766966; // 'avif'
    }
}
