using System.IO;
using Nedev.ImageSharp;
using Nedev.ImageSharp.PixelFormats;
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
}