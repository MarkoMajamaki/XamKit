using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Windows.Input;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public abstract class SelectionElementBase : Layout<View>, IToggable
    {
        // 0 -> 1
        protected double _toggledAnimationProcess = 0;
        protected double _toggledAnimationProcessWithoutEasing = 0;
        protected const string _animationName = "anim";

        protected SKCanvasView _skiaCanvas;
        protected View _actualContentElement;

        protected Size _textSize = new Size();

        protected Size _selectionViewSize = new Size();
        protected HorizontalLocations _selectionViewLocation = HorizontalLocations.Left;

        protected float DeviceScale { get; private set; } = 1;

        /// <summary>
        /// Event to raise when IsToggled changes. Parameter is IsToggled value
        /// </summary>
        public event EventHandler<bool> IsToggledChanged;

        #region Binding properties

        /// <summary>
        /// Content element
        /// </summary>
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create("Content", typeof(object), typeof(SelectionElementBase), null, propertyChanged: OnContentChanged);

        private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as SelectionElementBase).OnContentChanged(oldValue, newValue);
        }

        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Template to create content
        /// </summary>
        public static readonly BindableProperty ContentTemplateProperty =
            BindableProperty.Create("ContentTemplate", typeof(DataTemplate), typeof(SelectionElementBase), null, propertyChanged: OnContentTemplateChanged);

        private static void OnContentTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as SelectionElementBase).OnContentTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        /// <summary>
        /// Is checkbox toggled
        /// </summary>
        public static readonly BindableProperty IsToggledProperty =
            BindableProperty.Create("IsToggled", typeof(bool), typeof(SelectionElementBase), false);

        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); }
        }

        /// <summary>
        /// Is button pressed
        /// </summary>
        public static readonly BindableProperty IsPressedProperty =
            BindableProperty.Create("IsPressed", typeof(bool), typeof(SelectionElementBase), false);

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            private set { SetValue(IsPressedProperty, value); }
        }

        /// <summary>
        /// Is button pressed
        /// </summary>
        public static readonly BindableProperty IsMouseOverProperty =
            BindableProperty.Create("IsMouseOver", typeof(bool), typeof(SelectionElementBase), false);

        public bool IsMouseOver
        {
            get { return (bool)GetValue(IsMouseOverProperty); }
            private set { SetValue(IsMouseOverProperty, value); }
        }

        public static readonly BindableProperty AnimationDurationProperty =
            BindableProperty.Create("AnimationDuration", typeof(int), typeof(SelectionElementBase), 400);

        public int AnimationDuration
        {
            get { return (int)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly BindableProperty AnimationEasingProperty =
            BindableProperty.Create("AnimationEasing", typeof(Easing), typeof(SelectionElementBase), Easing.CubicOut);

        public Easing AnimationEasing
        {
            get { return (Easing)GetValue(AnimationEasingProperty); }
            set { SetValue(AnimationEasingProperty, value); }
        }

        /// <summary>
        /// Executed when checkbox is toggled
        /// </summary>
        public static readonly BindableProperty ToggledCommandProperty =
            BindableProperty.Create("ToggledCommand", typeof(ICommand), typeof(SelectionElementBase), null);

        public ICommand ToggledCommand
        {
            get { return (ICommand)GetValue(ToggledCommandProperty); }
            set { SetValue(ToggledCommandProperty, value); }
        }

        #endregion

        #region Binding properties - Text

        /// <summary>
        /// Button text
        /// </summary>
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create("Text", typeof(string), typeof(SelectionElementBase), null);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Text font size
        /// </summary>
        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create("FontSize", typeof(double), typeof(SelectionElementBase), 15.0);

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        /// Text font family
        /// </summary>
        public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create("FontFamily", typeof(string), typeof(SelectionElementBase), Font.Default.ToString());

        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        /// <summary>
        /// Is text wrapping
        /// </summary>
        public static readonly BindableProperty IsTextWrappingProperty =
            BindableProperty.Create("IsTextWrapping", typeof(bool), typeof(SelectionElementBase), false);

        public bool IsTextWrapping
        {
            get { return (bool)GetValue(IsTextWrappingProperty); }
            set { SetValue(IsTextWrappingProperty, value); }
        }

        /// <summary>
        /// Text font style (Italic, Oblique etc..)
        /// </summary>
        public static readonly BindableProperty FontStyleProperty =
            BindableProperty.Create("FontStyle", typeof(FontStyles), typeof(SelectionElementBase), FontStyles.Upright);

        public FontStyles FontStyle
        {
            get { return (FontStyles)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        /// <summary>
        /// Text font weight (Bold, SemiBold etc...)
        /// </summary>
        public static readonly BindableProperty FontWeightProperty =
            BindableProperty.Create("FontWeight", typeof(FontWeights), typeof(SelectionElementBase), FontWeights.Normal);

        public FontWeights FontWeight
        {
            get { return (FontWeights)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        /// <summary>
        /// Is text upper
        /// </summary>
        public static readonly BindableProperty IsTextUpperProperty =
            BindableProperty.Create("IsTextUpper", typeof(bool), typeof(SelectionElementBase), false);

        public bool IsTextUpper
        {
            get { return (bool)GetValue(IsTextUpperProperty); }
            set { SetValue(IsTextUpperProperty, value); }
        }

        /// <summary>
        /// Text margin
        /// </summary>
        public static readonly BindableProperty TextMarginProperty =
            BindableProperty.Create("TextMargin", typeof(Thickness), typeof(SelectionElementBase), new Thickness());

        public Thickness TextMargin
        {
            get { return (Thickness)GetValue(TextMarginProperty); }
            set { SetValue(TextMarginProperty, value); }
        }

        #endregion

        #region Binding properties - Ellipse animation

        /// <summary>
        /// Is thumb ellise animation enabled
        /// </summary>
        public static readonly BindableProperty IsEllipseAnimationEnabledProperty =
            BindableProperty.Create("IsEllipseAnimationEnabled", typeof(bool), typeof(Switch), false);

        public bool IsEllipseAnimationEnabled
        {
            get { return (bool)GetValue(IsEllipseAnimationEnabledProperty); }
            set { SetValue(IsEllipseAnimationEnabledProperty, value); }
        }

        /// <summary>
        /// Thumb border thickness
        /// </summary>
        public static readonly BindableProperty EllipseDiameterProperty =
            BindableProperty.Create("EllipseDiameter", typeof(double), typeof(Switch), 0.0);

        public double EllipseDiameter
        {
            get { return (double)GetValue(EllipseDiameterProperty); }
            set { SetValue(EllipseDiameterProperty, value); }
        }

        /// <summary>
        /// Thumb border thickness
        /// </summary>
        public static readonly BindableProperty EllipseColorProperty =
            BindableProperty.Create("EllipseColor", typeof(Color), typeof(Switch), Color.Transparent);

        public Color EllipseColor
        {
            get { return (Color)GetValue(EllipseColorProperty); }
            set { SetValue(EllipseColorProperty, value); }
        }

        #endregion

        #region Binding properties - Color

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create("TextColor", typeof(Color), typeof(SelectionElementBase), Color.Transparent);

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly BindableProperty TextHoverColorProperty =
            BindableProperty.Create("TextHoverColor", typeof(Color), typeof(SelectionElementBase), Color.Transparent);

        public Color TextHoverColor
        {
            get { return (Color)GetValue(TextHoverColorProperty); }
            set { SetValue(TextHoverColorProperty, value); }
        }

        public static readonly BindableProperty TextPressedColorProperty =
            BindableProperty.Create("TextPressedColor", typeof(Color), typeof(SelectionElementBase), Color.Transparent);

        public Color TextPressedColor
        {
            get { return (Color)GetValue(TextPressedColorProperty); }
            set { SetValue(TextPressedColorProperty, value); }
        }

        public static readonly BindableProperty ToggledTextColorProperty =
            BindableProperty.Create("ToggledTextColor", typeof(Color), typeof(SelectionElementBase), Color.Transparent);

        public Color ToggledTextColor
        {
            get { return (Color)GetValue(ToggledTextColorProperty); }
            set { SetValue(ToggledTextColorProperty, value); }
        }

        public static readonly BindableProperty ToggledTextHoverColorProperty =
            BindableProperty.Create("ToggledTextHoverColor", typeof(Color), typeof(SelectionElementBase), Color.Transparent);

        public Color ToggledTextHoverColor
        {
            get { return (Color)GetValue(ToggledTextHoverColorProperty); }
            set { SetValue(ToggledTextHoverColorProperty, value); }
        }

        public static readonly BindableProperty ToggledTextPressedColorProperty =
            BindableProperty.Create("ToggledTextPressedColor", typeof(Color), typeof(SelectionElementBase), Color.Transparent);

        public Color ToggledTextPressedColor
        {
            get { return (Color)GetValue(ToggledTextPressedColorProperty); }
            set { SetValue(ToggledTextPressedColorProperty, value); }
        }

        public static readonly BindableProperty TextDisabledColorProperty =
            BindableProperty.Create("TextDisabledColor", typeof(Color), typeof(SelectionElementBase), Color.Transparent);

        public Color TextDisabledColor
        {
            get { return (Color)GetValue(TextDisabledColorProperty); }
            set { SetValue(TextDisabledColorProperty, value); }
        }

        #endregion

        public SelectionElementBase()
        {
            TouchEffect touch = new TouchEffect();
            touch.TouchAction += OnTouchAction;
            this.Effects.Add(touch);

            _skiaCanvas = new SKCanvasView();
            _skiaCanvas.PaintSurface += OnPaint;
            _skiaCanvas.InputTransparent = true;
            Children.Add(_skiaCanvas);
        }

        #region Measure / Layout

        /// <summary>
        /// Do measure with checkbox
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Size size = new Size();
            _textSize = new Size();

            double availableContentWidth = widthConstraint - _selectionViewSize.Width;
            double availableContentHeight = heightConstraint;

            SizeRequest contentSize = new SizeRequest();
            if (_actualContentElement != null)
            {
                contentSize = _actualContentElement.Measure(availableContentWidth, availableContentHeight, MeasureFlags.IncludeMargins);
                size = contentSize.Request;
            }
            // Measure text size
            else if (String.IsNullOrEmpty(Text) == false)
            {
                _textSize = MeasureTextSize(availableContentWidth, availableContentHeight);

                // Add text margin
                size.Width = _textSize.Width + TextMargin.HorizontalThickness;
                size.Height = _textSize.Height + TextMargin.VerticalThickness;
            }

            // If do not take all horizontal space
            if (HorizontalOptions.Alignment != LayoutAlignment.Fill)
            {
                // Width is based on track width and text size
                size.Width += _selectionViewSize.Width;
            }
            else
            {
                // Width is available width
                size.Width = widthConstraint;
            }

            // Height is text, track or thumb height
            size.Height = Math.Max(size.Height, _selectionViewSize.Height);

            return new SizeRequest(size, size);
        }

        /// <summary>
        /// Layout children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            LayoutChildIntoBoundingRegion(_skiaCanvas, new Rectangle(x - EllipseDiameter, y - EllipseDiameter, width + (EllipseDiameter * 2), height + (EllipseDiameter * 2)));

            if (_actualContentElement != null)
            {
                double checkBoxWidth = _selectionViewSize.Width;

                if (_selectionViewLocation == HorizontalLocations.Left)
                {
                    LayoutChildIntoBoundingRegion(_actualContentElement, new Rectangle(x + checkBoxWidth, y, width - checkBoxWidth, height));
                }
                else
                {
                    LayoutChildIntoBoundingRegion(_actualContentElement, new Rectangle(x, y, width - checkBoxWidth, height));
                }
            }
        }

        /// <summary>
        /// Measure total size
        /// </summary>
        protected Size MeasureTextSize(double widthConstraint, double heightConstraint)
        {
            SKPaint paint = new SKPaint();
            paint.TextSize = (float)FontSize;

            SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(FontStyle);
            SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(FontWeight);
            paint.Typeface = SKTypeface.FromFamilyName(FontFamily, fontWeight, SKFontStyleWidth.Normal, slant);

            float lineHeight = (float)FontSize;
            return SkiaUtils.MeasureText(paint, (float)widthConstraint, lineHeight, IsTextWrapping, Text);
        }

        #endregion

        #region Paint

        /// <summary>
        /// Paint checkbox and other content. Paint checkbox even custom content is used.
        /// </summary>
        /// <param name="e">Skiasharp paint surface</param>
        /// <param name="availableSpace">Available space for checbox and other content</param>
        private void OnPaint(object sender, SKPaintSurfaceEventArgs e)
        {
            // Get device pixel intencity scale
            DeviceScale = (float)(e.Info.Width / (Width + (EllipseDiameter * 2)));

            // clear the canvas / view
            e.Surface.Canvas.Clear();

            if (String.IsNullOrEmpty(Text) == false)
            {
                OnPaintText(e);
            }

            OnPaintSelectionElement(e);
        }

        /// <summary>
        /// Paint selection element
        /// </summary>
        protected abstract void OnPaintSelectionElement(SKPaintSurfaceEventArgs e);

        /// <summary>
        /// Paint text
        /// </summary>
        private void OnPaintText(SKPaintSurfaceEventArgs e)
        {
            SKPaint textPaint = new SKPaint();
            textPaint.Color = AnimationUtils.ColorTransform(_toggledAnimationProcess, TextColor, ToggledTextColor).ToSKColor();
            textPaint.TextSize = (float)FontSize * DeviceScale;
            textPaint.TextAlign = SKTextAlign.Left;
            textPaint.IsAntialias = true;

            SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(FontStyle);
            SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(FontWeight);
            textPaint.Typeface = SKTypeface.FromFamilyName(FontFamily, fontWeight, SKFontStyleWidth.Normal, slant);

            SKRect skTextBounds = new SKRect();
            textPaint.MeasureText(Text, ref skTextBounds);

            float skNegativePadding = (float)EllipseDiameter * DeviceScale;

            float xText = skNegativePadding - skTextBounds.Left + (float)(TextMargin.Left * DeviceScale);
            float yText = -skTextBounds.Top + (e.Info.Height - skTextBounds.Height) / 2;

            double checkBoxWidth = _selectionViewSize.Width;

            if (_selectionViewLocation == HorizontalLocations.Left)
            {
                xText += (float)(checkBoxWidth * DeviceScale);
            }

            float lineHeight = (float)FontSize * DeviceScale;
            float skAvailableWidth = (float)(Width - checkBoxWidth - TextMargin.HorizontalThickness) * DeviceScale;

            SkiaUtils.DrawTextArea(e.Surface.Canvas, textPaint, xText, yText, skAvailableWidth, lineHeight, IsTextWrapping, Text);
        }

        #endregion

        #region Interaction

        private void OnTouchAction(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Entered)
            {
                IsMouseOver = true;
            }
            else if (args.Type == TouchActionType.Exited)
            {
                IsPressed = false;
                IsMouseOver = false;
            }
            else if (args.Type == TouchActionType.Pressed)
            {
                IsPressed = true;
            }
            else if (args.Type == TouchActionType.Released && IsPressed)
            {
                IsPressed = false;

                IsToggled = !IsToggled;

                IsToggledChanged?.Invoke(this, IsToggled);

                ToggledCommand?.Execute(null);
            }
        }

        private void OnIsMouseOverChanged(bool isMouseOver)
        {
        }

        /// <summary>
        /// Animate toggle
        /// </summary>
        private void OnIsToggledChanged(bool toggled)
        {
            AnimationExtensions.AbortAnimation(this, _animationName);

            double start = _toggledAnimationProcessWithoutEasing;
            double end = 1;

            if (toggled == false)
            {
                start = _toggledAnimationProcessWithoutEasing;
                end = 0;
            }

            Animation anim = new Animation(d =>
            {
                if (toggled)
                {
                    _toggledAnimationProcess = AnimationEasing.Ease(d);
                }
                else
                {
                    _toggledAnimationProcess = 1 - AnimationEasing.Ease(1 - d);
                }

                _toggledAnimationProcessWithoutEasing = d;
                _skiaCanvas.InvalidateSurface();

            }, start, end);

            anim.Commit(this, _animationName, 64, (uint)AnimationDuration, Easing.Linear);
        }

        #endregion

        #region Content

        /// <summary>
        /// Add content to children collection
        /// </summary>
        private void OnContentChanged(object oldContent, object newContent)
        {
            if (newContent is View newContentView)
            {
                if (_actualContentElement != null && Children.Contains(_actualContentElement))
                {
                    Children.Remove(_actualContentElement);
                }

                _actualContentElement = newContentView;

                if (_actualContentElement != null && Children.Contains(_actualContentElement) == false)
                {
                    Children.Add(_actualContentElement);
                }
            }
            else if (oldContent is View oldContentView)
            {
                if (_actualContentElement != null && Children.Contains(_actualContentElement))
                {
                    Children.Remove(_actualContentElement);
                }

                _actualContentElement = null;
            }
            else if (_actualContentElement != null)
            {
                _actualContentElement.BindingContext = newContent;
            }
        }

        /// <summary>
        /// Create content with content template
        /// </summary>
        private void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
        {
            if (Content == null || Content is View == false)
            {
                if (Children.Contains(_actualContentElement))
                {
                    Children.Remove(_actualContentElement);
                }

                _actualContentElement = newContentTemplate.CreateContent() as View;
                _actualContentElement.BindingContext = Content;
                Children.Add(_actualContentElement);
            }
            else
            {
                // Do nothing with template
            }
        }

        #endregion

        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == IsMouseOverProperty.PropertyName)
            {
                OnIsMouseOverChanged(IsMouseOver);
            }
            else if (propertyName == IsToggledProperty.PropertyName)
            {
                OnIsToggledChanged(IsToggled);
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }
    }
}
