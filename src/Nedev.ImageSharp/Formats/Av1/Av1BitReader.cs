// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;

namespace Nedev.ImageSharp.Formats.Av1
{
    internal ref struct Av1BitReader
    {
        private readonly ReadOnlySpan<byte> buffer;
        private int byteIndex;
        private int bitIndex;

        public Av1BitReader(ReadOnlySpan<byte> buffer)
        {
            this.buffer = buffer;
            this.byteIndex = 0;
            this.bitIndex = 0;
        }

        public bool EndOfStream => this.byteIndex >= this.buffer.Length;

        public int BytePosition => this.byteIndex;
        public int BitPosition => this.bitIndex;

        public int ReadBit()
        {
            if (this.EndOfStream)
            {
                throw new InvalidOperationException("Reached end of buffer while reading bits.");
            }

            int value = (this.buffer[this.byteIndex] >> (7 - this.bitIndex)) & 1;
            this.bitIndex++;
            if (this.bitIndex == 8)
            {
                this.bitIndex = 0;
                this.byteIndex++;
            }

            return value;
        }

        public uint ReadBits(int count)
        {
            if (count < 0 || count > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            uint result = 0;
            for (int i = 0; i < count; i++)
            {
                result = (result << 1) | (uint)this.ReadBit();
            }

            return result;
        }

        public ulong ReadUleb128()
        {
            // LEB128 used for size fields in AV1 OBU headers.
            // The value is encoded in 7-bit groups, little-endian, with MSB as continuation flag.
            ulong value = 0;
            int shift = 0;

            while (!this.EndOfStream)
            {
                byte b = this.ReadByte();
                value |= (ulong)(b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    return value;
                }

                shift += 7;
                if (shift > 56)
                {
                    throw new InvalidDataException("ULEB128 value too large.");
                }
            }

            throw new InvalidDataException("Unexpected end of data while reading ULEB128.");
        }

        public byte ReadByte()
        {
            if (this.bitIndex == 0)
            {
                if (this.byteIndex >= this.buffer.Length)
                {
                    throw new InvalidOperationException("Reached end of buffer while reading a byte.");
                }

                return this.buffer[this.byteIndex++];
            }

            // Read remaining bits spanning across bytes.
            int value = 0;
            for (int i = 0; i < 8; i++)
            {
                value = (value << 1) | this.ReadBit();
            }

            return (byte)value;
        }

        public ReadOnlySpan<byte> ReadBytes(int count)
        {
            if (this.bitIndex != 0)
            {
                throw new InvalidOperationException("ReadBytes is only supported when bit index is byte-aligned.");
            }

            if (this.byteIndex + count > this.buffer.Length)
            {
                throw new InvalidOperationException("Reached end of buffer while reading bytes.");
            }

            var slice = this.buffer.Slice(this.byteIndex, count);
            this.byteIndex += count;
            return slice;
        }

        public void SkipToByteBoundary()
        {
            if (this.bitIndex != 0)
            {
                this.bitIndex = 0;
                this.byteIndex++;
            }
        }
    }
}
