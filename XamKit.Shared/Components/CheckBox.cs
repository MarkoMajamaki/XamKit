using System;
using System.Reflection;
using System.IO;

// Xamarin
using Xamarin.Forms;

// Skiasharp
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiaSharp.Extended.Svg;
using System.Windows.Input;

namespace XamKit
{
	public enum CheckBoxSelectionModes
	{
		// Selection is done by pressing whole element
		Element,

		// Selection is done only by pressing checkbox
		CheckBox,        
	}

    public class CheckBox : SelectionElementBase
    {
        private SkiaSharp.Extended.Svg.SKSvg _checkMarkSvg = null;
        
        #region Binding properties - CheckBox

        public static readonly BindableProperty CheckMarkIconResourceKeyProperty =
            BindableProperty.Create("CheckMarkIconResourceKey", typeof(string), typeof(CheckBox), null);

        public string CheckMarkIconResourceKey
        {
            get { return (string)GetValue(CheckMarkIconResourceKeyProperty); }
            set { SetValue(CheckMarkIconResourceKeyProperty, value); }
        }

        public static readonly BindableProperty CheckMarkIconAssemblyNameProperty =
            BindableProperty.Create("CheckMarkIconAssemblyName", typeof(string), typeof(CheckBox), null);

        public string CheckMarkIconAssemblyName
        {
            get { return (string)GetValue(CheckMarkIconAssemblyNameProperty); }
            set { SetValue(CheckMarkIconAssemblyNameProperty, value); }
        }

        public static readonly BindableProperty CheckBoxHeightRequestProperty =
            BindableProperty.Create("CheckBoxHeightRequest", typeof(double), typeof(CheckBox), 20.0);

        public double CheckBoxHeightRequest
        {
            get { return (double)GetValue(CheckBoxHeightRequestProperty); }
            set { SetValue(CheckBoxHeightRequestProperty, value); }
        }

        public static readonly BindableProperty CheckBoxWidthRequestProperty =
            BindableProperty.Create("CheckBoxWidthRequest", typeof(double), typeof(CheckBox), 20.0);

        public double CheckBoxWidthRequest
        {
            get { return (double)GetValue(CheckBoxWidthRequestProperty); }
            set { SetValue(CheckBoxWidthRequestProperty, value); }
        }

        public static readonly BindableProperty CheckMarkIconMarginProperty =
            BindableProperty.Create("CheckMarkIconMargin", typeof(Thickness), typeof(CheckBox), new Thickness(0, 0, 0, 0));

		public Thickness CheckMarkIconMargin
		{
			get { return (Thickness)GetValue(CheckMarkIconMarginProperty); }
			set { SetValue(CheckMarkIconMarginProperty, value); }
		}

        public static readonly BindableProperty CheckBoxMarginProperty =
            BindableProperty.Create("CheckBoxMargin", typeof(Thickness), typeof(CheckBox), new Thickness(0, 0, 0, 0));

        public Thickness CheckBoxMargin
        {
            get { return (Thickness)GetValue(CheckBoxMarginProperty); }
            set { SetValue(CheckBoxMarginProperty, value); }
        }

        public static readonly BindableProperty CheckBoxBorderThicknessProperty =
            BindableProperty.Create("CheckBoxBorderThickness", typeof(double), typeof(CheckBox), 1.0);

		public double CheckBoxBorderThickness
        {
            get { return (double)GetValue(CheckBoxBorderThicknessProperty); }
            set { SetValue(CheckBoxBorderThicknessProperty, value); }
        }

        public static readonly BindableProperty CheckBoxCornerRadiusProperty =
            BindableProperty.Create("CheckBoxCornerRadius", typeof(double), typeof(CheckBox), 1.0);

        public double CheckBoxCornerRadius
        {
            get { return (double)GetValue(CheckBoxCornerRadiusProperty); }
            set { SetValue(CheckBoxCornerRadiusProperty, value); }
        }

        public static readonly BindableProperty CheckBoxVerticalOptionsProperty =
            BindableProperty.Create("CheckBoxVerticalOptions", typeof(LayoutOptions), typeof(CheckBox), LayoutOptions.Center);

        public LayoutOptions CheckBoxVerticalOptions
        {
            get { return (LayoutOptions)GetValue(CheckBoxVerticalOptionsProperty); }
            set { SetValue(CheckBoxVerticalOptionsProperty, value); }
        }

		public static readonly BindableProperty SelectionModeProperty =
            BindableProperty.Create("SelectionMode", typeof(CheckBoxSelectionModes), typeof(CheckBox), CheckBoxSelectionModes.Element);

        public CheckBoxSelectionModes SelectionMode
		{
			get { return (CheckBoxSelectionModes)GetValue(SelectionModeProperty); }
			set { SetValue(SelectionModeProperty, value); }
		}

        public static readonly BindableProperty CheckBoxLocationProperty =
            BindableProperty.Create("CheckBoxLocation", typeof(HorizontalLocations), typeof(CheckBox), HorizontalLocations.Left);

        public HorizontalLocations CheckBoxLocation
        {
            get { return (HorizontalLocations)GetValue(CheckBoxLocationProperty); }
            set { SetValue(CheckBoxLocationProperty, value); }
        }

        #endregion

        #region Binding properties - Color

        // Default

        public static readonly BindableProperty CheckBoxBorderColorProperty =
            BindableProperty.Create("CheckBoxBorderColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckBoxBorderColor
        {
            get { return (Color)GetValue(CheckBoxBorderColorProperty); }
            set { SetValue(CheckBoxBorderColorProperty, value); }
        }

        public static readonly BindableProperty CheckBoxBackgroundColorProperty =
			BindableProperty.Create("CheckBoxBackgroundColor", typeof(Color), typeof(CheckBox), Color.Transparent);

		public Color CheckBoxBackgroundColor
		{
			get { return (Color)GetValue(CheckBoxBackgroundColorProperty); }
			set { SetValue(CheckBoxBackgroundColorProperty, value); }
		}

        public static readonly BindableProperty CheckMarkIconColorProperty =
            BindableProperty.Create("CheckMarkIconColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckMarkIconColor
        {
            get { return (Color)GetValue(CheckMarkIconColorProperty); }
            set { SetValue(CheckMarkIconColorProperty, value); }
        }

        // Hover

        public static readonly BindableProperty CheckBoxBorderHoverColorProperty =
            BindableProperty.Create("CheckBoxBorderHoverColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckBoxBorderHoverColor
        {
            get { return (Color)GetValue(CheckBoxBorderHoverColorProperty); }
            set { SetValue(CheckBoxBorderHoverColorProperty, value); }
        }

        public static readonly BindableProperty CheckBoxBackgroundHoverColorProperty =
            BindableProperty.Create("CheckBoxBackgroundHoverColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckBoxBackgroundHoverColor
        {
            get { return (Color)GetValue(CheckBoxBackgroundHoverColorProperty); }
            set { SetValue(CheckBoxBackgroundHoverColorProperty, value); }
        }

        public static readonly BindableProperty CheckMarkIconHoverColorProperty =
            BindableProperty.Create("CheckMarkIconHoverColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckMarkIconHoverColor
        {
            get { return (Color)GetValue(CheckMarkIconHoverColorProperty); }
            set { SetValue(CheckMarkIconHoverColorProperty, value); }
        }

        // Pressed

        public static readonly BindableProperty CheckBoxBorderPressedColorProperty =
            BindableProperty.Create("CheckBoxBorderPressedColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckBoxBorderPressedColor
        {
            get { return (Color)GetValue(CheckBoxBorderPressedColorProperty); }
            set { SetValue(CheckBoxBorderPressedColorProperty, value); }
        }

        public static readonly BindableProperty CheckBoxBackgroundPressedColorProperty =
            BindableProperty.Create("CheckBoxBackgroundPressedColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckBoxBackgroundPressedColor
        {
            get { return (Color)GetValue(CheckBoxBackgroundPressedColorProperty); }
            set { SetValue(CheckBoxBackgroundPressedColorProperty, value); }
        }

        public static readonly BindableProperty CheckMarkIconPressedColorProperty =
            BindableProperty.Create("CheckMarkIconPressedColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckMarkIconPressedColor
        {
            get { return (Color)GetValue(CheckMarkIconPressedColorProperty); }
            set { SetValue(CheckMarkIconPressedColorProperty, value); }
        }

        // Default toggled

        public static readonly BindableProperty ToggledCheckBoxBorderColorProperty =
            BindableProperty.Create("ToggledCheckBoxBorderColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color ToggledCheckBoxBorderColor
        {
            get { return (Color)GetValue(ToggledCheckBoxBorderColorProperty); }
            set { SetValue(ToggledCheckBoxBorderColorProperty, value); }
        }

        public static readonly BindableProperty ToggledCheckBoxBackgroundColorProperty =
            BindableProperty.Create("ToggledCheckBoxBackgroundColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color ToggledCheckBoxBackgroundColor
        {
            get { return (Color)GetValue(ToggledCheckBoxBackgroundColorProperty); }
            set { SetValue(ToggledCheckBoxBackgroundColorProperty, value); }
        }

        public static readonly BindableProperty ToggledCheckMarkIconColorProperty =
            BindableProperty.Create("ToggledCheckMarkIconColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color ToggledCheckMarkIconColor
        {
            get { return (Color)GetValue(ToggledCheckMarkIconColorProperty); }
            set { SetValue(ToggledCheckMarkIconColorProperty, value); }
        }

        // Hover toggled

        public static readonly BindableProperty ToggledCheckBoxBorderHoverColorProperty =
            BindableProperty.Create("ToggledCheckBoxBorderHoverColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color ToggledCheckBoxBorderHoverColor
        {
            get { return (Color)GetValue(ToggledCheckBoxBorderHoverColorProperty); }
            set { SetValue(ToggledCheckBoxBorderHoverColorProperty, value); }
        }

        public static readonly BindableProperty ToggledCheckBoxBackgroundHoverColorProperty =
            BindableProperty.Create("ToggledCheckBoxBackgroundHoverColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color ToggledCheckBoxBackgroundHoverColor
        {
            get { return (Color)GetValue(ToggledCheckBoxBackgroundHoverColorProperty); }
            set { SetValue(ToggledCheckBoxBackgroundHoverColorProperty, value); }
        }

        public static readonly BindableProperty ToggledCheckMarkIconHoverColorProperty =
            BindableProperty.Create("ToggledCheckMarkIconHoverColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color ToggledCheckMarkIconHoverColor
        {
            get { return (Color)GetValue(ToggledCheckMarkIconHoverColorProperty); }
            set { SetValue(ToggledCheckMarkIconHoverColorProperty, value); }
        }

        // Pressed toggled

        public static readonly BindableProperty ToggledCheckBoxBorderPressedColorProperty =
            BindableProperty.Create("ToggledCheckBoxBorderPressedColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color ToggledCheckBoxBorderPressedColor
        {
            get { return (Color)GetValue(ToggledCheckBoxBorderPressedColorProperty); }
            set { SetValue(ToggledCheckBoxBorderPressedColorProperty, value); }
        }

        public static readonly BindableProperty ToggledCheckBoxBackgroundPressedColorProperty =
            BindableProperty.Create("ToggledCheckBoxBackgroundPressedColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color ToggledCheckBoxBackgroundPressedColor
        {
            get { return (Color)GetValue(ToggledCheckBoxBackgroundPressedColorProperty); }
            set { SetValue(ToggledCheckBoxBackgroundPressedColorProperty, value); }
        }

        public static readonly BindableProperty ToggledCheckMarkIconPressedColorProperty =
            BindableProperty.Create("ToggledCheckMarkIconPressedColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color ToggledCheckMarkIconPressedColor
        {
            get { return (Color)GetValue(ToggledCheckMarkIconPressedColorProperty); }
            set { SetValue(ToggledCheckMarkIconPressedColorProperty, value); }
        }

        // Disabled

        public static readonly BindableProperty CheckBoxBorderDisabledColorProperty =
            BindableProperty.Create("CheckBoxBorderDisabledColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckBoxBorderDisabledColor
        {
            get { return (Color)GetValue(CheckBoxBorderDisabledColorProperty); }
            set { SetValue(CheckBoxBorderDisabledColorProperty, value); }
        }

        public static readonly BindableProperty CheckBoxBackgroundDisabledColorProperty =
            BindableProperty.Create("CheckBoxBackgroundDisabledColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckBoxBackgroundDisabledColor
        {
            get { return (Color)GetValue(CheckBoxBackgroundDisabledColorProperty); }
            set { SetValue(CheckBoxBackgroundDisabledColorProperty, value); }
        }

        public static readonly BindableProperty CheckMarkIconDisabledColorProperty =
            BindableProperty.Create("CheckMarkIconDisabledColor", typeof(Color), typeof(CheckBox), Color.Transparent);

        public Color CheckMarkIconDisabledColor
        {
            get { return (Color)GetValue(CheckMarkIconDisabledColorProperty); }
            set { SetValue(CheckMarkIconDisabledColorProperty, value); }
        }

        #endregion

        public CheckBox()
        {
            _selectionViewSize = new Size(CheckBoxMargin.HorizontalThickness + CheckBoxWidthRequest, CheckBoxMargin.VerticalThickness + CheckBoxHeightRequest);
            _selectionViewLocation = CheckBoxLocation;
        }

        #region Paint

        /// <summary>
        /// Paint checkbox
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaintSelectionElement(SKPaintSurfaceEventArgs e)
        {
            if (_checkMarkSvg == null && string.IsNullOrEmpty(CheckMarkIconAssemblyName) == false && string.IsNullOrEmpty(CheckMarkIconResourceKey) == false)
            {
                _checkMarkSvg = SvgImage.GetSvgImage(CheckMarkIconAssemblyName, CheckMarkIconResourceKey);
            }

            Rectangle checkBoxAvailableLocation = new Rectangle();
            Rectangle availableSpace = new Rectangle(EllipseDiameter, EllipseDiameter, Width, Height - Padding.VerticalThickness);

            if (CheckBoxLocation == HorizontalLocations.Left)
            {
                checkBoxAvailableLocation = availableSpace;
            }
            else
            {
                checkBoxAvailableLocation = new Rectangle(availableSpace.Width - CheckBoxWidthRequest - CheckBoxMargin.HorizontalThickness + EllipseDiameter, EllipseDiameter, availableSpace.Width, availableSpace.Height);
            }

            Rectangle checkBoxActualLocation = CheckBoxActualLocation(checkBoxAvailableLocation);

            float skCheckBoxBorderThickness = (float)CheckBoxBorderThickness * DeviceScale;
            float skCheckBoxX = (float)checkBoxActualLocation.X * DeviceScale;
            float skCheckBoxY = (float)checkBoxActualLocation.Y * DeviceScale;
            float skCheckBoxWidth = (float)CheckBoxWidthRequest * DeviceScale;
            float skCheckBoxHeight = (float)CheckBoxHeightRequest * DeviceScale;
            float skCornerRadius = (float)CheckBoxCornerRadius * DeviceScale;

            SKMatrix checkMarkMatrix = new SKMatrix();
            SKPoint checkMarkPosition = new SKPoint();

            Size checkMarkIconSize = new Size(CheckBoxWidthRequest - CheckMarkIconMargin.HorizontalThickness, CheckBoxHeightRequest - CheckMarkIconMargin.VerticalThickness);

            float scale = SvgImage.CalculateScale(_checkMarkSvg.Picture.CullRect.Size, checkMarkIconSize.Width, checkMarkIconSize.Height);

            Size actualCheckMarkIconSize = new Size(_checkMarkSvg.Picture.CullRect.Width * scale, _checkMarkSvg.Picture.CullRect.Height * scale);

            scale = scale * DeviceScale;

            checkMarkPosition.X = (float)skCheckBoxX + (float)((CheckBoxWidthRequest - actualCheckMarkIconSize.Width) / 2) * DeviceScale;
            checkMarkPosition.Y = (float)skCheckBoxY + (float)((CheckBoxHeightRequest - actualCheckMarkIconSize.Height) / 2) * DeviceScale;
            checkMarkMatrix.SetScaleTranslate(scale, scale, checkMarkPosition.X, checkMarkPosition.Y);

            SKRect checkBoxPaintRect = new SKRect(skCheckBoxX + skCheckBoxBorderThickness / 2,
                                                  skCheckBoxY + skCheckBoxBorderThickness / 2,
                                                  skCheckBoxX + skCheckBoxWidth - skCheckBoxBorderThickness / 2,
                                                  skCheckBoxY + skCheckBoxHeight - skCheckBoxBorderThickness / 2);

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

                e.Surface.Canvas.DrawCircle(new SKPoint(checkBoxPaintRect.MidX, checkBoxPaintRect.MidY), (float)(EllipseDiameter / 2) * DeviceScale, ellipsePaint);
            }

            Color backgroundColor = Color.Transparent;
            if (CheckBoxBackgroundColor != null && CheckBoxBackgroundColor != Color.Transparent && ToggledCheckBoxBackgroundColor != null && ToggledCheckBoxBackgroundColor != Color.Transparent)
            {
                backgroundColor = AnimationUtils.ColorTransform(_toggledAnimationProcess, CheckBoxBackgroundColor, ToggledCheckBoxBackgroundColor);
            }
            else if ((CheckBoxBackgroundColor == null || CheckBoxBackgroundColor == Color.Transparent) && ToggledCheckBoxBackgroundColor != null && ToggledCheckBoxBackgroundColor != Color.Transparent)
            {
                backgroundColor = ToggledCheckBoxBackgroundColor;
            }
            else
            {
                backgroundColor = CheckBoxBackgroundColor;
            }

            SKPaint checkBoxBackgroundPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = backgroundColor.ToSKColor(),
            };

            SKRect rect = new SKRect(
                skCheckBoxX + skCheckBoxBorderThickness,
                skCheckBoxY + skCheckBoxBorderThickness,
                skCheckBoxX + skCheckBoxWidth - skCheckBoxBorderThickness,
                skCheckBoxY + skCheckBoxHeight - skCheckBoxBorderThickness);

            e.Surface.Canvas.Save();

            SKRect r = SKRect.Create(rect.Left, rect.Top, rect.Width, rect.Height);
            if (_toggledAnimationProcess <= 0.75)
            {
                float v = (float)(_toggledAnimationProcess * (1 / 0.75));
                r.Inflate(-rect.Width * v / 2, -rect.Height * v / 2);
                e.Surface.Canvas.ClipRect(r, SKClipOperation.Difference);
            }

            e.Surface.Canvas.DrawRoundRect(checkBoxPaintRect, skCornerRadius, skCornerRadius, checkBoxBackgroundPaint);
            e.Surface.Canvas.Restore();

            SKPaint checkBoxBorderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                StrokeWidth = skCheckBoxBorderThickness,
                Color = AnimationUtils.ColorTransform(_toggledAnimationProcess, CheckBoxBorderColor, ToggledCheckBoxBorderColor).ToSKColor(),
            };

            e.Surface.Canvas.DrawRoundRect(checkBoxPaintRect, skCornerRadius, skCornerRadius, checkBoxBorderPaint);

            using (var paint = new SKPaint())
            {
                Color color = Color.Transparent;
                
                if (_toggledAnimationProcess > 0.75)
                {
                    float v = (float)((_toggledAnimationProcess - 0.75) * (1 / 0.25));
                    color = AnimationUtils.ColorTransform(v, CheckMarkIconColor, ToggledCheckMarkIconColor);
                }

                if (color != Color.Transparent)
                {
                    paint.ColorFilter = SKColorFilter.CreateBlendMode(color.ToSKColor(), SKBlendMode.SrcIn);
                    paint.Style = SKPaintStyle.Fill;
                    paint.IsAntialias = true;

                    e.Surface.Canvas.DrawPicture(_checkMarkSvg.Picture, ref checkMarkMatrix, paint);
                }
            }
        }

        /// <summary>
        /// Calculate checkbox actual location with alignment
        /// </summary>
        private Rectangle CheckBoxActualLocation(Rectangle availableSpace)
        {
            double checkBoxX = availableSpace.X;
            double checkBoxY = availableSpace.Y;

            switch (CheckBoxVerticalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                case LayoutAlignment.Start:
                    checkBoxX += CheckBoxMargin.Left;
                    checkBoxY += CheckBoxMargin.Top;
                    break;
                case LayoutAlignment.Center:
                    checkBoxX += CheckBoxMargin.Left;
                    checkBoxY += (availableSpace.Height - CheckBoxHeightRequest) / 2;
                    break;
                case LayoutAlignment.End:
                    checkBoxX += CheckBoxMargin.Left;
                    checkBoxY += availableSpace.Height - CheckBoxHeightRequest;
                    break;
            }

            return new Rectangle(checkBoxX, checkBoxY, CheckBoxWidthRequest, CheckBoxHeightRequest);
        }

        #endregion

        #region Property changes callbacks

        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == CheckMarkIconResourceKeyProperty.PropertyName ||
                propertyName == CheckMarkIconAssemblyNameProperty.PropertyName ||
                propertyName == CheckBoxHeightRequestProperty.PropertyName ||
                propertyName == CheckBoxWidthRequestProperty.PropertyName ||
                propertyName == CheckMarkIconMarginProperty.PropertyName ||
                propertyName == CheckBoxMarginProperty.PropertyName ||
                propertyName == CheckBoxBorderThicknessProperty.PropertyName ||
                propertyName == CheckBoxCornerRadiusProperty.PropertyName ||
                propertyName == CheckBoxVerticalOptionsProperty.PropertyName ||
                propertyName == CheckBoxBorderColorProperty.PropertyName)
            {
                _selectionViewSize = new Size(CheckBoxMargin.HorizontalThickness + CheckBoxWidthRequest, CheckBoxMargin.VerticalThickness + CheckBoxHeightRequest);

                InvalidateMeasure();

                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName == CheckBoxLocationProperty.PropertyName)
            {
                _selectionViewLocation = CheckBoxLocation;
            }
            else if (propertyName == SelectionModeProperty.PropertyName)
            {
                InvalidateMeasure();

                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName == CheckMarkIconColorProperty.PropertyName ||
                     propertyName == CheckBoxBorderColorProperty.PropertyName ||
                     propertyName == CheckBoxBackgroundColorProperty.PropertyName)
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
    }
}
