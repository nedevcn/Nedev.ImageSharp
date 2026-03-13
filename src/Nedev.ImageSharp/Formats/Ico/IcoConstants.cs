// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Nedev.ImageSharp.Formats.Ico
{
    internal static class IcoConstants
    {
        public static readonly string[] MimeTypes = { "image/x-icon", "image/vnd.microsoft.icon" };
        public static readonly string[] FileExtensions = { "ico", "cur" };

        public const uint PngSignature = 0x474E5089;
        public const int HeaderSize = 6;
        public const int DirectoryEntrySize = 16;
    }
}
