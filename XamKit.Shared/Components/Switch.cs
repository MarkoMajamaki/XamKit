using System;

// Xamarin
using Xamarin.Forms;

// Skia
using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Runtime.CompilerServices;

namespace XamKit
{
    public class Switch : SelectionElementBase
    {
        public enum VisualStyles { UWP, Android, iOS }

        private float _shadowLenght = 3;

        private VisualSettings _visualSettings;

        #region Binding properties

        /// <summary>
        /// Switch style
        /// </summary>
        public static readonly BindableProperty VisualStyleProperty =
            BindableProperty.Create("VisualStyle", typeof(VisualStyles), typeof(Switch), VisualStyles.Android);

        public VisualStyles VisualStyle
        {
            get { return (VisualStyles)GetValue(VisualStyleProperty); }
            set { SetValue(VisualStyleProperty, value); }
        }

        /// <summary>
        /// Switch location relative to text
        /// </summary>
        public static readonly BindableProperty SwitchLocationProperty =
            BindableProperty.Create("SwitchLocation", typeof(HorizontalLocations), typeof(Switch), HorizontalLocations.Left);

        public HorizontalLocations SwitchLocation
        {
            get { return (HorizontalLocations)GetValue(SwitchLocationProperty); }
            set { SetValue(SwitchLocationProperty, value); }
        }
        
        /// <summary>
        /// Thumb shadow depth. Zero if not enabled.
        /// </summary>
        public static readonly BindableProperty IsThumbShadowEnabledProperty =
            BindableProperty.Create("IsThumbShadowEnabled", typeof(bool), typeof(Switch), false);

        public bool IsThumbShadowEnabled
        {
            get { return (bool)GetValue(IsThumbShadowEnabledProperty); }
            set { SetValue(IsThumbShadowEnabledProperty, value); }
        }

        #endregion

        #region BindingProperties - Colors

        /// <summary>
        /// Track border color when toggled
        /// </summary>
        public static readonly BindableProperty TrackBorderToggledColorProperty =
            BindableProperty.Create("TrackBorderToggledColor", typeof(Color), typeof(Switch), Color.Transparent);

        public Color TrackBorderToggledColor
        {
            get { return (Color)GetValue(TrackBorderToggledColorProperty); }
            set { SetValue(TrackBorderToggledColorProperty, value); }
        }

        /// <summary>
        /// Track border color when untoggled
        /// </summary>
        public static readonly BindableProperty TrackBorderUnToggledColorProperty =
            BindableProperty.Create("TrackBorderUnToggledColor", typeof(Color), typeof(Switch), Color.Transparent);

        public Color TrackBorderUnToggledColor
        {
            get { return (Color)GetValue(TrackBorderUnToggledColorProperty); }
            set { SetValue(TrackBorderUnToggledColorProperty, value); }
        }

        /// <summary>
        /// Track color when toggled
        /// </summary>
        public static readonly BindableProperty TrackToggledColorProperty =
            BindableProperty.Create("TrackToggledColor", typeof(Color), typeof(Switch), Color.LimeGreen);

        public Color TrackToggledColor
        {
            get { return (Color)GetValue(TrackToggledColorProperty); }
            set { SetValue(TrackToggledColorProperty, value); }
        }

        /// <summary>
        /// Tack color when untoggled
        /// </summary>
        public static readonly BindableProperty TrackUnToggledColorProperty =
            BindableProperty.Create("TrackUnToggledColor", typeof(Color), typeof(Switch), Color.Gray);

        public Color TrackUnToggledColor
        {
            get { return (Color)GetValue(TrackUnToggledColorProperty); }
            set { SetValue(TrackUnToggledColorProperty, value); }
        }

        // Thumb

        /// <summary>
        /// Thumb border color when toggled
        /// </summary>
        public static readonly BindableProperty ThumbBorderToggledColorProperty =
            BindableProperty.Create("ThumbBorderToggledColor", typeof(Color), typeof(Switch), Color.Transparent);

        public Color ThumbBorderToggledColor
        {
            get { return (Color)GetValue(ThumbBorderToggledColorProperty); }
            set { SetValue(ThumbBorderToggledColorProperty, value); }
        }

        /// <summary>
        /// Thumb border color when untoggled
        /// </summary>
        public static readonly BindableProperty ThumbBorderUnToggledColorProperty =
            BindableProperty.Create("ThumbBorderUnToggledColor", typeof(Color), typeof(Switch), Color.Transparent);

        public Color ThumbBorderUnToggledColor
        {
            get { return (Color)GetValue(ThumbBorderUnToggledColorProperty); }
            set { SetValue(ThumbBorderUnToggledColorProperty, value); }
        }

        /// <summary>
        /// Thumb color when toggled
        /// </summary>
        public static readonly BindableProperty ThumbToggledColorProperty =
            BindableProperty.Create("ThumbToggledColor", typeof(Color), typeof(Switch), Color.DarkGray);

        public Color ThumbToggledColor
        {
            get { return (Color)GetValue(ThumbToggledColorProperty); }
            set { SetValue(ThumbToggledColorProperty, value); }
        }

        /// <summary>
        /// Foreground movable part color when untoggled
        /// </summary>
        public static readonly BindableProperty ThumbUnToggledColorProperty =
            BindableProperty.Create("ThumbUnToggledColor", typeof(Color), typeof(Switch), Color.DarkGray);

        public Color ThumbUnToggledColor
        {
            get { return (Color)GetValue(ThumbUnToggledColorProperty); }
            set { SetValue(ThumbUnToggledColorProperty, value); }
        }

        #endregion
        
        private double NegativePadding
        {
            get
            {
                return Math.Max(EllipseDiameter, _shadowLenght);
            }
        }

        public Switch()
        {
            _visualSettings = new VisualSettings(VisualStyle);

            _selectionViewSize = new Size(
                Math.Max(_visualSettings.TrackWidthRequest, _visualSettings.ThumbWidthRequest + _visualSettings.ThumbPadding.HorizontalThickness), 
                Math.Max(_visualSettings.TrackWidthRequest, _visualSettings.ThumbHeightRequest + _visualSettings.ThumbPadding.VerticalThickness));

            _selectionViewLocation = SwitchLocation;
        }

        #region Paint

        /// <summary>
        /// Paint swich
        /// </summary>
        protected override void OnPaintSelectionElement(SKPaintSurfaceEventArgs e)
        {
            float skTrackCornerRadius = (float)(_visualSettings.TrackCornerRadius * DeviceScale);
            float skTrackBorderThickness = (float)(_visualSettings.TrackBorderThickness * DeviceScale);
            float skActualTrackHeight = (float)(_visualSettings.TrackHeightRequest * DeviceScale);
            float skActualTrackWidth = (float)(_visualSettings.TrackWidthRequest * DeviceScale);

            float skThumbCornerRadius = (float)(_visualSettings.ThumbCornerRadius * DeviceScale);
            float skThumbBorderThickness = (float)(_visualSettings.ThumbBorderThickness * DeviceScale);
            float skActualThumbHeight = (float)(_visualSettings.ThumbHeightRequest * DeviceScale);
            float skActualThumbWidth = (float)(_visualSettings.ThumbWidthRequest * DeviceScale);

            float skHorizontalTextMargin = string.IsNullOrEmpty(Text) == false ? (float)(TextMargin.HorizontalThickness * DeviceScale) : 0;

            float skNegativePadding = (float)NegativePadding * DeviceScale;
            float skWidth = (float)e.Info.Width;
            float skHeight = (float)e.Info.Height;

            //
            // Track location
            //
            
            float skActualTrackCornerRadius = Math.Min(skTrackCornerRadius, Math.Min(skActualTrackHeight, skActualTrackWidth));

            float skStartX = 0;
            if (SwitchLocation == HorizontalLocations.Right)
            {
                if (HorizontalOptions.Alignment == LayoutAlignment.Fill)
                {
                    skStartX = skWidth - skNegativePadding - skActualTrackWidth - (float)((Padding.Right + _visualSettings.ThumbHorizontalNegativeRightPadding) * DeviceScale);
                }
                else
                {
                    skStartX = (float)((_textSize.Width + _visualSettings.ThumbHorizontalNegativeLeftPadding) * DeviceScale) + skHorizontalTextMargin;
                }
            }
            else
            {
                skStartX = skNegativePadding + (float)(Padding.Right + _visualSettings.ThumbHorizontalNegativeRightPadding) * DeviceScale;
            }

            SKRect trackLocation = new SKRect();
            trackLocation.Left = skStartX;
            trackLocation.Top = (skHeight - skActualTrackHeight) / 2;
            trackLocation.Right = trackLocation.Left + skActualTrackWidth;
            trackLocation.Bottom = trackLocation.Top + skActualTrackHeight;

            float skActualThumbCornerRadius = Math.Min(skThumbCornerRadius, Math.Min(skActualThumbHeight / 2, skActualThumbWidth / 2));

            //
            // Thumb location
            //

            SKRect thumbLocation = new SKRect();
            thumbLocation.Left = trackLocation.Left + (float)(_visualSettings.ThumbPadding.Left * DeviceScale) + (float)_toggledAnimationProcess * (skActualTrackWidth - skActualThumbWidth - (float)(_visualSettings.ThumbPadding.HorizontalThickness * DeviceScale));
            thumbLocation.Top = (skHeight - skActualThumbHeight + (float)(_visualSettings.ThumbPadding.VerticalThickness * DeviceScale)) / 2;
            thumbLocation.Right = thumbLocation.Left + skActualThumbWidth;
            thumbLocation.Bottom = thumbLocation.Top + skActualThumbHeight;

            //
            // Draw Thumb ellipse
            //

            if (EllipseDiameter > 0 && _toggledAnimationProcess > 0 && _toggledAnimationProcess < 1 && IsEllipseAnimationEnabled)
            {
                SKPaint ellipsePaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                };

                if (_toggledAnimationProcess <= 0.5)
                {
                    ellipsePaint.Color = EllipseColor.MultiplyAlpha(_toggledAnimationProcessWithoutEasing * 2).ToSKColor();
                }
                else
                {
                    ellipsePaint.Color = EllipseColor.MultiplyAlpha(1 - (_toggledAnimationProcessWithoutEasing - 0.5) * 2).ToSKColor();
                }

                e.Surface.Canvas.DrawCircle(new SKPoint(thumbLocation.MidX, thumbLocation.MidY), (float)(EllipseDiameter / 2) * DeviceScale, ellipsePaint);
            }

            //
            // Draw track
            //

            SKPaint trackPaint = new SKPaint()
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = AnimationUtils.ColorTransform(_toggledAnimationProcess, TrackUnToggledColor, TrackToggledColor).ToSKColor(),
            };

            e.Surface.Canvas.DrawRoundRect(trackLocation, skActualTrackCornerRadius, skActualTrackCornerRadius, trackPaint);

            //
            // Draw track border
            //

            if (skTrackBorderThickness > 0)
            {
                SKRect trackBorderLocation = new SKRect();
                trackBorderLocation.Left = trackLocation.Left + skTrackBorderThickness / 2;
                trackBorderLocation.Top = trackLocation.Top + (skTrackBorderThickness / 2);
                trackBorderLocation.Right = trackLocation.Right - (skTrackBorderThickness / 2);
                trackBorderLocation.Bottom = trackLocation.Bottom - (skTrackBorderThickness / 2);

                SKPaint trackBorderPaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    Color = AnimationUtils.ColorTransform(_toggledAnimationProcess, TrackBorderUnToggledColor, TrackBorderToggledColor).ToSKColor(),
                    StrokeWidth = skTrackBorderThickness,
                };

                e.Surface.Canvas.DrawRoundRect(trackBorderLocation, skActualTrackCornerRadius, skActualTrackCornerRadius, trackBorderPaint);
            }

            //
            // Draw thumb shadow
            //

            if (IsThumbShadowEnabled)
            {
                SKRect shadowLocation = new SKRect();
                shadowLocation.Left = thumbLocation.Left;
                shadowLocation.Right = thumbLocation.Right;
                shadowLocation.Top = thumbLocation.Top + _shadowLenght;
                shadowLocation.Bottom = thumbLocation.Bottom + _shadowLenght;
                SkiaUtils.DrawShadow(e, shadowLocation, skActualThumbCornerRadius, (float)(_shadowLenght * DeviceScale), Color.Black, 0.2, true);
            }

            //
            // Draw thumb
            //

            SKPaint thumbPaint = new SKPaint()
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = AnimationUtils.ColorTransform(_toggledAnimationProcess, ThumbUnToggledColor, ThumbToggledColor).ToSKColor(),
            };

            SKRect thumbBackgroundLocation = thumbLocation;
            thumbBackgroundLocation.Inflate(-skThumbBorderThickness / 2, -skThumbBorderThickness / 2);

            e.Surface.Canvas.DrawRoundRect(thumbBackgroundLocation, skActualThumbCornerRadius, skActualThumbCornerRadius, thumbPaint);

            //
            // Draw thumb border
            //

            if (skThumbBorderThickness > 0)
            {
                SKRect thumbBorderLocation = new SKRect();
                thumbBorderLocation.Left = thumbLocation.Left + (skThumbBorderThickness / 2);
                thumbBorderLocation.Top = thumbLocation.Top + (skThumbBorderThickness / 2);
                thumbBorderLocation.Right = thumbLocation.Right - (skThumbBorderThickness / 2);
                thumbBorderLocation.Bottom = thumbLocation.Bottom - (skThumbBorderThickness / 2);

                SKPaint thumbBorderPaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    Color = AnimationUtils.ColorTransform(_toggledAnimationProcess, ThumbBorderUnToggledColor, ThumbBorderToggledColor).ToSKColor(),
                    StrokeWidth = skThumbBorderThickness,
                };

                e.Surface.Canvas.DrawRoundRect(thumbBorderLocation, skActualThumbCornerRadius, skActualThumbCornerRadius, thumbBorderPaint);
            }
        }

        #endregion

        #region Property values changed

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == Switch.TextProperty.PropertyName ||
                propertyName == Switch.FontStyleProperty.PropertyName ||
                propertyName == Switch.FontFamilyProperty.PropertyName ||
                propertyName == Switch.FontWeightProperty.PropertyName ||
                propertyName == Switch.TextMarginProperty.PropertyName ||
                propertyName == Switch.IsThumbShadowEnabledProperty.PropertyName ||
                propertyName == Switch.VisualStyleProperty.PropertyName ||
                propertyName == Switch.SwitchLocationProperty.PropertyName)
            {

                _visualSettings = new VisualSettings(VisualStyle);

                _selectionViewSize = new Size(
                    Math.Max(_visualSettings.TrackWidthRequest, _visualSettings.ThumbWidthRequest + _visualSettings.ThumbPadding.HorizontalThickness),
                    Math.Max(_visualSettings.TrackWidthRequest, _visualSettings.ThumbHeightRequest + _visualSettings.ThumbPadding.VerticalThickness));

                _selectionViewLocation = SwitchLocation;

                InvalidateMeasure();

                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }                
            }
            else if (propertyName == Switch.TrackUnToggledColorProperty.PropertyName ||
                     propertyName == Switch.TrackToggledColorProperty.PropertyName ||
                     propertyName == Switch.TrackBorderUnToggledColorProperty.PropertyName ||
                     propertyName == Switch.TrackBorderToggledColorProperty.PropertyName ||
                     propertyName == Switch.ThumbBorderToggledColorProperty.PropertyName ||
                     propertyName == Switch.ThumbBorderUnToggledColorProperty.PropertyName ||
                     propertyName == Switch.ThumbToggledColorProperty.PropertyName ||
                     propertyName == Switch.ThumbUnToggledColorProperty.PropertyName ||
                     propertyName == Switch.TextColorProperty.PropertyName)
            {
                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        #endregion

        #region Internal classes

        private class VisualSettings
        {
            public double TrackBorderThickness { get; private set; }
            public double TrackCornerRadius { get; private set; }
            public double TrackHeightRequest { get; private set; }
            public double TrackWidthRequest { get; private set; }

            public double ThumbBorderThickness { get; private set; }
            public double ThumbCornerRadius { get; private set; }
            public double ThumbHeightRequest { get; private set; }
            public double ThumbWidthRequest { get; private set; }
            public Thickness ThumbPadding { get; private set; }

            public double ThumbHorizontalNegativeLeftPadding { get; private set; }
            public double ThumbHorizontalNegativeRightPadding { get; private set; }
            public double ThumbHorizontalNegativePadding { get; private set; }

            public VisualSettings(VisualStyles style)
            {
                if (style == VisualStyles.Android)
                {
                    TrackBorderThickness = 0;
                    TrackCornerRadius = 100;
                    TrackHeightRequest = 14;
                    TrackWidthRequest = 20;

                    ThumbBorderThickness = 0;
                    ThumbCornerRadius = 100;
                    ThumbHeightRequest = 20;
                    ThumbWidthRequest = 20;

                    ThumbPadding = new Thickness(-10, 0, -10, 0);
                }
                else if (style == VisualStyles.iOS)
                {
                    TrackBorderThickness = 2;
                    TrackCornerRadius = 100;
                    TrackHeightRequest = 30;
                    TrackWidthRequest = 50;

                    ThumbBorderThickness = 0;
                    ThumbCornerRadius = 100;
                    ThumbHeightRequest = 26;
                    ThumbWidthRequest = 26;

                    ThumbPadding = new Thickness(2, 0, 2, 0);
                }
                else if (style == VisualStyles.UWP)
                {
                    TrackBorderThickness = 2;
                    TrackCornerRadius = 100;
                    TrackHeightRequest = 20;
                    TrackWidthRequest = 44;

                    ThumbBorderThickness = 2.5;
                    ThumbCornerRadius = 100;
                    ThumbHeightRequest = 15;
                    ThumbWidthRequest = 15;

                    ThumbPadding = new Thickness(2, 0, 2, 0);
                }

                ThumbHorizontalNegativeLeftPadding = ThumbPadding.Left < 0 ? Math.Abs(ThumbPadding.Left) : 0;
                ThumbHorizontalNegativeRightPadding = ThumbPadding.Right < 0 ? Math.Abs(ThumbPadding.Right) : 0;
                ThumbHorizontalNegativePadding = Math.Abs(ThumbHorizontalNegativeRightPadding + ThumbHorizontalNegativeLeftPadding);
            }
        }

        #endregion
    }
}
