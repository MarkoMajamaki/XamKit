using System;
using System.Collections.Generic;
using System.Text;

// Xamarin
using Xamarin.Forms;

// Skiasharp
using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Runtime.CompilerServices;

namespace XamKit
{
    public class ShadowView : Layout<View>
    {
        private SKCanvasView _skiaCanvas;
        private SKPaint _backgroundPaint;
        private SKPaint _borderPaint;

        protected float DeviceScale { get; private set; } = 1;

        #region BindingProperties

        /// <summary>
        /// Shadow lenght
        /// </summary>
        public static readonly BindableProperty ShadowLenghtProperty =
            BindableProperty.Create("ShadowLenght", typeof(double), typeof(ShadowView), 0.0);

        public double ShadowLenght
        {
            get { return (double)GetValue(ShadowLenghtProperty); }
            set { SetValue(ShadowLenghtProperty, value); }
        }

        /// <summary>
        /// Border stroke thickness
        /// </summary>
        public static readonly BindableProperty BorderThicknessProperty =
            BindableProperty.Create("BorderThickness", typeof(double), typeof(ShadowView), 0.0);

        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        /// <summary>
        /// Common radius for rounded corners
        /// </summary>
        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create("CornerRadius", typeof(double), typeof(ShadowView), 0.0);

        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Common radius for rounded corners
        /// </summary>
        public static readonly BindableProperty ShadowOpacityProperty =
            BindableProperty.Create("ShadowOpacity", typeof(double), typeof(ShadowView), 1.0);

        public double ShadowOpacity
        {
            get { return (double)GetValue(ShadowOpacityProperty); }
            set { SetValue(ShadowOpacityProperty, value); }
        }

        public static readonly BindableProperty IsShadowEnabledProperty =
            BindableProperty.Create("IsShadowEnabled", typeof(bool), typeof(ShadowView), true);

        public bool IsShadowEnabled
        {
            get { return (bool)GetValue(IsShadowEnabledProperty); }
            set { SetValue(IsShadowEnabledProperty, value); }
        }

        #endregion

        #region Colors

        /// <summary>
        /// Border color
        /// </summary>
        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create("BorderColor", typeof(Color), typeof(ShadowView), Color.Transparent);

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        /// <summary>
        /// Background color
        /// </summary>
        public static readonly BindableProperty BorderBackgroundColorProperty =
            BindableProperty.Create("BorderBackgroundColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color BorderBackgroundColor
        {
            get { return (Color)GetValue(BorderBackgroundColorProperty); }
            set { SetValue(BorderBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Shadow color
        /// </summary>
        public static readonly BindableProperty ShadowColorProperty =
            BindableProperty.Create("ShadowColor", typeof(Color), typeof(ShadowView), Color.Transparent);

        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }

        #endregion

        public ShadowView()
        {
            _skiaCanvas = new SKCanvasView();
            _skiaCanvas.PaintSurface += OnPaintSurface;
            Children.Add(_skiaCanvas);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == BorderColorProperty.PropertyName ||
                propertyName == BorderBackgroundColorProperty.PropertyName ||
                propertyName == ShadowColorProperty.PropertyName ||
                propertyName == CornerRadiusProperty.PropertyName ||
                propertyName == ShadowOpacityProperty.PropertyName ||
                propertyName == IsShadowEnabledProperty.PropertyName)
            {
                _borderPaint = null;
                _backgroundPaint = null;

                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (
                propertyName == BorderThicknessProperty.PropertyName ||
                propertyName == ShadowLenghtProperty.PropertyName)
            {
                _borderPaint = null;
                _backgroundPaint = null;

                if (HorizontalOptions.Alignment != LayoutAlignment.Fill || VerticalOptions.Alignment != LayoutAlignment.Fill)
                {
                    InvalidateMeasure();
                }

                InvalidateLayout();

                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }

            base.OnPropertyChanged(propertyName);
        }

        #region Measure / Layout

        /// <summary>
        /// Measure total size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            LowerChild(_skiaCanvas);

            Size totalSize = new Size();

            if (HorizontalOptions.Alignment != LayoutAlignment.Fill ||
                VerticalOptions.Alignment != LayoutAlignment.Fill ||
                double.IsNaN(widthConstraint) ||
                double.IsPositiveInfinity(widthConstraint) ||
                double.IsNaN(heightConstraint) ||
                double.IsPositiveInfinity(heightConstraint))
            {
                foreach (View child in Children)
                {
                    Size size = new Size();
                    if (child != _skiaCanvas)
                    {
                        size = child.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins).Request;
                    }

                    totalSize.Width = Math.Max(totalSize.Width, size.Width);
                    totalSize.Height = Math.Max(totalSize.Height, size.Height);
                }
            }
            else
            {
                totalSize = new Size(widthConstraint, heightConstraint);
            }

            return new SizeRequest(totalSize, totalSize);
        }

        /// <summary>
        /// Layout all children. Skia canvas go outside of bounds. Do not clip!
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            foreach (View child in Children)
            {
                if (child == _skiaCanvas)
                {
                    Rectangle canvasLocation = new Rectangle();
                    canvasLocation.X = -ShadowLenght;
                    canvasLocation.Y = -ShadowLenght;
                    canvasLocation.Width = width + (ShadowLenght * 2);
                    canvasLocation.Height = height + (ShadowLenght * 2);

                    LayoutChildIntoBoundingRegion(child, canvasLocation);
                }
                else
                {
                    LayoutChildIntoBoundingRegion(child, new Rectangle(x, y, width, height));
                }
            }
        }

        #endregion

        /// <summary>
        /// Paint background, border and shadow
        /// </summary>
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear();
            /*
            if (ShadowLenght.Equals(0))
            {
                return;
            }
            */
            // Get device pixel intencity scale
            DeviceScale = (float)(e.Info.Width / _skiaCanvas.Width);

            Rectangle contentLocationRelativeToCanvas = new Rectangle(
                ShadowLenght,
                ShadowLenght,
                Width - (ShadowLenght * 2),
                Height - (ShadowLenght * 2));

            float maxCornerRadius = (float)Math.Min(Width / 2, Height / 2) * DeviceScale;
            float skCornerRadius = Math.Min(maxCornerRadius, (float)(CornerRadius * DeviceScale));
            float skBorderThickness = (float)BorderThickness * DeviceScale;

            float skShadowLenght = (float)Math.Round(ShadowLenght * DeviceScale, MidpointRounding.AwayFromZero);

            SKRect skBackgroundDrawLocation = new SKRect(skShadowLenght, skShadowLenght, e.Info.Width - skShadowLenght, e.Info.Height - skShadowLenght);

            // Shadow

            if (ShadowLenght > 0 && ShadowColor != Color.Transparent && IsShadowEnabled)
            {
                SkiaUtils.DrawShadow(e, skBackgroundDrawLocation, skCornerRadius, skShadowLenght, ShadowColor, ShadowOpacity, false);
            }

            // Background

            if (BorderBackgroundColor != Color.Transparent)
            {
                if (_backgroundPaint == null)
                {
                    _backgroundPaint = new SKPaint();
                    _backgroundPaint.IsAntialias = true;
                    _backgroundPaint.Style = SKPaintStyle.Fill;
                    _backgroundPaint.Color = BorderBackgroundColor.ToSKColor();
                }

                SKRect backgroundRect = skBackgroundDrawLocation;

                if (BorderThickness > 0)
                {
                    backgroundRect.Inflate(-skBorderThickness, -skBorderThickness);
                }

                SKRoundRect backgroundRoundedRect = new SKRoundRect(backgroundRect, skCornerRadius, skCornerRadius);
                e.Surface.Canvas.DrawRoundRect(backgroundRoundedRect, _backgroundPaint);
            }

            // Border

            if (BorderThickness > 0 && BorderColor != Color.Transparent)
            {
                if (_borderPaint == null)
                {
                    _borderPaint = new SKPaint();
                    _borderPaint.Color = BorderColor.ToSKColor();
                    _borderPaint.IsAntialias = true;
                    _borderPaint.Style = SKPaintStyle.Stroke;
                    _borderPaint.StrokeWidth = skBorderThickness;
                }

                SKRect borderRect = skBackgroundDrawLocation;
                borderRect.Inflate(-skBorderThickness / 2, -skBorderThickness / 2);

                SKRoundRect borderRoundedRect = new SKRoundRect(borderRect, skCornerRadius, skCornerRadius);

                e.Surface.Canvas.DrawRoundRect(borderRoundedRect, _borderPaint);
            }
        }
    }
}
