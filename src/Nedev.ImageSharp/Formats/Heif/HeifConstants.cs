// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Nedev.ImageSharp.Formats.Heif
{
    internal static class HeifConstants
    {
        public static readonly string[] MimeTypes = { "image/heic", "image/heif" };
        public static readonly string[] FileExtensions = { "heic", "heif" };

        public const int HeaderSize = 12;
        public const uint FtypBox = 0x66747970; // 'ftyp'

        public static readonly uint[] ValidBrands =
        {
            0x68656963, // 'heic'
            0x6D696631, // 'mif1'
            0x6D736631, // 'msf1'
            0x68656978, // 'heix'
        };
    }
}
