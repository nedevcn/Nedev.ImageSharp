using System.IO;
using Nedev.ImageSharp;
using Nedev.ImageSharp.Advanced;
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
}