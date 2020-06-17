using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

// SkiaSharp
using SkiaSharp;
using SkiaSharp.Views.Forms;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    [ContentProperty("Content")]
    public class Button : Layout<View>, ITappable, IIsPressed, IContent, IFilter
    {
        // When command is execute
        public enum CommandExecuteEvents { TouchReleased, AfterAnimation, MiddleAnimation }

        /// <summary>
        /// Cache for SVG icons. Each SVG is kept in memory only once even if it's used multple places in application.
        /// </summary>
        protected static readonly Dictionary<string, SkiaSharp.Extended.Svg.SKSvg> SvgCache = new Dictionary<string, SkiaSharp.Extended.Svg.SKSvg>();

        public const string DefaultStateName = "Default";
        public const string PressedStateName = "Pressed";
        public const string MouseEnterStateName = "Entered";
        public const string MouseLeaveStateName = "Exited";

        protected string _pressedAnimationName = "pressedAnimationName";
        protected string _releasedAnimationName = "releasedAnimationName";
        protected string _hoverAnimationName = "hoverAnimationName";
        protected string _iconChangeAnimationName = "iconChangeAnimationName";

        protected SKCanvasView _skiaCanvas = null;
        private View _rightContent = null;
        private SkiaSharp.Extended.Svg.SKSvg _svg = null;

        private View _actualContentElement;

        protected Size _iconSize = new Size();
        protected Size _rightContentSize = new Size();
        protected Size _contentSize = new Size();

        private float _textLineHeightMultiplier = 1.2f;
        private float _extraTextLineHeightMultiplier = 1.2f;

        // 0 -> 1 includes press and release
        protected double _pressedAnimationProcess = 0;
        protected double _hoverAnimationProcess = 0;

        private double _normalizedAnimationProcess = 0;

        // Icon change animations
        protected double _iconAnimatedScale = 1;
        protected double _iconAnimatedOpacity = 1;
        protected double _iconAnimatedRotation = 0;

        protected float DeviceScale { get; private set; } = 1;

        protected Point _mousePosition = Point.Zero;

        /// <summary>
        /// Event when button is tapped
        /// </summary>
		public event EventHandler Tapped;

        /// <summary>
        /// Event when button is pressed
        /// </summary>
		public event EventHandler<bool> IsPressedChanged;

        /// <summary>
        /// Event to raise when 'IsMouseOver' changes
        /// </summary>
        public event EventHandler<bool> IsMouseOverChanged;

        /// <summary>
        /// Is debugging text enabled
        /// </summary>
        public bool IsDebugEnabled { get; set; } = false;

        #region Binding properties - Common

        /// <summary>
        /// Content element
        /// </summary>
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create("Content", typeof(object), typeof(Button), null, propertyChanged: OnContentChanged);

        private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as Button).OnContentChanged(oldValue, newValue);
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
            BindableProperty.Create("ContentTemplate", typeof(DataTemplate), typeof(Button), null, propertyChanged: OnContentTemplateChanged);

        private static void OnContentTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as Button).OnContentTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        /// <summary>
        /// Border stroke thickness
        /// </summary>
        public static readonly BindableProperty BorderThicknessProperty =
            BindableProperty.Create("BorderThickness", typeof(double), typeof(Border), 0.0);

        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        /// <summary>
        /// Common radius for rounded corners
        /// </summary>
        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create("CornerRadius", typeof(double), typeof(Border), 0.0);

        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Button command which is executed on tap gesture
        /// </summary>
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(Button), null);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Command parameter
        /// </summary>
        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create("CommandParameter", typeof(object), typeof(Button), null);

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// Content horizontal alignment
        /// </summary>
        public static readonly BindableProperty ContentHorizontalOptionsProperty =
            BindableProperty.Create("ContentHorizontalOptions", typeof(LayoutOptions), typeof(Button), LayoutOptions.Start);

        public LayoutOptions ContentHorizontalOptions
        {
            get { return (LayoutOptions)GetValue(ContentHorizontalOptionsProperty); }
            set { SetValue(ContentHorizontalOptionsProperty, value); }
        }

        /// <summary>
        /// Content vertical alignment
        /// </summary>
        public static readonly BindableProperty ContentVerticalOptionsProperty =
            BindableProperty.Create("ContentVerticalOptions", typeof(LayoutOptions), typeof(Button), LayoutOptions.Center);

        public LayoutOptions ContentVerticalOptions
        {
            get { return (LayoutOptions)GetValue(ContentVerticalOptionsProperty); }
            set { SetValue(ContentVerticalOptionsProperty, value); }
        }

        /// <summary>
        /// TextAlignment horizontal alignment
        /// </summary>
        public static readonly BindableProperty TextAlignmentProperty =
            BindableProperty.Create("TextAlignment", typeof(TextAlignments), typeof(Button), TextAlignments.Left);

        public TextAlignments TextAlignment
        {
            get { return (TextAlignments)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        /// <summary>
        /// Default text line spacing between Text and ExtraText if both are used
        /// </summary>
        public static readonly BindableProperty TextLineSpacingRequestProperty =
            BindableProperty.Create("TextLineSpacingRequest", typeof(double), typeof(Button), 3.0);

        public double TextLineSpacingRequest
        {
            get { return (double)GetValue(TextLineSpacingRequestProperty); }
            set { SetValue(TextLineSpacingRequestProperty, value); }
        }

        /// <summary>
        /// Min height if enought available height
        /// </summary>
        public static readonly BindableProperty MinHeightRequestProperty =
            BindableProperty.Create("MinHeightRequest", typeof(double), typeof(Button), 0.0);

        public double MinHeightRequest
        {
            get { return (double)GetValue(MinHeightRequestProperty); }
            set { SetValue(MinHeightRequestProperty, value); }
        }

        /// <summary>
        /// Text and ExtraText common margin
        /// </summary>
        public static readonly BindableProperty TextMarginProperty =
            BindableProperty.Create("TextMargin", typeof(Thickness), typeof(Button), new Thickness(0));

        public Thickness TextMargin
        {
            get { return (Thickness)GetValue(TextMarginProperty); }
            set { SetValue(TextMarginProperty, value); }
        }

        /// <summary>
        /// How visual states are animated
        /// </summary>
        public static readonly BindableProperty AnimationStyleProperty =
            BindableProperty.Create("AnimationStyle", typeof(AnimationStyles), typeof(Button), AnimationStyles.Default);

        public AnimationStyles AnimationStyle
        {
            get { return (AnimationStyles)GetValue(AnimationStyleProperty); }
            set { SetValue(AnimationStyleProperty, value); }
        }

        /// <summary>
        /// Pressed visual state transition duration in milliseconds
        /// </summary>
        public static readonly BindableProperty PressedAnimationDurationProperty =
           BindableProperty.Create("PressedAnimationDuration", typeof(int), typeof(Button), 150);

        public int PressedAnimationDuration
        {
            get { return (int)GetValue(PressedAnimationDurationProperty); }
            set { SetValue(PressedAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Pressed visual state transition easing function
        /// </summary>
        public static readonly BindableProperty PressedAnimationEasingProperty =
           BindableProperty.Create("PressedAnimationEasing", typeof(Easing), typeof(Button), Easing.Linear);

        public Easing PressedAnimationEasing
        {
            get { return (Easing)GetValue(PressedAnimationEasingProperty); }
            set { SetValue(PressedAnimationEasingProperty, value); }
        }

        /// <summary>
        /// Released visual state transition duration in milliseconds
        /// </summary>
        public static readonly BindableProperty ReleasedAnimationDurationProperty =
           BindableProperty.Create("ReleasedAnimationDuration", typeof(int), typeof(Button), 150);

        public int ReleasedAnimationDuration
        {
            get { return (int)GetValue(ReleasedAnimationDurationProperty); }
            set { SetValue(ReleasedAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Released visual state transition easing function
        /// </summary>
        public static readonly BindableProperty ReleasedAnimationEasingProperty =
           BindableProperty.Create("ReleasedAnimationEasing", typeof(Easing), typeof(Button), Easing.Linear);

        public Easing ReleasedAnimationEasing
        {
            get { return (Easing)GetValue(ReleasedAnimationEasingProperty); }
            set { SetValue(ReleasedAnimationEasingProperty, value); }
        }

        /// <summary>
        /// Hover visual state transition duration in milliseconds
        /// </summary>
        public static readonly BindableProperty HoverAnimationDurationProperty =
           BindableProperty.Create("HoverAnimationDuration", typeof(int), typeof(Button), 100);

        public int HoverAnimationDuration
        {
            get { return (int)GetValue(HoverAnimationDurationProperty); }
            set { SetValue(HoverAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Hover visual state transition easing function
        /// </summary>
        public static readonly BindableProperty HoverAnimationEasingProperty =
           BindableProperty.Create("HoverAnimationEasing", typeof(Easing), typeof(Button), Easing.Linear);

        public Easing HoverAnimationEasing
        {
            get { return (Easing)GetValue(HoverAnimationEasingProperty); }
            set { SetValue(HoverAnimationEasingProperty, value); }
        }

        /// <summary>
        /// When command is executed related to visual state transition
        /// </summary>
        public static readonly BindableProperty CommandExecuteEventProperty =
           BindableProperty.Create("CommandExecuteEvent", typeof(CommandExecuteEvents), typeof(Button), CommandExecuteEvents.MiddleAnimation);

        public CommandExecuteEvents CommandExecuteEvent
        {
            get { return (CommandExecuteEvents)GetValue(CommandExecuteEventProperty); }
            set { SetValue(CommandExecuteEventProperty, value); }
        }

        #endregion

        #region Binding properties - Interaction

        /// <summary>
        /// Is button toched down
        /// </summary>
        public static readonly BindableProperty IsPressedProperty =
           BindableProperty.Create("IsPressed", typeof(bool), typeof(Button), false);

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            protected set { SetValue(IsPressedProperty, value); }
        }

        /// <summary>
        /// Is mouse over button (on desktop)
        /// </summary>
        public static readonly BindableProperty IsMouseOverProperty =
           BindableProperty.Create("IsMouseOver", typeof(bool), typeof(Button), false);

        public bool IsMouseOver
        {
            get { return (bool)GetValue(IsMouseOverProperty); }
            protected set { SetValue(IsMouseOverProperty, value); }
        }
        
        #endregion

        #region Binding properties - Text

        /// <summary>
        /// Button text
        /// </summary>
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create("Text", typeof(string), typeof(Button), null);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Text font size
        /// </summary>
        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create("FontSize", typeof(double), typeof(Button), 15.0);

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        /// Text font family
        /// </summary>
        public static readonly BindableProperty TextFontFamilyProperty =
            BindableProperty.Create("TextFontFamily", typeof(string), typeof(Button), Font.Default.ToString());

        public string TextFontFamily
        {
            get { return (string)GetValue(TextFontFamilyProperty); }
            set { SetValue(TextFontFamilyProperty, value); }
        }

        /// <summary>
        /// Is text wrapping
        /// </summary>
        public static readonly BindableProperty IsTextWrappingProperty =
            BindableProperty.Create("IsTextWrapping", typeof(bool), typeof(Button), false);

        public bool IsTextWrapping
        {
            get { return (bool)GetValue(IsTextWrappingProperty); }
            set { SetValue(IsTextWrappingProperty, value); }
        }

        /// <summary>
        /// Text font style (Italic, Oblique etc..)
        /// </summary>
        public static readonly BindableProperty TextFontStyleProperty =
            BindableProperty.Create("TextFontStyle", typeof(FontStyles), typeof(Button), FontStyles.Upright);

        public FontStyles TextFontStyle
        {
            get { return (FontStyles)GetValue(TextFontStyleProperty); }
            set { SetValue(TextFontStyleProperty, value); }
        }

        /// <summary>
        /// Text font weight (Bold, SemiBold etc...)
        /// </summary>
        public static readonly BindableProperty TextFontWeightProperty =
            BindableProperty.Create("TextFontWeight", typeof(FontWeights), typeof(Button), FontWeights.Normal);

        public FontWeights TextFontWeight
        {
            get { return (FontWeights)GetValue(TextFontWeightProperty); }
            set { SetValue(TextFontWeightProperty, value); }
        }

        /// <summary>
        /// Is text upper
        /// </summary>
        public static readonly BindableProperty IsTextUpperProperty =
            BindableProperty.Create("IsTextUpper", typeof(bool), typeof(Button), false);

        public bool IsTextUpper
        {
            get { return (bool)GetValue(IsTextUpperProperty); }
            set { SetValue(IsTextUpperProperty, value); }
        }

        #endregion

        #region Binding properties - ExtraText

        /// <summary>
        /// Second line of text
        /// </summary>
        public static readonly BindableProperty ExtraTextProperty =
            BindableProperty.Create("ExtraText", typeof(string), typeof(Button), null);

        public string ExtraText
        {
            get { return (string)GetValue(ExtraTextProperty); }
            set { SetValue(ExtraTextProperty, value); }
        }

        /// <summary>
        /// Extra text font
        /// </summary>
        public static readonly BindableProperty ExtraTextFontSizeProperty =
            BindableProperty.Create("ExtraTextFontSize", typeof(double), typeof(Button), 15.0);

        public double ExtraTextFontSize
        {
            get { return (double)GetValue(ExtraTextFontSizeProperty); }
            set { SetValue(ExtraTextFontSizeProperty, value); }
        }

        /// <summary>
        /// Is ExtraText wrapped to multiple lines
        /// </summary>
        public static readonly BindableProperty IsExtraTextWrappingProperty =
            BindableProperty.Create("IsExtraTextWrapping", typeof(bool), typeof(Button), false);

        public bool IsExtraTextWrapping
        {
            get { return (bool)GetValue(IsExtraTextWrappingProperty); }
            set { SetValue(IsExtraTextWrappingProperty, value); }
        }

        /// <summary>
        /// ExtraText font family
        /// </summary>
        public static readonly BindableProperty ExtraTextFontFamilyProperty =
            BindableProperty.Create("ExtraTextFontFamily", typeof(string), typeof(Button), Font.Default.ToString());

        public string ExtraTextFontFamily
        {
            get { return (string)GetValue(ExtraTextFontFamilyProperty); }
            set { SetValue(ExtraTextFontFamilyProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly BindableProperty ExtraTextFontStyleProperty =
            BindableProperty.Create("ExtraTextFontStyle", typeof(FontStyles), typeof(Button), FontStyles.Upright);

        public FontStyles ExtraTextFontStyle
        {
            get { return (FontStyles)GetValue(ExtraTextFontStyleProperty); }
            set { SetValue(ExtraTextFontStyleProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly BindableProperty ExtraTextFontWeightProperty =
            BindableProperty.Create("ExtraTextFontWeight", typeof(FontWeights), typeof(Button), FontWeights.Normal);

        public FontWeights ExtraTextFontWeight
        {
            get { return (FontWeights)GetValue(ExtraTextFontWeightProperty); }
            set { SetValue(ExtraTextFontWeightProperty, value); }
        }

        /// <summary>
        /// Is extra text upper
        /// </summary>
        public static readonly BindableProperty IsExtraTextUpperProperty =
            BindableProperty.Create("IsExtraTextUpper", typeof(bool), typeof(Button), false);

        public bool IsExtraTextUpper
        {
            get { return (bool)GetValue(IsExtraTextUpperProperty); }
            set { SetValue(IsExtraTextUpperProperty, value); }
        }

        #endregion

        #region Binding properties - Icon

        /// <summary>
        /// Icon position to text
        /// </summary>
        public static readonly BindableProperty IconPlacementProperty =
            BindableProperty.Create("IconPlacement", typeof(ButtonIconPlacements), typeof(Button), ButtonIconPlacements.Left);

        public ButtonIconPlacements IconPlacement
        {
            get { return (ButtonIconPlacements)GetValue(IconPlacementProperty); }
            set { SetValue(IconPlacementProperty, value); }
        }

        /// <summary>
        /// Icon horizontal alignment on IconPosition
        /// </summary>
        public static readonly BindableProperty IconHorizontalOptionsProperty =
            BindableProperty.Create("IconHorizontalOptions", typeof(LayoutOptions), typeof(Button), LayoutOptions.Center);

        public LayoutOptions IconHorizontalOptions
        {
            get { return (LayoutOptions)GetValue(IconHorizontalOptionsProperty); }
            set { SetValue(IconHorizontalOptionsProperty, value); }
        }

        /// <summary>
        /// Icon vertical alignment in IconPosition
        /// </summary>
        public static readonly BindableProperty IconVerticalOptionsProperty =
            BindableProperty.Create("IconVerticalOptions", typeof(LayoutOptions), typeof(Button), LayoutOptions.Center);

        public LayoutOptions IconVerticalOptions
        {
            get { return (LayoutOptions)GetValue(IconVerticalOptionsProperty); }
            set { SetValue(IconVerticalOptionsProperty, value); }
        }

        /// <summary>
        /// Background and foreground icon margin
        /// </summary>
        public static readonly BindableProperty IconMarginProperty =
            BindableProperty.Create("IconMargin", typeof(Thickness), typeof(Button), new Thickness(0));

        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }
            set { SetValue(IconMarginProperty, value); }
        }

        // Foreground icon

        public static readonly BindableProperty IconResourceKeyProperty =
            BindableProperty.Create("IconResourceKey", typeof(string), typeof(Button), null);

        public string IconResourceKey
        {
            get { return (string)GetValue(IconResourceKeyProperty); }
            set { SetValue(IconResourceKeyProperty, value); }
        }

        /// <summary>
        /// The assembly name containing the svg file. Null if default used.
        /// </summary>
        public static readonly BindableProperty IconAssemblyNameProperty =
            BindableProperty.Create(nameof(IconAssemblyName), typeof(string), typeof(Button), default(string));

        public string IconAssemblyName
        {
            get { return (string)GetValue(IconAssemblyNameProperty); }
            set { SetValue(IconAssemblyNameProperty, value); }
        }

        public static readonly BindableProperty IconHeightRequestProperty =
            BindableProperty.Create("IconHeightRequest", typeof(double), typeof(Button), -1.0);

        public double IconHeightRequest
        {
            get { return (double)GetValue(IconHeightRequestProperty); }
            set { SetValue(IconHeightRequestProperty, value); }
        }

        public static readonly BindableProperty IconWidthRequestProperty =
            BindableProperty.Create("IconWidthRequest", typeof(double), typeof(Button), -1.0);

        public double IconWidthRequest
        {
            get { return (double)GetValue(IconWidthRequestProperty); }
            set { SetValue(IconWidthRequestProperty, value); }
        }

        public static readonly BindableProperty IsIconVisibleProperty =
            BindableProperty.Create("IsIconVisible", typeof(bool), typeof(Button), true);

        public bool IsIconVisible
        {
            get { return (bool)GetValue(IsIconVisibleProperty); }
            set { SetValue(IsIconVisibleProperty, value); }
        }

        public static readonly BindableProperty IconChangeAnimationProperty =
            BindableProperty.Create("IconChangeAnimation", typeof(ButtonIconChangeAnimations), typeof(Button), ButtonIconChangeAnimations.None);

        public ButtonIconChangeAnimations IconChangeAnimation
        {
            get { return (ButtonIconChangeAnimations)GetValue(IconChangeAnimationProperty); }
            set { SetValue(IconChangeAnimationProperty, value); }
        }

        public static readonly BindableProperty IconChangeAnimationDurationProperty =
            BindableProperty.Create("IconChangeAnimationDuration", typeof(int), typeof(Button), 200);

        public int IconChangeAnimationDuration
        {
            get { return (int)GetValue(IconChangeAnimationDurationProperty); }
            set { SetValue(IconChangeAnimationDurationProperty, value); }
        }

        public static readonly BindableProperty IconChangeAnimationEasingProperty =
            BindableProperty.Create("IconChangeAnimationEasing", typeof(Easing), typeof(Button), Easing.Linear);

        public Easing IconChangeAnimationEasing
        {
            get { return (Easing)GetValue(IconChangeAnimationEasingProperty); }
            set { SetValue(IconChangeAnimationEasingProperty, value); }
        }

        #endregion

        #region Binding properties - Right content

        public static readonly BindableProperty RightContentProperty =
            BindableProperty.Create("RightContent", typeof(object), typeof(Button), null);

        public object RightContent
        {
            get { return (object)GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }

        public static readonly BindableProperty RightContentTemplateProperty =
            BindableProperty.Create("RightContentTemplate", typeof(DataTemplate), typeof(Button), null);

        public DataTemplate RightContentTemplate
        {
            get { return (DataTemplate)GetValue(RightContentTemplateProperty); }
            set { SetValue(RightContentTemplateProperty, value); }
        }

        #endregion

        #region Binding properties - Color

        // Default

        /// <summary>
        /// Text color
        /// </summary>
        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create("TextColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        /// <summary>
        /// Bottom text color
        /// </summary>
        public static readonly BindableProperty ExtraTextColorProperty =
            BindableProperty.Create("ExtraTextColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color ExtraTextColor
        {
            get { return (Color)GetValue(ExtraTextColorProperty); }
            set { SetValue(ExtraTextColorProperty, value); }
        }

        /// <summary>
        /// Icon color
        /// </summary>
        public static readonly BindableProperty IconColorProperty =
            BindableProperty.Create("IconColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color IconColor
        {
            get { return (Color)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }

        /// <summary>
        /// Border color
        /// </summary>
        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create("BorderColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        /// <summary>
        /// Background color
        /// </summary>
        public static readonly new BindableProperty BackgroundColorProperty =
            BindableProperty.Create("BackgroundColor", typeof(Color), typeof(Button), Color.Transparent);

        public new Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        // Hover

        /// <summary>
        /// Text hover color
        /// </summary>
        public static readonly BindableProperty TextHoverColorProperty =
            BindableProperty.Create("TextHoverColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color TextHoverColor
        {
            get { return (Color)GetValue(TextHoverColorProperty); }
            set { SetValue(TextHoverColorProperty, value); }
        }

        /// <summary>
        /// Bottom text hover color
        /// </summary>
        public static readonly BindableProperty ExtraTextHoverColorProperty =
            BindableProperty.Create("ExtraTextHoverColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color ExtraTextHoverColor
        {
            get { return (Color)GetValue(ExtraTextHoverColorProperty); }
            set { SetValue(ExtraTextHoverColorProperty, value); }
        }

        /// <summary>
        /// Icon hover color
        /// </summary>
        public static readonly BindableProperty IconHoverColorProperty =
            BindableProperty.Create("IconHoverColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color IconHoverColor
        {
            get { return (Color)GetValue(IconHoverColorProperty); }
            set { SetValue(IconHoverColorProperty, value); }
        }

        /// <summary>
        /// Border hover color
        /// </summary>
        public static readonly BindableProperty BorderHoverColorProperty =
            BindableProperty.Create("BorderHoverColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color BorderHoverColor
        {
            get { return (Color)GetValue(BorderHoverColorProperty); }
            set { SetValue(BorderHoverColorProperty, value); }
        }

        /// <summary>
        /// Background hover color
        /// </summary>
        public static readonly BindableProperty BackgroundHoverColorProperty =
            BindableProperty.Create("BackgroundHoverColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color BackgroundHoverColor
        {
            get { return (Color)GetValue(BackgroundHoverColorProperty); }
            set { SetValue(BackgroundHoverColorProperty, value); }
        }

        // Pressed

        /// <summary>
        /// Text pressed color
        /// </summary>
        public static readonly BindableProperty TextPressedColorProperty =
            BindableProperty.Create("TextPressedColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color TextPressedColor
        {
            get { return (Color)GetValue(TextPressedColorProperty); }
            set { SetValue(TextPressedColorProperty, value); }
        }

        /// <summary>
        /// Bottom text pressed color
        /// </summary>
        public static readonly BindableProperty ExtraTextPressedColorProperty =
            BindableProperty.Create("ExtraTextPressedColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color ExtraTextPressedColor
        {
            get { return (Color)GetValue(ExtraTextPressedColorProperty); }
            set { SetValue(ExtraTextPressedColorProperty, value); }
        }

        /// <summary>
        /// Icon pressed color
        /// </summary>
        public static readonly BindableProperty IconPressedColorProperty =
            BindableProperty.Create("IconPressedColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color IconPressedColor
        {
            get { return (Color)GetValue(IconPressedColorProperty); }
            set { SetValue(IconPressedColorProperty, value); }
        }

        /// <summary>
        /// Border pressed color
        /// </summary>
        public static readonly BindableProperty BorderPressedColorProperty =
            BindableProperty.Create("BorderPressedColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color BorderPressedColor
        {
            get { return (Color)GetValue(BorderPressedColorProperty); }
            set { SetValue(BorderPressedColorProperty, value); }
        }

        /// <summary>
        /// Background pressed color
        /// </summary>
        public static readonly BindableProperty BackgroundPressedColorProperty =
            BindableProperty.Create("BackgroundPressedColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color BackgroundPressedColor
        {
            get { return (Color)GetValue(BackgroundPressedColorProperty); }
            set { SetValue(BackgroundPressedColorProperty, value); }
        }

        // Disabled

        /// <summary>
        /// Text disabled color
        /// </summary>
        public static readonly BindableProperty TextDisabledColorProperty =
            BindableProperty.Create("TextDisabledColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color TextDisabledColor
        {
            get { return (Color)GetValue(TextDisabledColorProperty); }
            set { SetValue(TextDisabledColorProperty, value); }
        }

        /// <summary>
        /// Bottom text disabled color
        /// </summary>
        public static readonly BindableProperty ExtraTextDisabledColorProperty =
            BindableProperty.Create("ExtraTextDisabledColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color ExtraTextDisabledColor
        {
            get { return (Color)GetValue(ExtraTextDisabledColorProperty); }
            set { SetValue(ExtraTextDisabledColorProperty, value); }
        }

        /// <summary>
        /// Icon disabled color
        /// </summary>
        public static readonly BindableProperty IconDisabledColorProperty =
            BindableProperty.Create("IconDisabledColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color IconDisabledColor
        {
            get { return (Color)GetValue(IconDisabledColorProperty); }
            set { SetValue(IconDisabledColorProperty, value); }
        }

        /// <summary>
        /// Border disabled color
        /// </summary>
        public static readonly BindableProperty BorderDisabledColorProperty =
            BindableProperty.Create("BorderDisabledColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color BorderDisabledColor
        {
            get { return (Color)GetValue(BorderDisabledColorProperty); }
            set { SetValue(BorderDisabledColorProperty, value); }
        }

        /// <summary>
        /// Background disabled color
        /// </summary>
        public static readonly BindableProperty BackgroundDisabledColorProperty =
            BindableProperty.Create("BackgroundDisabledColor", typeof(Color), typeof(Button), Color.Transparent);

        public Color BackgroundDisabledColor
        {
            get { return (Color)GetValue(BackgroundDisabledColorProperty); }
            set { SetValue(BackgroundDisabledColorProperty, value); }
        }

        #endregion

        #region Properties
        
        protected bool ActualIsIconVisible
        {
            get
            {
                return IsIconVisible && string.IsNullOrEmpty(IconResourceKey) == false;
            }
        }

        #endregion

        public Button()
        {
            _skiaCanvas = new SKCanvasView();
            _skiaCanvas.PaintSurface += OnPaint;
            Children.Add(_skiaCanvas);

            /*
            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += OnTappedGesture;
            GestureRecognizers.Add(tap);
            */
            
            TouchEffect touchEffect = new TouchEffect();
            Effects.Add(touchEffect);
            touchEffect.TouchAction += OnTouchEffectAction;            
        }

        internal void SetIsPressed(bool isPressed)
        {
            IsPressed = isPressed;
        }

        public bool IsFiltered(string filter)
        {
            if (filter == null || string.IsNullOrEmpty(filter))
            {
                return false;
            }
            else
            {
                if (Text != null && Text.ToLower().Contains(filter.ToLower()))
                {
                    return false;
                }
                else if (ExtraText != null && ExtraText.ToLower().Contains(filter.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            if (Text == null)
            {
                return "";
            }
            else
            {
                return Text.ToString();
            }
        }

        protected override void InvalidateMeasure()
        {
            _iconSize = Size.Zero;
            _contentSize = Size.Zero;
            base.InvalidateMeasure();
        }

        protected override void InvalidateLayout()
        {
            _iconSize = Size.Zero;
            _contentSize = Size.Zero;
            base.InvalidateLayout();
        }

        #region Measure and layout

        /// <summary>
        /// Measure all children
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("Button.OnMeasure: " + Text + " " + ExtraText); }

            return MeasureChildren(widthConstraint, heightConstraint);
        }

        /// <summary>
        /// Layout all children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("Button.LayoutChildren: " + Text + " " + ExtraText); }

            if (width > 0 && height > 0 && _iconSize.IsZero && _contentSize.IsZero)
            {
                MeasureChildren(width, height);
            }

            LayoutChildIntoBoundingRegion(_skiaCanvas, new Rectangle(0, 0, width + Padding.HorizontalThickness, height + Padding.VerticalThickness));

            if (Content != null)
            {
                LayoutContent(x, y, width, height);
            }

            if (_rightContent != null)
            {
                Rectangle location = new Rectangle(width - _rightContentSize.Width - _rightContent.Margin.Right,
                                                   (height - _rightContentSize.Height) / 2,
                                                   _rightContentSize.Width - _rightContent.Margin.HorizontalThickness,
                                                   _rightContentSize.Height - -_rightContent.Margin.VerticalThickness);

                if (_rightContent.Bounds != location)
                {
                    _rightContent.Layout(location);
                }
            }
        }
    
        protected virtual void LayoutContent(double x, double y, double width, double height)
        {
            if (_actualContentElement != null)
            {
                LayoutChildIntoBoundingRegion(_actualContentElement, new Rectangle(x, y, width - _rightContentSize.Width, height));
            }
        }

        /// <summary>
        /// Measure all children
        /// </summary>
        private SizeRequest MeasureChildren(double width, double height)
        {
            _contentSize = new Size();
            _iconSize = new Size();
            _rightContentSize = new Size();

            if (_rightContent != null)
            {
                _rightContentSize = _rightContent.Measure(width - (BorderThickness * 2), height - (BorderThickness * 2), MeasureFlags.IncludeMargins).Request;
            }
        
            if (_actualContentElement == null)
            {
                Thickness actualIconMargin = new Thickness();
                if (ActualIsIconVisible)
                {
                    _iconSize = MeasureIcon(width - (BorderThickness * 2), height - (BorderThickness * 2));
                    actualIconMargin = IconMargin;
                }

                Size availableTextSize = new Size();

                switch (IconPlacement)
                {
                    case ButtonIconPlacements.Left:
                    case ButtonIconPlacements.Right:
                        availableTextSize.Width = width - _iconSize.Width - actualIconMargin.HorizontalThickness - _rightContentSize.Width;
                        availableTextSize.Height = height;
                        break;
                    case ButtonIconPlacements.Top:
                    case ButtonIconPlacements.Bottom:
                        availableTextSize.Width = width - _rightContentSize.Width;
                        availableTextSize.Height = height - _iconSize.Height - actualIconMargin.VerticalThickness;
                        break;
                }

                availableTextSize.Width -= BorderThickness * 2;
                availableTextSize.Height -= BorderThickness * 2;

                Size textSize = MeasureDefaultTextContentSize(availableTextSize.Width, availableTextSize.Height);

                switch (IconPlacement)
                {
                    case ButtonIconPlacements.Left:
                    case ButtonIconPlacements.Right:
                        _contentSize.Width = _iconSize.Width + actualIconMargin.HorizontalThickness + textSize.Width;
                        _contentSize.Height = Math.Max(_iconSize.Height + actualIconMargin.VerticalThickness, textSize.Height);
                        break;
                    case ButtonIconPlacements.Top:
                    case ButtonIconPlacements.Bottom:
                        _contentSize.Width = Math.Max(textSize.Width, _iconSize.Width + actualIconMargin.HorizontalThickness);
                        _contentSize.Height = _iconSize.Height + textSize.Height + actualIconMargin.VerticalThickness;
                        break;
                }
            }
            else
            {
                _contentSize = _actualContentElement.Measure(width - (BorderThickness * 2), height - (BorderThickness * 2), MeasureFlags.IncludeMargins).Request;
            }

            Size totalSize = new Size();
            totalSize.Width = _contentSize.Width + _rightContentSize.Width + (BorderThickness * 2);
            totalSize.Height = Math.Max(_contentSize.Height, _rightContentSize.Height) + (BorderThickness * 2);
            totalSize.Height = Math.Max(MinHeightRequest, totalSize.Height);

            return new SizeRequest(totalSize, totalSize);
        }

        /// <summary>
        /// Measure icon size
        /// </summary>
        private Size MeasureIcon(double widthConstraint, double heightConstraint)
        {
            if (IconWidthRequest >= 0 && IconHeightRequest >= 0)
            {
                return new Size(IconWidthRequest, IconHeightRequest);
            }
            else
            {
                if (_svg == null)
                {
                    _svg = SvgImage.GetSvgImage(IconAssemblyName, IconResourceKey);
                }

                float scale = SvgImage.CalculateScale(_svg.Picture.CullRect.Size, IconWidthRequest, IconHeightRequest);

                return new Size(_svg.Picture.CullRect.Width * scale, _svg.Picture.CullRect.Height * scale);
            }
        }

        /// <summary>
        /// If Content is null then use default text as content. Measure default content size in Xamarin coordinates.
        /// </summary>
        private Size MeasureDefaultTextContentSize(double width, double height)
        {
            Size textSize = new Size();

            if (string.IsNullOrEmpty(Text) == false)
            {
                SKPaint paint = CreateTextPaint();
                paint.TextSize = (float)FontSize;

                float lineHeight = paint.TextSize * _textLineHeightMultiplier;

                textSize = SkiaUtils.MeasureText(paint, (float)width, lineHeight, IsTextWrapping, IsTextUpper ? Text.ToUpper() : Text);

                // Remove extra line height from last line
                textSize.Height = textSize.Height - lineHeight + paint.TextSize;

                textSize.Width += TextMargin.HorizontalThickness;
            }

            Size extraTextSize = new Size();

            if (string.IsNullOrEmpty(ExtraText) == false)
            {
                SKPaint paint = CreateExtraTextPaint();
                paint.TextSize = (float)ExtraTextFontSize;

                float lineHeight = paint.TextSize * _extraTextLineHeightMultiplier;

                extraTextSize = SkiaUtils.MeasureText(paint, (float)width, lineHeight, IsExtraTextWrapping, IsExtraTextUpper ? ExtraText.ToUpper() : ExtraText);

                // Remove extra line height from last line
                extraTextSize.Height = extraTextSize.Height - lineHeight + paint.TextSize;

                extraTextSize.Width += TextMargin.HorizontalThickness;
            }

            double actualHeight = textSize.Height + extraTextSize.Height;

            if (string.IsNullOrEmpty(Text) == false || string.IsNullOrEmpty(ExtraText) == false)
            {
                actualHeight += TextMargin.VerticalThickness;
            }
            if (string.IsNullOrEmpty(ExtraText) == false && string.IsNullOrEmpty(Text) == false)
            {
                actualHeight += TextLineSpacingRequest;
            }

            return new Size(Math.Max(textSize.Width, extraTextSize.Width), actualHeight);
        }

        #endregion

        #region Paint

        /// <summary>
        /// Paint visual
        /// </summary>
        protected void OnPaint(object sender, SKPaintSurfaceEventArgs e)
        {
            if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("Button.OnPaint: " + Text + " " + ExtraText); }

            if (Width <= 0 || Height <= 0)
            {
                return;
            }
            else if (_iconSize.IsZero && _contentSize.IsZero)
            {
                MeasureChildren(Width, Height);
            }

            // Cleaer surface always before any pant
            e.Surface.Canvas.Clear();

            // Get device pixel intencity scale
            DeviceScale = (float)(e.Info.Width / _skiaCanvas.Width);

            // Paint background
            OnPaintBackground(e, new Rectangle(0, 0, _skiaCanvas.Width, _skiaCanvas.Height));

            // Paint foreground
            OnPaintForeground(e, new Rectangle(Padding.Left + BorderThickness, Padding.Top + BorderThickness, _skiaCanvas.Width - _rightContentSize.Width - (BorderThickness * 2) - Padding.HorizontalThickness, _skiaCanvas.Height - (BorderThickness * 2) - Padding.VerticalThickness));
        }

        /// <summary>
        /// Paint background
        /// </summary>
        /// <param name="e">Skiasharp paint surface event args</param>
        /// <param name="availableSpace">Available space for paint in Xamarin forms coordinates</param>
        protected virtual void OnPaintBackground(SKPaintSurfaceEventArgs e, Rectangle availableSpace)
        {
            float maxCornerRadius = (float)Math.Min(availableSpace.Width / 2, availableSpace.Height / 2) * DeviceScale;
            float skCornerRadius = Math.Min(maxCornerRadius, (float)(CornerRadius * DeviceScale));
            float skBorderThickness = (float)BorderThickness * DeviceScale;

            SKRect skAvailableSpace = new SKRect(
                (float)availableSpace.X * DeviceScale,
                (float)availableSpace.Y * DeviceScale,
                (float)availableSpace.Width * DeviceScale,
                (float)availableSpace.Height * DeviceScale);

            SKRoundRect clipRect = new SKRoundRect(skAvailableSpace, skCornerRadius, skCornerRadius);
            e.Surface.Canvas.ClipRoundRect(clipRect);

            // Background

            SKPaint backgroundPaint = new SKPaint();
            backgroundPaint.Color = GetActualBackgroundColor().ToSKColor();
            backgroundPaint.IsAntialias = true;
            backgroundPaint.Style = SKPaintStyle.Fill;

            SKRect backgroundRect = skAvailableSpace;

            if (BorderThickness > 0)
            {
                backgroundRect.Inflate(-skBorderThickness / 2, -skBorderThickness / 2);
            }

            SKRoundRect backgroundRoundedRect = new SKRoundRect(backgroundRect, skCornerRadius, skCornerRadius);
            e.Surface.Canvas.DrawRoundRect(backgroundRoundedRect, backgroundPaint);

            if (AnimationStyle == AnimationStyles.Ellipse)
            {
                OnPaintEllipseAnimation(e);
            }

            // Border
            if (BorderThickness > 0)
            {
                SKPaint borderPaint = new SKPaint();
                borderPaint.Color = GetActualBorderColor().ToSKColor();
                borderPaint.IsAntialias = true;
                borderPaint.Style = SKPaintStyle.Stroke;
                borderPaint.StrokeWidth = skBorderThickness;

                SKRect borderRect = new SKRect(skAvailableSpace.Left, skAvailableSpace.Top, skAvailableSpace.Right, skAvailableSpace.Bottom);
                borderRect.Inflate(-skBorderThickness / 2, -skBorderThickness / 2);

                SKRoundRect borderRoundedRect = new SKRoundRect(borderRect, skCornerRadius, skCornerRadius);

                e.Surface.Canvas.DrawRoundRect(borderRoundedRect, borderPaint);
            }
        }

        /// <summary>
        /// Paint foreground
        /// </summary>
        /// <param name="e">Skiasharp paint surface event args</param>
        /// <param name="availableSpace">Available space for paint in Xamarin forms coordinates</param>
        protected virtual void OnPaintForeground(SKPaintSurfaceEventArgs e, Rectangle availableSpace)
        {
            if (Content == null)
            {
                SKCanvas canvas = e.Surface.Canvas;

                Rectangle actualAvailableSpace = GetAvailableLocation(availableSpace, _contentSize);

                Size actualIconSize = new Size();
                if (ActualIsIconVisible)
                {
                    OnPaintIcon(e, actualAvailableSpace);
                    actualIconSize = new Size(_iconSize.Width + IconMargin.HorizontalThickness, _iconSize.Height + IconMargin.VerticalThickness);
                }

                Rectangle contentAvailableSpace = new Rectangle();

                switch (IconPlacement)
                {
                    case ButtonIconPlacements.Left:
                        contentAvailableSpace = new Rectangle(
                            actualAvailableSpace.X + actualIconSize.Width,
                            actualAvailableSpace.Y,
                            actualAvailableSpace.Width - actualIconSize.Width,
                            actualAvailableSpace.Height);
                        break;
                    case ButtonIconPlacements.Right:
                        contentAvailableSpace = new Rectangle(
                            actualAvailableSpace.X,
                            actualAvailableSpace.Y,
                            actualAvailableSpace.Width - actualIconSize.Width,
                            actualAvailableSpace.Height);
                        break;
                    case ButtonIconPlacements.Top:
                        contentAvailableSpace = new Rectangle(
                            actualAvailableSpace.X,
                            actualAvailableSpace.Y + actualIconSize.Height,
                            actualAvailableSpace.Width - _rightContentSize.Width,
                            actualAvailableSpace.Height - actualIconSize.Height);
                        break;
                    case ButtonIconPlacements.Bottom:
                        contentAvailableSpace = new Rectangle(
                            actualAvailableSpace.X,
                            actualAvailableSpace.Y,
                            actualAvailableSpace.Width - _rightContentSize.Width,
                            actualAvailableSpace.Height - actualIconSize.Height);
                        break;
                }

                OnPaintDefaultText(e, contentAvailableSpace);
            }
        }

        /// <summary>
        /// Get available location for icon and default content
        /// </summary>
        private Rectangle GetAvailableLocation(Rectangle availableSpace, Size defaultContentSize)
        {
            Rectangle location = new Rectangle(availableSpace.X, availableSpace.Y, defaultContentSize.Width, defaultContentSize.Height);

            if (ContentHorizontalOptions.Alignment != LayoutAlignment.Fill)
            {
                if (ContentHorizontalOptions.Alignment == LayoutAlignment.Center)
                {
                    location.X = availableSpace.X + (availableSpace.Width - location.Width) / 2;
                }
                else if (ContentHorizontalOptions.Alignment == LayoutAlignment.End)
                {
                    location.X = availableSpace.Width - location.Width;
                }
            }

            if (ContentVerticalOptions.Alignment != LayoutAlignment.Fill)
            {
                if (ContentVerticalOptions.Alignment == LayoutAlignment.Center)
                {
                    location.Y = availableSpace.Y + (availableSpace.Height - location.Height) / 2;
                }
                else if (ContentVerticalOptions.Alignment == LayoutAlignment.End)
                {
                    location.Y = availableSpace.Height - location.Height;
                }
            }

            return location;
        }

        /// <summary>
        /// Paint icon
        /// </summary>
        /// <param name="e">E.</param>
        /// <param name="availableSpace">Available space.</param>
        protected void OnPaintIcon(SKPaintSurfaceEventArgs e, Rectangle availableSpace)
        {
            if (string.IsNullOrEmpty(IconResourceKey) || string.IsNullOrEmpty(IconAssemblyName))
            {
                return;
            }

            if (_svg == null)
            {
                _svg = SvgImage.GetSvgImage(IconAssemblyName, IconResourceKey);
            }

            SKPoint position = CalculateIconCoordinate(IconPlacement, availableSpace);
            float scale = SvgImage.CalculateScale(_svg.Picture.CullRect.Size, IconWidthRequest, IconHeightRequest) * DeviceScale;
            Color actualIconColor = GetActualIconColor();

            // For animation
            if (_iconAnimatedScale != 1)
            {
                scale = scale * (float)_iconAnimatedScale;
            }
            if (_iconAnimatedOpacity != 1)
            {
                actualIconColor = actualIconColor.MultiplyAlpha(_iconAnimatedOpacity);
            }
            if (_iconAnimatedRotation != 0)
            {
                e.Surface.Canvas.Save();
                SKPoint middle = new SKPoint(e.Info.Width / 2, e.Info.Height / 2);
                e.Surface.Canvas.RotateDegrees((float)_iconAnimatedRotation, middle.X, middle.Y);
            }

            SKPaint paint = new SKPaint();
            paint.ColorFilter = SKColorFilter.CreateBlendMode(actualIconColor.ToSKColor(), SKBlendMode.SrcIn);
            paint.Style = SKPaintStyle.Fill;
            paint.IsAntialias = true;

            SKMatrix matrix = new SKMatrix();
            matrix.SetScaleTranslate(scale, scale, position.X, position.Y);
            e.Surface.Canvas.DrawPicture(_svg.Picture, ref matrix, paint);

            if (_iconAnimatedRotation != 0)
            {
                e.Surface.Canvas.Restore();
            }
        }

        /// <summary>
        /// Calculate icon location in skia coordinates
        /// </summary>
        private SKPoint CalculateIconCoordinate(ButtonIconPlacements iconPlacement, Rectangle availableSpace)
        {
            Rectangle availableLocation = new Rectangle();

            switch (IconPlacement)
            {
                case ButtonIconPlacements.Left:
                    availableLocation = new Rectangle(
                        availableSpace.X + IconMargin.Left,
                        availableSpace.Y + IconMargin.Top, 
                        _iconSize.Width,
                        availableSpace.Height - IconMargin.VerticalThickness);
                    break;
                case ButtonIconPlacements.Right:
                    availableLocation = new Rectangle(
                        availableSpace.X + availableSpace.Width - _iconSize.Width - IconMargin.Right,
                        availableSpace.Y + IconMargin.Top, 
                        _iconSize.Width,
                        availableSpace.Height - IconMargin.VerticalThickness);
                    break;
                case ButtonIconPlacements.Top:
                    availableLocation = new Rectangle(
                        availableSpace.X + IconMargin.Left,
                        availableSpace.Y + IconMargin.Top,
                        availableSpace.Width - IconMargin.HorizontalThickness, 
                        _iconSize.Height);
                    break;
                case ButtonIconPlacements.Bottom:
                    availableLocation = new Rectangle(
                        availableSpace.X + IconMargin.Left,
                        availableSpace.Y + availableSpace.Height - _iconSize.Height - IconMargin.Bottom,
                        availableSpace.Width - IconMargin.HorizontalThickness, 
                        _iconSize.Height);
                    break;
            }

            double x = 0;
            double y = 0;

            switch (IconHorizontalOptions.Alignment)
            {
                case LayoutAlignment.Start:
                    x = availableLocation.X;
                    break;
                case LayoutAlignment.Fill:
                    x = availableLocation.X;
                    break;
                case LayoutAlignment.End:
                    x = availableLocation.Right - _iconSize.Width - IconMargin.Right;
                    break;
                case LayoutAlignment.Center:
                    x = availableLocation.X + (availableLocation.Width - _iconSize.Width) / 2;
                    break;
            }

            switch (IconVerticalOptions.Alignment)
            {
                case LayoutAlignment.Start:
                    y = availableLocation.Y;
                    break;
                case LayoutAlignment.Fill:
                    y = availableLocation.Y;
                    break;
                case LayoutAlignment.End:
                    y = availableLocation.Bottom - _iconSize.Height - IconMargin.Bottom;
                    break;
                case LayoutAlignment.Center:
                    y = availableLocation.Y + (availableLocation.Height - _iconSize.Height) / 2;
                    break;
            }

            return new SKPoint((float)x * DeviceScale, (float)y * DeviceScale);
        }

        /// <summary>
        /// Do icon change animations
        /// </summary>
        private void DoIconChangeAnimation()
        {
            AnimationExtensions.AbortAnimation(this, _iconChangeAnimationName);

            Animation anim = null;
            double previousProcessValue = 0;

            if (IconChangeAnimation == ButtonIconChangeAnimations.Rolling)
            {
                anim = new Animation(d =>
                {
                    if (previousProcessValue < 0.5 && d > 0.5)
                    {
                        _svg = null;
                    }

                    if (d < 0.5)
                    {
                        double actualProcess = d * 2;
                        double start = 0;
                        double target = 90;
                        _iconAnimatedRotation = start + (target - start) * actualProcess;
                    }
                    else
                    {
                        double actualProcess = (d - 0.5) * 2;
                        double start = -90;
                        double target = 0;
                        _iconAnimatedRotation = start + (target - start) * actualProcess;
                    }

                    previousProcessValue = d;

                    _skiaCanvas.InvalidateSurface();
                }, 0, 1);
            }
            else if (IconChangeAnimation == ButtonIconChangeAnimations.Scale)
            {
                anim = new Animation(d =>
                {
                    if (previousProcessValue < 0.5 && d > 0.5)
                    {
                        _svg = null;
                    }

                    if (d < 0.5)
                    {
                        double actualProcess = d * 2;
                        double start = 1;
                        double target = 0;
                        _iconAnimatedScale = start + (target - start) * actualProcess;
                    }
                    else
                    {
                        double actualProcess = (d - 0.5) * 2;
                        double start = 0;
                        double target = 1;
                        _iconAnimatedScale = start + (target - start) * actualProcess;
                    }

                    previousProcessValue = d;

                    _skiaCanvas.InvalidateSurface();
                }, 0, 1);
            }
            else if (IconChangeAnimation == ButtonIconChangeAnimations.Fade)
            {
                anim = new Animation(d =>
                {
                    if (previousProcessValue < 0.5 && d > 0.5)
                    {
                        _svg = null;
                    }

                    if (d < 0.5)
                    {
                        double actualProcess = d * 2;
                        double start = 1;
                        double target = 0;
                        _iconAnimatedOpacity = start + (target - start) * actualProcess;
                    }
                    else
                    {
                        double actualProcess = (d - 0.5) * 2;
                        double start = 0;
                        double target = 1;
                        _iconAnimatedOpacity = start + (target - start) * actualProcess;
                    }

                    previousProcessValue = d;

                    _skiaCanvas.InvalidateSurface();
                }, 0, 1);
            }
            else
            {
                throw new NotImplementedException();
            }

            anim.Commit(this, _iconChangeAnimationName, 64, (uint)IconChangeAnimationDuration, IconChangeAnimationEasing);
        }

        /// <summary>
        /// Paint default text content
        /// </summary>
        protected void OnPaintDefaultText(SKPaintSurfaceEventArgs e, Rectangle availableSpace)
        {
            bool hasRows = string.IsNullOrEmpty(Text) == false && string.IsNullOrEmpty(ExtraText) == false;

            // Measure Text
            Size skTextSize = new Size(); // actual size
            SKRect skTextBounds = new SKRect(); // take x and y for location

            float skFontSize = (float)FontSize * DeviceScale;
            float skTextLineHeight = skFontSize * _textLineHeightMultiplier;

            SKPaint textPaint = CreateTextPaint();
            textPaint.TextSize = skFontSize;

            if (string.IsNullOrEmpty(Text) == false)
            {
                textPaint.MeasureText(Text, ref skTextBounds);

                if (IsTextWrapping)
                {
                    skTextSize = SkiaUtils.MeasureText(textPaint, (float)(availableSpace.Width - TextMargin.HorizontalThickness) * DeviceScale, skTextLineHeight, IsTextWrapping, IsTextUpper ? Text.ToUpper() : Text);
                    
                    // Remove extra line height from last line
                    skTextSize.Height = skTextSize.Height - skTextLineHeight + skFontSize;
                }
                else
                {
                    skTextSize.Height = skTextBounds.Height;
                    skTextSize.Width = skTextBounds.Width;
                }
            }

            // Measure ExtraText

            Size skExtraTextSize = new Size(); // actual size
            SKRect skExtraTextBounds = new SKRect(); // take x and y for location

            float skExtraTextFontSize = (float)ExtraTextFontSize * DeviceScale;
            float skExtraTextLineHeight = skExtraTextFontSize * _extraTextLineHeightMultiplier;

            SKPaint extraTextPaint = CreateExtraTextPaint();
            extraTextPaint.TextSize = skExtraTextFontSize;

            if (string.IsNullOrEmpty(ExtraText) == false)
            {
                extraTextPaint.MeasureText(ExtraText, ref skExtraTextBounds);

                if (IsExtraTextWrapping)
                {
                    skExtraTextSize = SkiaUtils.MeasureText(extraTextPaint, (float)availableSpace.Width * DeviceScale, skExtraTextLineHeight, IsExtraTextWrapping, IsExtraTextUpper ? ExtraText.ToUpper() : ExtraText);

                    // Remove extra line height from last line
                    skExtraTextSize.Height = skExtraTextSize.Height - skExtraTextLineHeight + skExtraTextFontSize;
                }
                else
                {
                    skExtraTextSize.Height = skTextBounds.Height;
                    skExtraTextSize.Width = skTextBounds.Width;
                }
            }

            SKRect skAvailableSpace = new SKRect();
            skAvailableSpace.Left = (float)(availableSpace.X + TextMargin.Left) * DeviceScale;
            skAvailableSpace.Top = (float)(availableSpace.Y + TextMargin.Top) * DeviceScale;
            skAvailableSpace.Right = skAvailableSpace.Left + ((float)(availableSpace.Width - TextMargin.HorizontalThickness)) * DeviceScale;
            skAvailableSpace.Bottom = skAvailableSpace.Top + ((float)(availableSpace.Height - TextMargin.VerticalThickness)) * DeviceScale;
                
            float skTextLineSpacingRequest = hasRows ? (float)TextLineSpacingRequest * DeviceScale : 0;

            // Text skia coordinates
            float skTextX = 0;
            float skTextY = 0;

            if (string.IsNullOrEmpty(Text) == false)
            {
                // X
                switch (TextAlignment)
                {
                    case TextAlignments.Center:
                        skTextX = skAvailableSpace.Left + (skAvailableSpace.Width - (float)skTextSize.Width) / 2;
                        break;
                    case TextAlignments.Right:
                        skTextX = skAvailableSpace.Right - (float)skTextSize.Width;
                        break;
                    case TextAlignments.Left:
                    default:
                        skTextX = skAvailableSpace.Left;
                        break;
                }

                // Y
                switch (ContentVerticalOptions.Alignment)
                {
                    case LayoutAlignment.Center:
                        if (string.IsNullOrEmpty(ExtraText))
                        {
                            skTextY = skAvailableSpace.Top + (skAvailableSpace.Height - (float)skTextSize.Height) / 2;
                        }
                        else
                        {
                            skTextY = skAvailableSpace.Top + (skAvailableSpace.Height - (float)skTextSize.Height - (float)skExtraTextSize.Height - skTextLineSpacingRequest) / 2;
                        }
                        break;
                    case LayoutAlignment.End:
                        skTextY = skAvailableSpace.Height - (float)skTextSize.Height - (float)skExtraTextSize.Height - skTextLineSpacingRequest;
                        break;
                    case LayoutAlignment.Start:
                    case LayoutAlignment.Fill:
                    default:
                        skTextY = skAvailableSpace.Top;
                        break;
                }

                SkiaUtils.DrawTextArea(e.Surface.Canvas, textPaint, skTextX - skTextBounds.Left, skTextY - skTextBounds.Top, skAvailableSpace.Width, skTextLineHeight, IsTextWrapping, IsTextUpper ? Text.ToUpper() : Text);
            }

            // ExtraText

            if (string.IsNullOrEmpty(ExtraText) == false)
            {
                // ExtraText skia coordinates
                float skExtraTextX = 0;
                float skExtraTextY = 0;

                // X
                switch (TextAlignment)
                {
                    case TextAlignments.Center:
                        skExtraTextX = skAvailableSpace.Left + (skAvailableSpace.Width - (float)skExtraTextSize.Width) / 2;
                        break;
                    case TextAlignments.Right:
                        skExtraTextX = skAvailableSpace.Right - (float)skExtraTextSize.Width;
                        break;
                    case TextAlignments.Left:
                    default:
                        skExtraTextX = skAvailableSpace.Left;
                        break;
                }

                switch (ContentVerticalOptions.Alignment)
                {
                    case LayoutAlignment.Center:
                        if (string.IsNullOrEmpty(Text))
                        {
                            skExtraTextY = skAvailableSpace.Top + (skAvailableSpace.Height - (float)skExtraTextSize.Height) / 2;
                        }
                        else
                        {
                            skExtraTextY = skAvailableSpace.Top + (skAvailableSpace.Height - (float)skTextSize.Height - (float)skExtraTextSize.Height - skTextLineSpacingRequest) / 2 + (float)skTextSize.Height + skTextLineSpacingRequest;
                        }
                        break;
                    case LayoutAlignment.End:
                        skTextY = skAvailableSpace.Height - (float)skExtraTextSize.Height;
                        break;
                    case LayoutAlignment.Start:
                    case LayoutAlignment.Fill:
                    default:
                        skExtraTextY = skAvailableSpace.Height + (float)skTextSize.Height;
                        break;
                }

                SkiaUtils.DrawTextArea(e.Surface.Canvas, extraTextPaint, skExtraTextX - skExtraTextBounds.Left, skExtraTextY - skExtraTextBounds.Top, skAvailableSpace.Width, skExtraTextLineHeight, IsExtraTextWrapping, IsExtraTextUpper ? ExtraText.ToUpper() : ExtraText);
            }
        }

        private SKPaint CreateTextPaint()
        {
            SKPaint paint = new SKPaint();
            paint.Color = GetActualTextColor().ToSKColor();
            paint.IsAntialias = true;
            paint.TextAlign = SKTextAlign.Left;

            SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(TextFontStyle);
            SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(TextFontWeight);
            paint.Typeface = SKTypeface.FromFamilyName(TextFontFamily, fontWeight, SKFontStyleWidth.Normal, slant);

            return paint;
        }

        private SKPaint CreateExtraTextPaint()
        {
            SKPaint paint = new SKPaint();
            paint.Color = GetActualExtraTextColor().ToSKColor();
            paint.IsAntialias = true;
            paint.TextAlign = SKTextAlign.Left;

            SKFontStyleSlant slant = SkiaUtils.ConvertToSKFontStyle(ExtraTextFontStyle);
            SKFontStyleWeight fontWeight = SkiaUtils.ConvertToSKFontWeight(ExtraTextFontWeight);
            paint.Typeface = SKTypeface.FromFamilyName(ExtraTextFontFamily, fontWeight, SKFontStyleWidth.Normal, slant);

            return paint;
        }

        #endregion

        #region Interaction

        protected virtual void OnPressed()
        {
            return;
        }

        protected virtual void OnReleased()
        {
            return;
        }

        protected virtual void OnMouseEnter()
        {
            return;
        }

        protected virtual void OnMouseLeave()
        {
            return;
        }

        protected virtual void OnCancelled()
        {
            return;
        }

        protected virtual void OnTouchStarted(TouchActionEventArgs args)
        {
            return;
        }

        private void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            OnTouchStarted(args);

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    _mousePosition = args.Point;
                    OnPressed(args);
                    break;
                case TouchActionType.Released:
                    if (IsPressed)
                    {
                        OnReleased(args);
                    }
                    break;
                case TouchActionType.Entered:
                    OnIsMouseOver(args, true);
                    break;
                case TouchActionType.Cancelled:
                case TouchActionType.Exited:
                    if (IsMouseOver)
                    {
                        OnIsMouseOver(args, false);
                    }
                    if (IsPressed)
                    {
                        OnCancelled(args);
                    }
                    break;
                    /*
                case TouchActionType.Cancelled:
                    OnCancelled(args);
                    break;
                    */
            }
        }

        private void OnPressed(TouchActionEventArgs args)
        {
            VisualStateManager.GoToState(this, PressedStateName);
            IsPressed = true;

            OnPressed();

            if (AnimationStyle != AnimationStyles.Disabled)
            {
                _normalizedAnimationProcess = 0;
                DoPressedAnimation(_normalizedAnimationProcess, 0.5);
            }

            if (IsPressedChanged != null)
            {
                IsPressedChanged(this, true);
            }
        }

        private void OnReleased(TouchActionEventArgs args)
        {
            VisualStateManager.GoToState(this, DefaultStateName);
            IsPressed = false;

            OnReleased();

            Action action = new Action(() =>
            {
                OnTapped();

                if (IsPressedChanged != null)
                {
                    IsPressedChanged(this, false);
                }
            });

            if (AnimationStyle != AnimationStyles.Disabled)
            {
                if (CommandExecuteEvent == CommandExecuteEvents.MiddleAnimation)
                {
                    DoPressedAnimation(_normalizedAnimationProcess, 1, action);
                }
                else if (CommandExecuteEvent == CommandExecuteEvents.AfterAnimation)
                {
                    DoPressedAnimation(_normalizedAnimationProcess, 1, null, action);
                }
                else
                {
                    DoPressedAnimation(_normalizedAnimationProcess, 1);
                    action.Invoke();
                }
            }
            else
            {
                action.Invoke();
            }
        }

        private void OnIsMouseOver(TouchActionEventArgs args, bool isOver)
        {
            IsMouseOver = isOver;

            if (isOver)
            {
                VisualStateManager.GoToState(this, MouseEnterStateName);

                OnMouseEnter();

                if (AnimationStyle != AnimationStyles.Disabled)
                {
                    DoHoverAnimation(_hoverAnimationProcess, 1);
                }
            }
            else
            {
                VisualStateManager.GoToState(this, MouseLeaveStateName);

                OnMouseLeave();

                if (AnimationStyle != AnimationStyles.Disabled)
                {
                    DoHoverAnimation(_hoverAnimationProcess, 0);
                }
            }
        }

        private void OnCancelled(TouchActionEventArgs args)
        {
            VisualStateManager.GoToState(this, DefaultStateName);
            IsPressed = false;

            if (AnimationStyle != AnimationStyles.Disabled)
            {
                DoPressedAnimation(_normalizedAnimationProcess, 1);
            }

            if (IsPressedChanged != null)
            {
                IsPressedChanged(this, false);
            }

            OnCancelled();
        }

        private void OnTappedGesture(object sender, EventArgs e)
        {
            OnTapped();
        }

        /// <summary>
        /// Execute commands and events for tapping
        /// </summary>
        protected virtual void OnTapped()
        {
            if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("Button.OnTapped"); }

            if (Command != null)
            {
                Command.Execute(CommandParameter);
            }

            RaiseTapped();
        }

        /// <summary>
        /// Raise Tapped event if it has listeners
        /// </summary>
        protected void RaiseTapped()
        {
            if (Tapped != null)
            {
                Tapped(this, new EventArgs());
            }
        }

        #endregion

        #region State transition animations

        protected void DoHoverAnimation(double start, double end)
        {
            this.AbortAnimation(_hoverAnimationName);

            new Animation(d =>
            {
                _hoverAnimationProcess = d;
                _skiaCanvas.InvalidateSurface();
            }, start, end).Commit(this, _hoverAnimationName, 64, (uint)HoverAnimationDuration, HoverAnimationEasing);
        }

        /// <summary>
        /// Start visual state change animation
        /// </summary>
        protected async void DoPressedAnimation(double start = 0, double end = 1, Action middle = null, Action finished = null)
        {
            this.AbortAnimation(_pressedAnimationName);
            AnimationExtensions.AbortAnimation(this, _pressedAnimationName);

            this.AbortAnimation(_releasedAnimationName);
            AnimationExtensions.AbortAnimation(this, _releasedAnimationName);

            if (start < 0.5)
            {

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                new Animation(d =>
                {
                    _normalizedAnimationProcess = d;
                    _pressedAnimationProcess = PressedAnimationEasing != null ? PressedAnimationEasing.Ease(d) : d;
                    _skiaCanvas.InvalidateSurface();
                }, start, 0.5)
                .Commit(this, _pressedAnimationName, 64, (uint)PressedAnimationDuration, Easing.Linear, finished: (d, a) =>
                {
                    if (a == false && IsPressed == false)
                    {
                        tcs.SetResult(true);
                    }
                    else
                    {
                        tcs.SetResult(false);
                    }
                });

                bool startAnim = await tcs.Task;

                if (startAnim)
                {
                    if (middle != null)
                    {
                        middle?.Invoke();

                        await Task.Delay(30);
                    }

                    new Animation(k =>
                    {
                        _normalizedAnimationProcess = k;
                        _pressedAnimationProcess = ReleasedAnimationEasing != null ? ReleasedAnimationEasing.Ease(k) : k;
                        _skiaCanvas.InvalidateSurface();
                    }, 0.5, end)
                    .Commit(this, _releasedAnimationName, 64, (uint)ReleasedAnimationDuration, Easing.Linear, (j, b) =>
                    {
                        if (finished != null && b == false)
                        {
                            finished();
                        }

                        _pressedAnimationProcess = 0;

                        if (IsMouseOver == false || IsVisible == false || InputTransparent == true)
                        {
                            _hoverAnimationProcess = 0;
                        }

                        _skiaCanvas.InvalidateSurface();
                    });
                }
            }
            else
            {
                if (middle != null)
                {
                    middle();
                }

                new Animation(d =>
                {
                    _normalizedAnimationProcess = d;
                    _pressedAnimationProcess = ReleasedAnimationEasing != null ? ReleasedAnimationEasing.Ease(d) : d;
                    _skiaCanvas.InvalidateSurface();
                }, start, end).Commit(this, _releasedAnimationName, 64, (uint)ReleasedAnimationDuration, Easing.Linear, (d, isAborted) =>
                {
                    if (finished != null && isAborted == false)
                    {
                        finished();
                    }
                    _pressedAnimationProcess = 0;

                    if (IsMouseOver == false || IsVisible == false || InputTransparent == true)
                    {
                        _hoverAnimationProcess = 0;
                    }

                    _skiaCanvas.InvalidateSurface();
                });
            }
        }

        /// <summary>
        /// Get icon color
        /// </summary>
        protected virtual Color GetActualIconColor()
        {
            return GetColor(IconColor, IconHoverColor, IconPressedColor, IconDisabledColor);
        }

        /// <summary>
        /// Get text color
        /// </summary>
        protected virtual Color GetActualTextColor()
        {
            return GetColor(TextColor, TextHoverColor, TextPressedColor, TextDisabledColor);
        }

        /// <summary>
        /// Get extra text color
        /// </summary>
        protected virtual Color GetActualExtraTextColor()
        {
            return GetColor(ExtraTextColor, ExtraTextHoverColor, ExtraTextPressedColor, ExtraTextDisabledColor);
        }

        /// <summary>
        /// Get background color
        /// </summary>
        protected virtual Color GetActualBackgroundColor()
        {
            if (AnimationStyle == AnimationStyles.Ellipse)
            {
                if (IsEnabled)
                {
                    return AnimationUtils.ColorTransform(_hoverAnimationProcess, BackgroundColor, BackgroundHoverColor);
                }
                else
                {
                    return BackgroundDisabledColor;
                }
            }
            else
            {
                return GetColor(BackgroundColor, BackgroundHoverColor, BackgroundPressedColor, BackgroundDisabledColor);
            }
        }

        /// <summary>
        /// Get border color
        /// </summary>
        protected virtual Color GetActualBorderColor()
        {
            return GetColor(BorderColor, BorderHoverColor, BorderPressedColor, BorderDisabledColor);
        }

        /// <summary>
        /// Get actual ellipse background color
        /// </summary>
        protected virtual Color GetActualEllipseColor()
        {
            return BackgroundPressedColor;            
        }

        /// <summary>
        /// Get color based on IsPressed and animation process.
        /// </summary>
        protected Color GetColor(Color defaultColor, Color hoverColor, Color pressedColor, Color disabledColor)
        {
            if (IsEnabled == false)
            {
                return disabledColor;
            }
            else
            {
                if (_pressedAnimationProcess < 1 || _hoverAnimationProcess < 1)
                {
                    // Add toggled color shade
                    Color color = defaultColor;

                    // Add pressed and hover color shade depending on is button toggled
                    color = AnimationUtils.ColorTransform(_hoverAnimationProcess, color, hoverColor);

                    if (_pressedAnimationProcess <= 0.5)
                    {
                        color = AnimationUtils.ColorTransform(_pressedAnimationProcess * 2, color, pressedColor);
                    }
                    else
                    {
                        color = AnimationUtils.ColorTransform((_pressedAnimationProcess - 0.5) * 2, pressedColor, color);
                    }

                    return color;
                }
                else
                {
                    if (IsPressed)
                    {
                        return pressedColor;
                    }
                    else if (IsMouseOver)
                    {
                        return hoverColor;
                    }
                    else
                    {
                        return defaultColor;
                    }
                }
            }
        }

        /// <summary>
        /// Ons the paint background animation.
        /// </summary>
        /// <param name="e">E.</param>
        private void OnPaintEllipseAnimation(SKPaintSurfaceEventArgs e)
        {
            if (_pressedAnimationProcess.Equals(0))
            {
                return;
            }

            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            float skWidth = (float)(_skiaCanvas.Width * DeviceScale);
            float skHeight = (float)(_skiaCanvas.Height * DeviceScale);

            double skWidthFromMouse = Math.Max(skWidth - (_mousePosition.X * DeviceScale), _mousePosition.X * DeviceScale);
            double skHeightFromMouse = Math.Max(skHeight - (_mousePosition.Y * DeviceScale), _mousePosition.Y * DeviceScale);
            SKPoint skMousePosition = new SKPoint((float)_mousePosition.X * DeviceScale, (float)_mousePosition.Y * DeviceScale);

            float skMaxRadius = (float)Math.Sqrt(Math.Pow((double)skWidthFromMouse, 2) + Math.Pow((double)skHeightFromMouse, 2));
            float skRadius = 0;

            Color actualColor = GetActualEllipseColor();

            if (_pressedAnimationProcess <= 0.5)
            {
                skRadius = (skMaxRadius / 6) + ((float)(_pressedAnimationProcess * 2) * (skMaxRadius - (skMaxRadius / 6)));
                actualColor = actualColor.MultiplyAlpha(_pressedAnimationProcess * 4);
            }
            else
            {
                actualColor = actualColor.MultiplyAlpha(1 - ((_pressedAnimationProcess - 0.5) * 2));
                skRadius = skMaxRadius;
            }

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = actualColor.ToSKColor(),
            };

            canvas.DrawCircle(skMousePosition.X, skMousePosition.Y, skRadius, paint);
        }

        #endregion

        #region Properties changes

        /// <summary>
        /// Called when any binding property value changes
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == ContentHorizontalOptionsProperty.PropertyName ||
                propertyName == ContentVerticalOptionsProperty.PropertyName)
            {
                if (_actualContentElement != null)
                {
                    _actualContentElement.HorizontalOptions = ContentHorizontalOptions;
                    _actualContentElement.VerticalOptions = ContentVerticalOptions;
                }
                else
                {
                    InvalidateMeasure();
                    InvalidateLayout();

                    if (_skiaCanvas != null)
                    {
                        _skiaCanvas.InvalidateSurface();
                    }
                }
            }
            else if (propertyName == IsMouseOverProperty.PropertyName)
            {
                IsMouseOverChanged?.Invoke(this, IsMouseOver);
            }
            else if (propertyName == WidthProperty.PropertyName || propertyName == HeightProperty.PropertyName)
            {
                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            /*
            else if (propertyName == BackgroundColorProperty.PropertyName ||
                     propertyName == BorderColorProperty.PropertyName)
            {
                if (m_skiaCanvas != null)
                {
                    m_skiaCanvas.InvalidateSurface();
                }

                base.OnPropertyChanged(propertyName);
            }
            */
            // For debug
            /*
            else if (propertyName == IsPressedProperty.PropertyName)
            {
                InvalidateMeasure();
                InvalidateLayout();

                if (m_skiaCanvas != null)
                {
                    m_skiaCanvas.InvalidateSurface();
                }
            }
            */
            else if (propertyName == IconAssemblyNameProperty.PropertyName || 
                     propertyName == IconResourceKeyProperty.PropertyName)
            {
                if (_svg != null && IconChangeAnimation != ButtonIconChangeAnimations.None)
                {
                    DoIconChangeAnimation();
                }
                else
                {
                    _svg = null;

                    InvalidateMeasure();
                    InvalidateLayout();

                    if (_skiaCanvas != null)
                    {
                        _skiaCanvas.InvalidateSurface();
                    }
                }
            }
            else if (propertyName == TextProperty.PropertyName ||
                     propertyName == FontSizeProperty.PropertyName ||
                     propertyName == TextFontFamilyProperty.PropertyName ||
                     propertyName == IsTextWrappingProperty.PropertyName ||
                     propertyName == TextFontStyleProperty.PropertyName ||
                     propertyName == TextFontWeightProperty.PropertyName ||
                     propertyName == TextMarginProperty.PropertyName ||
                     propertyName == TextLineSpacingRequestProperty.PropertyName ||
                     propertyName == ExtraTextProperty.PropertyName ||
                     propertyName == ExtraTextFontSizeProperty.PropertyName ||
                     propertyName == IsExtraTextWrappingProperty.PropertyName ||
                     propertyName == ExtraTextFontFamilyProperty.PropertyName ||
                     propertyName == ExtraTextFontStyleProperty.PropertyName ||
                     propertyName == ExtraTextFontWeightProperty.PropertyName ||
                     propertyName == IconPlacementProperty.PropertyName ||
                     propertyName == IconHorizontalOptionsProperty.PropertyName ||
                     propertyName == IconVerticalOptionsProperty.PropertyName ||
                     propertyName == IconMarginProperty.PropertyName ||
                     propertyName == IconHeightRequestProperty.PropertyName ||
                     propertyName == IconWidthRequestProperty.PropertyName ||
                     propertyName == IsIconVisibleProperty.PropertyName ||
                     propertyName == IsTextUpperProperty.PropertyName ||
                     propertyName == IsExtraTextUpperProperty.PropertyName)
            {
                InvalidateMeasure();
                InvalidateLayout();

                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName == TextColorProperty.PropertyName ||
                     propertyName == ExtraTextColorProperty.PropertyName ||
                     propertyName == IconColorProperty.PropertyName ||
                     propertyName == BorderColorProperty.PropertyName ||
                     propertyName == BackgroundColorProperty.PropertyName)
            {
                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName == RightContentProperty.PropertyName)
            {
                if (RightContent is View)
                {
                    if (_rightContent != null && Children.Contains(_rightContent))
                    {
                        Children.Remove(_rightContent);
                    }

                    _rightContent = RightContent as View;
                    Children.Add(_rightContent);
                }
                else
                {
                    if (_rightContent != null)
                    {
                        _rightContent.BindingContext = RightContent;
                    }
                }
            }
            else if (propertyName == RightContentTemplateProperty.PropertyName)
            {
                // If RightContent is View then ignore this
                if (RightContent is View == false)
                {
                    if (_rightContent != null && Children.Contains(_rightContent))
                    {
                        Children.Remove(_rightContent);
                    }

                    if (RightContentTemplate != null)
                    {
                        _rightContent = RightContentTemplate.CreateContent() as View;

                        Children.Add(_rightContent);
                    }
                    else
                    {
                        _rightContent = null;
                    }
                }
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

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
    }

    /// <summary>
    /// Button text alignments
    /// </summary>
    public enum TextAlignments { Left, Center, Right }

    /// <summary>
    /// How button states transitions are animated
    /// </summary>
    public enum AnimationStyles { Default, Ellipse, Disabled }

    /// <summary>
    /// Animations for button icon changes
    /// </summary>
    public enum ButtonIconChangeAnimations { None, Scale, Rolling, Fade }

    /// <summary>
    /// Button icon placement on available space
    /// </summary>
    public enum ButtonIconPlacements
    {
        Left,
        Top,
        Right,
        Bottom
    }
}
