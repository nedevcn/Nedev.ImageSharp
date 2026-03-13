// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Nedev.ImageSharp.Formats.Av1
{
    public readonly struct Av1SequenceHeader
    {
        public Av1SequenceHeader(int profile, bool stillPicture, bool reducedStillPictureHeader, int bitDepth)
        {
            this.Profile = profile;
            this.StillPicture = stillPicture;
            this.ReducedStillPictureHeader = reducedStillPictureHeader;
            this.BitDepth = bitDepth;
        }

        public int Profile { get; }
        public bool StillPicture { get; }
        public bool ReducedStillPictureHeader { get; }
        public int BitDepth { get; }
    }
}
