﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Artemis.Core.Extensions;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.RGB.NET
{
    public class BitmapBrush : AbstractDecoratable<IBrushDecorator>, IBrush, IDisposable
    {
        #region Constructors

        public BitmapBrush(Scale scale)
        {
            Scale = scale;
        }

        #endregion

        #region Properties & Fields

        /// <inheritdoc />
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc />
        public BrushCalculationMode BrushCalculationMode { get; set; } = BrushCalculationMode.Absolute;

        /// <inheritdoc />
        public double Brightness { get; set; }

        /// <inheritdoc />
        public double Opacity { get; set; }

        /// <inheritdoc />
        public IList<IColorCorrection> ColorCorrections { get; } = new List<IColorCorrection>();

        /// <inheritdoc />
        public Rectangle RenderedRectangle { get; private set; }

        /// <inheritdoc />
        public Dictionary<BrushRenderTarget, Color> RenderedTargets { get; } = new Dictionary<BrushRenderTarget, Color>();

        public Scale Scale { get; set; }
        public SKBitmap Bitmap { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual void PerformRender(Rectangle rectangle, IEnumerable<BrushRenderTarget> renderTargets)
        {
            if (RenderedRectangle != rectangle || RenderedScale != Scale)
                Bitmap = null;

            RenderedRectangle = rectangle;
            RenderedScale = Scale;
            RenderedTargets.Clear();

            if (Bitmap == null)
                CreateBitmap(RenderedRectangle);

            foreach (var renderTarget in renderTargets)
            {
                // TODO: Right now the sample size is 1, make this configurable to something higher and average the samples out
                var scaledLocation = renderTarget.Point * Scale;
                if (scaledLocation.X < Bitmap.Width && scaledLocation.Y < Bitmap.Height)
                    RenderedTargets[renderTarget] = Bitmap.GetPixel(RoundToInt(scaledLocation.X), RoundToInt(scaledLocation.Y)).ToRgbColor();
            }
        }

        public Scale RenderedScale { get; private set; }

        private void CreateBitmap(Rectangle rectangle)
        {
            // TODO: Test this max size, it applied to System.Drawing.Bitmap but SKBitmap might scale better or worse
            var width = Math.Min((rectangle.Location.X + rectangle.Size.Width) * Scale.Horizontal, 4096);
            var height = Math.Min((rectangle.Location.Y + rectangle.Size.Height) * Scale.Vertical, 4096);
            Bitmap = new SKBitmap(new SKImageInfo(width.RoundToInt(), height.RoundToInt()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int RoundToInt(double number)
        {
            return (int) Math.Round(number, MidpointRounding.AwayFromZero);
        }

        /// <inheritdoc />
        public virtual void PerformFinalize()
        {
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
        }

        #endregion
    }
}