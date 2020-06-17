using System;
using System.Threading.Tasks;
using System.Windows.Input;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// TextIconToggleButton wrapper class for xamarin forms
    /// </summary>
    public class ToggleButton : Button, IToggable
    {
        public event EventHandler<bool> IsToggledChanged;

        public const string ToggledStateName = "Toggled";

        protected double _toggledAnimationProcess = 0;
        protected string _toggledAnimationName = "toggledAnimationName";

        #region Properties

        /// <summary>
        /// Executed when button is toggled
        /// </summary>
        public static readonly BindableProperty ToggledCommandProperty =
            BindableProperty.Create("ToggledCommand", typeof(ICommand), typeof(ToggleButton), null);

        public ICommand ToggledCommand
        {
            get { return (ICommand)GetValue(ToggledCommandProperty); }
            set { SetValue(ToggledCommandProperty, value); }
        }

        /// <summary>
        /// Executed when button is untoggled
        /// </summary>
        public static readonly BindableProperty UnToggledCommandProperty =
            BindableProperty.Create("UnToggledCommand", typeof(ICommand), typeof(ToggleButton), null);

        public ICommand UnToggledCommand
        {
            get { return (ICommand)GetValue(UnToggledCommandProperty); }
            set { SetValue(UnToggledCommandProperty, value); }
        }

        /// <summary>
        /// Is button toggled
        /// </summary>
        public static readonly BindableProperty IsToggledProperty =
            BindableProperty.Create("IsToggled", typeof(bool), typeof(ToggleButton), false, propertyChanged: OnIsToggledChanged);

        static void OnIsToggledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((ToggleButton)bindable).OnIsToggledChangedInternal((bool)newValue);
        }

        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); }
        }

        /// <summary>
        /// Text when button is toggled (overrides 'Text' property)
        /// </summary>
        public static readonly BindableProperty ToggledTextProperty =
            BindableProperty.Create("ToggledText", typeof(string), typeof(ToggleButton), null, propertyChanged: OnToggledTextChanged);

        static void OnToggledTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((ToggleButton)bindable).OnToggledTextChanged(oldValue as string, newValue as string);
        }

        public string ToggledText
        {
            get { return (string)GetValue(ToggledTextProperty); }
            set { SetValue(ToggledTextProperty, value); }
        }

        /// <summary>
        /// Text when button is untoggled (overrides 'Text' property)
        /// </summary>
        public static readonly BindableProperty UnToggledTextProperty =
            BindableProperty.Create("UnToggledText", typeof(string), typeof(ToggleButton), null, propertyChanged: OnUnToggledTextChanged);

        static void OnUnToggledTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((ToggleButton)bindable).OnUnToggledTextChanged(oldValue as string, newValue as string);
        }

        public string UnToggledText
        {
            get { return (string)GetValue(UnToggledTextProperty); }
            set { SetValue(UnToggledTextProperty, value); }
        }

        #endregion

        #region Colors

        // Default Toggled

        /// <summary>
        /// Toggled text color
        /// </summary>
        public static readonly BindableProperty ToggledTextColorProperty =
            BindableProperty.Create("ToggledTextColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledTextColor
        {
            get { return (Color)GetValue(ToggledTextColorProperty); }
            set { SetValue(ToggledTextColorProperty, value); }
        }

        /// <summary>
        /// Toggled bottom text color
        /// </summary>
        public static readonly BindableProperty ToggledExtraTextColorProperty =
            BindableProperty.Create("ToggledExtraTextColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledExtraTextColor
        {
            get { return (Color)GetValue(ToggledExtraTextColorProperty); }
            set { SetValue(ToggledExtraTextColorProperty, value); }
        }

        /// <summary>
        /// Toggled icon color
        /// </summary>
        public static readonly BindableProperty ToggledIconColorProperty =
            BindableProperty.Create("ToggledIconColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledIconColor
        {
            get { return (Color)GetValue(ToggledIconColorProperty); }
            set { SetValue(ToggledIconColorProperty, value); }
        }

        /// <summary>
        /// Toggled border color
        /// </summary>
        public static readonly BindableProperty ToggledBorderColorProperty =
            BindableProperty.Create("ToggledBorderColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledBorderColor
        {
            get { return (Color)GetValue(ToggledBorderColorProperty); }
            set { SetValue(ToggledBorderColorProperty, value); }
        }

        /// <summary>
        /// Toggled background color
        /// </summary>
        public static readonly BindableProperty ToggledBackgroundColorProperty =
            BindableProperty.Create("ToggledBackgroundColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledBackgroundColor
        {
            get { return (Color)GetValue(ToggledBackgroundColorProperty); }
            set { SetValue(ToggledBackgroundColorProperty, value); }
        }

        // Toggled Hover

        /// <summary>
        /// Text hover color
        /// </summary>
        public static readonly BindableProperty ToggledTextHoverColorProperty =
            BindableProperty.Create("ToggledTextHoverColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledTextHoverColor
        {
            get { return (Color)GetValue(ToggledTextHoverColorProperty); }
            set { SetValue(ToggledTextHoverColorProperty, value); }
        }

        /// <summary>
        /// Toggled bottom text hover color
        /// </summary>
        public static readonly BindableProperty ToggledExtraTextHoverColorProperty =
            BindableProperty.Create("ToggledExtraTextHoverColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledExtraTextHoverColor
        {
            get { return (Color)GetValue(ToggledExtraTextHoverColorProperty); }
            set { SetValue(ToggledExtraTextHoverColorProperty, value); }
        }

        /// <summary>
        /// Toggled icon hover color
        /// </summary>
        public static readonly BindableProperty ToggledIconHoverColorProperty =
            BindableProperty.Create("ToggledIconHoverColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledIconHoverColor
        {
            get { return (Color)GetValue(ToggledIconHoverColorProperty); }
            set { SetValue(ToggledIconHoverColorProperty, value); }
        }

        /// <summary>
        /// Toggled border hover color
        /// </summary>
        public static readonly BindableProperty ToggledBorderHoverColorProperty =
            BindableProperty.Create("ToggledBorderHoverColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledBorderHoverColor
        {
            get { return (Color)GetValue(ToggledBorderHoverColorProperty); }
            set { SetValue(ToggledBorderHoverColorProperty, value); }
        }

        /// <summary>
        /// Toggled background hover color
        /// </summary>
        public static readonly BindableProperty ToggledBackgroundHoverColorProperty =
            BindableProperty.Create("ToggledBackgroundHoverColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledBackgroundHoverColor
        {
            get { return (Color)GetValue(ToggledBackgroundHoverColorProperty); }
            set { SetValue(ToggledBackgroundHoverColorProperty, value); }
        }

        // Toggled pressed

        /// <summary>
        /// Text pressed color
        /// </summary>
        public static readonly BindableProperty ToggledTextPressedColorProperty =
            BindableProperty.Create("ToggledTextPressedColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledTextPressedColor
        {
            get { return (Color)GetValue(ToggledTextPressedColorProperty); }
            set { SetValue(ToggledTextPressedColorProperty, value); }
        }

        /// <summary>
        /// Toggled bottom text pressed color
        /// </summary>
        public static readonly BindableProperty ToggledExtraTextPressedColorProperty =
            BindableProperty.Create("ToggledExtraTextPressedColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledExtraTextPressedColor
        {
            get { return (Color)GetValue(ToggledExtraTextPressedColorProperty); }
            set { SetValue(ToggledExtraTextPressedColorProperty, value); }
        }

        /// <summary>
        /// Toggled icon pressed color
        /// </summary>
        public static readonly BindableProperty ToggledIconPressedColorProperty =
            BindableProperty.Create("ToggledIconPressedColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledIconPressedColor
        {
            get { return (Color)GetValue(ToggledIconPressedColorProperty); }
            set { SetValue(ToggledIconPressedColorProperty, value); }
        }

        /// <summary>
        /// Toggled border pressed color
        /// </summary>
        public static readonly BindableProperty ToggledBorderPressedColorProperty =
            BindableProperty.Create("ToggledBorderPressedColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledBorderPressedColor
        {
            get { return (Color)GetValue(ToggledBorderPressedColorProperty); }
            set { SetValue(ToggledBorderPressedColorProperty, value); }
        }

        /// <summary>
        /// Toggled background pressed color
        /// </summary>
        public static readonly BindableProperty ToggledBackgroundPressedColorProperty =
            BindableProperty.Create("ToggledBackgroundPressedColor", typeof(Color), typeof(ToggleButton), Color.Transparent);

        public Color ToggledBackgroundPressedColor
        {
            get { return (Color)GetValue(ToggledBackgroundPressedColorProperty); }
            set { SetValue(ToggledBackgroundPressedColorProperty, value); }
        }

        #endregion

        public ToggleButton()
        {
        }

        /// <summary>
        /// Called when button is tapped
        /// </summary>
        protected override void OnTapped()
        {
            if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("ToggleButton.OnTapped"); }

            if (Command != null)
            {
                // Execute command with parameter
                if (CommandParameter != null)
                {
                    Command.Execute(CommandParameter);
                }
                else
                {
                    Command.Execute(IsToggled);
                }            
            }

            RaiseTapped();
        }

        /// <summary>
        /// Event when interaction is released
        /// </summary>
        protected override void OnReleased()
        {
            base.OnReleased();

            IsToggled = !IsToggled;

            DoToggleAnimation(IsToggled);
        }

        /// <summary>
        /// Do toggle animation
        /// </summary>
        protected void DoToggleAnimation(bool isToggled, Action<double, bool> finished = null)
        {
            AnimationExtensions.AbortAnimation(this, _toggledAnimationName);

            double start = 0;
            double to = 0;

            if (isToggled)
            {
                start = 0;
                to = 1;
            }
            else
            {
                start = 1;
                to = 0;
            }

            new Animation((d) =>
            {
                _toggledAnimationProcess = d;
                _skiaCanvas.InvalidateSurface();
            }, start, to).Commit(this, _toggledAnimationName, 64, (uint)PressedAnimationDuration, PressedAnimationEasing, finished);
        }

        /// <summary>
        /// Called when IsToggled changed (after animation)
        /// </summary>
        protected virtual void OnIsToggledChanged(bool toggled)
        {
            return;
        }

        /// <summary>
        /// Handle IsToggled changes
        /// </summary>
        protected void OnIsToggledChangedInternal(bool newValue)
        {
            if (string.IsNullOrEmpty(ToggledText) == false && string.IsNullOrEmpty(ToggledText) == false)
            {
                if (IsToggled)
                {
                    Text = ToggledText;
                }
                else
                {
                    Text = UnToggledText;
                }
            }

            if (IsToggled)
            {
                VisualStateManager.GoToState(this, ToggledStateName);
            }
            else
            {
                VisualStateManager.GoToState(this, DefaultStateName);
            }

            _toggledAnimationProcess = newValue ? 1 : 0;
            _skiaCanvas.InvalidateSurface();

            OnIsToggledChanged(IsToggled);

            if (IsToggledChanged != null)
            {
                IsToggledChanged(this, IsToggled);
            }

            if (UnToggledCommand != null && IsToggled == false)
            {
                UnToggledCommand.Execute(CommandParameter);
            }

            if (ToggledCommand != null && IsToggled == true)
            {
                ToggledCommand.Execute(CommandParameter);
            }
        }

        /// <summary>
        /// Update toggled text
        /// </summary>
        private void OnToggledTextChanged(string oldToggledText, string newToggledText)
        {
            if (string.IsNullOrEmpty(ToggledText) == false && string.IsNullOrEmpty(ToggledText) == false)
            {
                if (IsToggled == true)
                {
                    Text = newToggledText;
                }
            }
        }

        /// <summary>
        /// Update untoggled text
        /// </summary>
        private void OnUnToggledTextChanged(string oldUnToggledText, string newUnToggledText)
        {
            if (string.IsNullOrEmpty(ToggledText) == false && string.IsNullOrEmpty(ToggledText) == false)
            {
                if (IsToggled == false)
                {
                    Text = newUnToggledText;
                }
            }
        }

        #region State transition animations

        /// <summary>
        /// Get icon color
        /// </summary>
        protected override Color GetActualIconColor()
        {
            return GetColor(
                IconColor, 
                IconHoverColor, 
                IconPressedColor, 
                ToggledIconColor, 
                ToggledIconHoverColor, 
                ToggledIconPressedColor, 
                IconDisabledColor);
        }

        /// <summary>
        /// Get text color
        /// </summary>
        protected override Color GetActualTextColor()
        {
            return GetColor(
                TextColor, 
                TextHoverColor, 
                TextPressedColor, 
                ToggledTextColor, 
                ToggledTextHoverColor, 
                ToggledTextPressedColor, 
                TextDisabledColor);
        }

        /// <summary>
        /// Get extra text color
        /// </summary>
        protected override Color GetActualExtraTextColor()
        {
            return GetColor(
                ExtraTextColor, 
                ExtraTextHoverColor, 
                ExtraTextPressedColor, 
                ToggledExtraTextColor, 
                ToggledExtraTextHoverColor, 
                ToggledExtraTextPressedColor, 
                ExtraTextDisabledColor);
        }

        /// <summary>
        /// Get background color
        /// </summary>
        protected override Color GetActualBackgroundColor()
        {
            if (AnimationStyle == AnimationStyles.Ellipse)
            {
                if (IsEnabled)
                {
                    Color def = AnimationUtils.ColorTransform(_toggledAnimationProcess, BackgroundColor, ToggledBackgroundColor);
                    Color hover = AnimationUtils.ColorTransform(_toggledAnimationProcess, BackgroundHoverColor, ToggledBackgroundHoverColor);
                    return AnimationUtils.ColorTransform(_hoverAnimationProcess, def, hover);
                }
                else
                {
                    return BackgroundDisabledColor;
                }
            }
            else
            {
                return GetColor(
                    BackgroundColor, 
                    BackgroundHoverColor, 
                    BackgroundPressedColor,
                    ToggledBackgroundColor,
                    ToggledBackgroundHoverColor, 
                    ToggledBackgroundPressedColor,
                    BackgroundDisabledColor);
            }
        }

        /// <summary>
        /// Get border color
        /// </summary>
        protected override Color GetActualBorderColor()
        {
            return GetColor(
                BorderColor, 
                BorderHoverColor, 
                BorderPressedColor, 
                ToggledBorderColor, 
                ToggledBorderHoverColor,
                ToggledBorderPressedColor,
                BorderDisabledColor);
        }

        /// <summary>
        /// Get actual ellipse background color
        /// </summary>
        protected override Color GetActualEllipseColor()
        {
            return AnimationUtils.ColorTransform(_toggledAnimationProcess, BackgroundPressedColor, ToggledBackgroundPressedColor);
        }

        /// <summary>
        /// Get color based on IsPressed, IsToggled and animation process.
        /// </summary>
        protected virtual Color GetColor(
            Color defaultColor,
            Color hoverColor,
            Color pressedColor, 
            Color toggledColor,
            Color toggledHoverColor,
            Color toggledPressedColor, 
            Color disabledColor)
        {
            if (IsEnabled == false)
            {
                return disabledColor;
            }
            else
            {
                if (_pressedAnimationProcess < 1 || _hoverAnimationProcess < 1 || (_toggledAnimationProcess < 1 && _toggledAnimationProcess > 0))
                {
                    Color def = AnimationUtils.ColorTransform(_toggledAnimationProcess, defaultColor, toggledColor);
                    Color hover = AnimationUtils.ColorTransform(_toggledAnimationProcess, hoverColor, toggledHoverColor);
                    Color pressed = AnimationUtils.ColorTransform(_toggledAnimationProcess, pressedColor, toggledPressedColor);

                    Color finalColor = def;

                    finalColor = AnimationUtils.ColorTransform(_hoverAnimationProcess, finalColor, hover);

                    if (_pressedAnimationProcess <= 0.5)
                    {
                        finalColor = AnimationUtils.ColorTransform((_pressedAnimationProcess * 2), finalColor, pressed);
                    }
                    else
                    {
                        finalColor = AnimationUtils.ColorTransform(((_pressedAnimationProcess - 0.5) * 2), pressed, finalColor);
                    }

                    return finalColor;
                }
                else
                {
                    if (IsPressed)
                    {
                        if (IsToggled)
                        {
                            return toggledPressedColor;
                        }
                        else
                        {
                            return pressedColor;
                        }
                    }
                    else if (IsMouseOver)
                    {
                        if (IsToggled)
                        {
                            return toggledHoverColor;
                        }
                        else
                        {
                            return hoverColor;
                        }
                    }
                    else
                    {
                        if (IsToggled)
                        {
                            return toggledColor;
                        }
                        else
                        {
                            return defaultColor;
                        }
                    }
                }
            }
        }

        #endregion
    }
}

