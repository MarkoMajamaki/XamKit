using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamKit
{
    public class ShellApplication : Application
    {
        private RootPage _rootPage = null;
        private FlyoutMenu _flyoutMenu = null;
        private BoxView _titleBarSeparator = null;

        private NavigationPage _navigationPage = null;
        private NavigationPage _mainMenuNavigationPage = null;
        private NavigationPage _subMenuNavigationPage = null;
        private NavigationPage _modalNavigationPage = null;

        /// <summary>
        /// MainMenu open/close animation is started
        /// </summary>
        public event EventHandler<bool> IsMainMenuOpenChanging;

        /// <summary>
        /// MainMenu open/close animation is finished
        /// </summary>
        public event EventHandler<bool> IsMainMenuOpenChanged;

        /// <summary>
        /// SubMenu open/close animation is started
        /// </summary>
        public event EventHandler<bool> IsSubMenuOpenChanging;

        /// <summary>
        /// SubMenu open/close animation is finished
        /// </summary>
        public event EventHandler<bool> IsSubMenuOpenChanged;

        /// <summary>
		/// Navigation
		/// </summary>
		public INavigation Navigation { get { return _navigationPage; } }

        /// <summary>
        /// Navigation
        /// </summary>
        public INavigation MainMenuNavigation { get { return _mainMenuNavigationPage; } }

        /// <summary>
        /// Navigation
        /// </summary>
        public INavigation SubMenuNavigation { get { return _subMenuNavigationPage; } }

        /// <summary>
        /// Navigation
        /// </summary>
        public INavigation ModalNavigation { get { return _modalNavigationPage; } }

        /// <summary>
        /// Flyout menu
        /// </summary>
        public IFlyoutMenu FlyoutMenu { get { return _flyoutMenu; } }

        #region Properties

        /// <summary>
        /// Is main menu open
        /// </summary>
        public static readonly BindableProperty IsMainMenuOpenProperty =
            BindableProperty.Create("IsMainMenuOpen", typeof(bool), typeof(ShellApplication), false);

        public bool IsMainMenuOpen
        {
            get { return (bool)GetValue(IsMainMenuOpenProperty); }
            set { SetValue(IsMainMenuOpenProperty, value); }
        }

        /// <summary>
        /// Is main menu open
        /// </summary>
        public static readonly BindableProperty IsSubMenuOpenProperty =
            BindableProperty.Create("IsSubMenuOpen", typeof(bool), typeof(ShellApplication), false);

        public bool IsSubMenuOpen
        {
            get { return (bool)GetValue(IsSubMenuOpenProperty); }
            set { SetValue(IsSubMenuOpenProperty, value); }
        }

        /// <summary>
        /// Flyout menu style
        /// </summary>
        public static readonly BindableProperty FlyoutMenuStyleProperty =
            BindableProperty.Create("FlyoutMenuStyle", typeof(Style), typeof(ShellApplication), null);

        public Style FlyoutMenuStyle
        {
            get { return (Style)GetValue(FlyoutMenuStyleProperty); }
            set { SetValue(FlyoutMenuStyleProperty, value); }
        }

        /// <summary>
        /// NavigationPage style
        /// </summary>
        public static readonly BindableProperty NavigationPageStyleProperty =
            BindableProperty.Create("NavigationPageStyle", typeof(Style), typeof(ShellApplication), null);

        public Style NavigationPageStyle
        {
            get { return (Style)GetValue(NavigationPageStyleProperty); }
            set { SetValue(NavigationPageStyleProperty, value); }
        }

        /// <summary>
        /// Full screen modal NavigationPage style
        /// </summary>
        public static readonly BindableProperty ModalNavigationPageStyleProperty =
            BindableProperty.Create("ModalNavigationPageStyle", typeof(Style), typeof(ShellApplication), null);

        public Style ModalNavigationPageStyle
        {
            get { return (Style)GetValue(ModalNavigationPageStyleProperty); }
            set { SetValue(ModalNavigationPageStyleProperty, value); }
        }

        /// <summary>
        /// Title bar separator thickness
        /// </summary>
        public static readonly BindableProperty TitleBarSeparatorThicknessProperty =
            BindableProperty.Create("TitleBarSeparatorThickness", typeof(double), typeof(ShellApplication), 0.0);

        public double TitleBarSeparatorThickness
        {
            get { return (double)GetValue(TitleBarSeparatorThicknessProperty); }
            set { SetValue(TitleBarSeparatorThicknessProperty, value); }
        }

        /// <summary>
        /// Title bar separator color
        /// </summary>
        public static readonly BindableProperty TitleBarSeparatorColorProperty =
            BindableProperty.Create("TitleBarSeparatorColor", typeof(Color), typeof(ShellApplication), Color.Default);

        public Color TitleBarSeparatorColor
        {
            get { return (Color)GetValue(TitleBarSeparatorColorProperty); }
            set { SetValue(TitleBarSeparatorColorProperty, value); }
        }

        #endregion

        public ShellApplication()
        {
            _rootPage = new RootPage();
            _rootPage.DeviceBackButtonPressed += OnDeviceBackButtonPressed;

            // Create popup layout for popups
            Popup.PopupLayout = new PopupLayout();

            _flyoutMenu = new FlyoutMenu();
            _flyoutMenu.IsMainMenuOpenChanging += (object sender, bool isOpen) =>
            {
                IsMainMenuOpen = isOpen;
                IsMainMenuOpenChanging?.Invoke(sender, isOpen);
            };
            _flyoutMenu.IsMainMenuOpenChanged += (object sender, bool isOpen) =>
            {
                IsMainMenuOpenChanged?.Invoke(sender, isOpen);
            };

            _flyoutMenu.IsSubMenuOpenChanged += (object sender, bool isOpen) =>
            {
                IsSubMenuOpenChanged?.Invoke(sender, isOpen);
            };
            _flyoutMenu.IsSubMenuOpenChanging += (object sender, bool isOpen) =>
            {
                IsSubMenuOpen = isOpen;
                IsSubMenuOpenChanging?.Invoke(sender, isOpen);
            };

            // Content

            _navigationPage = new NavigationPage();
            _navigationPage.Style = NavigationPageStyle;
            _flyoutMenu.Content = _navigationPage;
            _navigationPage.MenuButtonTapped += (s, a) =>
            {
                _flyoutMenu.IsMainMenuOpen = true;
            };

            // MainMenu

            _mainMenuNavigationPage = new NavigationPage();
            _mainMenuNavigationPage.Style = NavigationPageStyle;
            _mainMenuNavigationPage.HasPagesChanged += (object s, bool hasPages) =>
            {
                if (hasPages)
                {
                    _flyoutMenu.MainMenu = _mainMenuNavigationPage;
                }
                else
                {
                    _flyoutMenu.MainMenu = null;
                }
            };

            // SubMenu

            _subMenuNavigationPage = new NavigationPage();
            _subMenuNavigationPage.Style = NavigationPageStyle;
            _subMenuNavigationPage.HasPagesChanged += (object s, bool hasPages) =>
            {
                if (hasPages)
                {
                    _flyoutMenu.SubMenu = _subMenuNavigationPage;
                }
                else
                {
                    _flyoutMenu.SubMenu = null;
                }
            };

            // Modal

            _modalNavigationPage = new NavigationPage();
            _modalNavigationPage.IsVisible = false;
            _modalNavigationPage.Style = ModalNavigationPageStyle;
            _modalNavigationPage.HasPagesChanged += (object s, bool hasPages) =>
            {
                _modalNavigationPage.IsVisible = hasPages;
            };

            // Titlebar

            _titleBarSeparator = new BoxView();
            _titleBarSeparator.VerticalOptions = LayoutOptions.Start;
            _titleBarSeparator.HeightRequest = TitleBarSeparatorThickness;
            _titleBarSeparator.BackgroundColor = TitleBarSeparatorColor;

            // Add navigation page and popup layout to root page content
            StackLayout m_rootLayout = new StackLayout();
            m_rootLayout.Orientation = StackOrientations.Depth;
            m_rootLayout.Children.Add(_flyoutMenu);
            m_rootLayout.Children.Add(_modalNavigationPage);
            m_rootLayout.Children.Add(Popup.PopupLayout);
            m_rootLayout.Children.Add(_titleBarSeparator);

            // Add to content
            _rootPage.Content = m_rootLayout;

            MainPage = _rootPage;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == NavigationPageStyleProperty.PropertyName)
            {
                _navigationPage.Style = NavigationPageStyle;
                _mainMenuNavigationPage.Style = NavigationPageStyle;
                _subMenuNavigationPage.Style = NavigationPageStyle;
            }
            else if (propertyName == TitleBarSeparatorThicknessProperty.PropertyName)
            {
                _titleBarSeparator.HeightRequest = TitleBarSeparatorThickness;
            }
            else if (propertyName == TitleBarSeparatorColorProperty.PropertyName)
            {
                _titleBarSeparator.BackgroundColor = TitleBarSeparatorColor;
            }
            else if (propertyName == FlyoutMenuStyleProperty.PropertyName)
            {
                _flyoutMenu.Style = FlyoutMenuStyle;
            }
            else if (propertyName == IsMainMenuOpenProperty.PropertyName)
            {
                _flyoutMenu.IsMainMenuOpen = IsMainMenuOpen;
            }
            else if (propertyName == IsSubMenuOpenProperty.PropertyName)
            {
                _flyoutMenu.IsSubMenuOpen = IsSubMenuOpen;
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        public Task SetIsMainMenuOpenAsync(bool isOpen)
        {
            return _flyoutMenu.SetIsMainMenuOpenAsync(isOpen);
        }

        public Task SetIsSubMenuOpenAsync(bool isOpen)
        {
            return _flyoutMenu.SetIsSubMenuOpenAsync(isOpen);
        }

        /// <summary>
        /// Handle device back button pressed
        /// </summary>
        /// <returns>True if navigation to out of the app is ignored</returns>
        private bool OnDeviceBackButtonPressed()
        {
            if (Popup.IsAnyPopupOpen)
            {
                Popup.CloseAll();
                return true;
            }
            if (ToolBar.IsAnyToolBarMenuOpen)
            {
                ToolBar.CloseAll();
                return true;
            }
            if (_flyoutMenu.IsSubMenuOpen)
            {
                _flyoutMenu.IsSubMenuOpen = false;
                return true;
            }
            if (_flyoutMenu.IsMainMenuOpen)
            {
                _flyoutMenu.IsMainMenuOpen = false;
                return true;
            }
            if (_modalNavigationPage.OnDeviceBackButtonPressed())
            {
                return true;
            }
            if (_navigationPage.OnDeviceBackButtonPressed())
            {
                return true;
            }

            return false;
        }
    }
}
