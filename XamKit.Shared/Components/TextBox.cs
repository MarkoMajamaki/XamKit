using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;

// SkiaSharp
using SkiaSharp;
using SkiaSharp.Views.Forms;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace XamKit
{
	public enum CaptionPlacements { Left, Top, Inside, OverBorder }

    public enum BorderTypes { Border, Line }

    /// <summary>
    /// Textbox which wraps icon, border, caption, placeholder and entry
    /// </summary>
	public class TextBox : Layout<View>
    {
        // Parts
        protected View _textElement = null;
        protected SKCanvasView _skiaCanvas = null;

        // Parts locations
        private Size _textElementSize = new Size();
        private Size _captionSize = new Size();
        private Size _leftIconSize = new Size();
        private Size _rightIconSize = new Size();
        private Size _infoTextSize = new Size();
        protected Size _placeholderSize = new Size();

        protected Rectangle _entryLocation = new Rectangle();

        // private Size m_clearButtonSize = new Size(); // TODO

        // Icon
        private SkiaSharp.Extended.Svg.SKSvg _leftIconSvg = null;
        private SkiaSharp.Extended.Svg.SKSvg _rightIconSvg = null;

        // InfoText line height font size multiplier
        private const float _infoTextLineHeightMultiplier = 1; // 1.2f;
        private const float _captionLineHeightMultiplier = 1; // 1.2f;
        private const float _placeholderLineHeightMultiplier = 1.3f;

        private bool _isMeasureDone = false;
        private bool _isPlaceholderPainted = false;

        // Paint
        private SKPaint _placehoderMeasurePaint = null;
        private SKPaint _captionMeasurePaint = null;
        private SKPaint _infoTextMeasurePaint = null;

        private SKPaint _captionPaint = null;
        private SKPaint _placehoderPaint = null;
        private SKPaint _infoTextPaint = null;
        private SKPaint _linePaint = null;
        private SKPaint _backgroundPaint = null;
        private SKPaint _borderPaint = null;

        /// <summary>
        /// Event when keyboard focus change
        /// </summary>
        public event EventHandler<bool> KeyboardFocusChanged;

        /// <summary>
        /// Event when text changes
        /// </summary>
        public event EventHandler<TextChangedEventArgs> TextChanged;

        // Visual states
        private const string _normal = "Normal";
        private const string _focused = "Focused";
        private const string _disabled = "Disabled";
        private const string _hover = "Hover";

        // Animation 0 -> 1
        protected double _focusedAnimationProcess = 0;
        protected double _captionAnimationProcess = 0;
        protected const string _animationName = "animationName";

        #region BindingProperties - Text

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create("Text", typeof(string), typeof(TextBox), null);

        public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create("FontSize", typeof(double), typeof(TextBox), 16.0);

        public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create("FontFamily", typeof(string), typeof(TextBox), null);

        public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		public static readonly BindableProperty TextMarginProperty =
            BindableProperty.Create("TextMargin", typeof(Thickness), typeof(TextBox), new Thickness(0, 0, 0, 0));

        public Thickness TextMargin
		{
			get { return (Thickness)GetValue(TextMarginProperty); }
			set { SetValue(TextMarginProperty, value); }
		}

        public static readonly BindableProperty TextPaddingProperty =
            BindableProperty.Create("TextPadding", typeof(Thickness), typeof(TextBox), new Thickness(0, 4, 0, 4));

        public Thickness TextPadding
        {
            get { return (Thickness)GetValue(TextPaddingProperty); }
            set { SetValue(TextPaddingProperty, value); }
        }

        public static readonly BindableProperty IsTextWrappingProperty =
            BindableProperty.Create("IsTextWrapping", typeof(bool), typeof(TextBox), false);

        public bool IsTextWrapping
        {
            get { return (bool)GetValue(IsTextWrappingProperty); }
            set { SetValue(IsTextWrappingProperty, value); }
        }

        #endregion

        #region BindingProperties - Placeholder

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create("Placeholder", typeof(string), typeof(TextBox), null);
            
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly BindableProperty PlaceholderFontFamilyProperty =
            BindableProperty.Create("PlaceholderFontFamily", typeof(string), typeof(TextBox), null);

        public string PlaceholderFontFamily
        {
            get { return (string)GetValue(PlaceholderFontFamilyProperty); }
            set { SetValue(PlaceholderFontFamilyProperty, value); }
        }

        public static readonly BindableProperty PlaceholderFontStyleProperty =
            BindableProperty.Create("PlaceholderFontStyle", typeof(FontStyles), typeof(TextBox), FontStyles.Upright);

        public FontStyles PlaceholderFontStyle
        {
            get { return (FontStyles)GetValue(PlaceholderFontStyleProperty); }
            set { SetValue(PlaceholderFontStyleProperty, value); }
        }

        public static readonly BindableProperty PlaceholderFontWeightProperty =
            BindableProperty.Create("PlaceholderFontWeight", typeof(FontWeights), typeof(TextBox), FontWeights.Normal);

        public FontWeights PlaceholderFontWeight
        {
            get { return (FontWeights)GetValue(PlaceholderFontWeightProperty); }
            set { SetValue(PlaceholderFontWeightProperty, value); }
        }

        #endregion

        #region BindingProperties - Caption

        public static readonly BindableProperty CaptionProperty =
			BindableProperty.Create("Caption", typeof(string), typeof(TextBox), null);

        public string Caption
		{
			get { return (string)GetValue(CaptionProperty); }
			set { SetValue(CaptionProperty, value); }
		}

        /// <summary>
        /// Caption font size if location left or top
        /// </summary>
		public static readonly BindableProperty CaptionFontSizeProperty =
            BindableProperty.Create("CaptionFontSize", typeof(double), typeof(TextBox), 16.0);

        public double CaptionFontSize
		{
			get { return (double)GetValue(CaptionFontSizeProperty); }
			set { SetValue(CaptionFontSizeProperty, value); }
		}

        public static readonly BindableProperty CaptionFontFamilyProperty =
            BindableProperty.Create("CaptionFontFamily", typeof(string), typeof(TextBox), null);

        public string CaptionFontFamily
		{
			get { return (string)GetValue(CaptionFontFamilyProperty); }
			set { SetValue(CaptionFontFamilyProperty, value); }
		}

		public static readonly BindableProperty CaptionPlacementProperty =
			BindableProperty.Create("CaptionPlacement", typeof(CaptionPlacements), typeof(TextBox), CaptionPlacements.Top);

        public CaptionPlacements CaptionPlacement
		{
			get { return (CaptionPlacements)GetValue(CaptionPlacementProperty); }
			set { SetValue(CaptionPlacementProperty, value); }
		}

        /// <summary>
        /// Caption margin if location left or top
        /// </summary>
		public static readonly BindableProperty CaptionMarginProperty =
			BindableProperty.Create("CaptionMargin", typeof(Thickness), typeof(TextBox), new Thickness(0));

        public Thickness CaptionMargin
		{
			get { return (Thickness)GetValue(CaptionMarginProperty); }
			set { SetValue(CaptionMarginProperty, value); }
		}

        /// <summary>
        /// Caption horizontal option if location left
        /// </summary>
		public static readonly BindableProperty CaptionHorizontalOptionsProperty =
			BindableProperty.Create("CaptionHorizontalOptions", typeof(LayoutOptions), typeof(TextBox), LayoutOptions.Start);

        public LayoutOptions CaptionHorizontalOptions
		{
			get { return (LayoutOptions)GetValue(CaptionHorizontalOptionsProperty); }
			set { SetValue(CaptionHorizontalOptionsProperty, value); }
		}

        /// <summary>
        /// Caption vertical option if location left
        /// </summary>
		public static readonly BindableProperty CaptionVerticalOptionsProperty =
            BindableProperty.Create("CaptionVerticalOptions", typeof(LayoutOptions), typeof(TextBox), LayoutOptions.Center);

        public LayoutOptions CaptionVerticalOptions
		{
			get { return (LayoutOptions)GetValue(CaptionVerticalOptionsProperty); }
			set { SetValue(CaptionVerticalOptionsProperty, value); }
		}

        /// <summary>
        /// Caption min width if location on left
        /// </summary>
        public static readonly BindableProperty CaptionMinWidthProperty =
            BindableProperty.Create("CaptionMinWidth", typeof(double), typeof(TextBox), 0.0);

        public double CaptionMinWidth
        {
            get { return (double)GetValue(CaptionMinWidthProperty); }
            set { SetValue(CaptionMinWidthProperty, value); }
        }

        /// <summary>
        /// Caption max width if location on left
        /// </summary>
        public static readonly BindableProperty CaptionMaxWidthProperty =
            BindableProperty.Create("CaptionMaxWidth", typeof(double), typeof(TextBox), double.MaxValue);

        public double CaptionMaxWidth
        {
            get { return (double)GetValue(CaptionMaxWidthProperty); }
            set { SetValue(CaptionMaxWidthProperty, value); }
        }

        /// <summary>
        /// Caption fixed width if location on left
        /// </summary>
        public static readonly BindableProperty CaptionWidthProperty =
            BindableProperty.Create("CaptionWidth", typeof(double?), typeof(TextBox), null);

        public double? CaptionWidth
        {
            get { return (double?)GetValue(CaptionWidthProperty); }
            set { SetValue(CaptionMaxWidthProperty, value); }
        }

        /// <summary>
        /// Is caption wrapping. Works only if location left or top
        /// </summary>
        public static readonly BindableProperty IsCaptionWrappingProperty =
            BindableProperty.Create("IsCaptionWrapping", typeof(bool), typeof(TextBox), false);

        public bool IsCaptionWrapping
        {
            get { return (bool)GetValue(IsCaptionWrappingProperty); }
            set { SetValue(IsCaptionWrappingProperty, value); }
        }

        public static readonly BindableProperty CaptionFontStyleProperty =
            BindableProperty.Create("CaptionFontStyle", typeof(FontStyles), typeof(TextBox), FontStyles.Upright);

        public FontStyles CaptionFontStyle
        {
            get { return (FontStyles)GetValue(CaptionFontStyleProperty); }
            set { SetValue(CaptionFontStyleProperty, value); }
        }

        public static readonly BindableProperty CaptionFontWeightProperty =
            BindableProperty.Create("CaptionFontWeight", typeof(FontWeights), typeof(TextBox), FontWeights.Normal);

        public FontWeights CaptionFontWeight
        {
            get { return (FontWeights)GetValue(CaptionFontWeightProperty); }
            set { SetValue(CaptionFontWeightProperty, value); }
        }

        #endregion

        #region BindingProperties - Left icon

        /// <summary>
        /// Left icon resource key
        /// </summary>
        public static readonly BindableProperty LeftIconResourceKeyProperty =
            BindableProperty.Create(nameof(LeftIconResourceKey), typeof(string), typeof(TextBox), null);

        public string LeftIconResourceKey
        {
            get { return (string)GetValue(LeftIconResourceKeyProperty); }
            set { SetValue(LeftIconResourceKeyProperty, value); }
        }

        /// <summary>
        /// The left icon assembly name containing the svg file
        /// </summary>
        public static readonly BindableProperty LeftIconAssemblyNameProperty =
            BindableProperty.Create(nameof(LeftIconAssemblyName), typeof(string), typeof(TextBox), null);

        public string LeftIconAssemblyName
        {
            get { return (string)GetValue(LeftIconAssemblyNameProperty); }
            set { SetValue(LeftIconAssemblyNameProperty, value); }
        }

        /// <summary>
        /// Left icon width
        /// </summary>
		public static readonly BindableProperty LeftIconWidthRequestProperty =
			BindableProperty.Create("LeftIconWidthRequest", typeof(double), typeof(TextBox), -1.0);
            
        public double LeftIconWidthRequest
        {
			get { return (double)GetValue(LeftIconWidthRequestProperty); }
			set { SetValue(LeftIconWidthRequestProperty, value); }
		}

        /// <summary>
        /// Left icon height
        /// </summary>
        public static readonly BindableProperty LeftIconHeightRequestProperty =
			BindableProperty.Create("LeftIconHeightRequest", typeof(double), typeof(TextBox), -1.0);
            
		public double LeftIconHeightRequest
        {
			get { return (double)GetValue(LeftIconHeightRequestProperty); }
			set { SetValue(LeftIconHeightRequestProperty, value); }
		}

        /// <summary>
        /// Left icon margin
        /// </summary>
        public static readonly BindableProperty LeftIconMarginProperty =
			BindableProperty.Create("LeftIconMargin", typeof(Thickness), typeof(TextBox), new Thickness(0));

        public Thickness LeftIconMargin
        {
			get { return (Thickness)GetValue(LeftIconMarginProperty); }
			set { SetValue(LeftIconMarginProperty, value); }
		}

        #endregion

        #region BindingProperties - Right icon

        /// <summary>
        /// Right icon resource key
        /// </summary>
        public static readonly BindableProperty RightIconResourceKeyProperty =
            BindableProperty.Create(nameof(RightIconResourceKey), typeof(string), typeof(TextBox), null);

        public string RightIconResourceKey
        {
            get { return (string)GetValue(RightIconResourceKeyProperty); }
            set { SetValue(RightIconResourceKeyProperty, value); }
        }

        /// <summary>
        /// The right icon assembly name containing the svg file
        /// </summary>
        public static readonly BindableProperty RightIconAssemblyNameProperty =
            BindableProperty.Create(nameof(RightIconAssemblyName), typeof(string), typeof(TextBox), null);

        public string RightIconAssemblyName
        {
            get { return (string)GetValue(RightIconAssemblyNameProperty); }
            set { SetValue(RightIconAssemblyNameProperty, value); }
        }

        /// <summary>
        /// Right icon width
        /// </summary>
		public static readonly BindableProperty RightIconWidthRequestProperty =
            BindableProperty.Create("RightIconWidthRequest", typeof(double), typeof(TextBox), -1.0);

        public double RightIconWidthRequest
        {
            get { return (double)GetValue(RightIconWidthRequestProperty); }
            set { SetValue(RightIconWidthRequestProperty, value); }
        }

        /// <summary>
        /// Right icon height
        /// </summary>
        public static readonly BindableProperty RightIconHeightRequestProperty =
            BindableProperty.Create("RightIconHeightRequest", typeof(double), typeof(TextBox), -1.0);

        public double RightIconHeightRequest
        {
            get { return (double)GetValue(RightIconHeightRequestProperty); }
            set { SetValue(RightIconHeightRequestProperty, value); }
        }

        /// <summary>
        /// Right icon margin
        /// </summary>
        public static readonly BindableProperty RightIconMarginProperty =
            BindableProperty.Create("RightIconMargin", typeof(Thickness), typeof(TextBox), new Thickness(0));

        public Thickness RightIconMargin
        {
            get { return (Thickness)GetValue(RightIconMarginProperty); }
            set { SetValue(RightIconMarginProperty, value); }
        }

        #endregion

        #region BindingProperties - InfoText

        /// <summary>
        /// Text below border
        /// </summary>
        public static readonly BindableProperty InfoTextProperty =
            BindableProperty.Create("InfoText", typeof(string), typeof(TextBox), null);

        public string InfoText
        {
            get { return (string)GetValue(InfoTextProperty); }
            set { SetValue(InfoTextProperty, value); }
        }


        public static readonly BindableProperty InfoTextFontSizeProperty =
            BindableProperty.Create("InfoTextFontSize", typeof(double), typeof(TextBox), 12.0);

        public double InfoTextFontSize
        {
            get { return (double)GetValue(InfoTextFontSizeProperty); }
            set { SetValue(InfoTextFontSizeProperty, value); }
        }

        public static readonly BindableProperty InfoTextFontFamilyProperty =
            BindableProperty.Create("InfoTextFontFamily", typeof(string), typeof(TextBox), null);
            
        public string InfoTextFontFamily
        {
            get { return (string)GetValue(InfoTextFontFamilyProperty); }
            set { SetValue(InfoTextFontFamilyProperty, value); }
        }

        public static readonly BindableProperty InfoTextMarginProperty =
            BindableProperty.Create("InfoTextMargin", typeof(Thickness), typeof(TextBox), new Thickness(0));

        public Thickness InfoTextMargin
        {
            get { return (Thickness)GetValue(InfoTextMarginProperty); }
            set { SetValue(InfoTextMarginProperty, value); }
        }

        public static readonly BindableProperty InfoTextFontStyleProperty =
            BindableProperty.Create("InfoTextFontStyle", typeof(FontStyles), typeof(TextBox), FontStyles.Upright);

        public FontStyles InfoTextFontStyle
        {
            get { return (FontStyles)GetValue(InfoTextFontStyleProperty); }
            set { SetValue(InfoTextFontStyleProperty, value); }
        }
        
        public static readonly BindableProperty InfoTextFontWeightProperty =
            BindableProperty.Create("InfoTextFontWeight", typeof(FontWeights), typeof(TextBox), FontWeights.Normal);

        public FontWeights InfoTextFontWeight
        {
            get { return (FontWeights)GetValue(InfoTextFontWeightProperty); }
            set { SetValue(InfoTextFontWeightProperty, value); }
        }

        #endregion

        #region BindingProperties - Border

        /// <summary>
        /// Border thickness
        /// </summary>
		public static readonly BindableProperty BorderThicknessProperty =
			BindableProperty.Create("BorderThickness", typeof(double), typeof(TextBox), 1.0);

        public double BorderThickness
		{
			get { return (double)GetValue(BorderThicknessProperty); }
			set { SetValue(BorderThicknessProperty, value); }
		}

        /// <summary>
        /// Border focused thickness
        /// </summary>
        public static readonly BindableProperty BorderFocusedThicknessProperty =
            BindableProperty.Create("BorderFocusedThickness", typeof(double?), typeof(TextBox), null);

        public double? BorderFocusedThickness
        {
            get { return (double?)GetValue(BorderFocusedThicknessProperty); }
            set { SetValue(BorderFocusedThicknessProperty, value); }
        }

        /// <summary>
        /// Corner radius (works only if BorderType is Border)
        /// </summary>
		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create("CornerRadius", typeof(double), typeof(TextBox), 2.0);

        public double CornerRadius
		{
			get { return (double)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

        #endregion

        #region BindingProperties - Line

        /// <summary>
        /// Is bottom line visible
        /// </summary>
        public static readonly BindableProperty IsLineVisibleProperty =
            BindableProperty.Create("IsLineVisible", typeof(bool), typeof(TextBox), false);

        public bool IsLineVisible
        {
            get { return (bool)GetValue(IsLineVisibleProperty); }
            set { SetValue(IsLineVisibleProperty, value); }
        }

        /// <summary>
        /// Bottom line thickness
        /// </summary>
        public static readonly BindableProperty LineThicknessProperty =
            BindableProperty.Create("LineThickness", typeof(double), typeof(TextBox), 2.0);

        public double LineThickness
        {
            get { return (double)GetValue(LineThicknessProperty); }
            set { SetValue(LineThicknessProperty, value); }
        }

        /// <summary>
        /// Bottom line thickness when focused
        /// </summary>
        public static readonly BindableProperty LineFocusedThicknessProperty =
            BindableProperty.Create("LineFocusedThickness", typeof(double?), typeof(TextBox), null);

        public double? LineFocusedThickness
        {
            get { return (double?)GetValue(LineFocusedThicknessProperty); }
            set { SetValue(LineFocusedThicknessProperty, value); }
        }

        public static readonly BindableProperty IsLineFocusAnimationEnabledProperty =
            BindableProperty.Create("IsLineFocusAnimationEnabled", typeof(bool), typeof(TextBox), true);

        public bool IsLineFocusAnimationEnabled
        {
            get { return (bool)GetValue(IsLineFocusAnimationEnabledProperty); }
            set { SetValue(IsLineFocusAnimationEnabledProperty, value); }
        }

        #endregion

        #region BindingProperties - Interaction

		public static readonly BindableProperty FocusedCommandProperty =
			BindableProperty.Create("FocusedCommand", typeof(ICommand), typeof(TextBox), null);

		public ICommand FocusedCommand
		{
			get { return (ICommand)GetValue(FocusedCommandProperty); }
			set { SetValue(FocusedCommandProperty, value); }
		}

		public static readonly BindableProperty UnFocusedCommandProperty =
			BindableProperty.Create("UnFocusedCommand", typeof(ICommand), typeof(TextBox), null);

		public ICommand UnFocusedCommand
		{
			get { return (ICommand)GetValue(UnFocusedCommandProperty); }
			set { SetValue(UnFocusedCommandProperty, value); }
		}

		public static readonly BindableProperty FocusCommandParameterProperty =
			BindableProperty.Create("FocusCommandParameter", typeof(object), typeof(TextBox), null);

		public object FocusCommandParameter
		{
			get { return (object)GetValue(FocusCommandParameterProperty); }
			set { SetValue(FocusCommandParameterProperty, value); }
		}

        #endregion

        #region BindingProperties - Animation

        public static readonly BindableProperty AnimationDurationProperty =
            BindableProperty.Create("AnimationDuration", typeof(int), typeof(TextBox), 200);

        public int AnimationDuration
        {
            get { return (int)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly BindableProperty AnimationEasingProperty =
            BindableProperty.Create("AnimationEasing", typeof(Easing), typeof(TextBox), Easing.CubicOut);

        public Easing AnimationEasing
        {
            get { return (Easing)GetValue(AnimationEasingProperty); }
            set { SetValue(AnimationEasingProperty, value); }
        }

        #endregion

        #region BindingProperties - ClearButton

        /// <summary>
        /// Enable clear button to replace right icon to clear text
        /// </summary>
        public static readonly BindableProperty IsClearButtonEnabledProperty =
            BindableProperty.Create("IsClearButtonEnabled", typeof(bool), typeof(TextBox), false);

        public bool IsClearButtonEnabled
        {
            get { return (bool)GetValue(IsClearButtonEnabledProperty); }
            set { SetValue(IsClearButtonEnabledProperty, value); }
        }

        /// <summary>
        /// Clear button template. Root element must implement ITappable!
        /// </summary>
        public static readonly BindableProperty ClearButtonTemplateProperty =
            BindableProperty.Create("ClearButtonTemplate", typeof(DataTemplate), typeof(TextBox), null);

        public DataTemplate ClearButtonTemplate
        {
            get { return (DataTemplate)GetValue(ClearButtonTemplateProperty); }
            set { SetValue(ClearButtonTemplateProperty, value); }
        }

        #endregion

        #region BindingProperties - RightContent

        /// <summary>
        /// Right content binding context or actual UI element.
        /// </summary>
        public static readonly BindableProperty RightContentProperty =
            BindableProperty.Create("RightContent", typeof(object), typeof(TextBox), null);

        public object RightContent
        {
            get { return (DataTemplate)GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }

        /// <summary>
        /// Template for any content on left side of right icon and clear button.
        /// </summary>
        public static readonly BindableProperty RightContentTemplateProperty =
            BindableProperty.Create("RightContentTemplate", typeof(DataTemplate), typeof(TextBox), null);

        public DataTemplate RightContentTemplate
        {
            get { return (DataTemplate)GetValue(RightContentTemplateProperty); }
            set { SetValue(RightContentTemplateProperty, value); }
        }

        #endregion

		#region BindingProperties - Colors

        // Text

		public static readonly BindableProperty TextColorProperty =
			BindableProperty.Create("TextColor", typeof(Color), typeof(TextBox), Color.Black);

		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

        public static readonly BindableProperty TextFocusedColorProperty =
            BindableProperty.Create("TextFocusedColor", typeof(Color), typeof(TextBox), Color.Black);

        public Color TextFocusedColor
        {
            get { return (Color)GetValue(TextFocusedColorProperty); }
            set { SetValue(TextFocusedColorProperty, value); }
        }

        public static readonly BindableProperty TextDisabledColorProperty =
            BindableProperty.Create("TextDisabledColor", typeof(Color), typeof(TextBox), Color.Gray);

        public Color TextDisabledColor
        {
            get { return (Color)GetValue(TextDisabledColorProperty); }
            set { SetValue(TextDisabledColorProperty, value); }
        }

        // Placeholder

        public static readonly BindableProperty PlaceholderColorProperty =
			BindableProperty.Create("PlaceholderColor", typeof(Color), typeof(TextBox), Color.Black);

		public Color PlaceholderColor
		{
			get { return (Color)GetValue(PlaceholderColorProperty); }
			set { SetValue(PlaceholderColorProperty, value); }
		}

        public static readonly BindableProperty PlaceholderFocusedColorProperty =
            BindableProperty.Create("PlaceholderFocusedColor", typeof(Color), typeof(TextBox), Color.Black);

        public Color PlaceholderFocusedColor
        {
            get { return (Color)GetValue(PlaceholderFocusedColorProperty); }
            set { SetValue(PlaceholderFocusedColorProperty, value); }
        }

        public static readonly BindableProperty PlaceholderDisabledColorProperty =
            BindableProperty.Create("PlaceholderDisabledColor", typeof(Color), typeof(TextBox), Color.Gray);

        public Color PlaceholderDisabledColor
        {
            get { return (Color)GetValue(PlaceholderDisabledColorProperty); }
            set { SetValue(PlaceholderDisabledColorProperty, value); }
        }

        // Caption

        public static readonly BindableProperty CaptionColorProperty =
			BindableProperty.Create("CaptionColor", typeof(Color), typeof(TextBox), Color.Black);

		public Color CaptionColor
		{
			get { return (Color)GetValue(CaptionColorProperty); }
			set { SetValue(CaptionColorProperty, value); }
		}

        public static readonly BindableProperty CaptionFocusedColorProperty =
            BindableProperty.Create("CaptionFocusedColor", typeof(Color), typeof(TextBox), Color.Black);

        public Color CaptionFocusedColor
        {
            get { return (Color)GetValue(CaptionFocusedColorProperty); }
            set { SetValue(CaptionFocusedColorProperty, value); }
        }

        public static readonly BindableProperty CaptionDisabledColorProperty =
            BindableProperty.Create("CaptionDisabledColor", typeof(Color), typeof(TextBox), Color.Black);

        public Color CaptionDisabledColor
        {
            get { return (Color)GetValue(CaptionDisabledColorProperty); }
            set { SetValue(CaptionDisabledColorProperty, value); }
        }

        // Border

        public static readonly BindableProperty BorderColorProperty =
			BindableProperty.Create("BorderColor", typeof(Color), typeof(TextBox), Color.Black);

		public Color BorderColor
		{
			get { return (Color)GetValue(BorderColorProperty); }
			set { SetValue(BorderColorProperty, value); }
		}

        public static readonly BindableProperty BorderFocusedColorProperty =
            BindableProperty.Create("BorderFocusedColor", typeof(Color), typeof(TextBox), Color.Black);

        public Color BorderFocusedColor
        {
            get { return (Color)GetValue(BorderFocusedColorProperty); }
            set { SetValue(BorderFocusedColorProperty, value); }
        }

        public static readonly BindableProperty BorderDisabledColorProperty =
            BindableProperty.Create("BorderDisabledColor", typeof(Color), typeof(TextBox), Color.Gray);

        public Color BorderDisabledColor
        {
            get { return (Color)GetValue(BorderDisabledColorProperty); }
            set { SetValue(BorderDisabledColorProperty, value); }
        }

        // Background

        public static readonly BindableProperty BorderBackgroundColorProperty =
			BindableProperty.Create("BorderBackgroundColor", typeof(Color), typeof(TextBox), Color.White);

		public Color BorderBackgroundColor
		{
			get { return (Color)GetValue(BorderBackgroundColorProperty); }
			set { SetValue(BorderBackgroundColorProperty, value); }
		}

        public static readonly BindableProperty BorderBackgroundFocusedColorProperty =
            BindableProperty.Create("BorderBackgroundFocusedColor", typeof(Color), typeof(TextBox), Color.White);

        public Color BorderBackgroundFocusedColor
        {
            get { return (Color)GetValue(BorderBackgroundFocusedColorProperty); }
            set { SetValue(BorderBackgroundFocusedColorProperty, value); }
        }

        public static readonly BindableProperty BorderBackgroundDisabledColorProperty =
           BindableProperty.Create("BorderBackgroundDisabledColor", typeof(Color), typeof(TextBox), Color.LightGray);

        public Color BorderBackgroundDisabledColor
        {
            get { return (Color)GetValue(BorderBackgroundDisabledColorProperty); }
            set { SetValue(BorderBackgroundDisabledColorProperty, value); }
        }

        // Left icon

        public static readonly BindableProperty LeftIconColorProperty =
            BindableProperty.Create("LeftIconColor", typeof(Color), typeof(TextBox), Color.Black);

		public Color LeftIconColor
        {
			get { return (Color)GetValue(LeftIconColorProperty); }
			set { SetValue(LeftIconColorProperty, value); }
		}

        public static readonly BindableProperty LeftIconFocusedColorProperty =
            BindableProperty.Create("LeftIconFocusedColor", typeof(Color), typeof(TextBox), Color.Black);

        public Color LeftIconFocusedColor
        {
            get { return (Color)GetValue(LeftIconFocusedColorProperty); }
            set { SetValue(LeftIconFocusedColorProperty, value); }
        }

        public static readonly BindableProperty LeftIconDisabledColorProperty =
            BindableProperty.Create("LeftIconDisabledColor", typeof(Color), typeof(TextBox), Color.Gray);

        public Color LeftIconDisabledColor
        {
            get { return (Color)GetValue(LeftIconDisabledColorProperty); }
            set { SetValue(LeftIconDisabledColorProperty, value); }
        }

        // Right icon

        public static readonly BindableProperty RightIconColorProperty =
            BindableProperty.Create("RightIconColor", typeof(Color), typeof(TextBox), Color.Black);

        public Color RightIconColor
        {
            get { return (Color)GetValue(RightIconColorProperty); }
            set { SetValue(RightIconColorProperty, value); }
        }

        public static readonly BindableProperty RightIconFocusedColorProperty =
            BindableProperty.Create("RightIconFocusedColor", typeof(Color), typeof(TextBox), Color.Black);

        public Color RightIconFocusedColor
        {
            get { return (Color)GetValue(RightIconFocusedColorProperty); }
            set { SetValue(RightIconFocusedColorProperty, value); }
        }

        public static readonly BindableProperty RightIconDisabledColorProperty =
            BindableProperty.Create("RightIconDisabledColor", typeof(Color), typeof(TextBox), Color.Gray);

        public Color RightIconDisabledColor
        {
            get { return (Color)GetValue(RightIconDisabledColorProperty); }
            set { SetValue(RightIconDisabledColorProperty, value); }
        }

        // Text

        public static readonly BindableProperty InfoTextColorProperty =
            BindableProperty.Create("InfoTextColor", typeof(Color), typeof(TextBox), Color.Black);

        public Color InfoTextColor
        {
            get { return (Color)GetValue(InfoTextColorProperty); }
            set { SetValue(InfoTextColorProperty, value); }
        }

        // Line

        public static readonly BindableProperty LineColorProperty =
            BindableProperty.Create("LineColor", typeof(Color), typeof(TextBox), Color.Default);

        public Color LineColor
        {
            get { return (Color)GetValue(LineColorProperty); }
            set { SetValue(LineColorProperty, value); }
        }

        public static readonly BindableProperty LineFocusedColorProperty =
            BindableProperty.Create("LineFocusedColor", typeof(Color), typeof(TextBox), Color.Default);

        public Color LineFocusedColor
        {
            get { return (Color)GetValue(LineFocusedColorProperty); }
            set { SetValue(LineFocusedColorProperty, value); }
        }

        public static readonly BindableProperty LineDisabledColorProperty =
            BindableProperty.Create("LineDisabledColor", typeof(Color), typeof(TextBox), Color.Default);

        public Color LineDisabledColor
        {
            get { return (Color)GetValue(LineDisabledColorProperty); }
            set { SetValue(LineDisabledColorProperty, value); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Make children proteted
        /// </summary>
        protected new IList<View> Children
		{
			get
			{
				return base.Children;
			}
		}

        protected bool ActualIsCaptionVisible
        {
            get
            {
                return string.IsNullOrEmpty(Caption) == false;
            }
        }

        protected bool ActualIsLeftIconVisible
        {
            get
            {
                return string.IsNullOrEmpty(LeftIconResourceKey) == false;
            }
        }

        protected bool ActualIsRightIconVisible
        {
            get
            {
                return string.IsNullOrEmpty(RightIconResourceKey) == false;
            }
        }

        protected bool ActualIsInfoTextVisible
        {
            get
            {
                return string.IsNullOrEmpty(InfoText) == false;
            }
        }

        protected bool ActualIsPlaceholderVisible
        {
            get
            {
                if (CaptionPlacement == CaptionPlacements.Inside ||CaptionPlacement == CaptionPlacements.OverBorder)
                {
                    return (string.IsNullOrEmpty(Placeholder) == false || string.IsNullOrEmpty(Caption) == false) && string.IsNullOrEmpty(Text) == true;
                }
                else
                {
                    return string.IsNullOrEmpty(Placeholder) == false && string.IsNullOrEmpty(Text) == true;
                }
            }
        }

        protected float DeviceScale { get; private set; } = 1;

        private double OverBorderCaptionLeftMargin
        {
            get
            {
                double margin = 0;
                if (ActualIsLeftIconVisible)
                {
                    margin = LeftIconMargin.Left + BorderThickness;
                }
                else
                {
                    margin = TextPadding.Left + BorderThickness;
                }

                if (BorderThickness > 0)
                {
                    margin = Math.Max(margin, Math.Max(8, CornerRadius));
                    return margin + OverBorderCaptionSpacing;
                }
                else
                {
                    return margin;
                }
            }
        }

        private double OverBorderCaptionSpacing
        {
            get
            {
                if (BorderThickness > 0)
                {
                    return 4;
                }
                else
                {
                    return 0;
                }
            }
        }

        protected virtual bool ActualIsFocused
        {
            get
            {
                if (_textElement != null)
                {
                    return _textElement.IsFocused;
                }
                else
                {
                    return false;
                }
            }
        }

        protected virtual bool ActualHasValue
        {
            get
            {
                if (_textElement != null && Children.Contains(_textElement) && _textElement.IsVisible)
                {
                    if (_textElement is Editor editor)
                    {
                        return editor.Text != null && editor.Text.Length > 0;
                    }
                    else if (_textElement is Entry entry)
                    {
                        return entry.Text != null && entry.Text.Length > 0;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        protected Thickness ActualTextPadding
        {
            get
            {
                if (CaptionPlacement == CaptionPlacements.Inside)
                {
                    double actualCaptionFontSize = 0;
                    if (string.IsNullOrEmpty(Caption) == false)
                    {
                        actualCaptionFontSize = CaptionFontSize;
                    }

                    if (TextPadding.Bottom < actualCaptionFontSize / 2)
                    {
                        double bottomPadding = TextPadding.Bottom - (actualCaptionFontSize / 2);

                        return new Thickness(TextPadding.Left, TextPadding.Top + (actualCaptionFontSize / 2) + Math.Abs(bottomPadding), TextPadding.Right, Math.Max(0, bottomPadding));
                    }
                    else
                    {
                        return new Thickness(TextPadding.Left, TextPadding.Top + (actualCaptionFontSize / 2), TextPadding.Right, TextPadding.Bottom - (actualCaptionFontSize / 2));
                    }
                }
                else
                {
                    return TextPadding;
                }
            }
        }

        public double ActualLineThickness
        {
            get
            {
                if (IsLineVisible)
                {
                    return LineThickness;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion

        public TextBox()
		{
            _skiaCanvas = CreateSkiaCanvas();
            Children.Add(_skiaCanvas);

            _textElement = CreateTextElement();
			Children.Add(_textElement);

			OnTextChanged(null, Text);
        }
        
        public new void Focus()
        {
            if (_textElement is Entry entry)
            {
                entry.Focus();
            }
        }

        #region Measure / Layout

        /// <summary>
        /// Measure layout size when parent is requesting size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
            _isMeasureDone = true;

            return MeasureChildren(widthConstraint, heightConstraint);
        }

		/// <summary>
		/// Layout children to panel
		/// </summary>
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
            if (_isMeasureDone == false)
            {
                MeasureChildren(width, height);
            }

            _isMeasureDone = false;

            Size actualLeftIconSize = new Size();
            if (ActualIsLeftIconVisible)
            {
                actualLeftIconSize = new Size(_leftIconSize.Width + LeftIconMargin.HorizontalThickness, _leftIconSize.Height + LeftIconMargin.VerticalThickness);
            }

            Size actualRightIconSize = new Size();
            if (ActualIsRightIconVisible)
            {
                actualRightIconSize = new Size(_rightIconSize.Width + RightIconMargin.HorizontalThickness, _rightIconSize.Height + RightIconMargin.VerticalThickness);
            }

            Size actualCaptionSize = new Size();
            if (ActualIsCaptionVisible)
            {
                actualCaptionSize = new Size(_captionSize.Width + CaptionMargin.HorizontalThickness, _captionSize.Height + CaptionMargin.VerticalThickness);
            }

            Size actualPlaceholderSize = new Size();
            if (string.IsNullOrEmpty(Placeholder) == false || string.IsNullOrEmpty(Caption) == false)
            {
                _placeholderSize = MeasurePlaceholder(width, height, actualCaptionSize, actualLeftIconSize, actualRightIconSize);
                actualPlaceholderSize = new Size(_placeholderSize.Width + TextPadding.HorizontalThickness, _placeholderSize.Height + TextPadding.VerticalThickness);
            }

            double textPartsHeight = Math.Max(_textElementSize.Height, Math.Max(actualLeftIconSize.Height, actualRightIconSize.Height));
            textPartsHeight = Math.Max(textPartsHeight, actualPlaceholderSize.Height);

            if (CaptionPlacement == CaptionPlacements.Top)
			{
                _entryLocation = new Rectangle(
                    TextMargin.Left + BorderThickness + actualLeftIconSize.Width,
                    actualCaptionSize.Height + TextMargin.Top + BorderThickness,
                    width - TextMargin.HorizontalThickness - actualLeftIconSize.Width - actualRightIconSize.Width - BorderThickness * 2,
                    textPartsHeight);
			}
            else if (CaptionPlacement == CaptionPlacements.Inside)
            {
                _entryLocation = new Rectangle(
                    TextMargin.Left + BorderThickness + actualLeftIconSize.Width,
                    BorderThickness,
                    width - TextMargin.HorizontalThickness - actualLeftIconSize.Width - actualRightIconSize.Width - (BorderThickness * 2),
                    Math.Max(_textElementSize.Height, actualPlaceholderSize.Height));
            }
            else if (CaptionPlacement == CaptionPlacements.OverBorder)
            {
                double actualCaptionHeight = 0;
                if (string.IsNullOrEmpty(Caption) == false)
                {
                    actualCaptionHeight = CaptionFontSize / 2;
                }

                _entryLocation = new Rectangle(
                    TextMargin.Left + BorderThickness + actualLeftIconSize.Width,
                    Math.Max(BorderThickness, actualCaptionHeight) + TextMargin.Top,
                    width - TextMargin.HorizontalThickness - actualLeftIconSize.Width - actualRightIconSize.Width - BorderThickness * 2,
                    textPartsHeight);
            }
			else // Left
			{
                _entryLocation = new Rectangle(
                    actualCaptionSize.Width + TextMargin.Left + BorderThickness + actualLeftIconSize.Width,
                    TextMargin.Top + BorderThickness,
                    width - actualCaptionSize.Width - actualLeftIconSize.Width - actualRightIconSize.Width - TextMargin.HorizontalThickness - BorderThickness * 2,
                    textPartsHeight);
            }

            if (_textElement.Bounds != _entryLocation)
            {
                LayoutChildIntoBoundingRegion(_textElement, _entryLocation);
            }

            LayoutChildIntoBoundingRegion(_skiaCanvas, new Rectangle(0, 0, width, height));
        }

        /// <summary>
        /// Measure subviews and update location caches
        /// </summary>
        private SizeRequest MeasureChildren(double width, double height)
        {
            Size size = new Size();

            Size actualLeftIconSize = new Size();
            if (ActualIsLeftIconVisible)
            {
                _leftIconSize = MeasureLeftIcon(width, height);
                actualLeftIconSize = new Size(_leftIconSize.Width + LeftIconMargin.HorizontalThickness, _leftIconSize.Height + LeftIconMargin.VerticalThickness);
            }
            else
            {
                _leftIconSize = new Size();
            }

            Size actualRightIconSize = new Size();
            if (ActualIsRightIconVisible)
            {
                _rightIconSize = MeasureRightIcon(width, height);
                actualRightIconSize = new Size(_rightIconSize.Width + RightIconMargin.HorizontalThickness, _rightIconSize.Height + RightIconMargin.VerticalThickness);
            }
            else
            {
                _rightIconSize = new Size();
            }

            Size actualCaptionSize = new Size();
            if (ActualIsCaptionVisible)
            {
                _captionSize = MeasureCaption(width, height);
                actualCaptionSize = new Size(_captionSize.Width + CaptionMargin.HorizontalThickness, _captionSize.Height + CaptionMargin.VerticalThickness);
            }
            else
            {
                _captionSize = new Size();
            }

            Size actualPlaceholderSize = new Size();
            if (string.IsNullOrEmpty(Placeholder) == false || string.IsNullOrEmpty(Caption) == false)
            {
                _placeholderSize = MeasurePlaceholder(width, height, actualCaptionSize, actualLeftIconSize, actualRightIconSize);
                actualPlaceholderSize = new Size(_placeholderSize.Width + TextPadding.HorizontalThickness, _placeholderSize.Height + TextPadding.VerticalThickness);
            }

            _textElementSize = MeasureEntry(width, height, actualLeftIconSize, actualRightIconSize, actualCaptionSize);

            Size actualInfoTextSize = new Size();
            if (ActualIsInfoTextVisible)
            {
                _infoTextSize = MeasureInfoText(width, height, _captionSize);
                actualInfoTextSize = new Size(_infoTextSize.Width + InfoTextMargin.HorizontalThickness, _infoTextSize.Height + InfoTextMargin.VerticalThickness);
            }
            else
            {
                _infoTextSize = new Size();
            }

            if (CaptionPlacement == CaptionPlacements.Top)
            {
                double textPartsHeight = Math.Max(_textElementSize.Height, Math.Max(actualLeftIconSize.Height, actualRightIconSize.Height));
                textPartsHeight = Math.Max(textPartsHeight, actualPlaceholderSize.Height);

                size.Height = 
                    actualCaptionSize.Height +
                    textPartsHeight +
                    TextMargin.VerticalThickness +
                    actualInfoTextSize.Height +
                    BorderThickness * 2;

                size.Width = 
                    Math.Max(_textElementSize.Width, actualPlaceholderSize.Width) + TextMargin.HorizontalThickness +
                    actualLeftIconSize.Width +
                    actualRightIconSize.Width +
                    BorderThickness * 2;

                size.Width = Math.Max(size.Width, actualCaptionSize.Width);
                size.Width = Math.Max(size.Width, actualInfoTextSize.Width);
            }
            else if (CaptionPlacement == CaptionPlacements.Inside)
            {
                double textPartsHeight = Math.Max(_textElementSize.Height, Math.Max(actualLeftIconSize.Height, actualRightIconSize.Height));
                textPartsHeight = Math.Max(textPartsHeight, actualPlaceholderSize.Height);

                size.Height =
                    textPartsHeight +
                    actualInfoTextSize.Height +
                    BorderThickness * 2 +
                    ActualLineThickness;

                size.Width =
                    Math.Max(_textElementSize.Width, actualPlaceholderSize.Width) + TextMargin.HorizontalThickness +
                    actualLeftIconSize.Width +
                    actualRightIconSize.Width +
                    BorderThickness * 2;

                size.Width = Math.Max(size.Width, actualCaptionSize.Width + TextMargin.HorizontalThickness + TextPadding.HorizontalThickness + actualLeftIconSize.Width);
                size.Width = Math.Max(size.Width, actualInfoTextSize.Width);
            }
            else if (CaptionPlacement == CaptionPlacements.OverBorder)
            {
                double textPartsHeight = Math.Max(_textElementSize.Height, Math.Max(actualLeftIconSize.Height, actualRightIconSize.Height));
                textPartsHeight = Math.Max(textPartsHeight, actualPlaceholderSize.Height);

                double actualCaptionHeight = 0;
                double actualCaptionWidth = 0;

                if (string.IsNullOrEmpty(Caption) == false)
                {
                    actualCaptionHeight = CaptionFontSize / 2;
                    actualCaptionWidth = actualCaptionSize.Width + OverBorderCaptionLeftMargin;
                }

                size.Height =
                    Math.Max(BorderThickness, actualCaptionHeight) +
                    textPartsHeight +
                    TextMargin.VerticalThickness +
                    actualInfoTextSize.Height +
                    BorderThickness;

                size.Width =
                    Math.Max(_textElementSize.Width, actualPlaceholderSize.Width) + TextMargin.HorizontalThickness +
                    actualLeftIconSize.Width +
                    actualRightIconSize.Width +
                    BorderThickness * 2;

                size.Width = Math.Max(size.Width, actualCaptionWidth);
                size.Width = Math.Max(size.Width, actualCaptionWidth);
            }
            else // Left
            {
                double textPartsHeight = Math.Max(_textElementSize.Height, Math.Max(actualLeftIconSize.Height, actualRightIconSize.Height));
                textPartsHeight = Math.Max(textPartsHeight, actualPlaceholderSize.Height);

                size.Height =
                    textPartsHeight +
                    TextMargin.VerticalThickness +
                    actualInfoTextSize.Height +
                    BorderThickness * 2;

                size.Height = Math.Max(size.Height, actualCaptionSize.Height);

                size.Width =
                    actualCaptionSize.Width +
                    Math.Max(_textElementSize.Width + actualLeftIconSize.Width + actualRightIconSize.Width, actualInfoTextSize.Width) + TextMargin.HorizontalThickness +
                    BorderThickness * 2;
            }

            return new SizeRequest(size, size);
		}

        /// <summary>
        /// Measure placeholder text size
        /// </summary>
        private Size MeasurePlaceholder(double width, double height, Size captionSize, Size leftIconSize, Size rightIconSize)
        {
            if (_placehoderMeasurePaint == null)
            {
                _placehoderMeasurePaint = new SKPaint();
                _placehoderMeasurePaint.TextSize = (float)FontSize;

                SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(PlaceholderFontStyle);
                SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(PlaceholderFontWeight);
                _placehoderMeasurePaint.Typeface = SKTypeface.FromFamilyName(PlaceholderFontFamily, fontWeight, SKFontStyleWidth.Normal, slant);
            }

            double availableWidth = width;

            Size placeholderAvailableSize = new Size();

            placeholderAvailableSize = new Size(
                width - TextMargin.HorizontalThickness - BorderThickness * 2 - TextPadding.HorizontalThickness - leftIconSize.Width - rightIconSize.Width,
                double.PositiveInfinity);

            if (CaptionPlacement == CaptionPlacements.Left)
            {
                placeholderAvailableSize.Width -= captionSize.Width;
            }

            float lineHeight = (float)FontSize * (IsTextWrapping ? _placeholderLineHeightMultiplier : 1);
            Size size = SkiaUtils.MeasureText(_placehoderMeasurePaint, (float)placeholderAvailableSize.Width, lineHeight, IsTextWrapping, Placeholder ?? Caption);

            if (size.Height <= lineHeight && IsTextWrapping)
            {
                size.Height = FontSize;
            }

            return size;
        }

        /// <summary>
        /// Measure left icon size
        /// </summary>
        private Size MeasureLeftIcon(double width, double height)
        {
            if (LeftIconWidthRequest >= 0 && LeftIconHeightRequest >= 0)
            {
                return new Size(LeftIconWidthRequest, LeftIconHeightRequest);
            }
            else
            {
                if (_leftIconSvg == null)
                {
                    _leftIconSvg = SvgImage.GetSvgImage(LeftIconAssemblyName, LeftIconResourceKey);
                }

                float scale = SvgImage.CalculateScale(_leftIconSvg.Picture.CullRect.Size, LeftIconWidthRequest, LeftIconHeightRequest);

                return new Size(_leftIconSvg.Picture.CullRect.Width * scale, _leftIconSvg.Picture.CullRect.Height * scale);
            }
        }

        /// <summary>
        /// Measure left icon size
        /// </summary>
        private Size MeasureRightIcon(double width, double height)
        {
            if (RightIconWidthRequest >= 0 && RightIconHeightRequest >= 0)
            {
                return new Size(RightIconWidthRequest, RightIconHeightRequest);
            }
            else
            {
                if (_rightIconSvg == null)
                {
                    _rightIconSvg = SvgImage.GetSvgImage(RightIconAssemblyName, RightIconResourceKey);
                }

                float scale = SvgImage.CalculateScale(_rightIconSvg.Picture.CullRect.Size, RightIconWidthRequest, RightIconHeightRequest);

                return new Size(_rightIconSvg.Picture.CullRect.Width * scale, _rightIconSvg.Picture.CullRect.Height * scale);
            }
        }

        /// <summary>
        /// Measure caption. Caption has always full width to show whole caption.
        /// </summary>
        private Size MeasureCaption(double width, double height)
        {
            if (_captionMeasurePaint == null)
            {
                _captionMeasurePaint = new SKPaint();
                _captionMeasurePaint.TextSize = (float)CaptionFontSize;

                SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(CaptionFontStyle);
                SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(CaptionFontWeight);
                _captionMeasurePaint.Typeface = SKTypeface.FromFamilyName(CaptionFontFamily, fontWeight, SKFontStyleWidth.Normal, slant);
            }

            if (CaptionPlacement == CaptionPlacements.Left)
            {
                double availableWidth = width;

                if (CaptionWidth.HasValue)
                {
                    availableWidth = CaptionWidth.Value + CaptionMargin.HorizontalThickness;
                }
                else
                {
                    availableWidth = Math.Min(availableWidth, CaptionMaxWidth);
                }

                float lineHeight = (float)CaptionFontSize * _captionLineHeightMultiplier;
                Size size = SkiaUtils.MeasureText(_captionMeasurePaint, (float)availableWidth, lineHeight, IsCaptionWrapping, Caption);

                if (CaptionWidth.HasValue)
                {
                    size.Width = availableWidth;
                }

                return size;
            }
            else
            {
                float lineHeight = (float)CaptionFontSize * _captionLineHeightMultiplier;
                return SkiaUtils.MeasureText(_captionMeasurePaint, (float)width, lineHeight, IsCaptionWrapping, Caption);
            }
        }

        /// <summary>
        /// Measure entry
        /// </summary>
        protected virtual Size MeasureEntry(double width, double height, Size actualLeftIconSize, Size actualRightIconSize, Size actualCaptionSize)
		{
            SizeRequest entrySize = new SizeRequest();

            double availableWidth = width - actualLeftIconSize.Width - actualRightIconSize.Width - BorderThickness * 2 - TextMargin.HorizontalThickness;

            if (CaptionPlacement == CaptionPlacements.Left)
			{
                availableWidth -= actualCaptionSize.Width;
			}

            entrySize = _textElement.Measure(availableWidth, double.PositiveInfinity, MeasureFlags.IncludeMargins);

            if (HorizontalOptions.Alignment == LayoutAlignment.Fill && double.IsInfinity(availableWidth) == false && double.IsNaN(availableWidth) == false)
            {
                entrySize.Request = new Size(availableWidth, entrySize.Request.Height);
            }

            return entrySize.Request;
        }

        /// <summary>
        /// Measure info text under border
        /// </summary>
        private Size MeasureInfoText(double width, double height, Size captionSize)
        {
            if (_infoTextMeasurePaint == null)
            {
                _infoTextMeasurePaint = new SKPaint();
                _infoTextMeasurePaint.TextSize = (float)InfoTextFontSize;

                SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(InfoTextFontStyle);
                SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(InfoTextFontWeight);
                _infoTextMeasurePaint.Typeface = SKTypeface.FromFamilyName(InfoTextFontFamily, fontWeight, SKFontStyleWidth.Normal, slant);
            }

            double availableWidth = width;
            if (CaptionPlacement == CaptionPlacements.Left)
            {
                availableWidth -= captionSize.Width;
            }

            float lineHeight = (float)InfoTextFontSize * _infoTextLineHeightMultiplier;
            Size infoTextSize = SkiaUtils.MeasureText(_infoTextMeasurePaint, (float)availableWidth, lineHeight, true, InfoText);

            return infoTextSize;
        }

        #endregion

        #region Paint

        /// <summary>
        /// Paint border, line, icon and caption
        /// </summary>
        private void OnPaint(object sender, SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear();

            DeviceScale = (float)(e.Info.Width / Width);

            OnPaintBorder(sender, e);

            if (IsLineVisible)
            {
                OnPaintLine(sender, e);
            }

            if (CaptionPlacement == CaptionPlacements.Inside || CaptionPlacement == CaptionPlacements.OverBorder)
            {
                OnPaintCaption(sender, e);
                OnPaintPlaceholder(sender, e);
            }
            else
            {
                if (ActualIsCaptionVisible)
                {
                    OnPaintCaption(sender, e);
                }
                if (ActualIsPlaceholderVisible)
                {
                    OnPaintPlaceholder(sender, e);
                }
            }

            _isPlaceholderPainted = ActualIsPlaceholderVisible;

            if (ActualIsLeftIconVisible)
            {
                OnPaintIcon(sender, e, HorizontalLocations.Left);
            }

            if (ActualIsRightIconVisible)
            {
                OnPaintIcon(sender, e, HorizontalLocations.Right);
            }

            if (ActualIsInfoTextVisible)
            {
                OnPaintInfoText(sender, e);
            }
        }

        /// <summary>
        /// Paint caption on top or left
        /// </summary>
        private void OnPaintCaption(object sender, SKPaintSurfaceEventArgs e)
        {
            if (ActualIsCaptionVisible == false)
            {
                return;
            }

            Size actualLeftIconSize = new Size();
            if (ActualIsLeftIconVisible)
            {
                actualLeftIconSize = new Size(_leftIconSize.Width + LeftIconMargin.HorizontalThickness, _leftIconSize.Height + LeftIconMargin.VerticalThickness);
            }

            if (_captionPaint == null)
            {
                _captionPaint = new SKPaint();
                _captionPaint.TextAlign = SKTextAlign.Left;
                _captionPaint.IsAntialias = true;

                SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(CaptionFontStyle);
                SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(CaptionFontWeight);
                _captionPaint.Typeface = SKTypeface.FromFamilyName(InfoTextFontFamily, fontWeight, SKFontStyleWidth.Normal, slant);
            }

            if (CaptionPlacement == CaptionPlacements.Left)
            {
                if (IsEnabled)
                {
                    if (CaptionColor != CaptionFocusedColor)
                    {
                        _captionPaint.Color = AnimationUtils.ColorTransform(_focusedAnimationProcess, CaptionColor, CaptionFocusedColor).ToSKColor();
                    }
                    else
                    {
                        _captionPaint.Color = CaptionColor.ToSKColor();
                    }
                }
                else
                {
                    _captionPaint.Color = CaptionDisabledColor.ToSKColor();
                }

                // Set caption font size
                _captionPaint.TextSize = (float)CaptionFontSize * DeviceScale;

                // Line height
                float lineHeight = (float)CaptionFontSize * _captionLineHeightMultiplier * DeviceScale;

                // Available width
                float skCaptionAvailableWidth = (float)_captionSize.Width * DeviceScale;

                // Measure caption (get row height, left and top)
                SKRect skTextBounds = new SKRect();
                _captionPaint.MeasureText(Caption, ref skTextBounds);

                float xText = -skTextBounds.Left + (float)(CaptionMargin.Left * DeviceScale);
                float yText = 0;

                // Set caption vertical alignment
                if (CaptionVerticalOptions.Alignment == LayoutAlignment.Center)
                {
                    yText = -skTextBounds.Top + (float)(CaptionMargin.Top * DeviceScale) + ((float)(_textElementSize.Height * DeviceScale) / 2) - (skTextBounds.Height / 2);
                }
                else
                {
                    yText = -skTextBounds.Top + (float)(CaptionMargin.Top * DeviceScale);
                }

                SkiaUtils.DrawTextArea(e.Surface.Canvas, _captionPaint, xText, yText, skCaptionAvailableWidth, lineHeight, IsCaptionWrapping, Caption);
            }
            else if (CaptionPlacement == CaptionPlacements.Top)
            {
                if (IsEnabled)
                {
                    if (CaptionColor != CaptionFocusedColor)
                    {
                        _captionPaint.Color = AnimationUtils.ColorTransform(_focusedAnimationProcess, CaptionColor, CaptionFocusedColor).ToSKColor();
                    }
                    else
                    {
                        _captionPaint.Color = CaptionColor.ToSKColor();
                    }
                }
                else
                {
                    _captionPaint.Color = CaptionDisabledColor.ToSKColor();
                }

                // Set caption font size
                _captionPaint.TextSize = (float)CaptionFontSize * DeviceScale;

                // Line height
                float lineHeight = (float)CaptionFontSize * _captionLineHeightMultiplier * DeviceScale;

                // Available width
                float skCaptionAvailableWidth = (float)_captionSize.Width * DeviceScale;

                SKRect skTextBounds = new SKRect();
                _captionPaint.MeasureText(Caption, ref skTextBounds);

                float xText = -skTextBounds.Left + (float)(CaptionMargin.Left * DeviceScale);
                float yText = -skTextBounds.Top + (float)(CaptionMargin.Top * DeviceScale);

                SkiaUtils.DrawTextArea(e.Surface.Canvas, _captionPaint, xText, yText, skCaptionAvailableWidth, lineHeight, IsCaptionWrapping, Caption);
            }
            else if (CaptionPlacement == CaptionPlacements.Inside)
            {
                if (IsEnabled)
                {
                    if (CaptionColor != CaptionFocusedColor)
                    {
                        _captionPaint.Color = AnimationUtils.ColorTransform(_focusedAnimationProcess, CaptionColor, CaptionFocusedColor).MultiplyAlpha(_captionAnimationProcess).ToSKColor();
                    }
                    else
                    {
                        _captionPaint.Color = CaptionColor.MultiplyAlpha(_captionAnimationProcess).ToSKColor(); ;
                    }
                }
                else
                {
                    _captionPaint.Color = CaptionDisabledColor.ToSKColor();
                }

                // Caption animated font size (placeholder -> caption)
                _captionPaint.TextSize = (float)(FontSize + (CaptionFontSize - FontSize) * _captionAnimationProcess) * DeviceScale;

                // Caption animated line height (placeholder -> caption)
                float startLineHeight = (float)FontSize * _placeholderLineHeightMultiplier * DeviceScale;
                float targetLineHeight = (float)CaptionFontSize * _captionLineHeightMultiplier * DeviceScale;
                float lineHeight = startLineHeight + (targetLineHeight - startLineHeight) * (float)_captionAnimationProcess;

                SKRect skTextBounds = new SKRect();
                _captionPaint.MeasureText(Caption, ref skTextBounds);

                // Get placeholder SK location
                SKRect skPlaceholderLocation = GetPlaceholderLocation(skTextBounds);

                // Animated caption x and y (from placeholder location)
                float targetX = -skTextBounds.Left + (float)(CaptionMargin.Left + actualLeftIconSize.Width + ActualTextPadding.Left) * DeviceScale;
                float startX = skPlaceholderLocation.Left;
                float targetY = -skTextBounds.Top + (float)(TextMargin.Top + (float)Math.Max(0, ActualTextPadding.Top - CaptionFontSize)) * DeviceScale;
                float startY = (float)(FontSize * DeviceScale) + skPlaceholderLocation.Top;
                float xText = startX + (targetX - startX) * (float)_captionAnimationProcess;
                float yText = startY + (targetY - startY) * (float)_captionAnimationProcess;

                SkiaUtils.DrawTextArea(e.Surface.Canvas, _captionPaint, xText, yText, skTextBounds.Width, lineHeight, IsCaptionWrapping, Caption);
            }
            else if (CaptionPlacement == CaptionPlacements.OverBorder)
            {
                if (IsEnabled)
                {
                    if (CaptionColor != CaptionFocusedColor)
                    {
                        _captionPaint.Color = AnimationUtils.ColorTransform(_focusedAnimationProcess, CaptionColor, CaptionFocusedColor).MultiplyAlpha(_captionAnimationProcess).ToSKColor();
                    }
                    else
                    {
                        _captionPaint.Color = CaptionColor.MultiplyAlpha(_captionAnimationProcess).ToSKColor();
                    }
                }
                else
                {
                    _captionPaint.Color = CaptionDisabledColor.ToSKColor();
                }

                // Caption animated font size (placeholder -> caption)
                _captionPaint.TextSize = (float)(FontSize + (CaptionFontSize - FontSize) * _captionAnimationProcess) * DeviceScale;

                // Caption animated line height (placeholder -> caption)
                float startLineHeight = (float)FontSize * _placeholderLineHeightMultiplier * DeviceScale;
                float targetLineHeight = (float)CaptionFontSize * _captionLineHeightMultiplier * DeviceScale;
                float lineHeight = startLineHeight + (targetLineHeight - startLineHeight) * (float)_captionAnimationProcess;

                SKRect skTextBounds = new SKRect();
                _captionPaint.MeasureText(Caption, ref skTextBounds);

                // Get placeholder SK location
                SKRect skPlaceholderLocation = GetPlaceholderLocation(skTextBounds);

                float targetX = -skTextBounds.Left + (float)(OverBorderCaptionLeftMargin * DeviceScale);
                float startX = skPlaceholderLocation.Left;
                float targetY = -skTextBounds.Top + ((BorderThickness > CaptionFontSize) ? (float)((BorderThickness - CaptionFontSize) / 2) * DeviceScale : 0);
                float startY = (float)(FontSize * DeviceScale) + skPlaceholderLocation.Top;
                float xText = startX + (targetX - startX) * (float)_captionAnimationProcess;
                float yText = startY + (targetY - startY) * (float)_captionAnimationProcess;

                SkiaUtils.DrawTextArea(e.Surface.Canvas, _captionPaint, xText, yText, skTextBounds.Width, lineHeight, IsCaptionWrapping, Caption);
            }
        }

        /// <summary>
        /// Paint placeholder (called only if Text is empty)
        /// </summary>
        private void OnPaintPlaceholder(object sender, SKPaintSurfaceEventArgs e)
        {
            string actualPlaceholder = Placeholder ?? Caption ?? "";
            if (string.IsNullOrEmpty(actualPlaceholder))
            {
                return;
            }

            Size actualLeftIconSize = new Size();
            if (ActualIsLeftIconVisible)
            {
                actualLeftIconSize = new Size(_leftIconSize.Width + LeftIconMargin.HorizontalThickness, _leftIconSize.Height + LeftIconMargin.VerticalThickness);
            }

            if (_placehoderPaint == null)
            {
                _placehoderPaint = new SKPaint();
                _placehoderPaint.TextAlign = SKTextAlign.Left;
                _placehoderPaint.IsAntialias = true;

                SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(PlaceholderFontStyle);
                SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(PlaceholderFontWeight);
                _placehoderPaint.Typeface = SKTypeface.FromFamilyName(PlaceholderFontFamily, fontWeight, SKFontStyleWidth.Normal, slant);
            }

            if (CaptionPlacement == CaptionPlacements.Inside)
            {
                if (IsEnabled)
                {
                    if (PlaceholderColor != PlaceholderFocusedColor)
                    {
                        _placehoderPaint.Color = AnimationUtils.ColorTransform(_focusedAnimationProcess, PlaceholderColor, PlaceholderFocusedColor).MultiplyAlpha(1 - _captionAnimationProcess).ToSKColor();
                    }
                    else
                    {
                        _placehoderPaint.Color = PlaceholderColor.MultiplyAlpha(1 - _captionAnimationProcess).ToSKColor();
                    }
                }
                else
                {
                    _placehoderPaint.Color = PlaceholderDisabledColor.ToSKColor();
                }

                // Placeholder animated font size (placeholder -> caption)
                _placehoderPaint.TextSize = (float)(FontSize + (CaptionFontSize - FontSize) * _captionAnimationProcess) * DeviceScale;

                // Placeholder animated line height (placeholder -> caption)
                float startLineHeight = (float)FontSize * _placeholderLineHeightMultiplier * DeviceScale;
                float targetLineHeight = (float)CaptionFontSize * _captionLineHeightMultiplier * DeviceScale;
                float lineHeight = startLineHeight + (targetLineHeight - startLineHeight) * (float)_captionAnimationProcess;

                SKRect skTextBounds = new SKRect();
                _placehoderPaint.MeasureText(actualPlaceholder, ref skTextBounds);

                // Get placeholder SK location
                SKRect skPlaceholderLocation = GetPlaceholderLocation(skTextBounds);

                // Animated caption x and y (from placeholder location)
                float targetX = -skTextBounds.Left + (float)(CaptionMargin.Left + actualLeftIconSize.Width + TextPadding.Left) * DeviceScale;
                float startX = skPlaceholderLocation.Left;
                float targetY = -skTextBounds.Top + (float)(TextMargin.Top + (float)(TextPadding.Top / 2)) * DeviceScale;
                float startY = (float)(FontSize * DeviceScale) + skPlaceholderLocation.Top;
                float xText = startX + (targetX - startX) * (float)_captionAnimationProcess;
                float yText = startY + (targetY - startY) * (float)_captionAnimationProcess;

                SkiaUtils.DrawTextArea(e.Surface.Canvas, _placehoderPaint, xText, yText, skPlaceholderLocation.Width, lineHeight, IsTextWrapping, actualPlaceholder);
            }
            else if (CaptionPlacement == CaptionPlacements.OverBorder)
            {
                if (IsEnabled)
                {
                    if (PlaceholderFocusedColor != PlaceholderColor)
                    {
                        _placehoderPaint.Color = AnimationUtils.ColorTransform(_focusedAnimationProcess, PlaceholderColor, PlaceholderFocusedColor).MultiplyAlpha(1 - _captionAnimationProcess).ToSKColor();
                    }
                    else
                    {
                        _placehoderPaint.Color = PlaceholderColor.MultiplyAlpha(1 - _captionAnimationProcess).ToSKColor();
                    }
                }
                else
                {
                    _placehoderPaint.Color = PlaceholderDisabledColor.ToSKColor();
                }

                // Placeholder animated font size (placeholder -> caption)
                _placehoderPaint.TextSize = (float)(FontSize + (CaptionFontSize - FontSize) * _captionAnimationProcess) * DeviceScale;

                // Caption animated line height (placeholder -> caption)
                float startLineHeight = (float)FontSize * _placeholderLineHeightMultiplier * DeviceScale;
                float targetLineHeight = (float)CaptionFontSize * _captionLineHeightMultiplier * DeviceScale;
                float lineHeight = startLineHeight + (targetLineHeight - startLineHeight) * (float)_captionAnimationProcess;

                SKRect skTextBounds = new SKRect();
                _placehoderPaint.MeasureText(actualPlaceholder, ref skTextBounds);

                // Get placeholder SK location
                SKRect skPlaceholderLocation = GetPlaceholderLocation(skTextBounds);

                float targetX = -skTextBounds.Left + (float)(OverBorderCaptionLeftMargin * DeviceScale);
                float startX = skPlaceholderLocation.Left;
                float targetY = -skTextBounds.Top + ((BorderThickness > CaptionFontSize) ? (float)((BorderThickness - CaptionFontSize) / 2) * DeviceScale : 0);
                float startY = (float)(FontSize * DeviceScale) + skPlaceholderLocation.Top;
                float xText = startX + (targetX - startX) * (float)_captionAnimationProcess;
                float yText = startY + (targetY - startY) * (float)_captionAnimationProcess;

                SkiaUtils.DrawTextArea(e.Surface.Canvas, _placehoderPaint, xText, yText, skPlaceholderLocation.Width, lineHeight, IsTextWrapping, actualPlaceholder);
            }
            else
            {
                if (IsEnabled)
                {
                    if (PlaceholderColor != PlaceholderFocusedColor)
                    {
                        _placehoderPaint.Color = AnimationUtils.ColorTransform(_focusedAnimationProcess, PlaceholderColor, PlaceholderFocusedColor).ToSKColor();
                    }
                    else
                    {
                        _placehoderPaint.Color = PlaceholderColor.ToSKColor();
                    }
                }
                else
                {
                    _placehoderPaint.Color = PlaceholderDisabledColor.ToSKColor();
                }

                _placehoderPaint.TextSize = (float)FontSize * DeviceScale;

                SKRect skTextBounds = new SKRect();
                _placehoderPaint.MeasureText(actualPlaceholder, ref skTextBounds);

                SKRect skPlaceholderLocation = GetPlaceholderLocation(skTextBounds);

                float skLineHeight = (float)FontSize * DeviceScale * (IsTextWrapping ? _placeholderLineHeightMultiplier : 1);

                float xText = skPlaceholderLocation.Left;
                float yText = (float)(FontSize * DeviceScale) + skPlaceholderLocation.Top;

                SkiaUtils.DrawTextArea(e.Surface.Canvas, _placehoderPaint, xText, yText, skPlaceholderLocation.Width, skLineHeight, IsTextWrapping, actualPlaceholder);
            }
        }

        /// <summary>
        /// Get placeholder location
        /// </summary>
        private SKRect GetPlaceholderLocation(SKRect skPlaceholderTextBounds)
        {
            Size actualLeftIconSize = new Size();
            if (ActualIsLeftIconVisible)
            {
                actualLeftIconSize = new Size(_leftIconSize.Width + LeftIconMargin.HorizontalThickness, _leftIconSize.Height + LeftIconMargin.VerticalThickness);
            }

            Size actualRightIconSize = new Size();
            if (ActualIsRightIconVisible)
            {
                actualRightIconSize = new Size(_rightIconSize.Width + RightIconMargin.HorizontalThickness, _rightIconSize.Height + RightIconMargin.VerticalThickness);
            }

            double textPadding = TextPadding.Top;
            if (ActualIsCaptionVisible && CaptionPlacement == CaptionPlacements.Inside)
            {
                textPadding = Math.Max(TextPadding.Top, CaptionFontSize / 2);
            }

            SKRect skBorderLocation = GetBorderLocation();
            float skPlaceholderY = (float)(BorderThickness + textPadding) * DeviceScale + skBorderLocation.Top;

            SKRect skPlaceholderLocation = new SKRect();
            skPlaceholderLocation.Size = new SKSize(skBorderLocation.Width - (float)(actualLeftIconSize.Width - actualRightIconSize.Width - TextPadding.HorizontalThickness) * DeviceScale, skPlaceholderTextBounds.Height);            
            skPlaceholderLocation.Location = new SKPoint(skBorderLocation.Left + (float)(actualLeftIconSize.Width + TextPadding.Left + BorderThickness) * DeviceScale, skPlaceholderY);

            if (Device.RuntimePlatform == Device.iOS)
            {
                skPlaceholderLocation.Left += (3 * DeviceScale);
            }

            return skPlaceholderLocation;
        }

        /// <summary>
        /// Paint info text under border
        /// </summary>
        private void OnPaintInfoText(object sender, SKPaintSurfaceEventArgs e)
        {
            if (_infoTextPaint == null)
            {
                _infoTextPaint = new SKPaint();
                _infoTextPaint.Color = InfoTextColor.ToSKColor();
                _infoTextPaint.TextSize = (float)InfoTextFontSize * DeviceScale;
                _infoTextPaint.TextAlign = SKTextAlign.Left;
                _infoTextPaint.IsAntialias = true;

                SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(InfoTextFontStyle);
                SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(InfoTextFontWeight);
                _infoTextPaint.Typeface = SKTypeface.FromFamilyName(InfoTextFontFamily, fontWeight, SKFontStyleWidth.Normal, slant);
            }

            SKRect skBorderLocation = GetBorderLocation();

            SKRect skTextBounds = new SKRect();
            _infoTextPaint.MeasureText(InfoText, ref skTextBounds);

            float xText = -skTextBounds.Left + skBorderLocation.Left + (float)(InfoTextMargin.Left * DeviceScale);
            float yText = -skTextBounds.Top + skBorderLocation.Bottom + (float)(InfoTextMargin.Top * DeviceScale);

            float lineHeight = (float)InfoTextFontSize * _infoTextLineHeightMultiplier * DeviceScale;
            SkiaUtils.DrawTextArea(e.Surface.Canvas, _infoTextPaint, xText, yText, (float)((Width - InfoTextMargin.HorizontalThickness) * DeviceScale) - skBorderLocation.Left, lineHeight, true, InfoText);
        }

        /// <summary>
        /// Paint line under text and icon
        /// </summary>
        private void OnPaintLine(object sender, SKPaintSurfaceEventArgs e)
        {
            SKRect skBorderLocation = GetBorderLocation();
            SKRoundRect borderRectangle = new SKRoundRect(skBorderLocation, (float)CornerRadius * DeviceScale, (float)CornerRadius * DeviceScale);

            if (_linePaint == null)
            {
                _linePaint = new SKPaint();
                _linePaint.Style = SKPaintStyle.Stroke;
                _linePaint.IsAntialias = true;
            }

            if (IsEnabled)
            {
                // Paint line to background
                _linePaint.Color = LineColor.ToSKColor();
                _linePaint.StrokeWidth = (float)LineThickness * DeviceScale;
                e.Surface.Canvas.DrawLine(new SKPoint(skBorderLocation.Left, skBorderLocation.Bottom - (_linePaint.StrokeWidth / 2)), new SKPoint(skBorderLocation.Right, skBorderLocation.Bottom - (_linePaint.StrokeWidth / 2)), _linePaint);

                if (IsLineFocusAnimationEnabled && ActualIsFocused)
                {
                    // Paint focused line
                    SKPaint focusedLinePaint = new SKPaint()
                    {
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = true,
                        Color = LineFocusedColor.ToSKColor(),
                        StrokeWidth = (float)(LineFocusedThickness ?? LineThickness) * DeviceScale,
                    };
                        
                    float lineHorizontalMarginForAnimation = (float)((skBorderLocation.Width / 2) * (1 - _focusedAnimationProcess));
                    e.Surface.Canvas.DrawLine(new SKPoint(skBorderLocation.Left + lineHorizontalMarginForAnimation, skBorderLocation.Bottom - (focusedLinePaint.StrokeWidth / 2)), new SKPoint(skBorderLocation.Right - lineHorizontalMarginForAnimation, skBorderLocation.Bottom - (focusedLinePaint.StrokeWidth / 2)), focusedLinePaint);
                }
                else
                {
                    // Paint line with animated thickness and color
                    SKPaint focusedLinePaint = new SKPaint()
                    {
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = true,
                        Color = LineFocusedColor.MultiplyAlpha(_focusedAnimationProcess).ToSKColor(),
                        StrokeWidth = (float)(LineFocusedThickness ?? LineThickness) * DeviceScale,
                    };

                    e.Surface.Canvas.DrawLine(new SKPoint(skBorderLocation.Left, skBorderLocation.Bottom - (_linePaint.StrokeWidth / 2)), new SKPoint(skBorderLocation.Right, skBorderLocation.Bottom - (_linePaint.StrokeWidth / 2)), focusedLinePaint);
                }
            }
            else
            {
                // Paint disabled line with animated thickness
                _linePaint.Color = LineDisabledColor.ToSKColor();
                _linePaint.StrokeWidth = (float)LineThickness * DeviceScale;
                e.Surface.Canvas.DrawLine(new SKPoint(skBorderLocation.Left, skBorderLocation.Bottom - (_linePaint.StrokeWidth / 2)), new SKPoint(skBorderLocation.Right, skBorderLocation.Bottom - (_linePaint.StrokeWidth / 2)), _linePaint);
            }
        }

        /// <summary>
        /// Paint border over text and icon
        /// </summary>
        private void OnPaintBorder(object sender, SKPaintSurfaceEventArgs e)
        {
            SKRect skBorderLocation = GetBorderLocation();

            float skCornerRadius = (float)(CornerRadius * DeviceScale);
            float skBorderThickness = (float)(BorderThickness * DeviceScale);

            if (BorderFocusedThickness != null)
            {
                double actualBorderThickness = BorderThickness + (BorderFocusedThickness.Value - BorderThickness) * (float)_focusedAnimationProcess;
                skBorderThickness = (float)actualBorderThickness * DeviceScale;
            }

            SKPath path = new SKPath();
            SKRect rect = new SKRect();
            rect.Size = new SKSize(skCornerRadius, skCornerRadius);

            double captionLeftMargin = CornerRadius;
            double captionPadding = 0;
            if (CaptionPlacement == CaptionPlacements.OverBorder)
            {
                captionLeftMargin = OverBorderCaptionLeftMargin;
                captionPadding = OverBorderCaptionSpacing;
            }

            path.MoveTo(skBorderLocation.Left + (float)(captionLeftMargin + _captionSize.Width + captionPadding) * DeviceScale, skBorderLocation.Top + (float)skBorderThickness / 2);

            rect.Location = new SKPoint(skBorderLocation.Right - skCornerRadius - (skBorderThickness / 2), skBorderLocation.Top + (float)skBorderThickness / 2);
            path.ArcTo(rect, 270, 90, false);

            if (IsLineVisible)
            {
                path.LineTo(skBorderLocation.Right - (float)Math.Ceiling(skBorderThickness / 2), (float)Math.Ceiling(skBorderLocation.Bottom - (float)(skBorderThickness / 2)));
                path.LineTo(skBorderLocation.Left + (float)Math.Ceiling(skBorderThickness / 2), (float)Math.Ceiling(skBorderLocation.Bottom - (float)(skBorderThickness / 2)));
            }
            else
            {
                rect.Location = new SKPoint(skBorderLocation.Right - skCornerRadius - (float)Math.Ceiling(skBorderThickness / 2), (float)skBorderLocation.Bottom - skCornerRadius - (float)Math.Ceiling(skBorderThickness / 2));
                path.ArcTo(rect, 0, 90, false);

                rect.Location = new SKPoint(skBorderLocation.Left + (float)Math.Ceiling(skBorderThickness / 2), (float)(skBorderLocation.Bottom) - skCornerRadius - (float)Math.Ceiling(skBorderThickness / 2));
                path.ArcTo(rect, 90, 90, false);
            }

            rect.Location = new SKPoint(skBorderLocation.Location.X + (float)(skBorderThickness / 2), skBorderLocation.Location.Y + (float)(skBorderThickness / 2));
            path.ArcTo(rect, 180, 90, false);

            path.LineTo(skBorderLocation.Left + (float)(captionLeftMargin - captionPadding) * DeviceScale, skBorderLocation.Top + (float)(skBorderThickness / 2));

            if ((IsCaptionAnimationDone() == false && _captionAnimationProcess == 0) || CaptionPlacement != CaptionPlacements.OverBorder || ActualIsCaptionVisible == false || (CaptionPlacement == CaptionPlacements.OverBorder && ActualIsFocused == false && ActualHasValue == false))
            {
                path.Close();
            }

            if (BorderBackgroundColor != null)
            {
                if (_backgroundPaint == null)
                {
                    _backgroundPaint = new SKPaint();
                    _backgroundPaint.IsAntialias = true;
                    _backgroundPaint.Style = SKPaintStyle.Fill;
                }

                if (IsEnabled)
                {
                    if (BorderBackgroundColor != BorderBackgroundFocusedColor)
                    {
                        _backgroundPaint.Color = AnimationUtils.ColorTransform(_focusedAnimationProcess, BorderBackgroundColor, BorderBackgroundFocusedColor).ToSKColor();
                    }
                    else
                    {
                        _backgroundPaint.Color = BorderBackgroundColor.ToSKColor();
                    }
                }
                else
                {
                    _backgroundPaint.Color = BorderBackgroundDisabledColor.ToSKColor();
                }

                e.Surface.Canvas.DrawPath(path, _backgroundPaint);
            }

            if (BorderColor != null && BorderThickness > 0)
            {
                if (_borderPaint == null)
                {
                    _borderPaint = new SKPaint();
                    _borderPaint.Style = SKPaintStyle.Stroke;
                    _borderPaint.IsAntialias = true;
                }

                _borderPaint.StrokeWidth = skBorderThickness;

                if (IsEnabled)
                {
                    if (BorderColor != BorderFocusedColor)
                    {
                        _borderPaint.Color = AnimationUtils.ColorTransform(_focusedAnimationProcess, BorderColor, BorderFocusedColor).ToSKColor();
                    }
                    else
                    {
                        _borderPaint.Color = BorderColor.ToSKColor();
                    }
                }
                else
                {
                    _borderPaint.Color = BorderDisabledColor.ToSKColor();
                }

                e.Surface.Canvas.DrawPath(path, _borderPaint);
            }
        }

        /// <summary>
        /// Calculate border outer location in SK coordinates
        /// </summary>
        /// <returns>The border location</returns>
        protected SKRect GetBorderLocation()
        {
            Size actualLeftIconSize = new Size();
            if (_leftIconSize != new Size())
            {
                actualLeftIconSize = new Size(_leftIconSize.Width + LeftIconMargin.HorizontalThickness, _leftIconSize.Height + LeftIconMargin.VerticalThickness);
            }

            Size actualRightIconSize = new Size();
            if (_rightIconSize != new Size())
            {
                actualRightIconSize = new Size(_rightIconSize.Width + RightIconMargin.HorizontalThickness, _rightIconSize.Height + RightIconMargin.VerticalThickness);
            }

            Size actualCaptionSize = new Size();
            if (string.IsNullOrEmpty(Caption) == false)
            {
                actualCaptionSize = new Size(_captionSize.Width + CaptionMargin.HorizontalThickness, _captionSize.Height + CaptionMargin.VerticalThickness);
            }

            Size actualPlaceholderSize = new Size();
            if (string.IsNullOrEmpty(Placeholder) == false || string.IsNullOrEmpty(Caption) == false)
            {
                actualPlaceholderSize = new Size(_placeholderSize.Width + TextPadding.HorizontalThickness, _placeholderSize.Height + TextPadding.VerticalThickness);
            }

            SKRect borderLocation = new SKRect();

            if (CaptionPlacement == CaptionPlacements.Top)
            {
                double textPartsHeight = Math.Max(_textElementSize.Height, Math.Max(actualLeftIconSize.Height, actualRightIconSize.Height));
                textPartsHeight = Math.Max(textPartsHeight, actualPlaceholderSize.Height);

                borderLocation.Location = new SKPoint(
                    (float)TextMargin.Left * DeviceScale,
                    (float)(actualCaptionSize.Height + TextMargin.Top) * DeviceScale);

                borderLocation.Size = new SKSize(
                    (float)(Math.Max(_textElementSize.Width, actualPlaceholderSize.Width) + actualLeftIconSize.Width + actualRightIconSize.Width + (BorderThickness * 2)) * DeviceScale,
                    (float)(textPartsHeight + (BorderThickness * 2)) * DeviceScale);
            }
            else if (CaptionPlacement == CaptionPlacements.Inside)
            {
                borderLocation.Location = new SKPoint(
                    (float)TextMargin.Left * DeviceScale,
                    (float)TextMargin.Top * DeviceScale);

                double textPartsHeight = Math.Max(_textElementSize.Height, actualPlaceholderSize.Height);
                textPartsHeight = Math.Max(textPartsHeight, actualRightIconSize.Height);
                textPartsHeight = Math.Max(textPartsHeight, actualLeftIconSize.Height);

                borderLocation.Size = new SKSize(
                    (float)(Math.Max(_textElementSize.Width, actualPlaceholderSize.Width) + actualLeftIconSize.Width + actualRightIconSize.Width + (BorderThickness * 2)) * DeviceScale,
                    (float)(textPartsHeight + (BorderThickness * 2) + ActualLineThickness) * DeviceScale);
            }
            else if (CaptionPlacement == CaptionPlacements.OverBorder)
            {
                double actualCaptionHeight = 0;
                if (string.IsNullOrEmpty(Caption) == false)
                {
                    actualCaptionHeight = CaptionFontSize / 2;
                }

                double textPartsHeight = Math.Max(_textElementSize.Height, Math.Max(actualLeftIconSize.Height, actualRightIconSize.Height));
                textPartsHeight = Math.Max(textPartsHeight, actualPlaceholderSize.Height);

                borderLocation.Location = new SKPoint(
                    (float)TextMargin.Left * DeviceScale,
                    (float)(Math.Max(0, actualCaptionHeight - BorderThickness) + TextMargin.Top) * DeviceScale);

                borderLocation.Size = new SKSize(
                    (float)(Math.Max(_textElementSize.Width, actualPlaceholderSize.Width) + actualLeftIconSize.Width + actualRightIconSize.Width + (BorderThickness * 2)) * DeviceScale,
                    (float)(textPartsHeight + (BorderThickness * 2)) * DeviceScale);
            }
            else
            {
                double textPartsHeight = Math.Max(_textElementSize.Height, Math.Max(actualLeftIconSize.Height, actualRightIconSize.Height));
                textPartsHeight = Math.Max(textPartsHeight, actualPlaceholderSize.Height);

                borderLocation.Location = new SKPoint(
                    (float)(TextMargin.Left + actualCaptionSize.Width) * DeviceScale,
                    (float)TextMargin.Top * DeviceScale);

                borderLocation.Size = new SKSize(
                    (float)(Math.Max(_textElementSize.Width, actualPlaceholderSize.Width) + actualLeftIconSize.Width + actualRightIconSize.Width + (BorderThickness * 2)) * DeviceScale,
                    (float)(textPartsHeight + (BorderThickness * 2)) * DeviceScale);
            }

            return borderLocation;
        }

        /// <summary>
        /// Paint icon on left or right side
        /// </summary>
        private void OnPaintIcon(object sender, SKPaintSurfaceEventArgs e, HorizontalLocations iconPlacement)
        {
            SkiaSharp.Extended.Svg.SKSvg svg = null;
            float scale = 1;
            Color color = Color.Black;

            if (iconPlacement == HorizontalLocations.Left)
            {
                if (_leftIconSvg == null)
                {
                    _leftIconSvg = SvgImage.GetSvgImage(LeftIconAssemblyName, LeftIconResourceKey);
                }

                svg = _leftIconSvg;
                scale = SvgImage.CalculateScale(svg.Picture.CullRect.Size, LeftIconWidthRequest, LeftIconHeightRequest) * DeviceScale;

                if (LeftIconColor != LeftIconFocusedColor)
                {
                    color = AnimationUtils.ColorTransform(_focusedAnimationProcess, LeftIconColor, LeftIconFocusedColor);
                }
                else
                {
                    color = LeftIconColor;
                }
            }
            else
            {
                if (_rightIconSvg == null)
                {
                    _rightIconSvg = SvgImage.GetSvgImage(RightIconAssemblyName, RightIconResourceKey);
                }

                svg = _rightIconSvg;
                scale = SvgImage.CalculateScale(svg.Picture.CullRect.Size, RightIconWidthRequest, RightIconHeightRequest) * DeviceScale;

                if (RightIconColor != RightIconFocusedColor)
                {
                    color = AnimationUtils.ColorTransform(_focusedAnimationProcess, RightIconColor, RightIconFocusedColor);
                }
                else
                {
                    color = LeftIconColor;
                }
            }

            SKPoint position = CalculateIconCoordinate(iconPlacement);

            SKMatrix matrix = new SKMatrix();
            matrix.SetScaleTranslate(scale, scale, position.X, position.Y);

            using (var paint = new SKPaint())
            {
                paint.ColorFilter = SKColorFilter.CreateBlendMode(color.ToSKColor(), SKBlendMode.SrcIn);
                paint.Style = SKPaintStyle.Fill;
                paint.IsAntialias = true;

                e.Surface.Canvas.DrawPicture(svg.Picture, ref matrix, paint);
            }
        }

        /// <summary>
        /// Calculate translation in skia coordinates
        /// </summary>
        private SKPoint CalculateIconCoordinate(HorizontalLocations iconPlacement)
        {
            Size iconSizeToCalculate = new Size();
            if (ActualIsLeftIconVisible)
            {
                iconSizeToCalculate = new Size(_leftIconSize.Width + LeftIconMargin.HorizontalThickness, _leftIconSize.Height + LeftIconMargin.VerticalThickness);
            }
            else if (iconPlacement == HorizontalLocations.Right && ActualIsRightIconVisible)
            {
                iconSizeToCalculate = new Size(_rightIconSize.Width + RightIconMargin.HorizontalThickness, _rightIconSize.Height + RightIconMargin.VerticalThickness);
            }

            SKSize skIconSizeToCalculate = new SKSize((float)iconSizeToCalculate.Width * DeviceScale, (float)iconSizeToCalculate.Height * DeviceScale);
            SKRect skBorderLocation = GetBorderLocation();

            Rectangle iconLocation = new Rectangle();

            if (iconPlacement == HorizontalLocations.Left)
            {
                iconLocation = new Rectangle(
                    skBorderLocation.Left + (float)((LeftIconMargin.Left + BorderThickness) * DeviceScale),
                    skBorderLocation.Top + (float)(LeftIconMargin.Top * DeviceScale) + (skBorderLocation.Height - skIconSizeToCalculate.Height) / 2,
                    skIconSizeToCalculate.Width - (float)(LeftIconMargin.HorizontalThickness * DeviceScale),
                    skIconSizeToCalculate.Height - (float)(LeftIconMargin.VerticalThickness * DeviceScale));
            }
            else
            {
                iconLocation = new Rectangle(
                    skBorderLocation.Right - skIconSizeToCalculate.Width - (float)(BorderThickness * DeviceScale),
                    skBorderLocation.Top + (float)(RightIconMargin.Top * DeviceScale) + (skBorderLocation.Height - skIconSizeToCalculate.Height) / 2,
                    skIconSizeToCalculate.Width - (float)(RightIconMargin.HorizontalThickness * DeviceScale),
                    skIconSizeToCalculate.Height - (float)(RightIconMargin.VerticalThickness * DeviceScale));
            }

            return new SKPoint((float)iconLocation.X, (float)iconLocation.Y);
        }

        #endregion

        #region Create parts

        protected virtual View CreateTextElement()
        {
            if (IsTextWrapping || Device.RuntimePlatform == Device.iOS)
            {
                Editor editor = new Editor();
                editor.AutoSize = EditorAutoSizeOption.TextChanges;
                editor.Padding = ActualTextPadding;
                editor.VerticalOptions = LayoutOptions.FillAndExpand;

                Binding bind = new Binding("Text");
                bind.Source = this;
                bind.Mode = BindingMode.TwoWay;
                editor.SetBinding(Editor.TextProperty, bind);

                bind = new Binding("TextColor");
                bind.Source = this;
                bind.Mode = BindingMode.OneWay;
                editor.SetBinding(Editor.TextColorProperty, bind);

                bind = new Binding("FontSize");
                bind.Source = this;
                bind.Mode = BindingMode.OneWay;
                editor.SetBinding(Editor.FontSizeProperty, bind);

                bind = new Binding("FontFamily");
                bind.Source = this;
                bind.Mode = BindingMode.OneWay;
                editor.SetBinding(Editor.FontFamilyProperty, bind);
           
                editor.Focused += GotFocus;
                editor.Unfocused += LostFocus;
                editor.TextChanged += OnTextChangedInternal;

                return editor;
            }
            else
            {
                Entry entry = new Entry();
                entry.Padding = ActualTextPadding;

                Binding bind = new Binding("Text");
                bind.Source = this;
                bind.Mode = BindingMode.TwoWay;
                entry.SetBinding(Entry.TextProperty, bind);

                bind = new Binding("TextColor");
                bind.Source = this;
                bind.Mode = BindingMode.OneWay;
                entry.SetBinding(Entry.TextColorProperty, bind);

                bind = new Binding("FontSize");
                bind.Source = this;
                bind.Mode = BindingMode.OneWay;
                entry.SetBinding(Entry.FontSizeProperty, bind);

                bind = new Binding("FontFamily");
                bind.Source = this;
                bind.Mode = BindingMode.OneWay;
                entry.SetBinding(Entry.FontFamilyProperty, bind);
               
                entry.Focused += GotFocus;
                entry.Unfocused += LostFocus;
                entry.TextChanged += OnTextChangedInternal;

                return entry;
            }
        }

        private SKCanvasView CreateSkiaCanvas()
        {
            SKCanvasView canvas = new SKCanvasView();
            canvas.PaintSurface += OnPaint;
            return canvas;
        }

        #endregion

        #region Property values changed

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {            
            if (propertyName == TextBox.IsTextWrappingProperty.PropertyName)
            {
                if (_textElement != null)
                {
                    if (Children.Contains(_textElement))
                    {
                        Children.Remove(_textElement);
                    }
                    _textElement = null;
                    _textElement = CreateTextElement();
                    Children.Add(_textElement);
                }
            }
            else if (propertyName == TextBox.BorderBackgroundColorProperty.PropertyName ||
                     propertyName == TextBox.BorderColorProperty.PropertyName ||
                     propertyName == TextBox.LineColorProperty.PropertyName ||
                     propertyName == TextBox.IsLineVisibleProperty.PropertyName ||
                     propertyName == TextBox.LineThicknessProperty.PropertyName)
            {
                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName == TextBox.CaptionPlacementProperty.PropertyName ||
                     propertyName == TextBox.CaptionProperty.PropertyName)
            {
                if (_textElement is Entry entry)
                {
                    entry.Padding = ActualTextPadding;
                }
                else if (_textElement is Editor editor)
                {
                    editor.Padding = ActualTextPadding;
                }

                if (_textElement != null)
                {
                    InvalidateMeasure();
                    InvalidateLayout();
                }

                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName == TextBox.TextMarginProperty.PropertyName ||
                     propertyName == TextBox.PlaceholderProperty.PropertyName ||
                     propertyName == TextBox.PlaceholderFontFamilyProperty.PropertyName ||
                     propertyName == TextBox.PlaceholderFontWeightProperty.PropertyName ||
                     propertyName == TextBox.PlaceholderFontStyleProperty.PropertyName ||
                     propertyName == TextBox.CaptionFontSizeProperty.PropertyName ||
                     propertyName == TextBox.CaptionWidthProperty.PropertyName ||
                     propertyName == TextBox.CaptionMaxWidthProperty.PropertyName ||
                     propertyName == TextBox.CaptionMinWidthProperty.PropertyName ||
                     propertyName == TextBox.CaptionFontFamilyProperty.PropertyName ||
                     propertyName == TextBox.CaptionPlacementProperty.PropertyName ||
                     propertyName == TextBox.CaptionMarginProperty.PropertyName ||
                     propertyName == TextBox.IsCaptionWrappingProperty.PropertyName ||
                     propertyName == TextBox.CaptionFontStyleProperty.PropertyName ||
                     propertyName == TextBox.CaptionFontWeightProperty.PropertyName ||
                     propertyName == TextBox.LeftIconResourceKeyProperty.PropertyName ||
                     propertyName == TextBox.LeftIconAssemblyNameProperty.PropertyName ||
                     propertyName == TextBox.LeftIconWidthRequestProperty.PropertyName ||
                     propertyName == TextBox.LeftIconHeightRequestProperty.PropertyName ||
                     propertyName == TextBox.LeftIconMarginProperty.PropertyName ||
                     propertyName == TextBox.RightIconResourceKeyProperty.PropertyName ||
                     propertyName == TextBox.RightIconAssemblyNameProperty.PropertyName ||
                     propertyName == TextBox.RightIconWidthRequestProperty.PropertyName ||
                     propertyName == TextBox.RightIconHeightRequestProperty.PropertyName ||
                     propertyName == TextBox.RightIconMarginProperty.PropertyName ||
                     propertyName == TextBox.InfoTextProperty.PropertyName ||
                     propertyName == TextBox.InfoTextMarginProperty.PropertyName ||
                     propertyName == TextBox.InfoTextFontSizeProperty.PropertyName ||
                     propertyName == TextBox.InfoTextFontFamilyProperty.PropertyName ||
                     propertyName == TextBox.InfoTextFontStyleProperty.PropertyName ||
                     propertyName == TextBox.InfoTextFontWeightProperty.PropertyName ||
                     propertyName == TextBox.BorderThicknessProperty.PropertyName ||
                     propertyName == TextBox.CornerRadiusProperty.PropertyName)
            {
                if (_textElement != null)
                {
                    InvalidateMeasure();
                    InvalidateLayout();
                }

                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName == TextBox.CaptionHorizontalOptionsProperty.PropertyName ||
                     propertyName == TextBox.CaptionVerticalOptionsProperty.PropertyName)
            {
                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName == TextBox.TextPaddingProperty.PropertyName)
            {
                if (_textElement != null && Children.Contains(_textElement))
                {
                    if (_textElement is Editor editor)
                    {
                        editor.Padding = ActualTextPadding;
                    }
                    else if (_textElement is Entry entry)
                    {
                        entry.Padding = ActualTextPadding;
                    }

                    base.OnPropertyChanged(propertyName);

                    InvalidateMeasure();
                    InvalidateLayout();
                }
            }
            else
            {
                if (propertyName == TextBox.IsEnabledProperty.PropertyName)
                {
                    if (IsEnabled)
                    {
                        VisualStateManager.GoToState(this, _normal);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, _disabled);
                    }
                }

                base.OnPropertyChanged(propertyName);
            }
        }

        #endregion

        #region Interaction

        protected virtual void OnGotFocus()
        {
            return;
        }

        protected virtual void OnLostFocus()
        {
            return;
        }

        protected void GotFocus()
        {
            OnGotFocus();

            DoAnimation(ActualIsFocused);

            VisualStateManager.GoToState(this, _focused);
            _skiaCanvas.InvalidateSurface();

            if (FocusedCommand != null)
            {
                FocusedCommand.Execute(FocusCommandParameter);
            }

            if (KeyboardFocusChanged != null)
            {
                KeyboardFocusChanged(this, true);
            }
        }

        protected void LostFocus()
        {
            OnLostFocus();

            DoAnimation(ActualIsFocused);

            VisualStateManager.GoToState(this, _normal);
            _skiaCanvas.InvalidateSurface();

            if (UnFocusedCommand != null)
            {
                UnFocusedCommand.Execute(FocusCommandParameter);
            }

            if (KeyboardFocusChanged != null)
            {
                KeyboardFocusChanged(this, false);
            }
        }

        private void GotFocus(object sender, FocusEventArgs e)
		{
            if (ActualIsFocused)
            {
                GotFocus();
            }
        }

        private void LostFocus(object sender, FocusEventArgs e)
		{
            if (ActualIsFocused == false)
            {
                LostFocus();
            }
        }

        private void OnTextChangedInternal(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.OldTextValue) == true && string.IsNullOrEmpty(e.NewTextValue) == false && _isPlaceholderPainted == true)
            {
                if (ActualIsFocused == false && AnimationExtensions.AnimationIsRunning(this, _animationName) == false)
                {
                    _captionAnimationProcess = 1;
                }

                _skiaCanvas.InvalidateSurface();
            }
            else if (string.IsNullOrEmpty(e.OldTextValue) == false && string.IsNullOrEmpty(e.NewTextValue) == true && _isPlaceholderPainted == false)
            {
                if (ActualIsFocused == false && AnimationExtensions.AnimationIsRunning(this, _animationName) == false)
                {
                    _captionAnimationProcess = 0;
                }

                _skiaCanvas.InvalidateSurface();
            }
            else if (HorizontalOptions.Alignment != LayoutAlignment.Fill)
            {
                _textElement.WidthRequest = 1; // Bug in entry measure
                _textElement.WidthRequest = -1;

                _skiaCanvas.InvalidateSurface();
            }

            if (TextChanged != null)
            {
                TextChanged(this, e);
            }

            OnTextChanged(e.OldTextValue, e.NewTextValue);
        }

        protected virtual void OnTextChanged(string oldText, string newText)
        {
            return;
        }

        #endregion

        #region Focus animation

        protected async void DoAnimation(bool isFocused)
        {
            AnimationExtensions.AbortAnimation(this, _animationName);

            if (Device.RuntimePlatform == Device.iOS)
            {
                await System.Threading.Tasks.Task.Delay(1);
            }

            if (isFocused)
            {
                new Animation(d =>
                {
                    _focusedAnimationProcess = d;

                    if (IsCaptionAnimationDone())
                    {
                        _captionAnimationProcess = d;
                    }

                    _skiaCanvas.InvalidateSurface();
                }, _focusedAnimationProcess, 1).Commit(this, _animationName, 64, (uint)AnimationDuration, AnimationEasing, finished: (process, isAborted) =>
                {
                    if (isAborted)
                    {
                        _skiaCanvas.InvalidateSurface();
                    }
                });
            }
            else
            {
                new Animation(d =>
                {
                    _focusedAnimationProcess = d;

                    if (IsCaptionAnimationDone())
                    {
                        _captionAnimationProcess = d;
                    }

                    _skiaCanvas.InvalidateSurface();
                }, _focusedAnimationProcess, 0).Commit(this, _animationName, 64, (uint)AnimationDuration, AnimationEasing, finished: (process, isAborted) =>
                {
                    if (isAborted)
                    {
                        _skiaCanvas.InvalidateSurface();
                    }
                });
            }
        }

        protected virtual bool IsCaptionAnimationDone()
        {
            return string.IsNullOrWhiteSpace(Text) && (CaptionPlacement == CaptionPlacements.OverBorder || CaptionPlacement == CaptionPlacements.Inside);
        }

        #endregion
    }
}
