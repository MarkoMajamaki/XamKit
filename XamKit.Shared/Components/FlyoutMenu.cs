using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public class FlyoutMenu : Layout<View>, IFlyoutMenu
    {
        private const string _subMenuAnimationName = "subMenuAnimationName";
        private const string _mainMenuAnimationName = "mainMenuAnimationName";
        private const string _modalAnimationName = "modalBackgroundAnimationName";

        // Menu containers
        private Container _mainMenuContainer = null;
        private Container _subMenuContainer = null;

        private BoxView _modalBackground = null;

        // Sizes
        private SizeRequest _menuButtonSize = new SizeRequest();
        private double _visibleMainMenuWidth = 0;
        private double _visibleSubMenuWidth = 0;

        private bool _previousIsMainMenuOpen = false;
        private bool _previousIsSubMenuOpen = false;

        private bool _ignoreIsOpenAsync = false;

        // Helpers for touch interaction
        private bool _wasModalActive = false;
        private bool _didPan = false;

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

        #region Binding properties

        /// <summary>
        /// Is panning Menu, Content or Submenu
        /// </summary>
        public static readonly BindableProperty IsPanningProperty =
            BindableProperty.Create("IsPanning", typeof(bool), typeof(FlyoutMenu), false);

        public bool IsPanning
        {
            get { return (bool)GetValue(IsPanningProperty); }
            protected set { SetValue(IsPanningProperty, value); }
        }

        /// <summary>
        /// Parts border thickness
        /// </summary>
        public static readonly BindableProperty BorderThicknessProperty =
            BindableProperty.Create("BorderThickness", typeof(double), typeof(FlyoutMenu), 0.0, propertyChanged: OnBorderThicknessChanged);

        private static void OnBorderThicknessChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnBorderThicknessChanged((double)newValue);
        }

        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        #endregion

        #region Binding properties - Parts

        /// <summary>
        /// Content
        /// </summary>
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create("Content", typeof(View), typeof(FlyoutMenu), null, propertyChanged: OnContentChanged);

        private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnContentChanged(oldValue as View, newValue as View);
        }

        public View Content
        {
            get { return (View)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Main menu
        /// </summary>
        public static readonly BindableProperty MainMenuProperty =
            BindableProperty.Create("MainMenu", typeof(View), typeof(FlyoutMenu), null, propertyChanged: OnMainMenuChanged);

        private static void OnMainMenuChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnMainMenuChanged(oldValue as View, newValue as View);
        }

        public View MainMenu
        {
            get { return (View)GetValue(MainMenuProperty); }
            set { SetValue(MainMenuProperty, value); }
        }

        /// <summary>
        /// Template to create main menu when it is appearing
        /// </summary>
        public static readonly BindableProperty MainMenuTemplateProperty =
            BindableProperty.Create("MainMenuTemplate", typeof(DataTemplate), typeof(FlyoutMenu), null, propertyChanged: OnMainMenuTemplateChanged);

        private static void OnMainMenuTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnMainMenuTemplateChanged(newValue as DataTemplate);
        }

        public DataTemplate MainMenuTemplate
        {
            get { return (DataTemplate)GetValue(MainMenuTemplateProperty); }
            set { SetValue(MainMenuTemplateProperty, value); }
        }

        /// <summary>
        /// Submenu
        /// </summary>
        public static readonly BindableProperty SubMenuProperty =
            BindableProperty.Create("SubMenu", typeof(View), typeof(FlyoutMenu), null, propertyChanged: OnSubMenuChanged);

        private static void OnSubMenuChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnSubMenuChanged(oldValue as View, newValue as View);
        }

        public View SubMenu
        {
            get { return (View)GetValue(SubMenuProperty); }
            set { SetValue(SubMenuProperty, value); }
        }

        /// <summary>
        /// Template to create submenu when it is appearing
        /// </summary>
        public static readonly BindableProperty SubMenuTemplateProperty =
            BindableProperty.Create("SubMenuTemplate", typeof(DataTemplate), typeof(FlyoutMenu), null, propertyChanged: OnSubMenuTemplateChanged);

        private static void OnSubMenuTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnSubMenuTemplateChanged(newValue as DataTemplate);
        }

        public DataTemplate SubMenuTemplate
        {
            get { return (DataTemplate)GetValue(SubMenuTemplateProperty); }
            set { SetValue(SubMenuTemplateProperty, value); }
        }

        #endregion

        #region Binding properties - Menu button

        /// <summary>
        /// Menu button which is located above everything.
        /// </summary>
        public static readonly BindableProperty MenuButtonProperty =
            BindableProperty.Create("MenuButton", typeof(View), typeof(FlyoutMenu), null, propertyChanged: OnMenuButtonChanged);

        private static void OnMenuButtonChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnMenuButtonChanged(oldValue as View, newValue as View);
        }

        public View MenuButton
        {
            get { return (View)GetValue(MenuButtonProperty); }
            set { SetValue(MenuButtonProperty, value); }
        }

        /// <summary>
        /// Menu button template which creates actual menu button
        /// </summary>
        public static readonly BindableProperty MenuButtonTemplateProperty =
            BindableProperty.Create("MenuButtonTemplate", typeof(DataTemplate), typeof(FlyoutMenu), null, propertyChanged: OnMenuButtonTemplateChanged);

        private static void OnMenuButtonTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnMenuButtonTemplateChanged(newValue as DataTemplate);
        }

        public DataTemplate MenuButtonTemplate
        {
            get { return (DataTemplate)GetValue(MenuButtonTemplateProperty); }
            set { SetValue(MenuButtonTemplateProperty, value); }
        }

        /// <summary>
        /// Menu button location
        /// </summary>
        public static readonly BindableProperty MenuButtonLocationProperty =
            BindableProperty.Create("MenuButtonLocation", typeof(MenuButtonLocations), typeof(FlyoutMenu), MenuButtonLocations.None, propertyChanged: OnMenuButtonLocationChanged);

        private static void OnMenuButtonLocationChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnMenuButtonLocationChanged((MenuButtonLocations)oldValue, (MenuButtonLocations)newValue);
        }

        public MenuButtonLocations MenuButtonLocation
        {
            get { return (MenuButtonLocations)GetValue(MenuButtonLocationProperty); }
            set { SetValue(MenuButtonLocationProperty, value); }
        }

        #endregion

        #region Binding properties - IsOpen

        /// <summary>
        /// Is menu open
        /// </summary>
        public static readonly BindableProperty IsMainMenuOpenProperty =
            BindableProperty.Create("IsMainMenuOpen", typeof(bool), typeof(FlyoutMenu), false, propertyChanged: OnIsMainMenuOpenChanged);

        private static async void OnIsMainMenuOpenChanged(BindableObject bindable, object oldValue, object newValue)
        {
            await (bindable as FlyoutMenu).SetIsMainMenuOpenAsync((bool)newValue);
        }

        public bool IsMainMenuOpen
        {
            get { return (bool)GetValue(IsMainMenuOpenProperty); }
            set { SetValue(IsMainMenuOpenProperty, value); }
        }

        /// <summary>
        /// Is submenu open
        /// </summary>
        public static readonly BindableProperty IsSubMenuOpenProperty =
            BindableProperty.Create("IsSubMenuOpen", typeof(bool), typeof(FlyoutMenu), false, propertyChanged: OnIsSubMenuOpenChanged);

        private static async void OnIsSubMenuOpenChanged(BindableObject bindable, object oldValue, object newValue)
        {
            await (bindable as FlyoutMenu).SetIsSubMenuOpenAsync((bool)newValue);
        }

        public bool IsSubMenuOpen
        {
            get { return (bool)GetValue(IsSubMenuOpenProperty); }
            set { SetValue(IsSubMenuOpenProperty, value); }
        }

        /// <summary>
        /// Target main menu state for opening
        /// </summary>
        public static readonly BindableProperty MainMenuOpenModeProperty =
            BindableProperty.Create("MainMenuOpenMode", typeof(MainMenuOpenModes), typeof(FlyoutMenu), MainMenuOpenModes.Open, propertyChanged: OnMainMenuOpenModeChanged);

        private static void OnMainMenuOpenModeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            FlyoutMenu flyoutMenu = bindable as FlyoutMenu;
            flyoutMenu.UpdateModalBackground();
            flyoutMenu.InvalidateLayout();
        }

        public MainMenuOpenModes MainMenuOpenMode
        {
            get { return (MainMenuOpenModes)GetValue(MainMenuOpenModeProperty); }
            set { SetValue(MainMenuOpenModeProperty, value); }
        }

        /// <summary>
        /// Target menu state for closing
        /// </summary>
        public static readonly BindableProperty MainMenuCloseModeProperty =
            BindableProperty.Create("MainMenuCloseMode", typeof(MainMenuCloseModes), typeof(FlyoutMenu), MainMenuCloseModes.Closed, propertyChanged: OnMainMenuCloseModeChanged);

        private static void OnMainMenuCloseModeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            FlyoutMenu flyoutMenu = bindable as FlyoutMenu;
            flyoutMenu.UpdateModalBackground();
            flyoutMenu.InvalidateLayout();
        }

        public MainMenuCloseModes MainMenuCloseMode
        {
            get { return (MainMenuCloseModes)GetValue(MainMenuCloseModeProperty); }
            set { SetValue(MainMenuCloseModeProperty, value); }
        }

        /// <summary>
        /// Target state for submenu opening
        /// </summary>
        public static readonly BindableProperty SubMenuOpenModeProperty =
            BindableProperty.Create("SubMenuOpenMode", typeof(SubMenuOpenModes), typeof(FlyoutMenu), SubMenuOpenModes.Floating, propertyChanged: OnSubMenuOpenModeChanged);

        private static void OnSubMenuOpenModeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            FlyoutMenu flyoutMenu = bindable as FlyoutMenu;
            flyoutMenu.UpdateModalBackground();
            flyoutMenu.InvalidateLayout();
        }

        public SubMenuOpenModes SubMenuOpenMode
        {
            get { return (SubMenuOpenModes)GetValue(SubMenuOpenModeProperty); }
            set { SetValue(SubMenuOpenModeProperty, value); }
        }

        #endregion

        #region Binding properties - Gesture behaviors

        /// <summary>
        /// How content behaves when menu or submenu is opened
        /// </summary>
        public static readonly BindableProperty ContentBehaviorProperty =
            BindableProperty.Create("ContentBehavior", typeof(ContentBehaviors), typeof(FlyoutMenu), ContentBehaviors.Move, propertyChanged: OnContentBehaviorChanged);

        private static void OnContentBehaviorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            FlyoutMenu flyoutMenu = bindable as FlyoutMenu;
            flyoutMenu.UpdateModalBackground();
            flyoutMenu.InvalidateLayout();
        }

        public ContentBehaviors ContentBehavior
        {
            get { return (ContentBehaviors)GetValue(ContentBehaviorProperty); }
            set { SetValue(ContentBehaviorProperty, value); }
        }

        /// <summary>
        /// Menu location
        /// </summary>
        public static readonly BindableProperty MenuLocationProperty =
            BindableProperty.Create("MenuLocation", typeof(HorizontalLocations), typeof(FlyoutMenu), HorizontalLocations.Left, propertyChanging: OnMenuLocationPropertyChanged);

        private static void OnMenuLocationPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnMenuLocationPropertyChanged((HorizontalLocations)oldValue, (HorizontalLocations)newValue);
        }

        public HorizontalLocations MenuLocation
        {
            get { return (HorizontalLocations)GetValue(MenuLocationProperty); }
            set { SetValue(MenuLocationProperty, value); }
        }

        #endregion

        #region Binding properties - Width

        /// <summary>
        /// Width when main menu is minimalized (if target closing state is Minimalized)
        /// </summary>
        public static readonly BindableProperty MainMenuMinimalizedWidthProperty =
            BindableProperty.Create("MainMenuMinimalizedWidth", typeof(double), typeof(FlyoutMenu), 48.0);

        public double MainMenuMinimalizedWidth
        {
            get { return (double)GetValue(MainMenuMinimalizedWidthProperty); }
            set { SetValue(MainMenuMinimalizedWidthProperty, value); }
        }

        /// <summary>
        /// MainMenu width when it is opened
        /// </summary>
        public static readonly BindableProperty MainMenuWidthProperty =
            BindableProperty.Create("MainMenuWidth", typeof(double), typeof(FlyoutMenu), 200.0, propertyChanging: OnMainMenuWidthChanged);

        private static void OnMainMenuWidthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnMainMenuWidthChanged((double)newValue);
        }

        public double MainMenuWidth
        {
            get { return (double)GetValue(MainMenuWidthProperty); }
            set { SetValue(MainMenuWidthProperty, value); }
        }

        /// <summary>
        /// SubMenu width when it is opened
        /// </summary>
        public static readonly BindableProperty SubMenuWidthProperty =
            BindableProperty.Create("SubMenuWidth", typeof(double), typeof(FlyoutMenu), 200.0, propertyChanged: OnSubMenuWidthChanged);

        private static void OnSubMenuWidthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnSubMenuWidthChanged((double)newValue);
        }

        public double SubMenuWidth
        {
            get { return (double)GetValue(SubMenuWidthProperty); }
            set { SetValue(SubMenuWidthProperty, value); }
        }


        /// <summary>
        /// Left or right margin spacing. If -1 and MenuButtonLocation is inside content then spacing is zero or menu button width.
        /// </summary>
        public static readonly BindableProperty MainMenuSpacingProperty =
            BindableProperty.Create("MainMenuSpacing", typeof(double), typeof(FlyoutMenu), -1.0);

        public double MainMenuSpacing
        {
            get { return (double)GetValue(MainMenuSpacingProperty); }
            set { SetValue(MainMenuSpacingProperty, value); }
        }

        #endregion

        #region Binding properties - Animations

        /// <summary>
        /// MainMenu opening easing function. Null if linear.
        /// </summary>
        public static readonly BindableProperty MainMenuOpeningEasingProperty =
            BindableProperty.Create("MainMenuOpeningEasing", typeof(Easing), typeof(FlyoutMenu), Easing.CubicOut);

        public Easing MainMenuOpeningEasing
        {
            get { return (Easing)GetValue(MainMenuOpeningEasingProperty); }
            set { SetValue(MainMenuOpeningEasingProperty, value); }
        }

        /// <summary>
        /// MainMenu closing easing function. Null if linear.
        /// </summary>
        public static readonly BindableProperty MainMenuClosingEasingProperty =
            BindableProperty.Create("MainMenuClosingEasing", typeof(Easing), typeof(FlyoutMenu), Easing.CubicOut);

        public Easing MainMenuClosingEasing
        {
            get { return (Easing)GetValue(MainMenuClosingEasingProperty); }
            set { SetValue(MainMenuClosingEasingProperty, value); }
        }

        /// <summary>
        /// MainMenu opening duration in milliseconds
        /// </summary>
        public static readonly BindableProperty MainMenuOpeningDurationProperty =
            BindableProperty.Create("MainMenuOpeningDuration", typeof(int), typeof(FlyoutMenu), 300);

        public int MainMenuOpeningDuration
        {
            get { return (int)GetValue(MainMenuOpeningDurationProperty); }
            set { SetValue(MainMenuOpeningDurationProperty, value); }
        }

        /// <summary>
        /// MainMenu closing duration in milliseconds
        /// </summary>
        public static readonly BindableProperty MainMenuClosingDurationProperty =
            BindableProperty.Create("MainMenuClosingDuration", typeof(int), typeof(FlyoutMenu), 300);

        public int MainMenuClosingDuration
        {
            get { return (int)GetValue(MainMenuClosingDurationProperty); }
            set { SetValue(MainMenuClosingDurationProperty, value); }
        }

        /// <summary>
        /// Submenu opening easing function. Null if linear.
        /// </summary>
        public static readonly BindableProperty SubMenuOpeningEasingProperty =
            BindableProperty.Create("SubMenuOpeningEasing", typeof(Easing), typeof(FlyoutMenu), Easing.CubicOut);

        public Easing SubMenuOpeningEasing
        {
            get { return (Easing)GetValue(SubMenuOpeningEasingProperty); }
            set { SetValue(SubMenuOpeningEasingProperty, value); }
        }

        /// <summary>
        /// Submenu closing easing function. Null if linear.
        /// </summary>
        public static readonly BindableProperty SubMenuClosingEasingProperty =
            BindableProperty.Create("SubMenuClosingEasing", typeof(Easing), typeof(FlyoutMenu), Easing.CubicOut);

        public Easing SubMenuClosingEasing
        {
            get { return (Easing)GetValue(SubMenuClosingEasingProperty); }
            set { SetValue(SubMenuClosingEasingProperty, value); }
        }

        /// <summary>
        /// Submenu opening duration in milliseconds
        /// </summary>
        public static readonly BindableProperty SubMenuOpeningDurationProperty =
            BindableProperty.Create("SubMenuOpeningDuration", typeof(int), typeof(FlyoutMenu), 300);

        public int SubMenuOpeningDuration
        {
            get { return (int)GetValue(SubMenuOpeningDurationProperty); }
            set { SetValue(SubMenuOpeningDurationProperty, value); }
        }

        /// <summary>
        /// Submenu closing duration in milliseconds
        /// </summary>
        public static readonly BindableProperty SubMenuClosingDurationProperty =
            BindableProperty.Create("SubMenuClosingDuration", typeof(int), typeof(FlyoutMenu), 300);

        public int SubMenuClosingDuration
        {
            get { return (int)GetValue(SubMenuClosingDurationProperty); }
            set { SetValue(SubMenuClosingDurationProperty, value); }
        }

        #endregion

        #region Binding properties - Modal darkness

        /// <summary>
        /// Modal darkness color
        /// </summary>
        public static readonly BindableProperty ModalColorProperty =
            BindableProperty.Create("ModalColor", typeof(Color), typeof(FlyoutMenu), Color.Black);

        public Color ModalColor
        {
            get { return (Color)GetValue(ModalColorProperty); }
            set { SetValue(ModalColorProperty, value); }
        }

        /// <summary>
        /// Modal darkness layer max opacity
        /// </summary>
        public static readonly BindableProperty MaxModalDarknessOpacityProperty =
            BindableProperty.Create("MaxModalDarknessOpacity", typeof(double), typeof(FlyoutMenu), 0.3);

        public double MaxModalDarknessOpacity
        {
            get { return (double)GetValue(MaxModalDarknessOpacityProperty); }
            set { SetValue(MaxModalDarknessOpacityProperty, value); }
        }

        #endregion

        #region Binding properties - Color

        /// <summary>
        /// Parts border
        /// </summary>
        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create("BorderColor", typeof(Color), typeof(FlyoutMenu), Color.Default, propertyChanged: OnBorderColorChanged);

        private static void OnBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlyoutMenu).OnBorderColorChanged((Color)newValue);
        }

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        #endregion

        #region Properties

        public double ActualMainMenuSpacing
        {
            get
            {
                if (MainMenuSpacing >= 0)
                {
                    return MainMenuSpacing;
                }
                else
                {
                    return _menuButtonSize.Request.Width;
                }
            }
        }

        #endregion

        public FlyoutMenu()
        {
            OnMainMenuWidthChanged(MainMenuWidth);

            OnSubMenuWidthChanged(SubMenuWidth);

            OnMainMenuChanged(null, MainMenu);

            _modalBackground = CreateModalBoxView();
            if (Content != null)
            {
                Children.Insert(Children.IndexOf(Content) + 1, _modalBackground);
            }
            else
            {
                Children.Insert(0, _modalBackground);
            }

            OnSubMenuChanged(null, SubMenu);

            if (MenuButton != null)
            {
                OnMenuButtonChanged(null, MenuButton);
            }
            else if (MenuButtonTemplate != null)
            {
                OnMenuButtonTemplateChanged(MenuButtonTemplate);
            }

            OnMenuLocationPropertyChanged(MenuLocation, MenuLocation);

            OnMenuButtonLocationChanged(null, MenuButtonLocation);

            SetMenuButtonIsToggled(IsMainMenuOpen);
        }

        /// <summary>
        /// Handle MenuLocation changes
        /// </summary>
        private void OnMenuLocationPropertyChanged(HorizontalLocations oldLocation, HorizontalLocations newLocation)
        {
            // Abort all animations. Do changes immediatley.
            this.AbortAnimation(_subMenuAnimationName);
            this.AbortAnimation(_mainMenuAnimationName);
            this.AbortAnimation(_modalAnimationName);

            // Update modal background
            UpdateModalBackground();

            HorizontalLocations lineLocation = newLocation == HorizontalLocations.Left ? HorizontalLocations.Right : HorizontalLocations.Left;

            // Update containers menu location
            if (_mainMenuContainer != null)
            {
                _mainMenuContainer.LineLocation = lineLocation;
            }
            if (_subMenuContainer != null)
            {
                _subMenuContainer.LineLocation = lineLocation;
            }

            UpdateTranslations(Width, Height);
        }

        /// <summary>
        /// Change containers border thickness
        /// </summary>
        private void OnBorderThicknessChanged(double newValue)
        {
            if (_mainMenuContainer != null)
            {
                _mainMenuContainer.LineThickness = newValue;
            }
            if (_subMenuContainer != null)
            {
                _subMenuContainer.LineThickness = newValue;
            }
        }

        /// <summary>
        /// Change containers border color
        /// </summary>
        private void OnBorderColorChanged(Color color)
        {
            if (_mainMenuContainer != null)
            {
                _mainMenuContainer.LineColor = color;
            }
            if (_subMenuContainer != null)
            {
                _subMenuContainer.LineColor = color;
            }
        }

        /// <summary>
        /// Calculate MenuButton X based on SubMenu and MainMenu visible location
        /// </summary>
        /// <param name="availableWidth">All available width</param>
        private double GetMenuButtonTranslationX(double availableWidth)
        {
            if (MenuButtonLocation == MenuButtonLocations.TopLeft || MenuButtonLocation == MenuButtonLocations.None)
            {
                return 0;
            }
            else if (MenuButtonLocation == MenuButtonLocations.TopRight)
            {
                return availableWidth - _menuButtonSize.Request.Width;
            }
            else if (MenuButtonLocation == MenuButtonLocations.ContentTopLeft)
            {
                double x = 0;

                if (MenuLocation == HorizontalLocations.Left)
                {
                    if (SubMenuOpenMode == SubMenuOpenModes.Open)
                    {
                        x = _subMenuContainer.TranslationX + _visibleSubMenuWidth;
                    }
                    else if (MainMenuOpenMode == MainMenuOpenModes.Open)
                    {
                        x = _mainMenuContainer.TranslationX + _visibleMainMenuWidth;
                    }
                }

                return x;
            }
            else if (MenuButtonLocation == MenuButtonLocations.ContentTopRight)
            {
                double x = 0;

                if (MenuLocation == HorizontalLocations.Right)
                {
                    if (SubMenuOpenMode == SubMenuOpenModes.Open)
                    {
                        x = _subMenuContainer.TranslationX - _menuButtonSize.Request.Width;
                    }
                    else if (MainMenuOpenMode == MainMenuOpenModes.Open)
                    {
                        x = _mainMenuContainer.TranslationX - _menuButtonSize.Request.Width;
                    }
                }
                else
                {
                    x = availableWidth - _menuButtonSize.Request.Width;
                }

                return x;
            }

            return 0;
        }

        /// <summary>
        /// Calculate content translation X based on MainMenu and SubMenu visible location
        /// </summary>
        /// <param name="availableWidth">All available width</param>
        private double GetContentTranslationX(double availableWidth)
        {
            double x = 0;

            if (MenuLocation == HorizontalLocations.Left)
            {
                if (((SubMenuOpenMode == SubMenuOpenModes.Open && MainMenuOpenMode == MainMenuOpenModes.Open) || Device.Idiom == TargetIdiom.Phone) && SubMenu != null)
                {
                    x = _subMenuContainer.TranslationX + _visibleSubMenuWidth;
                }
                else if (MainMenuOpenMode == MainMenuOpenModes.Open)
                {
                    x = _mainMenuContainer.TranslationX + _visibleMainMenuWidth;
                }
                else if (MainMenuCloseMode == MainMenuCloseModes.Minimalized)
                {
                    x = MainMenuMinimalizedWidth;
                }
            }
            else if (ContentBehavior == ContentBehaviors.Move)
            {
                if (SubMenuOpenMode == SubMenuOpenModes.Open && MainMenuOpenMode == MainMenuOpenModes.Open)
                {
                    double menuX = _mainMenuContainer.TranslationX;
                    if (SubMenu != null)
                    {
                        menuX = Math.Min(_subMenuContainer.TranslationX, _mainMenuContainer.TranslationX);
                    }
                    x = -(availableWidth - menuX);
                    if (MainMenuCloseMode == MainMenuCloseModes.Minimalized)
                    {
                        x += MainMenuMinimalizedWidth;
                    }
                }
                else if (MainMenuOpenMode == MainMenuOpenModes.Open)
                {
                    x = -(Width - _mainMenuContainer.TranslationX);
                    if (MainMenuCloseMode == MainMenuCloseModes.Minimalized)
                    {
                        x += MainMenuMinimalizedWidth;
                    }
                }
            }

            return x;
        }

        /// <summary>
        /// Get content width based on MainMenu and SubMenu visible width and location
        /// </summary>
        /// <param name="availableWidth">All available width</param>
        private double GetContentWidth(double availableWidth)
        {
            double contentWidth = 0;

            if (ContentBehavior == ContentBehaviors.Resize)
            {
                if (MenuLocation == HorizontalLocations.Left)
                {
                    if (SubMenuOpenMode == SubMenuOpenModes.Open && MainMenuOpenMode == MainMenuOpenModes.Open)
                    {
                        contentWidth = availableWidth - (_subMenuContainer.TranslationX + _visibleSubMenuWidth);
                    }
                    else if (MainMenuOpenMode == MainMenuOpenModes.Open)
                    {
                        contentWidth = availableWidth - (_mainMenuContainer.TranslationX + _visibleMainMenuWidth);
                    }
                    else if (MainMenuCloseMode == MainMenuCloseModes.Minimalized)
                    {
                        contentWidth = availableWidth - MainMenuMinimalizedWidth;
                    }
                    else
                    {
                        contentWidth = availableWidth;
                    }
                }
                else
                {
                    if (SubMenuOpenMode == SubMenuOpenModes.Open && MainMenuOpenMode == MainMenuOpenModes.Open)
                    {
                        contentWidth = availableWidth - (availableWidth - Math.Min(_subMenuContainer.TranslationX, _mainMenuContainer.TranslationX));
                    }
                    else if (MainMenuOpenMode == MainMenuOpenModes.Open)
                    {
                        contentWidth = availableWidth - (availableWidth - _mainMenuContainer.TranslationX);
                    }
                    else if (MainMenuCloseMode == MainMenuCloseModes.Minimalized)
                    {
                        contentWidth = availableWidth - MainMenuMinimalizedWidth;
                    }
                    else
                    {
                        contentWidth = availableWidth;
                    }
                }
            }
            else
            {
                if (MainMenuCloseMode == MainMenuCloseModes.Minimalized)
                {
                    contentWidth = availableWidth - MainMenuMinimalizedWidth;
                }
                else
                {
                    contentWidth = availableWidth;
                }
            }


            return contentWidth;
        }

        #region Measure / Layout

        /// <summary>
        /// Measure total size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Size totalSize = new Size();

            if (double.IsInfinity(widthConstraint) == false && double.IsNaN(widthConstraint) == false)
            {
                totalSize.Width = widthConstraint;
            }
            else
            {
                throw new Exception("FlyoutMenu width should NOT be infinity or NaN!");
            }

            if (double.IsInfinity(heightConstraint) == false && double.IsNaN(heightConstraint) == false)
            {
                totalSize.Height = heightConstraint;
            }
            else
            {
                throw new Exception("FlyoutMenu height should NOT be infinity or NaN!");
            }

            return new SizeRequest(totalSize, totalSize);
        }

        /// <summary>
        /// Layout children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            _visibleSubMenuWidth = SubMenu != null ? SubMenuWidth : 0;

            if (IsPanning == false &&
                AnimationExtensions.AnimationIsRunning(this, _subMenuAnimationName) == false &&
                AnimationExtensions.AnimationIsRunning(this, _mainMenuAnimationName) == false &&
                AnimationExtensions.AnimationIsRunning(this, _modalAnimationName) == false)
            {
                _visibleMainMenuWidth = GetMainMenuVisibleWidth(width);
                UpdateTranslations(width, height);
            }

            // 
            // Layout MenuButton
            // 
            if (MenuButton != null && Children.Contains(MenuButton))
            {
                // Measure before layout
                if (MainMenu != null)
                {
                    _menuButtonSize = MenuButton.Measure(width, height, MeasureFlags.IncludeMargins);
                    Rectangle menuButtonLocation = new Rectangle(0, 0, _menuButtonSize.Request.Width, _menuButtonSize.Request.Height);

                    if (MenuButton.Bounds != menuButtonLocation)
                    {
                        LayoutChildIntoBoundingRegion(MenuButton, menuButtonLocation);
                    }
                }
                else
                {
                    LayoutChildIntoBoundingRegion(MenuButton, new Rectangle(0, 0, 0, 0));
                }
            }

            //
            // Layout MainMenu
            //
            Rectangle newMainMenuLocation = new Rectangle();
            if (MainMenu != null)
            {
                newMainMenuLocation = new Rectangle(0, 0, _visibleMainMenuWidth, height);
            }
            else
            {
                newMainMenuLocation = new Rectangle();
            }

            if (newMainMenuLocation != _mainMenuContainer.Bounds)
            {
                LayoutChildIntoBoundingRegion(_mainMenuContainer, newMainMenuLocation);
            }

            //
            // Layout SubMenu
            //
            Rectangle newSubMenuLocation = new Rectangle();
            if (SubMenu != null)
            {
                newSubMenuLocation = new Rectangle(0, 0, _visibleSubMenuWidth, height);
            }
            else
            {
                newSubMenuLocation = new Rectangle();
            }

            if (newSubMenuLocation != _subMenuContainer.Bounds)
            {
                LayoutChildIntoBoundingRegion(_subMenuContainer, newSubMenuLocation);
            }

            //
            // Layout Content
            //
            if (Content != null)
            {
                Rectangle newContentLocation = new Rectangle(0, 0, GetContentWidth(width), height);
                if (newContentLocation != Content.Bounds)
                {
                    LayoutChildIntoBoundingRegion(Content, newContentLocation);
                }
            }

            LayoutChildIntoBoundingRegion(_modalBackground, new Rectangle(0, 0, width, height));
        }

        /// <summary>
        /// Update parts translations
        /// </summary>
        private void UpdateTranslations(double availableWidth, double availableHeight)
        {
            PartsLocation locations = GetLocation(IsMainMenuOpen, IsSubMenuOpen, availableWidth, availableHeight);
            _mainMenuContainer.TranslationX = locations.MainMenuLocation.X;
            _subMenuContainer.TranslationX = locations.SubMenuLocation.X;

            if (Content != null)
            {
                Content.TranslationX = GetContentTranslationX(availableWidth);
            }

            if (MenuButton != null && Children.Contains(MenuButton))
            {
                MenuButton.TranslationX = GetMenuButtonTranslationX(availableWidth);
            }
        }

        /// <summary>
        /// Update visible mainmenu and submenu widths
        /// </summary>
        private double GetMainMenuVisibleWidth(double availableWidth)
        {
            double visibleMainMenuWidth = 0;

            double actualMainMenuWidth = MainMenuWidth > 0 ? MainMenuWidth : availableWidth - ActualMainMenuSpacing;
            actualMainMenuWidth = MainMenu != null ? actualMainMenuWidth : 0;

            if (IsMainMenuOpen)
            {
                visibleMainMenuWidth = actualMainMenuWidth;
            }
            else
            {
                if (MainMenuCloseMode == MainMenuCloseModes.Closed)
                {
                    visibleMainMenuWidth = actualMainMenuWidth;
                }
                else
                {
                    visibleMainMenuWidth = MainMenu != null ? MainMenuMinimalizedWidth : 0;
                }
            }

            return visibleMainMenuWidth;
        }

        #endregion

        #region MenuButton

        /// <summary>
        /// Event when main menu button is toggled
        /// </summary>
        private void OnMainMenuButtonToggledChanged(object sender, bool isToggled)
        {
            IsMainMenuOpen = isToggled;
        }

        /// <summary>
        /// Set menu button IsToggled state without event raises
        /// </summary>
        private void SetMenuButtonIsToggled(bool isToggled)
        {
            if (MenuButton != null && MenuButton is IToggable button && button.IsToggled != isToggled)
            {
                button.IsToggledChanged -= OnMainMenuButtonToggledChanged;
                button.IsToggled = isToggled;
                button.IsToggledChanged += OnMainMenuButtonToggledChanged;
            }
        }

        /// <summary>
        /// Add / remove MenuButton to / from children
        /// </summary>
        private void OnMenuButtonLocationChanged(MenuButtonLocations? oldLocation, MenuButtonLocations newLocation)
        {
            if (MenuButton != null && Children != null)
            {
                if (oldLocation == MenuButtonLocations.None)
                {
                    Children.Add(MenuButton);
                }
                else if (newLocation == MenuButtonLocations.None)
                {
                    Children.Remove(MenuButton);
                }
                else if (newLocation != MenuButtonLocations.None)
                {
                    Children.Add(MenuButton);
                }

                if (Children.Contains(MenuButton))
                {
                    MenuButton.TranslationX = GetMenuButtonTranslationX(Width);
                }
            }
        }

        #endregion

        #region Panning

        /// <summary>
        /// Handle pan gesture for menu and content
        /// </summary>
        public void OnTouchHandled(FlyoutMenuTouchEventArgs e)
        {
            double mainMenuActualWidth = MainMenuWidth > 0 ? MainMenuWidth : Width - ActualMainMenuSpacing;

            // Pan started
            if (e.Type == TouchActionType.Pressed)
            {
                _didPan = false;
                _wasModalActive = IsModalActive(IsMainMenuOpen, IsSubMenuOpen);
            }
            // During pan, update Content and MainMenu width and X
            else if (e.Type == TouchActionType.Move && e.IsPressed)
            {
                if (_wasModalActive == false)
                {
                    return;
                }

                IsPanning = true;
                double deltaX = e.DeltaX;
                double previousX = e.X - e.DeltaX;

                double actualPanDelta = deltaX;

                if (MenuLocation == HorizontalLocations.Left && previousX > mainMenuActualWidth)
                {
                    actualPanDelta += (previousX - mainMenuActualWidth);
                }
                else if (MenuLocation == HorizontalLocations.Right && previousX < Width - mainMenuActualWidth)
                {
                    actualPanDelta += (previousX - (Width - mainMenuActualWidth));
                }

                if (IsMainMenuOpen && (MainMenuOpenMode == MainMenuOpenModes.Floating || ContentBehavior == ContentBehaviors.Move))
                {
                    if (MainMenuCloseMode == MainMenuCloseModes.Closed)
                    {
                        if (MenuLocation == HorizontalLocations.Left)
                        {
                            if (IsMainMenuOpen && (e.X < mainMenuActualWidth || previousX < mainMenuActualWidth && e.X > mainMenuActualWidth))
                            {
                                double mainMenuX = _mainMenuContainer.TranslationX + actualPanDelta;
                                mainMenuX = Math.Min(0, mainMenuX);
                                mainMenuX = Math.Max(-mainMenuActualWidth, mainMenuX);
                                _mainMenuContainer.TranslationX = mainMenuX;
                                _didPan = true;
                            }
                        }
                        else
                        {
                            if (IsMainMenuOpen && (e.X >= Width - mainMenuActualWidth || previousX > Width - mainMenuActualWidth))
                            {
                                double mainMenuX = _mainMenuContainer.TranslationX + actualPanDelta;
                                mainMenuX = Math.Max(Width - mainMenuActualWidth, mainMenuX);
                                mainMenuX = Math.Min(Width, mainMenuX);
                                _mainMenuContainer.TranslationX = mainMenuX;
                                _didPan = true;
                            }
                        }
                    }
                    else
                    {
                        if (MenuLocation == HorizontalLocations.Left)
                        {
                            if (IsMainMenuOpen && (e.X < mainMenuActualWidth || previousX < mainMenuActualWidth))
                            {
                                double mainMenuWidth = _visibleMainMenuWidth + actualPanDelta;
                                mainMenuWidth = Math.Min(mainMenuActualWidth, mainMenuWidth);
                                mainMenuWidth = Math.Max(MainMenuMinimalizedWidth, mainMenuWidth);
                                _visibleMainMenuWidth = mainMenuWidth;
                                _didPan = true;
                            }
                        }
                        else
                        {
                            if (IsMainMenuOpen && (e.X > Width - mainMenuActualWidth || previousX > Width - mainMenuActualWidth))
                            {
                                double mainMenuWidth = _visibleMainMenuWidth - actualPanDelta;
                                mainMenuWidth = Math.Min(mainMenuActualWidth, mainMenuWidth);
                                mainMenuWidth = Math.Max(MainMenuMinimalizedWidth, mainMenuWidth);
                                _visibleMainMenuWidth = mainMenuWidth;
                                _mainMenuContainer.TranslationX = Width - _visibleMainMenuWidth;
                                _didPan = true;
                            }
                        }
                    }
                }

                if (IsSubMenuOpen && SubMenuOpenMode == SubMenuOpenModes.Floating || ContentBehavior == ContentBehaviors.Move)
                {
                    if (MenuLocation == HorizontalLocations.Left)
                    {
                        if (IsSubMenuOpen)
                        {
                            if (e.X < _visibleMainMenuWidth + SubMenuWidth || previousX < _visibleMainMenuWidth + SubMenuWidth)
                            {
                                double subMenuX = _subMenuContainer.TranslationX + deltaX;
                                subMenuX = Math.Min(_visibleMainMenuWidth, subMenuX);
                                subMenuX = Math.Max(MainMenuMinimalizedWidth - SubMenuWidth, subMenuX);
                                _subMenuContainer.TranslationX = subMenuX;
                                _didPan = true;
                            }
                        }
                        else
                        {
                            _subMenuContainer.TranslationX = _mainMenuContainer.TranslationX + _visibleMainMenuWidth - SubMenuWidth;
                        }
                    }
                    else
                    {
                        if (IsSubMenuOpen)
                        {
                            if (e.X > Width - _visibleMainMenuWidth - SubMenuWidth || previousX > Width - _visibleMainMenuWidth - SubMenuWidth)
                            {
                                double subMenuX = _subMenuContainer.TranslationX + deltaX;
                                subMenuX = Math.Max(Width - _visibleMainMenuWidth - SubMenuWidth, subMenuX);
                                subMenuX = Math.Min(Width, subMenuX);
                                _subMenuContainer.TranslationX = subMenuX;
                                _didPan = true;
                            }
                        }
                        else
                        {
                            _subMenuContainer.TranslationX = _mainMenuContainer.TranslationX;
                        }
                    }
                }

                if (Content != null)
                {
                    Content.TranslationX = GetContentTranslationX(Width);
                }

                if (MenuButton != null)
                {
                    MenuButton.TranslationX = GetMenuButtonTranslationX(Width);
                }

                // Invalidate layout
                InvalidateLayout();
            }
            // Pan ended
            else if (e.Type == TouchActionType.Released || e.Type == TouchActionType.Cancelled)
            {
                IsPanning = false;

                if (_wasModalActive == false)
                {
                    return;
                }

                double startX = e.X - e.TotalX;
                double startY = e.Y - e.TotalY;

                Rectangle subMenuLocation = new Rectangle(_subMenuContainer.TranslationX, 0, _visibleSubMenuWidth, _subMenuContainer.Bounds.Height);
                Rectangle mainMenuLocation = new Rectangle(_mainMenuContainer.TranslationX, 0, _visibleMainMenuWidth, _mainMenuContainer.Bounds.Height);

                // If tapped outside of the main menu
                if (mainMenuLocation.Contains(new Point(e.X, e.Y)) == false &&
                    mainMenuLocation.Contains(new Point(startX, startY)) == false &&
                    subMenuLocation.Contains(new Point(e.X, e.Y)) == false &&
                    subMenuLocation.Contains(new Point(startX, startY)) == false &&
                    _didPan == false)
                {
                    if (IsSubMenuOpen)
                    {
                        IsSubMenuOpen = false;
                    }
                    else if (IsMainMenuOpen)
                    {
                        IsMainMenuOpen = false;
                    }
                }
                // If done panning
                else if (e.TotalX != 0 && (IsSubMenuOpen || IsMainMenuOpen))
                {
                    bool isMainMenuOpen = true;
                    if ((MenuLocation == HorizontalLocations.Left && mainMenuLocation.Right < mainMenuActualWidth / 2) ||
                        (MenuLocation == HorizontalLocations.Right && mainMenuLocation.Left > Width - (mainMenuActualWidth / 2)))
                    {
                        isMainMenuOpen = false;
                    }

                    bool isSubMenuOpen = true;
                    if ((MenuLocation == HorizontalLocations.Left && subMenuLocation.Left < _visibleMainMenuWidth - (SubMenuWidth / 2)) ||
                        (MenuLocation == HorizontalLocations.Right && subMenuLocation.Left > Width - _visibleMainMenuWidth - (SubMenuWidth / 2)))
                    {
                        isSubMenuOpen = false;
                    }

                    Task t = SetIsOpenAsync(isMainMenuOpen, isSubMenuOpen);
                }
            }
        }

        public void OnSwiped(SwipeDirection direction, double velocity, Point endPoint)
        {
            if (direction == SwipeDirection.Left && MenuLocation == HorizontalLocations.Left && endPoint.X < _visibleMainMenuWidth && velocity > 1)
            {
                IsMainMenuOpen = false;
            }
            else if (direction == SwipeDirection.Right && MenuLocation == HorizontalLocations.Right && endPoint.X > Width - _visibleMainMenuWidth && velocity > 1)
            {
                IsMainMenuOpen = false;
            }
        }

        /// <summary>
        /// Calculate modal backgroud opacity based on parts locations pan.
        /// </summary>
        private double CalculateModalOpacity()
        {
            double percent = 0;

            /*
            if (IsMainMenuOpen && (MainMenuOpenMode == MainMenuOpenModes.Floating || ContentBehavior == ContentBehaviors.Move))
            {
                if (MainMenuCloseMode == MainMenuCloseModes.Minimalized && MenuLocation == HorizontalLocations.Left)
                {
                    percent = (m_mainMenuContainer.Width - MainMenuMinimalizedWidth) / (MainMenuWidth - MainMenuMinimalizedWidth);
                }
                else
                {
                    percent = (Math.Abs(m_mainMenuContainer.TranslationX) - MainMenuWidth) / (Math.Abs(max.X) - Math.Abs(min.X));

                    if (MenuLocation == HorizontalLocations.Right)
                    {
                        percent = 1 - percent;
                    }
                }
            }
            else if (IsSubMenuOpen && (SubMenuOpenMode == SubMenuOpenModes.Floating || ContentBehavior == ContentBehaviors.Move))
            {
                Rectangle current = Rectangle.Zero;
                Rectangle max = Rectangle.Zero;
                Rectangle min = Rectangle.Zero;
                GetSubMenuMaxAndMinBounds(out current, out min, out max);

                percent = (m_subMenuContainer.X - min.X) / ActualSubMenuWidth;

                if (MenuLocation == HorizontalLocations.Right)
                {
                    percent = 1 - percent;
                }
            }
            */
            return percent * MaxModalDarknessOpacity;
        }

        #endregion

        #region Modal background

        /// <summary>
        /// Create modal background view
        /// </summary>
        private BoxView CreateModalBoxView()
        {
            BoxView box = new BoxView();

            if (Device.RuntimePlatform == Device.Android)
            {
                box.GestureRecognizers.Add(new TapGestureRecognizer());
            }

            if (IsModalActive(IsMainMenuOpen, IsSubMenuOpen))
            {
                box.Opacity = MaxModalDarknessOpacity;
                box.InputTransparent = false;
            }
            else
            {
                box.Opacity = 0;
                box.InputTransparent = true;
            }

            Binding bind = new Binding("ModalColor");
            bind.Source = this;
            box.SetBinding(BoxView.BackgroundColorProperty, bind);

            return box;
        }

        private bool IsModalActive(bool isMainMenuOpen, bool isSubMenuOpen)
        {
            return (isSubMenuOpen && (SubMenuOpenMode == SubMenuOpenModes.Floating || ContentBehavior == ContentBehaviors.Move)) ||
                   (isMainMenuOpen && (MainMenuOpenMode == MainMenuOpenModes.Floating || ContentBehavior == ContentBehaviors.Move));
        }

        /// <summary>
        /// Event when modal background is tapped
        /// </summary>
        private void OnModalBackgroundTapped(object sender, EventArgs e)
        {
            if (SubMenu != null && IsSubMenuOpen)
            {
                IsSubMenuOpen = false;
            }
            else if (MainMenu != null && IsMainMenuOpen)
            {
                IsMainMenuOpen = false;
            }
        }

        private void UpdateModalBackground()
        {
            if (_modalBackground == null)
            {
                return;
            }

            AnimationExtensions.AbortAnimation(this, _modalAnimationName);

            if (IsModalActive(IsMainMenuOpen, IsSubMenuOpen))
            {
                _modalBackground.Opacity = MaxModalDarknessOpacity;
                _modalBackground.InputTransparent = false;
            }
            else
            {
                _modalBackground.Opacity = 0;
                _modalBackground.InputTransparent = true;
            }
        }

        #endregion

        #region Open / Close

        /// <summary>
        /// Set 'IsMainMenuOpen' value async
        /// </summary>
        public async Task SetIsMainMenuOpenAsync(bool isOpen)
        {
            await SetIsOpenAsync(isOpen, IsSubMenuOpen);
        }

        /// <summary>
        /// Set 'IsSubMenuOpen' value async
        /// </summary>
        public async Task SetIsSubMenuOpenAsync(bool isOpen)
        {
            await SetIsOpenAsync(IsMainMenuOpen, isOpen);
        }

        /// <summary>
        /// Change menu states
        /// </summary>
        private async Task SetIsOpenAsync(bool isMainMenuOpen, bool isSubMenuOpen)
        {
            if (Width < 0 || Height < 0)
            {
                return;
            }

            SetMenuButtonIsToggled(isMainMenuOpen);

            if (Children == null || _ignoreIsOpenAsync)
            {
                // If not measured
                if (_modalBackground != null && _ignoreIsOpenAsync == false)
                {
                    if (IsModalActive(isMainMenuOpen, isSubMenuOpen))
                    {
                        _modalBackground.Opacity = MaxModalDarknessOpacity;
                        _modalBackground.InputTransparent = false;
                    }
                    else
                    {
                        _modalBackground.Opacity = 0;
                        _modalBackground.InputTransparent = true;
                    }
                }

                return;
            }

            bool didIsMainMenuOpenChanged = _previousIsMainMenuOpen != isMainMenuOpen;
            bool didIsSubMenuOpenChanged = _previousIsSubMenuOpen != isSubMenuOpen;
            _previousIsMainMenuOpen = isMainMenuOpen;
            _previousIsSubMenuOpen = isSubMenuOpen;

            _ignoreIsOpenAsync = true;
            IsMainMenuOpen = isMainMenuOpen;
            IsSubMenuOpen = isSubMenuOpen;
            _ignoreIsOpenAsync = false;

            if (didIsMainMenuOpenChanged)
            {
                IsMainMenuOpenChanging?.Invoke(this, isMainMenuOpen);
            }
            if (didIsSubMenuOpenChanged)
            {
                IsSubMenuOpenChanging?.Invoke(this, isSubMenuOpen);
            }

            PartsLocation locations = GetLocation(isMainMenuOpen, isSubMenuOpen, Width, Height);

            Animation subMenuAnimation = null;
            Animation mainMenuAnimation = null;
            Animation modalAnimation = null;

            if (SubMenu != null)
            {
                subMenuAnimation = CreateSubMenuAnimation(locations);
            }

            if (MainMenu != null)
            {
                mainMenuAnimation = CreateMainMenuAnimation(locations);
            }

            if (IsModalActive(isMainMenuOpen, isSubMenuOpen) == true && _modalBackground.Opacity < MaxModalDarknessOpacity)
            {
                _modalBackground.InputTransparent = false;
                _modalBackground.Opacity = double.IsNaN(_modalBackground.Opacity) ? 0 : _modalBackground.Opacity;
                modalAnimation = new Animation(d => _modalBackground.Opacity = d, _modalBackground.Opacity, MaxModalDarknessOpacity);
            }
            if (IsModalActive(isMainMenuOpen, isSubMenuOpen) == false && _modalBackground.Opacity > 0)
            {
                _modalBackground.InputTransparent = true;
                _modalBackground.Opacity = double.IsNaN(_modalBackground.Opacity) ? 1 : _modalBackground.Opacity;
                modalAnimation = new Animation(d => _modalBackground.Opacity = d, _modalBackground.Opacity, 0);
            }

            _subMenuContainer.IsShadowVisible = isSubMenuOpen && IsModalActive(isMainMenuOpen, isSubMenuOpen);

            List <Task> tasks = new List<Task>();

            uint subMenuDuration = 0;
            uint mainMenuDuration = 0;

            AnimationExtensions.AbortAnimation(this, _mainMenuAnimationName);
            AnimationExtensions.AbortAnimation(this, _subMenuAnimationName);
            AnimationExtensions.AbortAnimation(this, _modalAnimationName);

            if (subMenuAnimation != null)
            {
                Easing subMenuEasing = null;

                if (isSubMenuOpen)
                {
                    subMenuDuration = (uint)SubMenuOpeningDuration;
                    subMenuEasing = SubMenuOpeningEasing;
                }
                else
                {
                    subMenuDuration = (uint)SubMenuClosingDuration;
                    subMenuEasing = SubMenuClosingEasing;
                }

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                subMenuAnimation.Commit(this, _subMenuAnimationName, 64, subMenuDuration, subMenuEasing, (d, b) => tcs.SetResult(true));
                tasks.Add(tcs.Task);
            }

            if (mainMenuAnimation != null)
            {
                Easing mainMenuEasing = null;

                if (isSubMenuOpen)
                {
                    mainMenuDuration = (uint)MainMenuOpeningDuration;
                    mainMenuEasing = MainMenuOpeningEasing;
                }
                else
                {
                    mainMenuDuration = (uint)MainMenuClosingDuration;
                    mainMenuEasing = MainMenuClosingEasing;
                }

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                mainMenuAnimation.Commit(this, _mainMenuAnimationName, 64, mainMenuDuration, mainMenuEasing, (d, b) => tcs.SetResult(true));
                tasks.Add(tcs.Task);
            }

            if (modalAnimation != null)
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                modalAnimation.Commit(this, _modalAnimationName, 64, Math.Max(subMenuDuration, mainMenuDuration), finished: (d, b) => tcs.SetResult(true));
                tasks.Add(tcs.Task);
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }

            if (didIsMainMenuOpenChanged)
            {
                IsMainMenuOpenChanged?.Invoke(this, isMainMenuOpen);
            }
            if (didIsSubMenuOpenChanged)
            {
                IsSubMenuOpenChanged?.Invoke(this, isSubMenuOpen);
            }
        }

        /// <summary>
        /// Create MainMenu animation to target location
        /// </summary>
        private Animation CreateMainMenuAnimation(PartsLocation targetLocation)
        {
            Animation anim = new Animation();

            if (_mainMenuContainer.Bounds.Width.Equals(targetLocation.MainMenuLocation.Width) == false)
            {
                anim.Add(0, 1, new Animation(d =>
                {
                    _visibleMainMenuWidth = d;
                    Content.TranslationX = GetContentTranslationX(Width);

                    if (MenuButton != null)
                    {
                        MenuButton.TranslationX = GetMenuButtonTranslationX(Width);
                    }

                    InvalidateLayout();

                }, _mainMenuContainer.Bounds.Width, targetLocation.MainMenuLocation.Width));
            }

            if (_mainMenuContainer.TranslationX.Equals(targetLocation.MainMenuLocation.X) == false)
            {
                anim.Add(0, 1, new Animation(d =>
                {
                    _mainMenuContainer.TranslationX = d;
                    Content.TranslationX = GetContentTranslationX(Width);

                    if (MenuButton != null)
                    {
                        MenuButton.TranslationX = GetMenuButtonTranslationX(Width);
                    }

                    if (MainMenuOpenMode == MainMenuOpenModes.Open && ContentBehavior == ContentBehaviors.Resize)
                    {
                        InvalidateLayout();
                    }
                }, _mainMenuContainer.TranslationX, targetLocation.MainMenuLocation.X));
            }

            if (anim.HasSubAnimations())
            {
                return anim;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Create SubMenu animation to target location
        /// </summary>
        private Animation CreateSubMenuAnimation(PartsLocation targetLocation)
        {
            Animation anim = new Animation();

            if (_subMenuContainer.TranslationX.Equals(targetLocation.SubMenuLocation.X) == false)
            {
                anim.Add(0, 1, new Animation(d =>
                {
                    _subMenuContainer.TranslationX = d;
                    Content.TranslationX = GetContentTranslationX(Width);

                    if (MenuButton != null)
                    {
                        MenuButton.TranslationX = GetMenuButtonTranslationX(Width);
                    }

                    if (SubMenuOpenMode == SubMenuOpenModes.Open && ContentBehavior == ContentBehaviors.Resize)
                    {
                        InvalidateLayout();
                    }

                }, _subMenuContainer.TranslationX, targetLocation.SubMenuLocation.X));
            }

            if (anim.HasSubAnimations())
            {
                return anim;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get Menu, Content and Submenu locations based on giving states
        /// </summary>
        /// <returns>Locations for all parts</returns>
        /// <param name="isMainMenuOpen">Is main menu opened</param>
        /// <param name="isSubMenuOpen">Is sub menu opened</param>
        /// <param name="height">Available height</param>
        private PartsLocation GetLocation(bool isMainMenuOpen, bool isSubMenuOpen, double width, double height)
        {
            PartsLocation location = new PartsLocation();

            double mainMenuWidth = MainMenuWidth > 0 ? MainMenuWidth : width - ActualMainMenuSpacing;
            mainMenuWidth = MainMenu != null ? mainMenuWidth : 0;

            double subMenuWidth = SubMenuWidth > 0 ? SubMenuWidth : width - ActualMainMenuSpacing;
            subMenuWidth = SubMenu != null ? subMenuWidth : 0;

            if (isMainMenuOpen == true && isSubMenuOpen == true)
            {
                if (MenuLocation == HorizontalLocations.Left)
                {
                    if (Device.RuntimePlatform == Device.UWP)
                    {
                        location.MainMenuLocation = new Rectangle(0, 0, mainMenuWidth, height);
                        location.SubMenuLocation = new Rectangle(mainMenuWidth, 0, subMenuWidth, height);
                    }
                    else
                    {
                        location.MainMenuLocation = new Rectangle(0, 0, mainMenuWidth, height);
                        location.SubMenuLocation = new Rectangle(mainMenuWidth, 0, subMenuWidth, height);

                        // Do default layout
                        if (_subMenuContainer.Bounds.Width != subMenuWidth)
                        {
                            _visibleSubMenuWidth = location.SubMenuLocation.Width;
                            LayoutChildIntoBoundingRegion(_subMenuContainer, location.SubMenuLocation);
                        }
                    }
                }
                else
                {
                    if (Device.RuntimePlatform == Device.UWP)
                    {
                        location.MainMenuLocation = new Rectangle(width - mainMenuWidth, 0, mainMenuWidth, height);
                        location.SubMenuLocation = new Rectangle(width - mainMenuWidth - subMenuWidth, 0, subMenuWidth, height);
                    }
                    else
                    {
                        location.MainMenuLocation = new Rectangle(width, 0, mainMenuWidth, height);
                        location.SubMenuLocation = new Rectangle(width - subMenuWidth, 0, subMenuWidth, height);
                    }
                }
            }
            else if (isMainMenuOpen == false && isSubMenuOpen == true)
            {
                if (MenuLocation == HorizontalLocations.Left)
                {
                    if (MainMenuCloseMode == MainMenuCloseModes.Closed)
                    {
                        location.MainMenuLocation = new Rectangle(-mainMenuWidth, 0, mainMenuWidth, height);
                        location.SubMenuLocation = new Rectangle(0, 0, subMenuWidth, height);
                    }
                    else
                    {
                        location.MainMenuLocation = new Rectangle(0, 0, MainMenuMinimalizedWidth, height);
                        location.SubMenuLocation = new Rectangle(MainMenuMinimalizedWidth, 0, subMenuWidth, height);
                    }
                }
                else
                {
                    if (MainMenuCloseMode == MainMenuCloseModes.Closed)
                    {
                        location.MainMenuLocation = new Rectangle(width, 0, mainMenuWidth, height);
                        location.SubMenuLocation = new Rectangle(width - subMenuWidth, 0, subMenuWidth, height);
                    }
                    else
                    {
                        location.MainMenuLocation = new Rectangle(width - MainMenuMinimalizedWidth, 0, MainMenuMinimalizedWidth, height);
                        location.SubMenuLocation = new Rectangle(width - MainMenuMinimalizedWidth - subMenuWidth, 0, subMenuWidth, height);
                    }
                }
            }
            else if (isMainMenuOpen == true && isSubMenuOpen == false)
            {
                if (MenuLocation == HorizontalLocations.Left)
                {
                    location.MainMenuLocation = new Rectangle(0, 0, mainMenuWidth, height);
                    location.SubMenuLocation = new Rectangle(mainMenuWidth - subMenuWidth, 0, subMenuWidth, height);
                }
                else
                {
                    location.MainMenuLocation = new Rectangle(width - mainMenuWidth, 0, mainMenuWidth, height);
                    location.SubMenuLocation = new Rectangle(width - mainMenuWidth, 0, subMenuWidth, height);
                }
            }
            else if (isMainMenuOpen == false && isSubMenuOpen == false)
            {
                if (MainMenuCloseMode == MainMenuCloseModes.Minimalized)
                {
                    if (MenuLocation == HorizontalLocations.Left)
                    {
                        location.MainMenuLocation = new Rectangle(0, 0, MainMenuMinimalizedWidth, height);
                        location.SubMenuLocation = new Rectangle(-subMenuWidth + MainMenuMinimalizedWidth, 0, subMenuWidth, height);
                    }
                    else
                    {
                        location.MainMenuLocation = new Rectangle(width - MainMenuMinimalizedWidth, 0, MainMenuMinimalizedWidth, height);
                        location.SubMenuLocation = new Rectangle(width - MainMenuMinimalizedWidth, 0, subMenuWidth, height);
                    }
                }
                else if (MainMenuCloseMode == MainMenuCloseModes.Closed)
                {
                    if (MenuLocation == HorizontalLocations.Left)
                    {
                        location.MainMenuLocation = new Rectangle(-mainMenuWidth, 0, mainMenuWidth, height);
                        location.SubMenuLocation = new Rectangle(-subMenuWidth, 0, subMenuWidth, height);
                    }
                    else
                    {
                        location.MainMenuLocation = new Rectangle(width, 0, mainMenuWidth, height);
                        location.SubMenuLocation = new Rectangle(width, 0, subMenuWidth, height);
                    }
                }
            }

            return location;
        }

        #endregion

        #region Width changes

        private void OnSubMenuWidthChanged(double width)
        {
            if (_subMenuContainer != null)
            {
                _subMenuContainer.ChildrenWidth = width;
            }
        }

        private void OnMainMenuWidthChanged(double width)
        {
            if (_mainMenuContainer != null)
            {
                _mainMenuContainer.ChildrenWidth = width;
            }
        }

        #endregion

        #region Parts

        private void OnContentChanged(View oldContent, View newContent)
        {
            if (Children == null)
            {
                return;
            }

            if (oldContent != null)
            {
                Children.Remove(oldContent);
            }

            if (newContent != null)
            {
                if (Width > 0)
                {
                    Content.TranslationX = GetContentTranslationX(Width);
                }

                Children.Insert(0, newContent);
            }
        }

        private void OnMainMenuChanged(View oldMainMenu, View newMainMenu)
        {
            if (Children == null)
            {
                return;
            }

            if (_mainMenuContainer == null)
            {
                _mainMenuContainer = new Container();
                _mainMenuContainer.IsClippedToBounds = true;
                _mainMenuContainer.LineThickness = BorderThickness;
                _mainMenuContainer.LineColor = BorderColor;
                _mainMenuContainer.LineLocation = MenuLocation == HorizontalLocations.Left ? HorizontalLocations.Right : HorizontalLocations.Left; ;
                Children.Add(_mainMenuContainer);
            }

            if (oldMainMenu != null)
            {
                _mainMenuContainer.Children.Remove(oldMainMenu);
            }
            if (newMainMenu != null && _mainMenuContainer.Children.Contains(newMainMenu) == false)
            {
                _mainMenuContainer.ChildrenWidth = MainMenuWidth;
                _mainMenuContainer.Children.Insert(0, newMainMenu);
            }
        }

        private void OnSubMenuChanged(View oldSubMenu, View newSubMenu)
        {
            if (Children == null)
            {
                return;
            }

            if (_subMenuContainer == null)
            {
                _subMenuContainer = new Container();
                _subMenuContainer.LineThickness = BorderThickness;
                _subMenuContainer.LineColor = BorderColor;
                _subMenuContainer.LineLocation = MenuLocation == HorizontalLocations.Left ? HorizontalLocations.Right : HorizontalLocations.Left;

                if (_mainMenuContainer != null)
                {
                    Children.Insert(Children.IndexOf(_mainMenuContainer), _subMenuContainer);
                }
                else
                {
                    Children.Add(_subMenuContainer);
                }
            }

            _visibleSubMenuWidth = newSubMenu != null ? SubMenuWidth : 0;

            if (oldSubMenu != null)
            {
                _subMenuContainer.Children.Remove(oldSubMenu);
            }
            if (newSubMenu != null && _subMenuContainer.Children.Contains(newSubMenu) == false)
            {
                PartsLocation locations = GetLocation(IsMainMenuOpen, IsSubMenuOpen, Width, Height);
                _subMenuContainer.TranslationX = locations.SubMenuLocation.X;

                _subMenuContainer.ChildrenWidth = SubMenuWidth;
                _subMenuContainer.Children.Insert(0, newSubMenu);
            }
        }

        private void OnMenuButtonChanged(View oldMainMenuButton, View newMainMenuButton)
        {
            if (Children == null)
            {
                return;
            }

            if (oldMainMenuButton != null)
            {
                Children.Remove(oldMainMenuButton);

                if (oldMainMenuButton is IToggable toggable)
                {
                    toggable.IsToggledChanged -= OnMainMenuButtonToggledChanged;
                }
            }
            if (newMainMenuButton != null)
            {
                if (MenuButtonLocation != MenuButtonLocations.None)
                {
                    Children.Add(newMainMenuButton);
                }

                if (newMainMenuButton is IToggable toggable)
                {
                    toggable.IsToggled = IsMainMenuOpen;
                    toggable.IsToggledChanged -= OnMainMenuButtonToggledChanged;
                    toggable.IsToggledChanged += OnMainMenuButtonToggledChanged;
                }
                else
                {
                    throw new Exception("FlyoutMenu menu button is not implementing IToggable interface!");
                }
            }
        }

        private void OnMainMenuTemplateChanged(DataTemplate dataTemplate)
        {
            if (dataTemplate != null)
            {
                MainMenu = dataTemplate.CreateContent() as View;
            }
            else
            {
                MainMenu = null;
            }
        }

        private void OnSubMenuTemplateChanged(DataTemplate dataTemplate)
        {
            if (dataTemplate != null)
            {
                SubMenu = dataTemplate.CreateContent() as View;
            }
            else
            {
                SubMenu = null;
            }
        }

        private void OnMenuButtonTemplateChanged(DataTemplate dataTemplate)
        {
            if (dataTemplate != null)
            {
                MenuButton = dataTemplate.CreateContent() as View;
            }
            else
            {
                MenuButton = null;
            }
        }

        #endregion

        private class PartsLocation
        {
            public Rectangle MainMenuLocation { get; set; }
            public Rectangle SubMenuLocation { get; set; }
            public Rectangle ContentLocation { get; set; }
        }

        #region Containers

        private class Container : Layout<View>
        {
            private BoxView _line = null;
            private GradientView _shadowView = null;
            private HorizontalLocations _lineLocation = HorizontalLocations.Right;
            private double _lineThickness = 1.0;
            private Color _lineColor = Color.Default;
            private double _childrenWidth = 0;
            private bool _isShadowVisible = false;

            private const double _shadowWidth = 12;

            #region Properties

            public double LineThickness
            {
                get
                {
                    return _lineThickness;
                }
                set
                {
                    _lineThickness = value;
                    _line.WidthRequest = value;
                    InvalidateLayout();
                }
            }

            public Color LineColor
            {
                get
                {
                    return _lineColor;
                }
                set
                {
                    _lineColor = value;
                    _line.BackgroundColor = value;
                }
            }

            public HorizontalLocations LineLocation
            {
                get
                {
                    return _lineLocation;
                }
                set
                {
                    _lineLocation = value;
                    InvalidateLayout();
                }
            }

            public double ChildrenWidth
            {
                get
                {
                    return _childrenWidth;
                }
                set
                {
                    _childrenWidth = value;
                    InvalidateLayout();
                }
            }

            public bool IsShadowVisible
            {
                get
                {
                    return _isShadowVisible;
                }
                set
                {
                    _isShadowVisible = value;

                    if (value)
                    {
                        _shadowView.StartColor = Color.Black.MultiplyAlpha(0.3);
                    }
                    else
                    {
                        _shadowView.StartColor = Color.Transparent;
                    }
                }
            }

            #endregion

            public Container()
            {
                _line = new BoxView();
                Children.Add(_line);

                _shadowView = new GradientView();
                _shadowView.StartColor = Color.Black.MultiplyAlpha(0.3);
                _shadowView.EndColor = Color.Transparent;
                _shadowView.InputTransparent = true;
                _shadowView.Horizontal = true;
                Children.Add(_shadowView);
            }

            protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
            {
                double actualWidth = ChildrenWidth > 0 ? ChildrenWidth : widthConstraint;
                actualWidth += LineThickness;

                Size size = new Size(actualWidth, heightConstraint);
                return new SizeRequest(size, size);
            }

            protected override void LayoutChildren(double x, double y, double width, double height)
            {
                double actualWidth = ChildrenWidth > 0 ? ChildrenWidth : width;

                foreach (View child in Children)
                {
                    Rectangle location = new Rectangle();

                    if (child == _line)
                    {
                        if (LineLocation == HorizontalLocations.Right)
                        {
                            location = new Rectangle(width - LineThickness, 0, LineThickness, height);
                        }
                        else
                        {
                            location = new Rectangle(0, 0, LineThickness, height);
                        }
                    }
                    else if (child == _shadowView)
                    {
                        if (LineLocation == HorizontalLocations.Right)
                        {
                            location = new Rectangle(actualWidth, 0, _shadowWidth, height);
                        }
                        else
                        {
                            location = new Rectangle(-_shadowWidth, 0, _shadowWidth, height);
                        }
                    }
                    else
                    {
                        if (LineLocation == HorizontalLocations.Right)
                        {
                            location = new Rectangle(0, 0, actualWidth - LineThickness, height);
                        }
                        else
                        {
                            location = new Rectangle(LineThickness, 0, actualWidth - LineThickness, height);
                        }
                    }

                    if (child.Bounds != location)
                    {
                        LayoutChildIntoBoundingRegion(child, location);
                    }
                }
            }
        }

        #endregion
    }

    public enum MainMenuCloseModes
    {
        /// <summary>
        /// MainMenu is hidden
        /// </summary>
        Closed,

        /// <summary>
        /// MainMenu is visible and width is MainMenuMinimalizedWidth
        /// </summary>
        Minimalized
    }

    public enum MainMenuOpenModes
    {
        /// <summary>
        /// MainMenu is visible and it's width is MainMenuWidth. Content width might change debending on ContentBehaviors.
        /// </summary>
        Open,

        /// <summary>
        /// MainMenu is visible over Content and it's width is MainMenuWidth
        /// </summary>
        Floating
    }

    public enum SubMenuOpenModes
    {
        /// <summary>
        /// SubMenu width is SubMenuWidth and Content width and/or location might change.
        /// </summary>
        Open,

        /// <summary>
        /// SubMenu is opened over Content and it's width is SubMenuWidth. Content width or/and location is NOT changing.
        /// </summary>
        Floating,
    }

    public enum ContentBehaviors
    {
        /// <summary>
        /// Content is moved if MainMenu or SubMenu is opened non floating mode
        /// </summary>
        Move,

        /// <summary>
        /// Content is resized if MainMenu or SubMenu is opened non floating mode
        /// </summary>
        Resize
    }

    public enum MenuButtonLocations
    {
        /// <summary>
        /// No menu button
        /// </summary>
        None,

        /// <summary>
        /// Menu button is located on top left
        /// </summary>
        TopLeft,

        /// <summary>
        /// Menu button is located on top right
        /// </summary>
        TopRight,

        /// <summary>
        /// Menu button is located on content top left. Moved when content is moved.
        /// </summary>
        ContentTopLeft,

        /// <summary>
        /// Menu button is located on content top right.
        /// </summary>
        ContentTopRight
    }

    public enum HorizontalLocations
    {
        /// <summary>
        /// MainMenu and SubMenu is located on left
        /// </summary>
        Left,

        /// <summary>
        /// MainMenu and SubMenu is located on right
        /// </summary>
        Right
    }

    public class FlyoutMenuTouchEventArgs
    {
        public double X { get; private set; }

        public double Y { get; private set; }

        public double TotalX { get; private set; }

        public double TotalY { get; private set; }

        public double DeltaX { get; private set; }

        public double DeltaY { get; private set; }

        public TouchActionType Type { get; private set; }

        public bool IsPressed { get; private set; }

        public FlyoutMenuTouchEventArgs(TouchActionType type, double x, double y, double totalX, double totalY, double deltaX, double deltaY, bool isPressed)
        {
            X = x;
            Y = y;
            TotalX = totalX;
            TotalY = totalY;
            DeltaX = deltaX;
            DeltaY = deltaY;
            Type = type;
            IsPressed = isPressed;
        }
    }
}
