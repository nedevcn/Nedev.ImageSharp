// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Nedev.ImageSharp.Formats.Av1
{
    internal readonly struct Av1Obu
    {
        public Av1Obu(Av1ObuHeader header, ReadOnlyMemory<byte> payload)
        {
            this.Header = header;
            this.Payload = payload;
        }

        public Av1ObuHeader Header { get; }
        public ReadOnlyMemory<byte> Payload { get; }
    }
}
