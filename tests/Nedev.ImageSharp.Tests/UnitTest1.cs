using System.IO;
using Nedev.ImageSharp;
using Nedev.ImageSharp.Advanced;
using Nedev.ImageSharp.Formats;
using Nedev.ImageSharp.PixelFormats;
using Nedev.ImageSharp.Processing;
using Xunit;

namespace Nedev.ImageSharp.Tests;

public class BasicUsageTests
{
    [Fact]
    public void DefaultConfiguration_IsNotNull()
    {
        Assert.NotNull(Configuration.Default);
    }

    [Fact]
    public void CanCreateAndSavePngToMemoryStream()
    {
        using var image = new Image<Rgba32>(1, 1);
        image[0, 0] = new Rgba32(255, 0, 0);

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);

        Assert.True(stream.Length > 0);
        Assert.True(stream.Position == stream.Length);
    }

    [Fact]
    public void Configuration_MinimumPixelsProcessedPerTask_IsAppliedToParallelExecutionSettings()
    {
        var config = new Configuration
        {
            MinimumPixelsProcessedPerTask = 12345
        };

        ParallelExecutionSettings settings = ParallelExecutionSettings.FromConfiguration(config);

        Assert.Equal(12345, settings.MinimumPixelsProcessedPerTask);
    }

    [Fact]
    public void Resize_UsesParallelResizeWorker_WhenMinimumPixelsProcessedPerTaskIsLow()
    {
        var config = new Configuration
        {
            MinimumPixelsProcessedPerTask = 1,
            MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount)
        };

        using var image = new Image<Rgba32>(200, 200);
        image[0, 0] = new Rgba32(255, 0, 0);

        image.Mutate(config, ctx => ctx.Resize(150, 150));

        Assert.Equal(150, image.Width);
        Assert.Equal(150, image.Height);
        // Ensure pixel data is still accessible after resize.
        Assert.Equal(255, image[0, 0].R);
    }

    [Fact]
    public void CanDetectAndDecodePngBasedIco()
    {
        using var image = new Image<Rgba32>(16, 16);
        image[0, 0] = new Rgba32(255, 0, 0);

        using MemoryStream icoStream = CreatePngBasedIcoStream(image);

        IImageFormat format = Image.DetectFormat(icoStream);
        Assert.Equal("ICO", format.Name);

        icoStream.Position = 0;
        using Image<Rgba32> decoded = Image.Load<Rgba32>(icoStream);
        Assert.Equal(16, decoded.Width);
        Assert.Equal(16, decoded.Height);
        Assert.Equal(255, decoded[0, 0].R);
    }

    [Fact]
    public void CanDetectAvifFormatHeader()
    {
        // Minimal AVIF: ftyp box with major_brand "avif"
        using var stream = new MemoryStream();

        // size = 12, "ftyp", "avif"
        stream.Write(BitConverter.GetBytes(12u), 0, 4);
        stream.Write(new byte[] { (byte)'f', (byte)'t', (byte)'y', (byte)'p' }, 0, 4);
        stream.Write(new byte[] { (byte)'a', (byte)'v', (byte)'i', (byte)'f' }, 0, 4);

        stream.Position = 0;
        IImageFormat format = Image.DetectFormat(stream);
        Assert.Equal("AVIF", format.Name);
    }

    [Fact]
    public void CanDetectHeicFormatHeader()
    {
        // Minimal HEIF: ftyp box with major_brand "heic"
        using var stream = new MemoryStream();

        // size = 12, "ftyp", "heic"
        stream.Write(BitConverter.GetBytes(12u), 0, 4);
        stream.Write(new byte[] { (byte)'f', (byte)'t', (byte)'y', (byte)'p' }, 0, 4);
        stream.Write(new byte[] { (byte)'h', (byte)'e', (byte)'i', (byte)'c' }, 0, 4);

        stream.Position = 0;
        IImageFormat format = Image.DetectFormat(stream);
        Assert.Equal("HEIC", format.Name);
    }

    [Fact]
    public void CanDecodeEmbeddedPngInAvif()
    {
        using var image = new Image<Rgba32>(16, 16);
        image[0, 0] = new Rgba32(255, 0, 0);

        // Create a fake AVIF stream containing a PNG inside.
        using var pngStream = new MemoryStream();
        image.SaveAsPng(pngStream);
        byte[] pngData = pngStream.ToArray();

        using var avifStream = new MemoryStream();
        // Fake AVIF header (ftyp + avif)
        avifStream.Write(BitConverter.GetBytes(12u), 0, 4);
        avifStream.Write(new byte[] { (byte)'f', (byte)'t', (byte)'y', (byte)'p' }, 0, 4);
        avifStream.Write(new byte[] { (byte)'a', (byte)'v', (byte)'i', (byte)'f' }, 0, 4);

        // Append PNG data
        avifStream.Write(pngData, 0, pngData.Length);
        avifStream.Position = 0;

        using Image<Rgba32> decoded = Image.Load<Rgba32>(avifStream);
        Assert.Equal(16, decoded.Width);
        Assert.Equal(16, decoded.Height);
        Assert.Equal(255, decoded[0, 0].R);
    }

    private static MemoryStream CreatePngBasedIcoStream(Image<Rgba32> source)
    {
        using var pngStream = new MemoryStream();
        source.SaveAsPng(pngStream);
        byte[] pngData = pngStream.ToArray();

        var icoStream = new MemoryStream();

        // ICONDIR
        icoStream.Write(new byte[] { 0, 0, 1, 0, 1, 0 });

        // Directory entry
        icoStream.WriteByte((byte)source.Width);
        icoStream.WriteByte((byte)source.Height);
        icoStream.WriteByte(0); // color count
        icoStream.WriteByte(0); // reserved
        icoStream.Write(BitConverter.GetBytes((ushort)1), 0, 2); // planes
        icoStream.Write(BitConverter.GetBytes((ushort)32), 0, 2); // bit count
        icoStream.Write(BitConverter.GetBytes((uint)pngData.Length), 0, 4);
        icoStream.Write(BitConverter.GetBytes((uint)6 + 16), 0, 4);

        icoStream.Write(pngData, 0, pngData.Length);
        icoStream.Position = 0;
        return icoStream;
    }
}
