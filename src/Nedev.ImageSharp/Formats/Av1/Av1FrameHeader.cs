// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Nedev.ImageSharp.Formats.Av1
{
    public readonly struct Av1FrameHeader
    {
        public Av1FrameHeader(int profile, bool showExistingFrame, bool isKeyFrame, int frameType, int frameWidth, int frameHeight)
        {
            this.Profile = profile;
            this.ShowExistingFrame = showExistingFrame;
            this.IsKeyFrame = isKeyFrame;
            this.FrameType = frameType;
            this.FrameWidth = frameWidth;
            this.FrameHeight = frameHeight;
        }

        public int Profile { get; }
        public bool ShowExistingFrame { get; }
        public bool IsKeyFrame { get; }
        public int FrameType { get; }
        public int FrameWidth { get; }
        public int FrameHeight { get; }
    }
}
