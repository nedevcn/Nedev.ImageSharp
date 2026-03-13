// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Nedev.ImageSharp.Formats.Av1
{
    /// <summary>
    /// Represents a parsed AV1 frame, including optional sequence header, the frame header, and any tile groups.
    /// </summary>
    public sealed class Av1ParsedFrame
    {
        public Av1ParsedFrame(Av1FrameHeader frameHeader, ReadOnlyMemory<byte>[] tileGroups, Av1SequenceHeader? sequenceHeader = null)
        {
            this.FrameHeader = frameHeader;
            this.TileGroups = tileGroups;
            this.SequenceHeader = sequenceHeader;
        }

        public Av1SequenceHeader? SequenceHeader { get; }

        public Av1FrameHeader FrameHeader { get; }

        public ReadOnlyMemory<byte>[] TileGroups { get; }
    }
}
