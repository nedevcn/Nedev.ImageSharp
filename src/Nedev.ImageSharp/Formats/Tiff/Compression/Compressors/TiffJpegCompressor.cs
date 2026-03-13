// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Nedev.ImageSharp.Formats.Jpeg;
using Nedev.ImageSharp.Formats.Tiff.Constants;
using Nedev.ImageSharp.Memory;
using Nedev.ImageSharp.PixelFormats;

namespace Nedev.ImageSharp.Formats.Tiff.Compression.Compressors
{
    internal class TiffJpegCompressor : TiffBaseCompressor
    {
        public TiffJpegCompressor(Stream output, MemoryAllocator memoryAllocator, int width, int bitsPerPixel, TiffPredictor predictor = TiffPredictor.None)
            : base(output, memoryAllocator, width, bitsPerPixel, predictor)
        {
        }

        /// <inheritdoc/>
        public override TiffCompression Method => TiffCompression.Jpeg;

        /// <inheritdoc/>
        public override void Initialize(int rowsPerStrip)
        {
        }

        /// <inheritdoc/>
        public override void CompressStrip(Span<byte> rows, int height)
        {
            int pixelCount = rows.Length / 3;
            int width = pixelCount / height;

            using var memoryStream = new MemoryStream();
            var image = Image.LoadPixelData<Rgb24>(rows, width, height);
            image.Save(memoryStream, new JpegEncoder()
            {
                ColorType = JpegColorType.Rgb
            });
            memoryStream.Position = 0;
            memoryStream.WriteTo(this.Output);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
        }
    }
}

