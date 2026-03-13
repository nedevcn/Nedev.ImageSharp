// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace Nedev.ImageSharp.Formats.Av1
{
    internal readonly struct Av1ObuHeader
    {
        public Av1ObuHeader(byte obuType, bool hasSize, int size)
        {
            this.ObuType = obuType;
            this.HasSize = hasSize;
            this.Size = size;
        }

        public byte ObuType { get; }
        public bool HasSize { get; }
        public int Size { get; }

        public bool IsSequenceHeader => this.ObuType == 1;
        public bool IsFrameHeader => this.ObuType == 3;
        public bool IsTileGroup => this.ObuType == 4;

        public static Av1ObuHeader Parse(ref Av1BitReader reader)
            => Parse(ref reader, out _);

        public static Av1ObuHeader Parse(ref Av1BitReader reader, out int headerSize)
        {
            // OBU header is byte-aligned.
            int start = reader.BytePosition;

            byte obuHeader = reader.ReadByte();
            bool forbiddenBit = (obuHeader & 0x80) != 0;
            if (forbiddenBit)
            {
                throw new InvalidDataException("Forbidden bit set in AV1 OBU header.");
            }

            byte obuType = (byte)((obuHeader >> 3) & 0x0F);
            bool obuExtensionFlag = (obuHeader & 0x04) != 0;
            bool obuHasSizeField = (obuHeader & 0x02) != 0;

            if (obuExtensionFlag)
            {
                // Skip extension header (8 bits). We don't need it for parsing most headers.
                reader.ReadByte();
            }

            int size = 0;
            if (obuHasSizeField)
            {
                ulong leb = reader.ReadUleb128();
                if (leb > int.MaxValue)
                {
                    throw new InvalidDataException("OBU size too large.");
                }

                size = (int)leb;
            }

            headerSize = reader.BytePosition - start;
            return new Av1ObuHeader(obuType, obuHasSizeField, size);
        }
    }
}
