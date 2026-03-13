// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;

namespace Nedev.ImageSharp.Formats.Av1
{
    public static class Av1Parser
    {
        public static bool TryParseSequenceHeader(ReadOnlySpan<byte> data, out Av1SequenceHeader header)
        {
            header = default;
            try
            {
                var reader = new Av1BitReader(data);
                var obu = Av1ObuHeader.Parse(ref reader);
                if (!obu.IsSequenceHeader)
                {
                    return false;
                }

                // Sequence header payload is in OBU payload (after header and optional size field).
                int payloadStart = reader.BytePosition;
                int payloadSize = obu.HasSize ? obu.Size : data.Length - payloadStart;
                if (payloadSize <= 0 || payloadStart + payloadSize > data.Length)
                {
                    return false;
                }

                ReadOnlySpan<byte> payload = data.Slice(payloadStart, payloadSize);
                var payloadReader = new Av1BitReader(payload);

                int profile = (int)payloadReader.ReadBits(3);
                bool stillPicture = payloadReader.ReadBit() != 0;
                bool reducedStillPictureHeader = payloadReader.ReadBit() != 0;

                // Reduced still picture header includes bit depth and other fields.
                int bitDepth = 8;
                if (reducedStillPictureHeader)
                {
                    // For reduced headers, the bit depth is encoded as `bit_depth_minus_8` in 2 bits.
                    int bitDepthMinus8 = (int)payloadReader.ReadBits(2);
                    bitDepth = 8 + bitDepthMinus8;
                }

                header = new Av1SequenceHeader(profile, stillPicture, reducedStillPictureHeader, bitDepth);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryParseFrameHeader(ReadOnlySpan<byte> data, out Av1FrameHeader header)
        {
            header = default;

            try
            {
                var reader = new Av1BitReader(data);
                var obu = Av1ObuHeader.Parse(ref reader);
                if (!obu.IsFrameHeader)
                {
                    return false;
                }

                int payloadStart = reader.BytePosition;
                int payloadSize = obu.HasSize ? obu.Size : data.Length - payloadStart;
                if (payloadSize <= 0 || payloadStart + payloadSize > data.Length)
                {
                    return false;
                }

                ReadOnlySpan<byte> payload = data.Slice(payloadStart, payloadSize);
                var payloadReader = new Av1BitReader(payload);

                int frameMarker = (int)payloadReader.ReadBits(2);
                if (frameMarker != 2)
                {
                    return false;
                }

                int profile = (int)payloadReader.ReadBits(2);
                bool showExistingFrame = payloadReader.ReadBit() != 0;
                bool isKeyFrame = false;
                int frameType = 0;

                if (showExistingFrame)
                {
                    int frameToShowMapIdx = (int)payloadReader.ReadBits(3);
                    header = new Av1FrameHeader(profile, true, true, frameToShowMapIdx, 0, 0);
                    return true;
                }

                frameType = (int)payloadReader.ReadBits(2);
                bool showFrame = payloadReader.ReadBit() != 0;
                bool errorResilientMode = payloadReader.ReadBit() != 0;

                isKeyFrame = frameType == 0;

                payloadReader.SkipToByteBoundary();

                int frameWidth = 0;
                int frameHeight = 0;
                if (isKeyFrame)
                {
                    ReadOnlySpan<byte> widthBytes = payloadReader.ReadBytes(2);
                    ReadOnlySpan<byte> heightBytes = payloadReader.ReadBytes(2);
                    frameWidth = (widthBytes[0] << 8) | widthBytes[1];
                    frameHeight = (heightBytes[0] << 8) | heightBytes[1];
                }

                header = new Av1FrameHeader(profile, showExistingFrame, isKeyFrame, frameType, frameWidth, frameHeight);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryParseFrame(ReadOnlySpan<byte> data, out Av1ParsedFrame parsedFrame)
        {
            parsedFrame = null;

            var tileGroups = new System.Collections.Generic.List<ReadOnlyMemory<byte>>();
            Av1SequenceHeader? sequenceHeader = null;
            Av1FrameHeader? frameHeader = null;

            int offset = 0;
            while (offset < data.Length)
            {
                if (!TryReadNextObu(data, offset, out Av1Obu obu, out int nextOffset))
                {
                    return false;
                }

                ReadOnlySpan<byte> obuData = data.Slice(offset, nextOffset - offset);

                if (sequenceHeader is null && obu.Header.IsSequenceHeader)
                {
                    TryParseSequenceHeader(obuData, out var seq);
                    sequenceHeader = seq;
                }

                if (frameHeader is null && obu.Header.IsFrameHeader)
                {
                    TryParseFrameHeader(obuData, out var hdr);
                    frameHeader = hdr;
                }

                if (obu.Header.IsTileGroup)
                {
                    tileGroups.Add(obu.Payload);
                }

                offset = nextOffset;
            }

            if (frameHeader is null)
            {
                return false;
            }

            parsedFrame = new Av1ParsedFrame(frameHeader.Value, tileGroups.ToArray(), sequenceHeader);
            return true;
        }

        public static bool DetectAv1Bitstream(Stream stream)
        {
            if (!stream.CanSeek)
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                stream = ms;
            }

            long originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                Span<byte> buffer = stackalloc byte[4096];
                int read = stream.Read(buffer);
                if (read < 4)
                {
                    return false;
                }

                // Look for an OBU header in the first few bytes.
                for (int i = 0; i < read - 1; i++)
                {
                    if ((buffer[i] & 0x80) != 0)
                    {
                        // forbidden bit set, skip
                        continue;
                    }

                    byte obuType = (byte)((buffer[i] >> 3) & 0x0F);
                    if (obuType == 1)
                    {
                        // Likely a sequence header.
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        public static bool TryExtractFirstTileGroup(ReadOnlySpan<byte> data, out ReadOnlyMemory<byte> tileGroupPayload)
        {
            tileGroupPayload = default;
            int offset = 0;

            while (offset < data.Length)
            {
                if (!TryReadNextObu(data, offset, out Av1Obu obu, out int nextOffset))
                {
                    return false;
                }

                offset = nextOffset;

                // OBU type 4 is used for tile groups.
                // If this OBU is a tile group, return its payload.
                if (obu.Header.IsTileGroup)
                {
                    tileGroupPayload = obu.Payload;
                    return true;
                }
            }

            return false;
        }

        private static bool TryReadNextObu(ReadOnlySpan<byte> data, int offset, out Av1Obu obu, out int nextOffset)
        {
            obu = default;
            nextOffset = offset;

            if (offset >= data.Length)
            {
                return false;
            }

            var reader = new Av1BitReader(data.Slice(offset));
            int headerSize;
            Av1ObuHeader header;
            try
            {
                header = Av1ObuHeader.Parse(ref reader, out headerSize);
            }
            catch
            {
                return false;
            }

            int payloadSize = header.HasSize ? header.Size : data.Length - (offset + headerSize);
            if (payloadSize < 0 || offset + headerSize + payloadSize > data.Length)
            {
                return false;
            }

            ReadOnlyMemory<byte> payload = new ReadOnlyMemory<byte>(data.Slice(offset + headerSize, payloadSize).ToArray());
            obu = new Av1Obu(header, payload);
            nextOffset = offset + headerSize + payloadSize;
            return true;
        }
    }
}
