using Nedev.ImageSharp.Formats.Av1;
using Xunit;

namespace Nedev.ImageSharp.Tests
{
    public class Av1ParserTests
    {
        [Fact]
        public void TryParseSequenceHeader_ReturnsTrueForMinimalSequenceHeaderObu()
        {
            // Construct a minimal AV1 OBU containing a sequence header payload.
            // OBU header (forbidden=0, type=1 (sequence header), extension=0, has_size=1): 0x0A
            // Size field (ULEB128) = 1
            // Payload: profile=0, still_picture=1, reduced_still_picture_header=1, bit_depth_minus_8=0
            byte[] data = { 0x0A, 0x01, 0x18 };

            Assert.True(Av1Parser.TryParseSequenceHeader(data, out var header));
            Assert.Equal(0, header.Profile);
            Assert.True(header.StillPicture);
            Assert.True(header.ReducedStillPictureHeader);
            Assert.Equal(8, header.BitDepth);
        }

        [Fact]
        public void TryParseFrameHeader_ReturnsTrueForMinimalKeyFrameObu()
        {
            // Build a minimal AV1 frame header OBU (keyframe) for testing.
            // OBU header (forbidden=0, type=3 (frame header), extension=0, has_size=1): 0x18
            // Size (ULEB128) = 6
            // Payload:
            //   - frame_marker=2 (2 bits)
            //   - profile=0 (2 bits)
            //   - show_existing_frame=0 (1 bit)
            //   - frame_type=0 (keyframe) (2 bits)
            //   - show_frame=1 (1 bit)
            //   - error_resilient_mode=0 (1 bit)
            //   (byte align)
            //   - frame_width=16 (0x0010)
            //   - frame_height=16 (0x0010)
            byte[] data = { 0x1A, 0x06, 0x81, 0x00, 0x00, 0x10, 0x00, 0x10 };

            Assert.True(Av1Parser.TryParseFrameHeader(data, out var header));
            Assert.True(header.IsKeyFrame);
            Assert.Equal(16, header.FrameWidth);
            Assert.Equal(16, header.FrameHeight);
        }

        [Fact]
        public void TryExtractFirstTileGroup_ReturnsTrueForMinimalTileGroupObu()
        {
            // OBU header (forbidden=0, type=4 (tile group), extension=0, has_size=1): 0x22
            // Size (ULEB128) = 1
            // Payload: 0x55
            byte[] data = { 0x22, 0x01, 0x55 };

            Assert.True(Av1Parser.TryExtractFirstTileGroup(data, out var tileGroupPayload));
            Assert.Equal(1, tileGroupPayload.Length);
            Assert.Equal(0x55, tileGroupPayload.Span[0]);
        }

        [Fact]
        public void TryParseFrame_ReturnsParsedFrameWithTileGroup()
        {
            // Sequence header OBU
            byte[] sequenceObu = { 0x0A, 0x01, 0x18 };
            // Frame header OBU (keyframe)
            byte[] frameHeaderObu = { 0x1A, 0x06, 0x81, 0x00, 0x00, 0x10, 0x00, 0x10 };
            // Tile group OBU with a single byte payload
            byte[] tileGroupObu = { 0x22, 0x01, 0x55 };

            byte[] data = new byte[sequenceObu.Length + frameHeaderObu.Length + tileGroupObu.Length];
            System.Buffer.BlockCopy(sequenceObu, 0, data, 0, sequenceObu.Length);
            System.Buffer.BlockCopy(frameHeaderObu, 0, data, sequenceObu.Length, frameHeaderObu.Length);
            System.Buffer.BlockCopy(tileGroupObu, 0, data, sequenceObu.Length + frameHeaderObu.Length, tileGroupObu.Length);

            Assert.True(Av1Parser.TryParseFrame(data, out var parsedFrame));
            Assert.NotNull(parsedFrame.SequenceHeader);
            Assert.True(parsedFrame.FrameHeader.IsKeyFrame);
            Assert.Single(parsedFrame.TileGroups);
            Assert.Equal(0x55, parsedFrame.TileGroups[0].Span[0]);
        }
    }
}
