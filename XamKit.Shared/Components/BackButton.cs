using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// Button which has multiple icons
    /// </summary>
    public class BackButton : Button, IBackButton
    {
        #region Binding properties

        public static readonly BindableProperty ModeProperty =
            BindableProperty.Create("Mode", typeof(BackButtonModes), typeof(BackButton), BackButtonModes.Back);

        public BackButtonModes Mode
        {
            get { return (BackButtonModes)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly BindableProperty MenuIconResourceKeyProperty =
            BindableProperty.Create("MenuIconResourceKey", typeof(string), typeof(BackButton), null);

        public string MenuIconResourceKey
        {
            get { return (string)GetValue(MenuIconResourceKeyProperty); }
            set { SetValue(MenuIconResourceKeyProperty, value); }
        }

        /// <summary>
        /// The assembly name containing the svg file for menu icon.
        /// </summary>
        public static readonly BindableProperty MenuIconAssemblyNameProperty =
            BindableProperty.Create(nameof(MenuIconAssemblyName), typeof(string), typeof(BackButton), null);

        public string MenuIconAssemblyName
        {
            get { return (string)GetValue(MenuIconAssemblyNameProperty); }
            set { SetValue(MenuIconAssemblyNameProperty, value); }
        }

        public static readonly BindableProperty CloseIconResourceKeyProperty =
            BindableProperty.Create("CloseIconResourceKey", typeof(string), typeof(BackButton), null);

        public string CloseIconResourceKey
        {
            get { return (string)GetValue(CloseIconResourceKeyProperty); }
            set { SetValue(CloseIconResourceKeyProperty, value); }
        }

        /// <summary>
        /// The assembly name containing the svg file for close icon.
        /// </summary>
        public static readonly BindableProperty CloseIconAssemblyNameProperty =
            BindableProperty.Create(nameof(CloseIconAssemblyName), typeof(string), typeof(BackButton), null);

        public string CloseIconAssemblyName
        {
            get { return (string)GetValue(CloseIconAssemblyNameProperty); }
            set { SetValue(CloseIconAssemblyNameProperty, value); }
        }

        public static readonly BindableProperty BackIconResourceKeyProperty =
            BindableProperty.Create("BackIconResourceKey", typeof(string), typeof(BackButton), null);

        public string BackIconResourceKey
        {
            get { return (string)GetValue(BackIconResourceKeyProperty); }
            set { SetValue(BackIconResourceKeyProperty, value); }
        }

        /// <summary>
        /// The assembly name containing the svg file for back arrow icon.
        /// </summary>
        public static readonly BindableProperty BackIconAssemblyNameProperty =
            BindableProperty.Create(nameof(BackIconAssemblyName), typeof(string), typeof(BackButton), null);

        public string BackIconAssemblyName
        {
            get { return (string)GetValue(BackIconAssemblyNameProperty); }
            set { SetValue(BackIconAssemblyNameProperty, value); }
        }

        #endregion

        public BackButton()
        {
            UpdateMode();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == ModeProperty.PropertyName)
            {
                UpdateMode();
            }
            else if (((propertyName == BackIconResourceKeyProperty.PropertyName || propertyName == BackIconAssemblyNameProperty.PropertyName) && Mode == BackButtonModes.Back) ||
                     ((propertyName == MenuIconResourceKeyProperty.PropertyName || propertyName == MenuIconAssemblyNameProperty.PropertyName) && Mode == BackButtonModes.Menu) ||
                     ((propertyName == CloseIconResourceKeyProperty.PropertyName || propertyName == CloseIconAssemblyNameProperty.PropertyName) && Mode == BackButtonModes.Close))
            {
                UpdateMode();
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        private void UpdateMode()
        {
            ButtonIconChangeAnimations originalIconAnimation = IconChangeAnimation;

            // Disable animation if not visible
            if (Opacity.Equals(0) || IsVisible == false)
            {
                IconChangeAnimation = ButtonIconChangeAnimations.None;
            }

            if (Mode == BackButtonModes.Back)
            {
                IconAssemblyName = BackIconAssemblyName;
                IconResourceKey = BackIconResourceKey;
                this.IsVisible = true;
            }
            else if (Mode == BackButtonModes.Close)
            {
                IconAssemblyName = CloseIconAssemblyName;
                IconResourceKey = CloseIconResourceKey;
                this.IsVisible = true;
            }
            else if (Mode == BackButtonModes.Hidden)
            {
                this.IsVisible = false;
            }
            else if (Mode == BackButtonModes.Menu)
            {
                IconAssemblyName = MenuIconAssemblyName;
                IconResourceKey = MenuIconResourceKey;
                this.IsVisible = true;
            }

            if (Opacity.Equals(0) || IsVisible == false)
            {
                IconChangeAnimation = originalIconAnimation;
            }
        }
    }
}
