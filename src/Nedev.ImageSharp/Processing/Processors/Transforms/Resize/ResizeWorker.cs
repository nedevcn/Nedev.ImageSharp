// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nedev.ImageSharp.Memory;
using Nedev.ImageSharp.PixelFormats;

namespace Nedev.ImageSharp.Processing.Processors.Transforms
{
    /// <summary>
    /// Implements the resize algorithm using a sliding window of size
    /// maximized by <see cref="Configuration.WorkingBufferSizeHintInBytes"/>.
    /// The height of the window is a multiple of the vertical kernel's maximum diameter.
    /// When sliding the window, the contents of the bottom window band are copied to the new top band.
    /// For more details, and visual explanation, see "ResizeWorker.pptx".
    /// </summary>
    internal sealed class ResizeWorker<TPixel> : IDisposable
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private Buffer2D<Vector4> transposedFirstPassBuffer;

        private Configuration configuration;

        private PixelConversionModifiers conversionModifiers;

        private ResizeKernelMap horizontalKernelMap;

        private Buffer2DRegion<TPixel> source;

        private Rectangle sourceRectangle;

        private IMemoryOwner<Vector4> tempRowBuffer;

        private IMemoryOwner<Vector4> tempColumnBuffer;

        private ResizeKernelMap verticalKernelMap;

        private Rectangle targetWorkingRect;

        private Point targetOrigin;

        private int windowBandHeight;

        private int workerHeight;

        private int bufferWidth;

        private int bufferHeight;

        private RowInterval currentWindow;

        public ResizeWorker(
            Configuration configuration,
            Buffer2DRegion<TPixel> source,
            PixelConversionModifiers conversionModifiers,
            ResizeKernelMap horizontalKernelMap,
            ResizeKernelMap verticalKernelMap,
            Rectangle targetWorkingRect,
            Point targetOrigin)
        {
            this.Initialize(
                configuration,
                source,
                conversionModifiers,
                horizontalKernelMap,
                verticalKernelMap,
                targetWorkingRect,
                targetOrigin);
        }

        public void Reset(
            Configuration configuration,
            Buffer2DRegion<TPixel> source,
            PixelConversionModifiers conversionModifiers,
            ResizeKernelMap horizontalKernelMap,
            ResizeKernelMap verticalKernelMap,
            Rectangle targetWorkingRect,
            Point targetOrigin)
        {
            this.Initialize(
                configuration,
                source,
                conversionModifiers,
                horizontalKernelMap,
                verticalKernelMap,
                targetWorkingRect,
                targetOrigin);
        }

        private void Initialize(
            Configuration configuration,
            Buffer2DRegion<TPixel> source,
            PixelConversionModifiers conversionModifiers,
            ResizeKernelMap horizontalKernelMap,
            ResizeKernelMap verticalKernelMap,
            Rectangle targetWorkingRect,
            Point targetOrigin)
        {
            this.configuration = configuration;
            this.source = source;
            this.sourceRectangle = source.Rectangle;
            this.conversionModifiers = conversionModifiers;
            this.horizontalKernelMap = horizontalKernelMap;
            this.verticalKernelMap = verticalKernelMap;
            this.targetWorkingRect = targetWorkingRect;
            this.targetOrigin = targetOrigin;

            this.windowBandHeight = verticalKernelMap.MaxDiameter;

            // We need to make sure the working buffer is contiguous:
            int workingBufferLimitHintInBytes = Math.Min(
                configuration.WorkingBufferSizeHintInBytes,
                configuration.MemoryAllocator.GetBufferCapacityInBytes());

            int numberOfWindowBands = ResizeHelper.CalculateResizeWorkerHeightInWindowBands(
                this.windowBandHeight,
                targetWorkingRect.Width,
                workingBufferLimitHintInBytes);

            this.workerHeight = Math.Min(this.sourceRectangle.Height, numberOfWindowBands * this.windowBandHeight);

            this.EnsureBufferCapacity(targetWorkingRect.Width, this.workerHeight);

            this.currentWindow = new RowInterval(0, this.workerHeight);
        }

        private void EnsureBufferCapacity(int width, int height)
        {
            // Only reallocate if the new size exceeds what we already have.
            if (width <= this.bufferWidth && height <= this.bufferHeight)
            {
                return;
            }

            this.transposedFirstPassBuffer?.Dispose();
            this.tempRowBuffer?.Dispose();
            this.tempColumnBuffer?.Dispose();

            this.bufferWidth = width;
            this.bufferHeight = height;

            this.transposedFirstPassBuffer = this.configuration.MemoryAllocator.Allocate2D<Vector4>(
                height,
                width,
                preferContiguosImageBuffers: true,
                options: AllocationOptions.Clean);

            this.tempRowBuffer = this.configuration.MemoryAllocator.Allocate<Vector4>(this.sourceRectangle.Width);
            this.tempColumnBuffer = this.configuration.MemoryAllocator.Allocate<Vector4>(width);
        }

        public void Dispose()
        {
            this.transposedFirstPassBuffer?.Dispose();
            this.tempRowBuffer?.Dispose();
            this.tempColumnBuffer?.Dispose();
        }

        [MethodImpl(InliningOptions.ShortMethod)]
        public Span<Vector4> GetColumnSpan(int x, int startY)
            => this.transposedFirstPassBuffer.DangerousGetRowSpan(x).Slice(startY - this.currentWindow.Min);

        public void Initialize()
            => this.CalculateFirstPassValues(this.currentWindow);

        public void FillDestinationPixels(RowInterval rowInterval, Buffer2D<TPixel> destination)
        {
            Span<Vector4> tempColSpan = this.tempColumnBuffer.GetSpan();

            // When creating transposedFirstPassBuffer, we made sure it's contiguous:
            Span<Vector4> transposedFirstPassBufferSpan = this.transposedFirstPassBuffer.DangerousGetSingleSpan();

            int left = this.targetWorkingRect.Left;
            int right = this.targetWorkingRect.Right;
            int width = this.targetWorkingRect.Width;
            for (int y = rowInterval.Min; y < rowInterval.Max; y++)
            {
                // Ensure offsets are normalized for cropping and padding.
                ResizeKernel kernel = this.verticalKernelMap.GetKernel(y - this.targetOrigin.Y);

                while (kernel.StartIndex + kernel.Length > this.currentWindow.Max)
                {
                    this.Slide();
                }

                ref Vector4 tempRowBase = ref MemoryMarshal.GetReference(tempColSpan);

                int top = kernel.StartIndex - this.currentWindow.Min;

                ref Vector4 fpBase = ref transposedFirstPassBufferSpan[top];

                for (nint x = 0; x < (right - left); x++)
                {
                    ref Vector4 firstPassColumnBase = ref Unsafe.Add(ref fpBase, x * this.workerHeight);

                    // Destination color components
                    Unsafe.Add(ref tempRowBase, x) = kernel.ConvolveCore(ref firstPassColumnBase);
                }

                Span<TPixel> targetRowSpan = destination.DangerousGetRowSpan(y).Slice(left, width);

                PixelOperations<TPixel>.Instance.FromVector4Destructive(this.configuration, tempColSpan, targetRowSpan, this.conversionModifiers);
            }
        }

        private void Slide()
        {
            int minY = this.currentWindow.Max - this.windowBandHeight;
            int maxY = Math.Min(minY + this.workerHeight, this.sourceRectangle.Height);

            // Copy previous bottom band to the new top:
            // (rows <--> columns, because the buffer is transposed)
            this.transposedFirstPassBuffer.DangerousCopyColumns(
                this.workerHeight - this.windowBandHeight,
                0,
                this.windowBandHeight);

            this.currentWindow = new RowInterval(minY, maxY);

            // Calculate the remainder:
            this.CalculateFirstPassValues(this.currentWindow.Slice(this.windowBandHeight));
        }

        private void CalculateFirstPassValues(RowInterval calculationInterval)
        {
            Span<Vector4> tempRowSpan = this.tempRowBuffer.GetSpan();
            Span<Vector4> transposedFirstPassBufferSpan = this.transposedFirstPassBuffer.DangerousGetSingleSpan();

            int left = this.targetWorkingRect.Left;
            int right = this.targetWorkingRect.Right;
            int targetOriginX = this.targetOrigin.X;
            for (int y = calculationInterval.Min; y < calculationInterval.Max; y++)
            {
                Span<TPixel> sourceRow = this.source.DangerousGetRowSpan(y);

                PixelOperations<TPixel>.Instance.ToVector4(
                    this.configuration,
                    sourceRow,
                    tempRowSpan,
                    this.conversionModifiers);

                // optimization for:
                // Span<Vector4> firstPassSpan = transposedFirstPassBufferSpan.Slice(y - this.currentWindow.Min);
                ref Vector4 firstPassBaseRef = ref transposedFirstPassBufferSpan[y - this.currentWindow.Min];

                for (nint x = left, z = 0; x < right; x++, z++)
                {
                    ResizeKernel kernel = this.horizontalKernelMap.GetKernel(x - targetOriginX);

                    // optimization for:
                    // firstPassSpan[x * this.workerHeight] = kernel.Convolve(tempRowSpan);
                    Unsafe.Add(ref firstPassBaseRef, z * this.workerHeight) = kernel.Convolve(tempRowSpan);
                }
            }
        }
    }
}

