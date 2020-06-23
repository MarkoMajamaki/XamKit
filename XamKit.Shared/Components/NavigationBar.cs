using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace XamKit
{
    /// <summary>
    /// NavigationBar for modal and non-modal pages
    /// </summary>
    public class NavigationBar : Layout<View>, INavigationBar
    {
        // How new page title is animated
        public enum Animations { SlideHorizontal, Fade }

        private const string _addAnimationName = "addAnimationName";
        private const string _removeAnimationName = "removeAnimationName";
        private const string _fadeContentAnimationName = "fadeContentAnimationName";
        private const string _titleVisibilityAnimationName = "titleVisibilityAnimationName";
        private const string _lineVisibilityAnimationName = "lineVisibilityAnimationName";
        private const string _backgroundColorAnimationName = "backgroundColorAnimationName";
        private const string _shadowVisibilityAnimationName = "shadowVisibilityAnimationName";

        private uint _rate = 64;
        private double _panPercent = 0;
        private double _topSpacing = 0;

        // Horizontal bottom line
        private BoxView _bottomLine = null;

        // Close button on right
        private View _closeButton = null;
        private Size _closeButtonSize = new Size();

        // Titles which are generated from 'TitleTemplate'
        private View _titleOnCenter = null;
        private View _titleOnHidden = null;
        private Size _titleOnCenterSize = new Size();
        private Size _titleOnHiddenSize = new Size();

        // Back text button which is created from 'BackTitleTemplate'
        private View _backTitle = null;
        private View _backTitleOnHidden = null;
        private Size _backTitleSize = new Size();
        private Size _backTitleOnHiddenSize = new Size();

        // Title which is created from View type of Title attached property
        private View _customTitleOnCenter = null;
        private View _customTitleOnHidden = null;
        private Size _customTitleOnCenterSize = new Size();
        private Size _customTitleOnHiddenSize = new Size();

        // Back title which is created from View type of BackTitle attached property
        private View _customBackTitle = null;
        private View _customBackTitleOnHidden = null;
        private Size _customBackTitleSize = new Size();
        private Size _customBackTitleOnHiddenSize = new Size();

        // Back button which is arrow, close or menu button
        private View _backButton = null;
        private Size _backButtonSize = new Size();

        // Any content on right
        private View _rightView = null;
        private View _rightViewOnHidden = null;
        private Size _rightViewSize = new Size();
        private Size _rightViewOnHiddenSize = new Size();

        // Custom content
        private View _customContent = null;
        private View _customContentOnHidden = null;
        private Size _customContentSize = new Size();
        private Size _customContentOnHiddenSize = new Size();

        private View _searchView = null;
        private Size _searchViewSize = new Size();

        private ContentPage _previousActivePage = null;

        private bool _ignoreInvalidation = false;

        private GradientView _shadowView = null;

        #region Properties

        private ContentPage CurrentPage
        { 
            get 
            { 
                if (NavigationHistory != null && NavigationHistory.Count > 0)
                {
                    return NavigationHistory[NavigationHistory.Count - 1];
                }
                else
                {
                    return null;
                }
            }
        }

        private ContentPage PreviousPage
        {
            get
            {
                if (NavigationHistory != null && NavigationHistory.Count > 1)
                {
                    return NavigationHistory[NavigationHistory.Count - 2];
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region BindingProperties

        /// <summary>
        /// Navigate to previous page command
        /// </summary>
        public static readonly BindableProperty BackCommandProperty =
            BindableProperty.Create("BackCommand", typeof(ICommand), typeof(NavigationBar), null);

        public ICommand BackCommand
        {
            get { return (ICommand)GetValue(BackCommandProperty); }
            set { SetValue(BackCommandProperty, value); }
        }
        
        /// <summary>
        /// Close button command for modal pages. Executed if ButtonMode is 'Close'
        /// </summary>
        public static readonly BindableProperty CloseCommandProperty =
            BindableProperty.Create("CloseCommand", typeof(ICommand), typeof(NavigationBar), null);

        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        /// <summary>
        /// Command to open main menu
        /// </summary>
        public static readonly BindableProperty MenuCommandProperty =
            BindableProperty.Create("MenuCommand", typeof(ICommand), typeof(NavigationBar), null);

        public ICommand MenuCommand
        {
            get { return (ICommand)GetValue(MenuCommandProperty); }
            set { SetValue(MenuCommandProperty, value); }
        }

        /// <summary>
        /// Navigation history list
        /// </summary>
        public static readonly BindableProperty NavigationHistoryProperty =
            BindableProperty.Create("NavigationHistory", typeof(IList<ContentPage>), typeof(NavigationBar), null, propertyChanged: OnNavigationHistoryChanged);

        private static void OnNavigationHistoryChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as NavigationBar).OnNavigationHistoryChanged(oldValue as IList<ContentPage>, newValue as IList<ContentPage>);
        }

        public IList<ContentPage> NavigationHistory
        {
            get { return (IList<ContentPage>)GetValue(NavigationHistoryProperty); }
            set { SetValue(NavigationHistoryProperty, value); }
        }

        /// <summary>
        /// Title alignment
        /// </summary>
        public static readonly BindableProperty TitleAlignmentProperty =
            BindableProperty.Create("TitleAlignment", typeof(TitleAlignments), typeof(NavigationBar), TitleAlignments.Center);

        public TitleAlignments TitleAlignment
        {
            get { return (TitleAlignments)GetValue(TitleAlignmentProperty); }
            set { SetValue(TitleAlignmentProperty, value); }
        }

        /// <summary>
        /// Is search mode active
        /// </summary>
        public static readonly BindableProperty IsSearchModeActiveProperty =
            BindableProperty.Create("IsSearchModeActive", typeof(bool), typeof(NavigationBar), false);

        public bool IsSearchModeActive
        {
            get { return (bool)GetValue(IsSearchModeActiveProperty); }
            private set { SetValue(IsSearchModeActiveProperty, value); }
        }

        /// <summary>
        /// Drop down shadow height
        /// </summary>
        public static readonly BindableProperty ShadowHeightProperty =
            BindableProperty.Create("ShadowHeight", typeof(double), typeof(NavigationBar), 0.0);

        public double ShadowHeight
        {
            get { return (double)GetValue(ShadowHeightProperty); }
            set { SetValue(ShadowHeightProperty, value); }
        }

        /// <summary>
        /// Drop down shadow height
        /// </summary>
        public static readonly BindableProperty LineHeightProperty =
            BindableProperty.Create("LineHeight", typeof(double), typeof(NavigationBar), 1.0);

        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        #endregion

        #region Templates

        /// <summary>
        /// Template to create current page title view
        /// </summary>
        public static readonly BindableProperty TitleTemplateProperty =
            BindableProperty.Create("TitleTemplate", typeof(DataTemplate), typeof(NavigationBar), null, propertyChanged: OnTitleTemplateChanged);

        private static void OnTitleTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((NavigationBar)bindable).OnTitleTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate TitleTemplate
        {
            get { return (DataTemplate)GetValue(TitleTemplateProperty); }
            set { SetValue(TitleTemplateProperty, value); }
        }

        /// <summary>
        /// Template to create previous page back title button. If implements ITappable, then go back if tapped.
        /// </summary>
        public static readonly BindableProperty BackTitleTemplateProperty =
            BindableProperty.Create("BackTitleTemplate", typeof(DataTemplate), typeof(NavigationBar), null, propertyChanged: OnBackTitleTemplateChanged);

        private static void OnBackTitleTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((NavigationBar)bindable).OnBackTitleTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate BackTitleTemplate
        {
            get { return (DataTemplate)GetValue(BackTitleTemplateProperty); }
            set { SetValue(BackTitleTemplateProperty, value); }
        }

        /// <summary>
        /// Back button template. Root element must implement INavigationBarButton interface to have all ButtonModes!
        /// </summary>
        public static readonly BindableProperty BackButtonTemplateProperty =
            BindableProperty.Create("BackButtonTemplate", typeof(DataTemplate), typeof(NavigationBar), null, propertyChanged: OnBackButtonTemplateChanged);

        private static void OnBackButtonTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((NavigationBar)bindable).OnBackButtonTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate BackButtonTemplate
        {
            get { return (DataTemplate)GetValue(BackButtonTemplateProperty); }
            set { SetValue(BackButtonTemplateProperty, value); }
        }
        
        /// <summary>
        /// Top right corner close button template for modal pages.
        /// </summary>
        public static readonly BindableProperty CloseButtonTemplateProperty =
            BindableProperty.Create("CloseButtonTemplate", typeof(DataTemplate), typeof(NavigationBar), null, propertyChanged: OnCloseButtonTemplateChanged);

        private static void OnCloseButtonTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((NavigationBar)bindable).OnCloseButtonTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate CloseButtonTemplate
        {
            get { return (DataTemplate)GetValue(CloseButtonTemplateProperty); }
            set { SetValue(CloseButtonTemplateProperty, value); }
        }

        /// <summary>
        /// Search mode template
        /// </summary>
        public static readonly BindableProperty SearchTemplateProperty =
            BindableProperty.Create("SearchTemplate", typeof(DataTemplate), typeof(NavigationBar), null, propertyChanged: OnSearchTemplateChanged);

        private static void OnSearchTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((NavigationBar)bindable).OnSearchTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate SearchTemplate
        {
            get { return (DataTemplate)GetValue(SearchTemplateProperty); }
            set { SetValue(SearchTemplateProperty, value); }
        }

        #endregion

        // Attached properties for ContentPage
        #region Attached properties

        /// <summary>
        /// Page navigation bar content. Override back title and title.
        /// </summary>
        public static readonly BindableProperty ContentProperty =
            BindableProperty.CreateAttached("Content", typeof(View), typeof(NavigationBar), null);

        public static View GetContent(BindableObject view)
        {
            return (View)view.GetValue(ContentProperty);
        }

        public static void SetContent(BindableObject view, View value)
        {
            view.SetValue(ContentProperty, value);
        }

        /// <summary>
        /// Page title
        /// </summary>
        public static readonly BindableProperty TitleProperty =
            BindableProperty.CreateAttached("Title", typeof(object), typeof(NavigationBar), null);

        public static object GetTitle(BindableObject view)
        {
            return (object)view.GetValue(TitleProperty);
        }

        public static void SetTitle(BindableObject view, object value)
        {
            view.SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Title when page is previous page.
        /// </summary>
        public static readonly BindableProperty BackTitleProperty =
            BindableProperty.CreateAttached("BackTitle", typeof(object), typeof(NavigationBar), null);

        public static object GetBackTitle(BindableObject view)
        {
            return (object)view.GetValue(BackTitleProperty);
        }

        public static void SetBackTitle(BindableObject view, object value)
        {
            view.SetValue(BackTitleProperty, value);
        }

        /// <summary>
        /// Is page title visible
        /// </summary>
        public static readonly BindableProperty IsTitleVisibleProperty =
            BindableProperty.CreateAttached("IsTitleVisible", typeof(bool), typeof(NavigationBar), true);

        public static bool GetIsTitleVisible(BindableObject view)
        {
            return (bool)view.GetValue(IsTitleVisibleProperty);
        }

        public static void SetIsTitleVisible(BindableObject view, bool value)
        {
            view.SetValue(IsTitleVisibleProperty, value);
        }

        /// <summary>
        /// Is back title visible in this page (NO effect to back button!)
        /// </summary>
        public static readonly BindableProperty IsBackTitleVisibleProperty =
            BindableProperty.CreateAttached("IsBackTitleVisible", typeof(bool), typeof(NavigationBar), true);

        public static bool GetIsBackTitleVisible(BindableObject view)
        {
            return (bool)view.GetValue(IsBackTitleVisibleProperty);
        }

        public static void SetIsBackTitleVisible(BindableObject view, bool value)
        {
            view.SetValue(IsBackTitleVisibleProperty, value);
        }

        /// <summary>
        /// Right close button visibile
        /// </summary>
        public static readonly BindableProperty IsCloseButtonVisibleProperty =
            BindableProperty.CreateAttached("IsCloseButtonVisible", typeof(bool), typeof(NavigationBar), false);

        public static bool GetIsCloseButtonVisible(BindableObject view)
        {
            return (bool)view.GetValue(IsCloseButtonVisibleProperty);
        }

        public static void SetIsCloseButtonVisible(BindableObject view, bool value)
        {
            view.SetValue(IsCloseButtonVisibleProperty, value);
        }

        /// <summary>
        /// Visibility mode: Visible = always visible, Hidden = always hidden, Scroll = visibility depends on host scroll viewer srolling
        /// </summary>
        public static readonly BindableProperty VisibilityProperty =
            BindableProperty.CreateAttached("Visibility", typeof(NavigationBarVisibility), typeof(NavigationBar), NavigationBarVisibility.Visible);

        public static NavigationBarVisibility GetVisibility(BindableObject view)
        {
            return (NavigationBarVisibility)view.GetValue(VisibilityProperty);
        }

        public static void SetVisibility(BindableObject view, NavigationBarVisibility value)
        {
            view.SetValue(VisibilityProperty, value);
        }

        /// <summary>
        /// NavigationBar background color
        /// </summary>
        public static readonly new BindableProperty BackgroundColorProperty =
            BindableProperty.CreateAttached("BackgroundColor", typeof(Color), typeof(NavigationBar), Color.Transparent);

        public static Color GetBackgroundColor(BindableObject view)
        {
            return (Color)view.GetValue(BackgroundColorProperty);
        }

        public static void SetBackgroundColor(BindableObject view, Color value)
        {
            view.SetValue(BackgroundColorProperty, value);
        }

        /// <summary>
        /// NavigationBar line color on this page
        /// </summary>
        public static readonly BindableProperty LineColorProperty =
            BindableProperty.CreateAttached("LineColor", typeof(Color), typeof(NavigationBar), Color.Transparent);

        public static Color GetLineColor(BindableObject view)
        {
            return (Color)view.GetValue(LineColorProperty);
        }

        public static void SetLineColor(BindableObject view, Color value)
        {
            view.SetValue(LineColorProperty, value);
        }

        /// <summary>
        /// NavigationBar line visibility
        /// </summary>
        public static readonly BindableProperty IsLineVisibleProperty =
            BindableProperty.CreateAttached("IsLineVisible", typeof(bool), typeof(NavigationBar), true);

        public static bool GetIsLineVisible(BindableObject view)
        {
            return (bool)view.GetValue(IsLineVisibleProperty);
        }

        public static void SetIsLineVisible(BindableObject view, bool value)
        {
            view.SetValue(IsLineVisibleProperty, value);
        }

        /// <summary>
        /// NavigationBar shadow visibility
        /// </summary>
        public static readonly BindableProperty IsShadowVisibleProperty =
            BindableProperty.CreateAttached("IsShadowVisible", typeof(bool), typeof(NavigationBar), true);

        public static bool GetIsShadowVisible(BindableObject view)
        {
            return (bool)view.GetValue(IsShadowVisibleProperty);
        }

        public static void SetIsShadowVisible(BindableObject view, bool value)
        {
            view.SetValue(IsShadowVisibleProperty, value);
        }

        /// <summary>
        /// Back button mode
        /// </summary>
        public static readonly BindableProperty BackButtonModeProperty =
            BindableProperty.CreateAttached("BackButtonMode", typeof(BackButtonModes), typeof(NavigationBar), BackButtonModes.Back);

        public static BackButtonModes GetBackButtonMode(BindableObject view)
        {
            return (BackButtonModes)view.GetValue(BackButtonModeProperty);
        }

        public static void SetBackButtonMode(BindableObject view, BackButtonModes value)
        {
            view.SetValue(BackButtonModeProperty, value);
        }

        /// <summary>
        /// NavigationBar right content
        /// </summary>
        public static readonly BindableProperty RightViewProperty =
            BindableProperty.CreateAttached("RightView", typeof(View), typeof(NavigationBar), null);

        public static View GetRightView(BindableObject view)
        {
            return (View)view.GetValue(RightViewProperty);
        }

        public static void SetRightView(BindableObject view, View value)
        {
            view.SetValue(RightViewProperty, value);
        }

        /// <summary>
        /// Is NavigationBar floating over this page content
        /// </summary>
        public static readonly BindableProperty IsFloatingProperty =
            BindableProperty.CreateAttached("IsFloating", typeof(bool), typeof(NavigationBar), false);

        public static bool GetIsFloating(BindableObject view)
        {
            return (bool)view.GetValue(IsFloatingProperty);
        }

        public static void SetIsFloating(BindableObject view, bool value)
        {
            view.SetValue(IsFloatingProperty, value);
        }

        /// <summary>
        /// Is search field visible
        /// </summary>
        public static readonly BindableProperty IsSearchModeProperty =
            BindableProperty.CreateAttached("IsSearchMode", typeof(bool), typeof(NavigationBar), false);

        public static bool GetIsSearchMode(BindableObject view)
        {
            return (bool)view.GetValue(IsSearchModeProperty);
        }

        public static void SetIsSearchMode(BindableObject view, bool value)
        {
            view.SetValue(IsSearchModeProperty, value);
        }

        /// <summary>
        /// Search text
        /// </summary>
        public static readonly BindableProperty SearchTextProperty =
            BindableProperty.CreateAttached("SearchText", typeof(string), typeof(NavigationBar), null);

        public static string GetSearchText(BindableObject view)
        {
            return (string)view.GetValue(SearchTextProperty);
        }

        public static void SetSearchText(BindableObject view, string value)
        {
            view.SetValue(SearchTextProperty, value);
        }

        /// <summary>
        /// Searchbox placeholder text
        /// </summary>
        public static readonly BindableProperty SearchPlaceholderProperty =
            BindableProperty.CreateAttached("SearchPlaceholder", typeof(string), typeof(NavigationBar), null);

        public static string GetSearchPlaceholder(BindableObject view)
        {
            return (string)view.GetValue(SearchPlaceholderProperty);
        }

        public static void SetSearchPlaceholder(BindableObject view, string value)
        {
            view.SetValue(SearchPlaceholderProperty, value);
        }

        /// <summary>
        /// Is background color animation enabled
        /// </summary>
        public static readonly BindableProperty IsBackgroundAnimationEnabledProperty =
            BindableProperty.CreateAttached("IsBackgroundAnimationEnabled", typeof(bool), typeof(NavigationBar), null);

        public static bool GetIsBackgroundAnimationEnabled(BindableObject view)
        {
            return (bool)view.GetValue(IsBackgroundAnimationEnabledProperty);
        }

        public static void SetIsBackgroundAnimationEnabled(BindableObject view, bool value)
        {
            view.SetValue(IsBackgroundAnimationEnabledProperty, value);
        }

        /// <summary>
        /// Is background color animation duration in milliseconds
        /// </summary>
        public static readonly BindableProperty BackgroundAnimationDurationProperty =
            BindableProperty.CreateAttached("BackgroundAnimationDuration", typeof(int), typeof(NavigationBar), 150);

        public static int GetBackgroundAnimationDuration(BindableObject view)
        {
            return (int)view.GetValue(BackgroundAnimationDurationProperty);
        }

        public static void SetBackgroundAnimationDuration(BindableObject view, int value)
        {
            view.SetValue(BackgroundAnimationDurationProperty, value);
        }

        #endregion

        #region Animation

        /// <summary>
        /// How new page title is animated
        /// </summary>
        public static readonly BindableProperty AnimationProperty =
            BindableProperty.Create("Animation", typeof(Animations), typeof(NavigationBar), Animations.Fade);

        public Animations Animation
        {
            get { return (Animations)GetValue(AnimationProperty); }
            set { SetValue(AnimationProperty, value); }
        }

        /// <summary>
        /// New page title animation duration
        /// </summary>
        public static readonly BindableProperty AnimationDurationProperty =
            BindableProperty.Create("AnimationDuration", typeof(int), typeof(NavigationBar), 400);

        public int AnimationDuration
        {
            get { return (int)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        /// <summary>
        /// New page title animation easing
        /// </summary>
        public static readonly BindableProperty AnimationEasingProperty =
            BindableProperty.Create("AnimationEasing", typeof(Easing), typeof(NavigationBar), Easing.CubicOut);

        public Easing AnimationEasing
        {
            get { return (Easing)GetValue(AnimationEasingProperty); }
            set { SetValue(AnimationEasingProperty, value); }
        }

        #endregion

        public NavigationBar()
        {
            NavigationHistory = new ObservableCollection<ContentPage>();
            
            OnTitleTemplateChanged(null, TitleTemplate);
            OnBackTitleTemplateChanged(null, BackTitleTemplate);
            OnBackButtonTemplateChanged(null, BackButtonTemplate);
            OnCloseButtonTemplateChanged(null, CloseButtonTemplate);

            _bottomLine = CreateLine();
            Children.Add(_bottomLine);

            _shadowView = new GradientView();
            _shadowView.StartColor = Color.Black.MultiplyAlpha(0.1);
            _shadowView.EndColor = Color.Transparent;
            _shadowView.InputTransparent = true;
            Children.Add(_shadowView);

            Binding bind = new Binding(ShadowHeightProperty.PropertyName);
            bind.Source = this;
            _shadowView.SetBinding(View.HeightRequestProperty, bind);

            if (Device.RuntimePlatform == Device.iOS)
            {
                // Memory leaks!
                RootPage.Instance.SafeAreaInsetsChanged += (s, t) =>
                {
                    _topSpacing = t.Top;
                    InvalidateMeasure();
                    InvalidateLayout();
                };
            }
        }

        #region INavigationBar

        /// <summary>
        /// Initialize with navigation history
        /// </summary>
        public void Initialize(IList<ContentPage> navigationHistory)
        {
            if (navigationHistory == null)
            {
                NavigationHistory = new List<ContentPage>();
            }
            else
            {
                NavigationHistory = navigationHistory;
            }
        }

        /// <summary>
        /// Clear while animation history without animation
        /// </summary>
        public void Clear()
        {
            NavigationHistory.Clear();
        }

        /// <summary>
        /// Update part locations when panned to previous page
        /// </summary>
        /// <param name="panX">Pan x-coordinate</param>
        public void BackPan(double panX)
        {
            if (_panPercent == 0 && panX != 0)
            {
                _ignoreInvalidation = true;
                InitializeBackAnimationHiddenParts(CurrentPage, PreviousPage);
                _ignoreInvalidation = false;

                if (Device.RuntimePlatform == Device.Android)
                {
                    LayoutChildren(0, 0, Width, Height);
                }
            }

            _panPercent = panX / Width;
            BackAnimation(_panPercent, Width, CurrentPage, PreviousPage);
        }

        #endregion

        #region Invalidation

        /// <summary>
        /// Will child add invalidate layout
        /// </summary>
        protected override bool ShouldInvalidateOnChildAdded(View child)
        {
            return base.ShouldInvalidateOnChildAdded(child) && !_ignoreInvalidation;
        }

        /// <summary>
        /// Will child remove invalidate layout
        /// </summary>
        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
            return base.ShouldInvalidateOnChildRemoved(child) && !_ignoreInvalidation;
        }

        protected override void OnChildMeasureInvalidated()
        {
            if (_ignoreInvalidation == false)
            {
                base.OnChildMeasureInvalidated();
            }
        }

        protected override void InvalidateLayout()
        {
            if (_ignoreInvalidation == false)
            {
                base.InvalidateLayout();
            }
        }

        protected override void InvalidateMeasure()
        {
            if (_ignoreInvalidation == false)
            {
                base.InvalidateMeasure();
            }
        }
        
        #endregion

        #region Measure / Layout

        /// <summary>
        /// Measure total size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return MeasureChildren(widthConstraint, heightConstraint);
        }

        /// <summary>
        /// Layout all children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            MeasureChildren(width, height);

            if (_titleOnCenter != null && Children.Contains(_titleOnCenter))
            {
                LayoutChildIntoBoundingRegion(_titleOnCenter, new Rectangle(0, _topSpacing, _titleOnCenterSize.Width, height - _topSpacing - LineHeight));               
            }
            if (_customTitleOnCenter != null && Children.Contains(_customTitleOnCenter))
            {
                LayoutChildIntoBoundingRegion(_customTitleOnCenter, new Rectangle(0, _topSpacing, _customTitleOnCenterSize.Width, height - _topSpacing - LineHeight));
            }
            if (_titleOnHidden != null && Children.Contains(_titleOnHidden))
            {
                LayoutChildIntoBoundingRegion(_titleOnHidden, new Rectangle(0, _topSpacing, _titleOnHiddenSize.Width, height - _topSpacing - LineHeight));
            }
            if (_customTitleOnHidden != null && Children.Contains(_customTitleOnHidden))
            {
                LayoutChildIntoBoundingRegion(_customTitleOnHidden, new Rectangle(0, _topSpacing, _customTitleOnHiddenSize.Width, height - _topSpacing - LineHeight));
            }
            if (_backTitle != null && Children.Contains(_backTitle))
            {
                LayoutChildIntoBoundingRegion(_backTitle, new Rectangle(0, _topSpacing, _backTitleSize.Width, height - _topSpacing - LineHeight));
            }
            if (_customBackTitle != null && Children.Contains(_customBackTitle))
            {
                LayoutChildIntoBoundingRegion(_customBackTitle, new Rectangle(0, _topSpacing, _customBackTitleSize.Width, height - _topSpacing - LineHeight));
            }
            if (_backTitleOnHidden != null && Children.Contains(_backTitleOnHidden))
            {
                LayoutChildIntoBoundingRegion(_backTitleOnHidden, new Rectangle(0, _topSpacing, _backTitleOnHiddenSize.Width, height - _topSpacing - LineHeight));
            }
            if (_customBackTitleOnHidden != null && Children.Contains(_customBackTitleOnHidden))
            {
                LayoutChildIntoBoundingRegion(_customBackTitleOnHidden, new Rectangle(0, _topSpacing, _customBackTitleOnHiddenSize.Width, height - _topSpacing - LineHeight));
            }
            if (_backButton != null && Children.Contains(_backButton))
            {
                LayoutChildIntoBoundingRegion(_backButton, new Rectangle(0, _topSpacing, _backButtonSize.Width, height - _topSpacing - LineHeight));               
            }
            if (_closeButton != null && Children.Contains(_closeButton))
            {
                LayoutChildIntoBoundingRegion(_closeButton, new Rectangle(width - _closeButtonSize.Width, _topSpacing, _closeButtonSize.Width, height - _topSpacing - LineHeight));
            }

            Size actualBackButtonSize = new Size();
            if (_backButton != null && Children.Contains(_backButton) && _backButton.InputTransparent == false)
            {
                actualBackButtonSize = _backButtonSize;
            }
            Size actualCloseButtonSize = new Size();
            if (_closeButton != null && Children.Contains(_closeButton) && _closeButton.InputTransparent == false)
            {
                actualCloseButtonSize = _closeButtonSize;
            }

            if (_rightView != null && Children.Contains(_rightView))
            {
                LayoutChildIntoBoundingRegion(_rightView, new Rectangle(width - _rightViewSize.Width - actualCloseButtonSize.Width, _topSpacing, _rightViewSize.Width, height - _topSpacing - LineHeight));
            }
            if (_rightViewOnHidden != null && Children.Contains(_rightViewOnHidden))
            {
                LayoutChildIntoBoundingRegion(_rightViewOnHidden, new Rectangle(width - _rightViewOnHiddenSize.Width - actualCloseButtonSize.Width, _topSpacing, _rightViewOnHiddenSize.Width, height - _topSpacing - LineHeight));
            }
            if (_searchView != null && Children.Contains(_searchView) && _searchView.InputTransparent == false)
            {
                LayoutChildIntoBoundingRegion(_searchView, new Rectangle(actualBackButtonSize.Width, _topSpacing, width - actualBackButtonSize.Width - actualCloseButtonSize.Width, height - _topSpacing - LineHeight));
            }
            if (_customContent != null && Children.Contains(_customContent))
            {
                LayoutChildIntoBoundingRegion(_customContent, new Rectangle(actualBackButtonSize.Width, _topSpacing, width - actualBackButtonSize.Width - actualCloseButtonSize.Width - _rightViewSize.Width, height - _topSpacing - LineHeight));
            }
            if (_customContentOnHidden != null && Children.Contains(_customContentOnHidden))
            {
                LayoutChildIntoBoundingRegion(_customContentOnHidden, new Rectangle(actualBackButtonSize.Width, _topSpacing, width - actualBackButtonSize.Width - actualCloseButtonSize.Width - _rightViewOnHiddenSize.Width, height - _topSpacing - LineHeight));
            }

            LayoutChildIntoBoundingRegion(_bottomLine, new Rectangle(0, height - _bottomLine.HeightRequest, width, _bottomLine.HeightRequest));

            LayoutChildIntoBoundingRegion(_shadowView, new Rectangle(0, height, width, _shadowView.HeightRequest));

            // If not panned, then init transitions
            if (_panPercent.Equals(0) && 
                AnimationExtensions.AnimationIsRunning(this, _addAnimationName) == false && 
                AnimationExtensions.AnimationIsRunning(this, _removeAnimationName) == false)
            {
                InitializeComponents(width, CurrentPage);
            }
        }

        /// <summary>
        /// Measure all children
        /// </summary>
        private SizeRequest MeasureChildren(double width, double height)
        {
            Size totalSize = new Size(width, 0);

            _titleOnCenterSize = new Size();
            _titleOnHiddenSize = new Size();
            _backTitleSize = new Size();
            _backButtonSize = new Size();
            _rightViewSize = new Size();
            _rightViewOnHiddenSize = new Size();
            _closeButtonSize = new Size();

            if (_backButton != null)
            {
                _backButtonSize = _backButton.Measure(width, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;
                totalSize.Height = Math.Max(totalSize.Height, _backButtonSize.Height);
            }

            if (_rightView != null)
            {
                _rightViewSize = _rightView.Measure(width, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;
                totalSize.Height = Math.Max(totalSize.Height, _rightViewSize.Height);
            }

            if (_rightViewOnHidden != null)
            {
                _rightViewOnHiddenSize = _rightViewOnHidden.Measure(width, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;
                totalSize.Height = Math.Max(totalSize.Height, _rightViewOnHiddenSize.Height);
            }

            if (_closeButton != null && Children.Contains(_closeButton))
            {
                _closeButtonSize = _closeButton.Measure(width, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;
                totalSize.Height = Math.Max(totalSize.Height, _closeButtonSize.Height);
            }

            if (_titleOnCenter != null)
            {
                _titleOnCenterSize = _titleOnCenter.Measure(width - _backButtonSize.Width - _rightViewSize.Width - _closeButtonSize.Width, height - _topSpacing - LineHeight, MeasureFlags.IncludeMargins).Request;
                totalSize.Height = Math.Max(totalSize.Height, _titleOnCenterSize.Height);
            }

            if (_customTitleOnCenter != null)
            {
                _customTitleOnCenterSize = _customTitleOnCenter.Measure(width - _backButtonSize.Width - _rightViewSize.Width - _closeButtonSize.Width, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;
                totalSize.Height = Math.Max(totalSize.Height, _customTitleOnCenterSize.Height);
            }

            if (_titleOnHidden != null)
            {
                _titleOnHiddenSize = _titleOnHidden.Measure(width - _backButtonSize.Width - _rightViewOnHiddenSize.Width - _closeButtonSize.Width, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;
                totalSize.Height = Math.Max(totalSize.Height, _titleOnHiddenSize.Height);
            }

            if (_customTitleOnHidden != null)
            {
                _customTitleOnHiddenSize = _customTitleOnHidden.Measure(width - _backButtonSize.Width - _rightViewOnHiddenSize.Width - _closeButtonSize.Width, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;
                totalSize.Height = Math.Max(totalSize.Height, _customTitleOnHiddenSize.Height);
            }

            if (_backTitle != null || _customBackTitle != null)
            {
                Size actualTitleOnCenterSize = _customTitleOnCenter != null ? _customTitleOnCenterSize : _titleOnCenterSize;
                double availableBackButtonWidth = 0;
                if (TitleAlignment == TitleAlignments.Center)
                {
                    // Has enought space on center
                    if ((Width - _rightViewSize.Width - _closeButtonSize.Width) > ((Width - actualTitleOnCenterSize.Width) / 2) + actualTitleOnCenterSize.Width)
                    {
                        availableBackButtonWidth = width - _backButtonSize.Width - (width - ((width - actualTitleOnCenterSize.Width) / 2));
                    }
                    else
                    {
                        availableBackButtonWidth = width - _backButtonSize.Width - actualTitleOnCenterSize.Width - _rightViewSize.Width - _closeButtonSize.Width;
                    }
                }
                else
                {
                    availableBackButtonWidth = width - _backButtonSize.Width - actualTitleOnCenterSize.Width - _rightViewSize.Width - _closeButtonSize.Width;
                }

                _backTitleSize = _backTitle.Measure(availableBackButtonWidth, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;

                if (_customBackTitle != null)
                {
                    _customBackTitleSize = _customBackTitle.Measure(availableBackButtonWidth, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;
                    totalSize.Height = Math.Max(totalSize.Height, _customBackTitleSize.Height);
                }
                else
                {
                    totalSize.Height = Math.Max(totalSize.Height, _backTitleSize.Height);
                }
            }

            if (_backTitleOnHidden != null || _customBackTitleOnHidden != null)
            {
                Size actualTitleOnHiddenSize = _customTitleOnHidden != null ? _customTitleOnHiddenSize : _titleOnHiddenSize;
                double availableBackButtonWidth = 0;
                if (TitleAlignment == TitleAlignments.Center)
                {
                    // Has enought space on center
                    if ((width - _rightViewOnHiddenSize.Width - _closeButtonSize.Width) > ((width - actualTitleOnHiddenSize.Width) / 2) + actualTitleOnHiddenSize.Width)
                    {
                        availableBackButtonWidth = width - _backButtonSize.Width - (width - ((width - actualTitleOnHiddenSize.Width) / 2));
                    }
                    else
                    {
                        availableBackButtonWidth = width - _backButtonSize.Width - actualTitleOnHiddenSize.Width - _rightViewOnHiddenSize.Width - _closeButtonSize.Width;
                    }
                }
                else
                {
                    availableBackButtonWidth = width - _backButtonSize.Width - actualTitleOnHiddenSize.Width - _rightViewOnHiddenSize.Width - _closeButtonSize.Width;
                }

                _backTitleOnHiddenSize = _backTitleOnHidden.Measure(availableBackButtonWidth, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;

                if (_customBackTitleOnHidden != null)
                {
                    _customBackTitleOnHiddenSize = _customBackTitleOnHidden.Measure(availableBackButtonWidth, height - LineHeight - _topSpacing, MeasureFlags.IncludeMargins).Request;
                    totalSize.Height = Math.Max(totalSize.Height, _customBackTitleOnHiddenSize.Height);
                }
                else
                {
                    totalSize.Height = Math.Max(totalSize.Height, _backTitleOnHiddenSize.Height);
                }
            }

            Size actualBackButtonSize = new Size();
            if (_backButton != null && Children.Contains(_backButton) && _backButton.InputTransparent == false)
            {
                actualBackButtonSize = _backButtonSize;
            }
            Size actualCloseButtonSize = new Size();
            if (_closeButton != null && Children.Contains(_closeButton) && _closeButton.InputTransparent == false)
            {
                actualCloseButtonSize = _closeButtonSize;
            }

            if (_searchView != null && Children.Contains(_searchView) && _searchView.InputTransparent == false)
            {
                _searchViewSize = _searchView.Measure(width - actualBackButtonSize.Width - actualCloseButtonSize.Width, height - LineHeight - _topSpacing).Request;
                totalSize.Height = Math.Max(totalSize.Height, _searchViewSize.Height);
            }
            if (_customContent != null && Children.Contains(_customContent))
            {
                _customContentSize = _customContent.Measure(width - actualBackButtonSize.Width - actualCloseButtonSize.Width - _rightViewSize.Width, height - LineHeight - _topSpacing).Request;
                totalSize.Height = Math.Max(totalSize.Height, _customContentSize.Height);
            }
            if (_customContentOnHidden != null && Children.Contains(_customContentOnHidden))
            {
                _customContentOnHiddenSize = _customContentOnHidden.Measure(width - actualBackButtonSize.Width - actualCloseButtonSize.Width - _rightViewOnHiddenSize.Width, height - LineHeight - _topSpacing).Request;
                totalSize.Height = Math.Max(totalSize.Height, _customContentOnHiddenSize.Height);
            }

            totalSize.Height += _topSpacing;
            totalSize.Height = Math.Min(totalSize.Height, height);
         
            return new SizeRequest(totalSize, totalSize);
        }

        #endregion

        #region Right content

        /// <summary>
        /// Handle RightContent changes
        /// </summary>
        private void OnRightViewChanged(View newView, object bindingContext)
        {
            if (Children == null)
            {
                return;
            }

            _rightViewOnHidden = newView;
            _rightViewOnHiddenSize = new Size();

            if (newView != null)
            {
                newView.BindingContext = bindingContext;
                newView.HorizontalOptions = LayoutOptions.End;
                Children.Add(newView);
            }

            Animation anim = new Animation();
            View oldContent = _rightView;

            if (_rightView != null)
            {
                anim.Add(0, newView != null ? 0.5 : 1, new Animation(d => oldContent.Opacity = d, oldContent.Opacity, 0));
            }

            if (newView != null)
            {
                anim.Add(oldContent != null ? 0.5 : 0, 1, new Animation(d => newView.Opacity = d, 0, 1));
            }

            if (anim.HasSubAnimations())
            {
                anim.Commit(this, _fadeContentAnimationName, 64, (uint)AnimationDuration, AnimationEasing, (d, isAborted) =>
                {
                    _rightView = newView;
                    _rightViewSize = _rightViewOnHiddenSize;

                    _rightViewOnHidden = null;
                    _rightViewOnHiddenSize = new Size();

                    if (oldContent != null)
                    {
                        Children.Remove(oldContent);
                    }
                });
            }
        }

        #endregion

        #region Title

        /// <summary>
        /// Handle title template changes
        /// </summary>
        private void OnTitleTemplateChanged(DataTemplate oldDataTemplate, DataTemplate newDataTemplate)
        {
            if (Children == null)
            {
                return;
            }

            // Remove old titles
            if (_titleOnCenter != null && Children.Contains(_titleOnCenter))
            {
                Children.Remove(_titleOnCenter);
            }
            if (_titleOnHidden != null && Children.Contains(_titleOnHidden))
            {
                Children.Remove(_titleOnHidden);
            }

            // Create new title
            if (newDataTemplate != null)
            {
                _titleOnCenter = newDataTemplate.CreateContent() as View;
                _titleOnHidden = newDataTemplate.CreateContent() as View;
            }

            // Hide new title if page title is set to hidden
            if (CurrentPage != null && GetIsTitleVisible(CurrentPage) == false)
            {
                _titleOnCenter.Opacity = 0;
                _titleOnCenter.InputTransparent = true;
            }

            if (_titleOnCenter != null)
            {
                if (CurrentPage != null)
                {
                    _titleOnCenter.BindingContext = CurrentPage.BindingContext;
                }

                Children.Add(_titleOnCenter);
            }

            if (_titleOnHidden != null)
            {
                Children.Add(_titleOnHidden);
                _titleOnHidden.Opacity = 0;
                _titleOnHidden.InputTransparent = true;
            }
        }

        #endregion

        #region Back title button

        /// <summary>
        /// Handle back text button template changes
        /// </summary>
        private void OnBackTitleTemplateChanged(DataTemplate oldDataTemplate, DataTemplate newDataTemplate)
        {
            if (Children == null)
            {
                return;
            }
            
            // Remove previous back titles
            if (_backTitle != null)
            {
                RemoveBackTitle(_backTitle);
                 _backTitleSize = new Size();
                _backTitle = null;
            }
            if (_backTitleOnHidden != null)
            {
                RemoveBackTitle(_backTitleOnHidden);
                _backTitleOnHiddenSize = new Size();
                _backTitleOnHidden = null;
            }

            if (newDataTemplate != null)
            {
                _backTitle = CreateBackTitle();

                if (_backTitle != null)
                {
                    if (PreviousPage != null)
                    {
                        _backTitle.BindingContext = PreviousPage.BindingContext;
                    }

                    Children.Add(_backTitle);

                    if (NavigationHistory == null || NavigationHistory.Count <= 1)
                    {
                        _backTitle.Opacity = 0;
                        _backTitle.InputTransparent = true;
                    }
                }

                _backTitleOnHidden = CreateBackTitle();
                _backTitleOnHidden.Opacity = 0;
                _backTitleOnHidden.InputTransparent = true;
                Children.Add(_backTitleOnHidden);
            }
        }

        /// <summary>
        /// Remove back title from children and stop listening events
        /// </summary>
        private void RemoveBackTitle(View backTitle)
        {
            if (backTitle is ITappable tappableBackTitle)
            {
                tappableBackTitle.Tapped -= OnBackButtonTapped;
            }
            if (backTitle is IIsPressed pressBackTitle)
            {
                pressBackTitle.IsPressedChanged -= OnBackTitleButtonIsPressedChanged;
            }

            if (Children.Contains(backTitle))
            {
                Children.Remove(backTitle);
            }
        }

        /// <summary>
        /// Create back title button from 'BackBTitleTemplate'
        /// </summary>
        private View CreateBackTitle()
        {
            if (BackTitleTemplate != null)
            {
                View backTitle = BackTitleTemplate.CreateContent() as View;

                if (backTitle is ITappable)
                {
                    (backTitle as ITappable).Tapped += OnBackButtonTapped;
                }
                if (backTitle is IIsPressed)
                {
                    (backTitle as IIsPressed).IsPressedChanged += OnBackTitleButtonIsPressedChanged;
                }
                return backTitle;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute 'PreviousCommand'
        /// </summary>
        private void OnBackButtonTapped(object sender, EventArgs e)
        {
            HandleBackButtonTapped();
        }

        /// <summary>
        /// Sync back title button IsPressed to back icon button IsPressed property
        /// </summary>
        private void OnBackTitleButtonIsPressedChanged(object sender, bool isPressed)
        {
            if (_backButton is Button pressedButton)
            {
                pressedButton.SetIsPressed(isPressed);
            }
        }

        private void HandleBackButtonTapped()
        {
            if (CurrentPage == null)
            {
                return;
            }

            if (GetBackButtonMode(CurrentPage) == BackButtonModes.Back || _backButton is IBackButton == false)
            {
                if (BackCommand != null)
                {
                    BackCommand.Execute(null);
                }
            }
            else if (GetBackButtonMode(CurrentPage) == BackButtonModes.Menu)
            {
                if (MenuCommand != null)
                {
                    MenuCommand.Execute(null);
                }
            }
            else if (GetBackButtonMode(CurrentPage) == BackButtonModes.Close)
            {
                if (CloseCommand != null)
                {
                    CloseCommand.Execute(null);
                }
            }
        }

        private void OnIsBackTitleVisibleChanged(bool isVisible)
        {
            if (_backTitle != null)
            {
                if (isVisible && NavigationHistory.Count > 1)
                {
                    _backTitle.Opacity = 1;
                    _backTitle.InputTransparent = false;
                }
                else
                {
                    _backTitle.Opacity = 0;
                    _backTitle.InputTransparent = true;
                }
            }

            if (_customBackTitle != null)
            {
                if (isVisible && NavigationHistory.Count > 1)
                {
                    _customBackTitle.Opacity = 1;
                    _customBackTitle.InputTransparent = false;
                }
                else
                {
                    _customBackTitle.Opacity = 0;
                    _customBackTitle.InputTransparent = true;
                }
            }
        }

        #endregion

        #region Back button

        /// <summary>
        /// Handle back arrow button template changes
        /// </summary>
        private void OnBackButtonTemplateChanged(DataTemplate oldDataTemplate, DataTemplate newDataTemplate)
        {
            if (Children == null)
            {
                return;
            }
            
            // Remove old back button

            if (_backButton != null && Children.Contains(_backButton))
            {
                Children.Remove(_backButton);
            }

            if (_backButton != null && _backButton is ITappable)
            {
                if (_backButton is ITappable)
                {
                    (_backButton as ITappable).Tapped -= OnBackButtonTapped;
                }
                if (_backButton is IIsPressed)
                {
                    (_backButton as IIsPressed).IsPressedChanged -= OnBackButtonIsPressedChanged;
                }
            }

            // Create new back button

            _backButton = BackButtonTemplate.CreateContent() as View;

            if (_backButton != null)
            {
                if (_backButton is ITappable)
                {
                    (_backButton as ITappable).Tapped += OnBackButtonTapped;
                }
                if (_backButton is IIsPressed)
                {
                    (_backButton as IIsPressed).IsPressedChanged += OnBackButtonIsPressedChanged;
                }

                Children.Add(_backButton);

                if (IsBackButtonVisible(CurrentPage))
                {
                    _backButton.Opacity = 1;
                    _backButton.InputTransparent = false;
                }
                else
                {
                    _backButton.Opacity = 0;
                    _backButton.InputTransparent = true;
                }

                if (CurrentPage != null && _backButton is IBackButton button)
                {
                    button.Mode = GetBackButtonMode(CurrentPage);
                }
            }
        }

        /// <summary>
        /// Sync back title button and back button 'IsPressed' states
        /// </summary>
        private void OnBackButtonIsPressedChanged(object sender, bool isPressed)
        {
            if (_customBackTitle is Button customBackTitleButton)
            {
                customBackTitleButton.SetIsPressed(isPressed);
            }
            else if (_backTitle is Button backTitleButton)
            {
                backTitleButton.SetIsPressed(isPressed);
            }
        }

        /// <summary>
        /// Set back button icon based on BackButtonModes
        /// </summary>
        private void UpdateBackButtonMode(BackButtonModes mode)
        {
            if (_backButton != null)
            {
                if (_backButton is IBackButton button)
                {
                    button.Mode = mode;
                }

                if (IsBackButtonVisible(CurrentPage))
                {
                    _backButton.Opacity = 1;
                    _backButton.InputTransparent = false;
                }
                else
                {
                    _backButton.Opacity = 0;
                    _backButton.InputTransparent = true;
                }
            }
        }

        #endregion

        #region Close button

        private void OnCloseButtonTemplateChanged(DataTemplate oldDataTemplate, DataTemplate newDataTemplate)
        {
            if (Children == null)
            {
                return;
            }

            if (_closeButton != null)
            {
                Children.Remove(_closeButton);

                if (_closeButton is ITappable tappable)
                {
                    tappable.Tapped -= OnCloseButtonTapped;
                }
            }

            if (newDataTemplate != null)
            {
                _closeButton = newDataTemplate.CreateContent() as View;

                if (_closeButton is ITappable tappable)
                {
                    tappable.Tapped += OnCloseButtonTapped;
                }

                Children.Add(_closeButton);

                if (CurrentPage != null && GetIsCloseButtonVisible(CurrentPage))
                {
                    _closeButton.InputTransparent = false;
                    _closeButton.Opacity = 1;
                }
                else
                {
                    _closeButton.InputTransparent = true;
                    _closeButton.Opacity = 0;
                }
            }
        }

        private void OnCloseButtonTapped(object sender, EventArgs e)
        {
            if (CloseCommand != null)
            {
                CloseCommand.Execute(null);
            }
        }

        private void OnIsCloseButtonVisibleChanged(bool newValue)
        {
            if (_closeButton != null)
            {
                if (newValue)
                {
                    _closeButton.Opacity = 1;
                    _closeButton.InputTransparent = false;
                }
                else
                {
                    _closeButton.Opacity = 0;
                    _closeButton.InputTransparent = true;
                }
            }
        }

        #endregion

        #region Search

        private void OnSearchTemplateChanged(DataTemplate oldDataTemplate, DataTemplate newDataTemplate)
        {
            if (Children == null)
            {
                return;
            }

            if (_searchView != null)
            {
                Children.Remove(_searchView);

                if (_searchView is TextBox textBox)
                {
                    textBox.TextChanged -= OnSearchTextChanged;
                }
            }

            if (newDataTemplate != null)
            {
                _searchView = newDataTemplate.CreateContent() as View;

                if (_searchView is TextBox textBox)
                {
                    textBox.TextChanged += OnSearchTextChanged;
                }

                Children.Add(_searchView);

                if (CurrentPage != null && GetIsSearchMode(CurrentPage))
                {
                    if (Children.Contains(_searchView) == false)
                    {
                        Children.Add(_searchView);
                    }
                }
                else
                {
                    if (Children.Contains(_searchView))
                    {
                        Children.Remove(_searchView);
                    }
                }
            }
        }

        /// <summary>
        /// Handle 'IsSearchMode' changes
        /// </summary>
        private void OnIsSearchModeChanged(ContentPage page)
        {
            IsSearchModeActive = GetIsSearchMode(page);

            if (IsSearchModeActive)
            {
                // Remove all parts except back button and search view

                if (_searchView != null)
                {
                    if (Children.Contains(_searchView) == false)
                    {
                        Children.Add(_searchView);
                    }
                }
                if (_backButton != null)
                {
                    if (IsBackButtonVisible(page))
                    {
                        _backButton.InputTransparent = false;
                        _backButton.Opacity = 1;
                    }
                    else
                    {
                        _backButton.InputTransparent = true;
                        _backButton.Opacity = 0;
                    }
                }
                if (_closeButton != null)
                {
                    if (GetIsCloseButtonVisible(page))
                    {
                        _closeButton.InputTransparent = false;
                        _closeButton.Opacity = 1;
                    }
                    else if (GetIsCloseButtonVisible(page) == true)
                    {
                        _closeButton.InputTransparent = true;
                        _closeButton.Opacity = 0;
                    }
                }
                if (_titleOnCenter != null)
                {
                    _titleOnCenter.InputTransparent = true;
                    _titleOnCenter.Opacity = 0;
                }
                if (_customTitleOnCenter != null)
                {
                    _customTitleOnCenter.InputTransparent = true;
                    _customTitleOnCenter.Opacity = 0;
                }
                if (_titleOnHidden != null)
                {
                    _titleOnHidden.InputTransparent = true;
                    _titleOnHidden.Opacity = 0;
                }
                if (_customTitleOnHidden != null)
                {
                    _customTitleOnHidden.InputTransparent = true;
                    _customTitleOnHidden.Opacity = 0;
                }
                if (_backTitle != null)
                {
                    _backTitle.InputTransparent = true;
                    _backTitle.Opacity = 0;
                }
                if (_customBackTitle != null)
                {
                    _customBackTitle.InputTransparent = true;
                    _customBackTitle.Opacity = 0;
                }
                if (_backTitleOnHidden != null)
                {
                    _backTitleOnHidden.InputTransparent = true;
                    _backTitleOnHidden.Opacity = 0;
                }
                if (_customBackTitleOnHidden != null)
                {
                    _customBackTitleOnHidden.InputTransparent = true;
                    _customBackTitleOnHidden.Opacity = 0;
                }
                if (_rightView != null)
                {
                    _rightView.InputTransparent = true;
                    _rightView.Opacity = 0;
                }
                if (_rightViewOnHidden != null)
                {
                    _rightViewOnHidden.InputTransparent = true;
                    _rightViewOnHidden.Opacity = 0;
                }
                if (_customContentOnHidden != null)
                {
                    _customContentOnHidden.InputTransparent = true;
                    _customContentOnHidden.Opacity = 0;
                }
                if (_customContent != null)
                {
                    _customContent.InputTransparent = true;
                    _customContent.Opacity = 0;
                }

                if (_searchView is TextBox textBox)
                {
                    textBox.Placeholder = GetSearchPlaceholder(page);
                    textBox.Text = "";
                    textBox.Focus();
                }
            }
            else
            {
                if (_searchView != null)
                {
                    if (Children.Contains(_searchView))
                    {
                        Children.Remove(_searchView);
                    }
                }
                if (_backButton != null && IsBackButtonVisible(page))
                {
                    _backButton.InputTransparent = false;
                    _backButton.Opacity = 1;
                }
                if (_closeButton != null && GetIsCloseButtonVisible(page))
                {
                    _closeButton.InputTransparent = false;
                    _closeButton.Opacity = 1;
                }
                if (_rightView != null)
                {
                    _rightView.InputTransparent = false;
                    _rightView.Opacity = 1;
                }
                if (_customContent != null)
                {
                    _customContent.InputTransparent = false;
                    _customContent.Opacity = 1;
                }
                else
                {
                    if (_customTitleOnCenter != null && GetIsTitleVisible(page))
                    {
                        _customTitleOnCenter.InputTransparent = false;
                        _customTitleOnCenter.Opacity = 1;
                    }
                    else if (_titleOnCenter != null && GetIsTitleVisible(page))
                    {
                        _titleOnCenter.InputTransparent = false;
                        _titleOnCenter.Opacity = 1;
                    }
                    if (_customBackTitle != null && GetIsBackTitleVisible(page))
                    {
                        _customBackTitle.InputTransparent = false;
                        _customBackTitle.Opacity = 1;
                    }
                    else if (_backTitle != null && GetIsBackTitleVisible(page))
                    {
                        _backTitle.InputTransparent = false;
                        _backTitle.Opacity = 1;
                    }
                }
            }
        }

        /// <summary>
        /// Event when search text changes
        /// </summary>
        private void OnSearchTextChanged(object sender, TextChangedEventArgs args)
        {
            CurrentPage.OnSearchTextChanged(args);

            SetSearchText(CurrentPage, args.NewTextValue);
        }

        #endregion

        #region Items

        /// <summary>
        /// Handle whole navigation stack list changes
        /// </summary>
        private void OnNavigationHistoryChanged(IList<ContentPage> oldIList, IList<ContentPage> newIList)
        {
            if (oldIList != null && oldIList.Count > 0)
            {
                ResetItems();
            }

            // Add new items without animation
            if (newIList != null && newIList.Count > 0)
            {
                HandleItemPush(CurrentPage, false, false);
            }

            UpdateCurrentPagePropertyChangedEventListener();
        }

        /// <summary>
        /// Push page to navigation stack
        /// </summary>
        public void Push(ContentPage page)
        {
            // Add to navigation history stack
            NavigationHistory.Add(page);

            HandleItemPush(page, true, false);

            UpdateCurrentPagePropertyChangedEventListener();
        }

        /// <summary>
        /// Push page to navigation stack root page
        /// </summary>
        public async void PushToRoot(ContentPage page)
        {
            // Add to navigation history stack
            NavigationHistory.Add(page);

            UpdateCurrentPagePropertyChangedEventListener();

            await HandleItemPush(page, true, true);

            int originalNavigationHistory = NavigationHistory.Count;
            for (int i = 0; i < originalNavigationHistory - 1; i++)
            {
                NavigationHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Pop page from navigation stack
        /// </summary>
        public void Pop()
        {
            ContentPage page = NavigationHistory[NavigationHistory.Count - 1];

            // Do actual remove from history stack
            NavigationHistory.Remove(page);

            HandleItemPop(page, true);

            UpdateCurrentPagePropertyChangedEventListener();
        }

        /// <summary>
        /// Set current page property changed event
        /// </summary>
        private void UpdateCurrentPagePropertyChangedEventListener()
        {
            // Stop listening previous active page property changed events
            if (_previousActivePage != null)
            {
                _previousActivePage.PropertyChanged -= OnCurrentPagePropertyChanged;
            }

            _previousActivePage = CurrentPage;

            // Start listening new current page property changed events
            if (CurrentPage != null)
            {
                CurrentPage.PropertyChanged += OnCurrentPagePropertyChanged;
            }
        }

        /// <summary>
        /// Event when current page navigation bar attached properties changed
        /// </summary>
        private void OnCurrentPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == NavigationBar.RightViewProperty.PropertyName)
            {
                OnRightViewChanged(GetRightView(CurrentPage), CurrentPage.BindingContext);
            }
            else if (e.PropertyName == NavigationBar.BackButtonModeProperty.PropertyName)
            {
                UpdateBackButtonMode(GetBackButtonMode(CurrentPage));
            }
            else if (e.PropertyName == NavigationBar.IsTitleVisibleProperty.PropertyName)
            {
                SetVisibility(GetIsTitleVisible(CurrentPage), _customTitleOnCenter != null ? _customTitleOnCenter : _titleOnCenter, _titleVisibilityAnimationName);
            }
            else if (e.PropertyName == NavigationBar.IsLineVisibleProperty.PropertyName)
            {
                SetVisibility(GetIsLineVisible(CurrentPage), _bottomLine, _lineVisibilityAnimationName);
            }
            else if (e.PropertyName == NavigationBar.BackgroundColorProperty.PropertyName)
            {
                SetBackgroundColor(GetBackgroundColor(CurrentPage), GetIsBackgroundAnimationEnabled(CurrentPage), GetBackgroundAnimationDuration(CurrentPage));
            }
            else if (e.PropertyName == NavigationBar.LineColorProperty.PropertyName)
            {
                _bottomLine.BackgroundColor = GetLineColor(CurrentPage);
            }
            else if (e.PropertyName == NavigationBar.IsShadowVisibleProperty.PropertyName)
            {
                AnimationExtensions.AbortAnimation(this, _shadowVisibilityAnimationName);

                if (_shadowView != null)
                {
                    _shadowView.Opacity = GetIsShadowVisible(CurrentPage) ? 1 : 0;
                }
            }
            else if (e.PropertyName == NavigationBar.TitleProperty.PropertyName)
            {
                object title = GetTitle(CurrentPage);

                if (Children.Contains(_customTitleOnCenter))
                {
                    Children.Remove(_customTitleOnCenter);
                }

                if (title is View customTitle)
                {                   
                    _customTitleOnCenter = customTitle;
                    Children.Add(_customTitleOnCenter);
                }
                else
                {
                    _customTitleOnCenter = null;
                    _titleOnCenter.BindingContext = title;
                }
            }
            else if (e.PropertyName == NavigationBar.BackTitleProperty.PropertyName)
            {
                object backTitle = GetBackTitle(CurrentPage);

                if (Children.Contains(_customBackTitle))
                {
                    Children.Remove(_customBackTitle);
                }

                if (backTitle is View customBackTitle)
                {
                    _customBackTitle = customBackTitle;
                    Children.Add(_customBackTitle);
                }
                else
                {
                    _backTitle.BindingContext = backTitle;
                }
            }
            else if (e.PropertyName == NavigationBar.IsBackTitleVisibleProperty.PropertyName)
            {
                OnIsBackTitleVisibleChanged(GetIsBackTitleVisible(CurrentPage));
            }
            else if (e.PropertyName == NavigationBar.IsCloseButtonVisibleProperty.PropertyName)
            {
                OnIsCloseButtonVisibleChanged(GetIsCloseButtonVisible(CurrentPage));
            }
            else if (e.PropertyName == NavigationBar.IsSearchModeProperty.PropertyName)
            {
                OnIsSearchModeChanged(CurrentPage);
            }
            else if (e.PropertyName == NavigationBar.SearchPlaceholderProperty.PropertyName)
            {
                if (_searchView is TextBox textBox)
                {
                    textBox.Placeholder = GetSearchPlaceholder(CurrentPage);
                }
            }
        }

        /// <summary>
        /// Set title opacity
        /// </summary>
        private void SetVisibility(bool isVisible, View view, string animationName)
        {
            this.AbortAnimation(animationName);

            Animation animation = new Animation();

            if (isVisible)
            {
                animation = new Animation(d => view.Opacity = d , view.Opacity, 1);
            }
            else
            {
                animation = new Animation(d => view.Opacity = d, view.Opacity, 0);
            }

            animation.Commit(this, animationName, 64, 200);
        }

        /// <summary>
        /// Set background color
        /// </summary>
        /// <param name="colorTo">New background color. Null if default color</param>
        /// <param name="isAnimated">Is color change animated</param>
        /// <param name="duration">Animation duration</param>
        private void SetBackgroundColor(Color colorTo, bool isAnimated = false, int duration = 150)
        {
            this.AbortAnimation(_backgroundColorAnimationName);

            if (isAnimated)
            {
                Color colorFrom = BackgroundColor;

                if (colorFrom != colorTo)
                {
                    new Animation(d => BackgroundColor = AnimationUtils.ColorTransform(d, colorFrom, colorTo)).Commit(this, _backgroundColorAnimationName, 64, (uint)duration);
                }
            }
            else
            {
                BackgroundColor = colorTo;                
            }
        }

        /// <summary>
        /// Add items with animation
        /// </summary>
        private Task HandleItemPush(ContentPage page, bool isAnimationEnabled, bool pushToRoot)
        {
            AnimationExtensions.AbortAnimation(this, _removeAnimationName);

            _ignoreInvalidation = true;

            ContentPage pageTo = page;
            ContentPage pageFrom = PreviousPage;

            object title = GetTitle(pageTo);
            if (title is View customTitle)
            {
                _customTitleOnHidden = customTitle;
                _customTitleOnHidden.BindingContext = pageTo.BindingContext;
                Children.Add(_customTitleOnHidden);
            }
            else if (_titleOnHidden != null)
            {
                SetBindingContext(_titleOnHidden, title);
                _titleOnHidden.InputTransparent = false;
            }

            if (pageFrom != null && pushToRoot == false)
            {
                object backTitle = GetBackTitle(pageFrom);
                if (backTitle is View customBackTitle)
                {
                    _customBackTitleOnHidden = customBackTitle;
                    _customBackTitleOnHidden.BindingContext = pageTo.BindingContext;
                    Children.Add(_customBackTitleOnHidden);
                }
                else if (_backTitleOnHidden != null)
                {
                    if (backTitle == null)
                    {
                        backTitle = pageFrom != null ? GetTitle(pageFrom) : null;
                        if (backTitle is View)
                        {
                            backTitle = null;
                        }
                    }

                    SetBindingContext(_backTitleOnHidden, backTitle);
                    _backTitleOnHidden.InputTransparent = false;
                }
            }

            View customContent = GetContent(pageTo);
            if (customContent != null)
            {
                _customContentOnHidden = customContent;
                Children.Add(customContent);
            }

            _rightViewOnHidden = GetRightView(pageTo);
            if (_rightViewOnHidden != null)
            {
                _rightViewOnHidden.HorizontalOptions = LayoutOptions.End;
                Children.Add(_rightViewOnHidden);
            }

            if (_backButton is IBackButton backButton)
            {
                if (pushToRoot && (GetBackButtonMode(pageTo) == BackButtonModes.Hidden || GetBackButtonMode(pageTo) == BackButtonModes.Back))
                {
                    // Do nothing...
                }
                else
                {
                    backButton.Mode = GetBackButtonMode(pageTo);
                }
            }

            if (GetIsSearchMode(pageTo) != IsSearchModeActive)
            {
                OnIsSearchModeChanged(pageTo);
            }

            OnIsCloseButtonVisibleChanged(GetIsCloseButtonVisible(pageTo));

            if (Device.RuntimePlatform == Device.UWP)
            {
                MeasureChildren(Width, Height);
                InitializeComponents(Width, pageTo);
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                LayoutChildren(0, 0, Width, Height);
            }

            _ignoreInvalidation = false;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (isAnimationEnabled)
            {
                new Animation(d =>
                {
                    NextAnimation(d, Width, pageFrom, pageTo, pushToRoot);
                }, 0, 1).Commit(this, _addAnimationName, _rate, (uint)AnimationDuration, Easing.Linear, finished: (animationProcess, isAborted) =>
                {
                    DoSwap();
                    RemoveHiddenContent();
                    tcs.SetResult(true);
                });
            }
            else
            {
                DoSwap();
                InitializeComponents(Width, pageTo);
                RemoveHiddenContent();
                tcs.SetResult(true);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Remove items with animation
        /// </summary>
        private void HandleItemPop(ContentPage page, bool isAnimationEnabled = true)
        {
            AnimationExtensions.AbortAnimation(this, _addAnimationName);

            _ignoreInvalidation = true;

            ContentPage pageTo = CurrentPage;
            ContentPage pageFrom = page;

            InitializeBackAnimationHiddenParts(pageFrom, pageTo);

            if (_backButton is IBackButton backButton)
            {
                backButton.Mode = GetBackButtonMode(pageTo);
            }

            if (GetIsSearchMode(pageTo) != IsSearchModeActive)
            {
                OnIsSearchModeChanged(pageTo);
            }

            OnIsCloseButtonVisibleChanged(GetIsCloseButtonVisible(pageTo));

            if (Device.RuntimePlatform == Device.UWP)
            {
                MeasureChildren(Width, Height);
                InitializeComponents(Width, pageTo);
            }
            if (Device.RuntimePlatform == Device.Android)
            {
                LayoutChildren(0, 0, Width, Height);
            }

            _ignoreInvalidation = false;

            if (isAnimationEnabled)
            {
                new Animation(d =>
                {
                    BackAnimation(d, Width, pageFrom, pageTo);
                }, _panPercent, 1).Commit(this, _addAnimationName, _rate, (uint)(AnimationDuration * (1 - _panPercent)), Easing.Linear, finished: (animationProcess, isAborted) =>
                {
                    DoSwap();
                    RemoveHiddenContent();
                    _panPercent = 0;
                });
            }
            else
            {
                DoSwap();
                InitializeComponents(Width, pageTo);
                RemoveHiddenContent();
            }
        }

        private void InitializeBackAnimationHiddenParts(ContentPage pageFrom, ContentPage pageTo)
        {
            object pageToTitle = GetTitle(pageTo);
            if (pageToTitle is View customTitle)
            {
                _customTitleOnHidden = customTitle;
                _customTitleOnHidden.BindingContext = pageTo.BindingContext;

                if (Children.Contains(_customTitleOnHidden) == false)
                {
                    Children.Add(_customTitleOnHidden);
                }
            }
            else if (_titleOnHidden != null)
            {
                SetBindingContext(_titleOnHidden, pageToTitle);
                _titleOnHidden.InputTransparent = false;
            }

            if (NavigationHistory.Count >= 3)
            {
                object pageToBackTitle = GetBackTitle(pageTo);
                if (pageToBackTitle is View customBackTitle)
                {
                    _customBackTitleOnHidden = customBackTitle;
                    _customBackTitleOnHidden.BindingContext = pageTo.BindingContext;

                    if (Children.Contains(_customBackTitleOnHidden) == false)
                    {
                        Children.Add(_customBackTitleOnHidden);
                    }
                }
                else if (_backTitleOnHidden != null)
                {
                    if (pageToBackTitle == null)
                    {
                        pageToBackTitle = pageTo != null ? GetTitle(NavigationHistory[NavigationHistory.Count - 3]) : null;
                        if (pageToBackTitle is View)
                        {
                            pageToBackTitle = null;
                        }
                    }

                    SetBindingContext(_backTitleOnHidden, pageToBackTitle);
                    _backTitleOnHidden.InputTransparent = false;
                }
            }
            else
            {
                if (_backTitleOnHidden != null)
                {
                    _backTitleOnHidden.BindingContext = null;
                    _backTitleOnHidden.InputTransparent = false;
                    _backTitleOnHidden.Opacity = 0;
                }
                if (_customBackTitleOnHidden != null)
                {
                    if (Children.Contains(_customBackTitleOnHidden))
                    {
                        Children.Remove(_customBackTitleOnHidden);
                    }
                    _customBackTitleOnHidden.BindingContext = null;
                    _customBackTitleOnHidden = null;
                }
            }

            _customContentOnHidden = GetContent(pageTo);
            if (_customContentOnHidden != null && Children.Contains(_customContentOnHidden) == false)
            {
                Children.Add(_customContentOnHidden);
            }

            _rightViewOnHidden = GetRightView(pageTo);
            if (_rightViewOnHidden != null && Children.Contains(_rightViewOnHidden) == false)
            {
                Children.Add(_rightViewOnHidden);
            }
        }

        /// <summary>
        /// Reset all parts which are based on current page
        /// </summary>
        private void ResetItems()
        {
            if (_backButton != null)
            {
                _backButton.Opacity = 0;
                _backButton.InputTransparent = true;
            }
            if (_backTitle != null)
            {
                _backTitle.Opacity = 0;
                _backButton.InputTransparent = true;
                _backButton.BindingContext = null;
            }
            if (_customBackTitle != null)
            {
                Children.Remove(_customBackTitle);
                _customBackTitle.BindingContext = null;
                _customBackTitle = null;
            }
            if (_titleOnCenter != null)
            {
                _titleOnCenter.Opacity = 0;
                _titleOnCenter.InputTransparent = true;
                _titleOnCenter.BindingContext = null;
            }
            if (_customTitleOnCenter != null)
            {
                _customTitleOnCenter.BindingContext = null;
                Children.Remove(_customTitleOnCenter);
                _customTitleOnCenter = null;
            }
            if (_rightView != null)
            {
                Children.Remove(_rightView);
                _rightView.BindingContext = null;
                _rightView = null;
            }
            if (_customContent != null)
            {
                Children.Remove(_customContent);
                _customContent.BindingContext = null;
                _customContent = null;
            }

            RemoveHiddenContent();
        }

        /// <summary>
        /// Remove all hidden parts
        /// </summary>
        private void RemoveHiddenContent()
        {
            _ignoreInvalidation = true;

            if (_backTitleOnHidden != null && Children.Contains(_backTitleOnHidden))
            {
                _backTitleOnHidden.Opacity = 0;
                SetBindingContext(_backTitleOnHidden, null);
                _backTitleOnHidden.InputTransparent = true;
                _backTitleOnHiddenSize = new Size();
            }
            if (_customBackTitleOnHidden != null && Children.Contains(_customBackTitleOnHidden))
            {
                Children.Remove(_customBackTitleOnHidden);
                _customBackTitleOnHidden.BindingContext = null;
                _customBackTitleOnHidden = null;
                _customBackTitleOnHiddenSize = new Size();
            }
            if (_titleOnHidden != null && Children.Contains(_titleOnHidden))
            {
                _titleOnHidden.Opacity = 0;
                SetBindingContext(_titleOnHidden, null);
                _titleOnHidden.InputTransparent = true;
                _titleOnHiddenSize = new Size();
            }
            if (_customTitleOnHidden != null && Children.Contains(_customTitleOnHidden))
            {
                Children.Remove(_customTitleOnHidden);
                _customTitleOnHidden.BindingContext = null;
                _customTitleOnHidden = null;
                _customTitleOnHiddenSize = new Size();
            }
            if (_rightViewOnHidden != null && Children.Contains(_rightViewOnHidden))
            {
                Children.Remove(_rightViewOnHidden);
                _rightViewOnHidden.BindingContext = null;
                _rightViewOnHiddenSize = new Size();
            }
            if (_customContentOnHidden != null && Children.Contains(_customContentOnHidden))
            {
                Children.Remove(_customContentOnHidden);
                _customContentOnHidden.BindingContext = null;
                _customContentOnHidden = null;
                _customContentOnHiddenSize = new Size();
            }

            _ignoreInvalidation = false;
        }

        /// <summary>
        /// Swap hidden and visible parts and sizes
        /// </summary>
        private void DoSwap()
        {
            Swap(ref _backTitle, ref _backTitleOnHidden);
            Swap(ref _customBackTitle, ref _customBackTitleOnHidden);
            Swap(ref _titleOnCenter, ref _titleOnHidden);
            Swap(ref _customTitleOnCenter, ref _customTitleOnHidden);
            Swap(ref _rightView, ref _rightViewOnHidden);
            Swap(ref _customContent, ref _customContentOnHidden);

            Swap(ref _backTitleSize, ref _backTitleOnHiddenSize);
            Swap(ref _customBackTitleSize, ref _customBackTitleOnHiddenSize);
            Swap(ref _titleOnCenterSize, ref _titleOnHiddenSize);
            Swap(ref _customTitleOnCenterSize, ref _customTitleOnHiddenSize);
            Swap(ref _rightViewSize, ref _rightViewOnHiddenSize);
            Swap(ref _customContentSize, ref _customContentOnHiddenSize);
        }

        #endregion

        #region Animation handling

        /// <summary>
        /// Set parts next page animation 0 -> 1
        /// </summary>
        /// <param name="animationProcess">0 = current page, 1 = next page without easing</param>
        private void NextAnimation(double animationProcess, double width, ContentPage pageFrom, ContentPage pageTo, bool pushToRoot)
        {
            // Do nothing if no navigation history
            if (NavigationHistory.Count == 0)
            {
                return;
            }

            double animationProcessWithEasing = animationProcess;
            if (AnimationEasing != null)
            {
                animationProcessWithEasing = AnimationEasing.Ease(animationProcess);
            }

            // Background color
            Color backgroundColorFrom = pageFrom != null ? GetBackgroundColor(pageFrom) : BackgroundColor;
            Color backgroundColorTo = GetBackgroundColor(pageTo);
            if (backgroundColorFrom != backgroundColorTo)
            {
                BackgroundColor = AnimationUtils.ColorTransform(animationProcess, backgroundColorFrom, backgroundColorTo);
            }

            _shadowView.Opacity = GetIsShadowVisible(CurrentPage) ? 1 : 0;

            bool hasPageToBackButtonVisible = pushToRoot ? (GetBackButtonMode(pageTo) == BackButtonModes.Close || GetBackButtonMode(pageTo) == BackButtonModes.Menu) : IsBackButtonVisible(pageTo);
            bool hasPageFromBackButtonVisible = IsBackButtonVisible(pageFrom);

            // Titles horizontal slide
            if (Animation == Animations.SlideHorizontal)
            {
                // Back title
                double backTitleStart = hasPageFromBackButtonVisible ? _backButtonSize.Width : 0;
                if (_customBackTitle != null)
                {
                    double end = -(((width - _customBackTitleSize.Width) / 2) - backTitleStart);
                    _customBackTitle.TranslationX = backTitleStart + (end - backTitleStart) * animationProcessWithEasing;
                }
                else if (_backTitle != null)
                {
                    double end = -(((width - _backTitleSize.Width) / 2) - backTitleStart);
                    _backTitle.TranslationX = backTitleStart + (end - backTitleStart) * animationProcessWithEasing;
                }

                // Back title on hidden
                double backTitleOnHiddenEnd = hasPageToBackButtonVisible ? _backButtonSize.Width : 0;
                double actualBackTitleOnHiddenWidth = 0;
                if (_customBackTitleOnHidden != null)
                {
                    double backTitleOnHiddenStart = (width - _customBackTitleOnHiddenSize.Width) / 2;
                    _customBackTitleOnHidden.TranslationX = backTitleOnHiddenStart + (backTitleOnHiddenEnd - backTitleOnHiddenStart) * animationProcessWithEasing;
                    actualBackTitleOnHiddenWidth = _customBackTitleOnHiddenSize.Width;
                }
                else if (_backTitleOnHidden != null)
                {
                    double backTitleOnHiddenStart = (width - _backTitleOnHiddenSize.Width) / 2;
                    _backTitleOnHidden.TranslationX = backTitleOnHiddenStart + (backTitleOnHiddenEnd - backTitleOnHiddenStart) * animationProcessWithEasing;
                    actualBackTitleOnHiddenWidth = _backTitleOnHiddenSize.Width;
                }

                // Title on center
                if (_customTitleOnCenter != null)
                {
                    double titleEnd = hasPageToBackButtonVisible ? _backButtonSize.Width : 0;
                    double titleStart = 0;

                    if (TitleAlignment == TitleAlignments.Center)
                    {
                        titleStart = (width - _customTitleOnCenterSize.Width) / 2;
                    }
                    else
                    {
                        titleStart = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0) + actualBackTitleOnHiddenWidth;
                    }
                    _customTitleOnCenter.TranslationX = titleStart + (titleEnd - titleStart) * animationProcessWithEasing;
                }
                else if (_titleOnCenter != null)
                {
                    double titleEnd = hasPageToBackButtonVisible ? _backButtonSize.Width : 0;
                    double titleStart = 0;

                    if (TitleAlignment == TitleAlignments.Center)
                    {
                        titleStart = (width - _titleOnCenterSize.Width) / 2;
                    }
                    else
                    {
                        titleStart = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0) + actualBackTitleOnHiddenWidth;
                    }
                    _titleOnCenter.TranslationX = titleStart + (titleEnd - titleStart) * animationProcessWithEasing;
                }

                // Title on hidden
                if (_customTitleOnHidden != null)
                {
                    double titleStart = width - (_customTitleOnHiddenSize.Width / 2);
                    double titleEnd = 0;
                    if (TitleAlignment == TitleAlignments.Center)
                    {
                        titleEnd = (width - _customTitleOnHiddenSize.Width) / 2;
                    }
                    else
                    {
                        titleEnd = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0) + actualBackTitleOnHiddenWidth;
                    }
                    _customTitleOnHidden.TranslationX = titleStart + (titleEnd - titleStart) * animationProcessWithEasing;
                }
                else if (_titleOnHidden != null)
                {
                    double titleStart = width - (_titleOnHiddenSize.Width / 2);
                    double titleEnd = 0;
                    if (TitleAlignment == TitleAlignments.Center)
                    {
                        titleEnd = (width - _titleOnHiddenSize.Width) / 2;
                    }
                    else
                    {
                        titleEnd = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0) + actualBackTitleOnHiddenWidth;
                    }
                    _titleOnHidden.TranslationX = titleStart + (titleEnd - titleStart) * animationProcessWithEasing;
                }

                //
                // Do opacity animations
                //

                // Back title opacity
                View actualBackTitle = _customBackTitle != null ? _customBackTitle : _backTitle;
                if (actualBackTitle != null)
                {
                    if (_customContent != null)
                    {
                        actualBackTitle.Opacity = 0;
                    }
                    else if (animationProcessWithEasing <= 0.5)
                    {
                        actualBackTitle.Opacity = 1 - animationProcessWithEasing * 2;
                    }
                    else
                    {
                        actualBackTitle.Opacity = 0;
                    }
                }

                // Back title on hidden opacity
                View actualBackTitleOnHidden = _customBackTitleOnHidden != null ? _customBackTitleOnHidden : _backTitleOnHidden;
                if (actualBackTitleOnHidden != null)
                {
                    if (_customContentOnHidden != null)
                    {
                        actualBackTitleOnHidden.Opacity = 0;
                    }
                    else if (animationProcessWithEasing <= 0.5)
                    {
                        actualBackTitleOnHidden.Opacity = 0;
                    }
                    else
                    {
                        actualBackTitleOnHidden.Opacity = (animationProcessWithEasing - 0.5) * 2;
                    }
                }

                // Title on center opacity
                View actualTitleOnCenter = _customTitleOnCenter != null ? _customTitleOnCenter : _titleOnCenter;
                if (actualTitleOnCenter != null)
                {
                    if (_customContent != null)
                    {
                        actualTitleOnCenter.Opacity = 0;
                    }
                    else if (animationProcessWithEasing <= 0.5)
                    {
                        actualTitleOnCenter.Opacity = 1 - animationProcessWithEasing * 2;
                    }
                    else
                    {
                        actualTitleOnCenter.Opacity = 0;
                    }
                }

                // Title on hidden opacity
                View actualTitleOnHidden = _customTitleOnHidden != null ? _customTitleOnHidden : _titleOnHidden;
                if (actualTitleOnHidden != null)
                {
                    if (_customContentOnHidden != null)
                    {
                        actualTitleOnHidden.Opacity = 0;
                    }
                    else if (animationProcessWithEasing <= 0.5)
                    {
                        actualTitleOnHidden.Opacity = animationProcessWithEasing * 2;
                    }
                    else
                    {
                        actualTitleOnHidden.Opacity = 1;
                    }
                }
            }
            // Titles fade
            else if (Animation == Animations.Fade)
            {
                InitializeComponents(width, pageTo);

                //
                // Do opacity animations
                //

                // Back title opacity
                View actualBackTitle = _customBackTitle != null ? _customBackTitle : _backTitle;
                if (actualBackTitle != null)
                {
                    if (_customContent != null)
                    {
                        actualBackTitle.Opacity = 0;
                    }
                    else if (animationProcessWithEasing <= 0.5)
                    {
                        actualBackTitle.Opacity = 1 - animationProcessWithEasing * 2;
                    }
                    else
                    {
                        actualBackTitle.Opacity = 0;
                    }
                }

                // Back title on hidden opacity
                View actualBackTitleOnHidden = _customBackTitleOnHidden != null ? _customBackTitleOnHidden : _backTitleOnHidden;
                if (actualBackTitleOnHidden != null)
                {
                    if (_customContentOnHidden != null)
                    {
                        actualBackTitleOnHidden.Opacity = 0;
                    }
                    else if (animationProcessWithEasing <= 0.5)
                    {
                        actualBackTitleOnHidden.Opacity = 0;
                    }
                    else
                    {
                        actualBackTitleOnHidden.Opacity = (animationProcessWithEasing - 0.5) * 2;
                    }
                }

                // Title on center opacity
                View actualTitleOnCenter = _customTitleOnCenter != null ? _customTitleOnCenter : _titleOnCenter;
                if (actualTitleOnCenter != null)
                {
                    if (_customContent != null)
                    {
                        actualTitleOnCenter.Opacity = 0;
                    }
                    else if (animationProcessWithEasing <= 0.5 && GetIsTitleVisible(pageFrom))
                    {
                        actualTitleOnCenter.Opacity = 1 - animationProcessWithEasing * 2;
                    }
                    else
                    {
                        actualTitleOnCenter.Opacity = 0;
                    }
                }

                // Title on hidden opacity
                View actualTitleOnHidden = _customTitleOnHidden != null ? _customTitleOnHidden : _titleOnHidden;
                if (actualTitleOnHidden != null)
                {
                    if (_customContentOnHidden != null)
                    {
                        actualTitleOnHidden.Opacity = 0;
                    }
                    else if (animationProcessWithEasing <= 0.5)
                    {
                        actualTitleOnHidden.Opacity = 0;
                    }
                    else if (GetIsTitleVisible(pageTo))
                    {
                        actualTitleOnHidden.Opacity = (animationProcessWithEasing - 0.5) * 2;
                    }
                }
            }

            // Back button opacity changes is same to all animations.
            if (_backButton != null)
            {
                if (hasPageToBackButtonVisible == true && hasPageFromBackButtonVisible == false)
                {
                    _backButton.Opacity = animationProcess;
                }
                // If not then, animate opacity to hidden if not hidden already
                else if (hasPageToBackButtonVisible == false && hasPageFromBackButtonVisible == true)
                {
                    _backButton.Opacity = 1 - animationProcess;
                }
                else if (hasPageToBackButtonVisible == false && hasPageFromBackButtonVisible == false)
                {
                    _backButton.Opacity = 0;
                }

                _backButton.InputTransparent = !hasPageToBackButtonVisible;
            }

            if (_rightViewOnHidden != null)
            {
                if (animationProcessWithEasing <= 0.5)
                {
                    _rightViewOnHidden.Opacity = 0;
                }
                else
                {
                    _rightViewOnHidden.Opacity = (animationProcessWithEasing - 0.5) * 2;
                }
            }
            if (_rightView != null)
            {
                if (animationProcessWithEasing <= 0.5)
                {
                    _rightView.Opacity = 1 - animationProcessWithEasing * 2;
                }
                else
                {
                    _rightView.Opacity = 0;
                }
            }

            if (_customContentOnHidden != null)
            {
                if (animationProcessWithEasing <= 0.5)
                {
                    _customContentOnHidden.Opacity = 0;
                }
                else
                {
                    _customContentOnHidden.Opacity = (animationProcessWithEasing - 0.5) * 2;
                }
            }
            if (_customContent != null)
            {
                if (animationProcessWithEasing <= 0.5)
                {
                    _customContent.Opacity = animationProcessWithEasing * 2;
                }
                else
                {
                    _customContent.Opacity = 0;
                }
            }
        }

        /// <summary>
        /// Set parts back animation 0 -> 1
        /// </summary>
        /// <param name="animationProcess">0 = current page, 1 = previous page without easing</param>
        /// <param name="pageFrom">Page when is navigated back</param>
        private void BackAnimation(double animationProcess, double width, ContentPage pageFrom, ContentPage pageTo)
        {
            if (NavigationHistory.Count == 0)
            {
                return;
            }

            double animationProcessWithEasing = animationProcess;
            if (AnimationEasing != null)
            {
                animationProcessWithEasing = AnimationEasing.Ease(animationProcess);
            }

            // Background color
            Color backgroundColorFrom = GetBackgroundColor(pageFrom);
            Color backgroundColorTo = GetBackgroundColor(pageTo);
            if (backgroundColorFrom != backgroundColorTo)
            {
                BackgroundColor = AnimationUtils.ColorTransform(animationProcess, backgroundColorFrom, backgroundColorTo);
            }

            _shadowView.Opacity = GetIsShadowVisible(CurrentPage) ? 1 : 0;

            bool hasPageToBackButtonVisible = IsBackButtonVisible(pageTo);
            bool hasPageFromBackButtonVisible = IsBackButtonVisible(pageFrom);

            if (Animation == Animations.SlideHorizontal)
            {
                // Back title
                double backTitleStart = hasPageFromBackButtonVisible ? _backButtonSize.Width : 0;
                if (_customBackTitle != null)
                {
                    double end = (width - _customBackTitleSize.Width) / 2;
                    _customBackTitle.TranslationX = backTitleStart + (end - backTitleStart) * animationProcessWithEasing;
                }
                else if (_backTitle != null)
                {
                    double end = (width - _backTitleSize.Width) / 2;
                    _backTitle.TranslationX = backTitleStart + (end - backTitleStart) * animationProcessWithEasing;
                }

                // Back title on hidden
                double backTitleOnHiddenEnd = hasPageToBackButtonVisible ? _backButtonSize.Width : 0;
                double actualBackTitleOnHiddenWidth = 0;
                if (_customBackTitleOnHidden != null)
                {
                    double backTitleOnHiddenStart = -(((width - _customBackTitleOnHiddenSize.Width) / 2) - backTitleOnHiddenEnd);
                    _customBackTitleOnHidden.TranslationX = backTitleOnHiddenStart + (backTitleOnHiddenEnd - backTitleOnHiddenStart) * animationProcessWithEasing;
                    actualBackTitleOnHiddenWidth = _customBackTitleOnHiddenSize.Width;
                }
                else if (_backTitleOnHidden != null)
                {
                    double backTitleOnHiddenStart = -(((width - _backTitleOnHiddenSize.Width) / 2) - backTitleOnHiddenEnd);
                    _backTitleOnHidden.TranslationX = backTitleOnHiddenStart + (backTitleOnHiddenEnd - backTitleOnHiddenStart) * animationProcessWithEasing;
                    actualBackTitleOnHiddenWidth = _backTitleOnHiddenSize.Width;
                }

                // Title on center
                if (_customTitleOnCenter != null)
                {
                    double titleEnd = width - (_customTitleOnCenterSize.Width / 2);
                    double titleStart = 0;

                    if (TitleAlignment == TitleAlignments.Center)
                    {
                        titleStart = (width - _customTitleOnCenterSize.Width) / 2;
                    }
                    else
                    {
                        titleStart = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0) + actualBackTitleOnHiddenWidth;
                    }
                    _customTitleOnCenter.TranslationX = titleStart + (titleEnd - titleStart) * animationProcessWithEasing;
                }
                else if (_titleOnCenter != null)
                {
                    double titleEnd = width - (_titleOnCenterSize.Width / 2);
                    double titleStart = 0;

                    if (TitleAlignment == TitleAlignments.Center)
                    {
                        titleStart = (width - _titleOnCenterSize.Width) / 2;
                    }
                    else
                    {
                        titleStart = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0) + actualBackTitleOnHiddenWidth;
                    }
                    _titleOnCenter.TranslationX = titleStart + (titleEnd - titleStart) * animationProcessWithEasing;
                }

                // Title on hidden
                if (_customTitleOnHidden != null)
                {
                    double titleStart = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0);
                    double titleEnd = 0;
                    if (TitleAlignment == TitleAlignments.Center)
                    {
                        titleEnd = (width - _customTitleOnHiddenSize.Width) / 2;
                    }
                    else
                    {
                        titleEnd = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0) + actualBackTitleOnHiddenWidth;
                    }
                    _customTitleOnHidden.TranslationX = titleStart + (titleEnd - titleStart) * animationProcessWithEasing;
                }
                else if (_titleOnHidden != null)
                {
                    double titleStart = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0);
                    double titleEnd = 0;
                    if (TitleAlignment == TitleAlignments.Center)
                    {
                        titleEnd = (width - _titleOnHiddenSize.Width) / 2;
                    }
                    else
                    {
                        titleEnd = (hasPageFromBackButtonVisible ? _backButtonSize.Width : 0) + actualBackTitleOnHiddenWidth;
                    }
                    _titleOnHidden.TranslationX = titleStart + (titleEnd - titleStart) * animationProcessWithEasing;
                }
            }
            // Titles fade
            else if (Animation == Animations.Fade)
            {
                // Do only opacity animations...
            }

            //
            // Do opacity animations. Same to all back animations.
            //

            // Is back button visible on page to back navigate
            if (_backButton != null)
            {
                if (hasPageToBackButtonVisible == true && hasPageFromBackButtonVisible == false)
                {
                    _backButton.Opacity = animationProcess;
                }
                // If not then, animate opacity to hidden if not hidden already
                else if (hasPageToBackButtonVisible == false && hasPageFromBackButtonVisible == true)
                {
                    _backButton.Opacity = 1 - animationProcess;
                }
                else if (hasPageToBackButtonVisible == false && hasPageFromBackButtonVisible == false)
                {
                    _backButton.Opacity = 0;
                }

                _backButton.InputTransparent = !hasPageToBackButtonVisible;
            }

            // Back title opacity
            View actualBackTitle = _customBackTitle != null ? _customBackTitle : _backTitle;
            if (actualBackTitle != null)
            {
                if (_customContent != null)
                {
                    actualBackTitle.Opacity = 0;
                }
                else if (animationProcessWithEasing <= 0.5)
                {
                    actualBackTitle.Opacity = 1 - animationProcessWithEasing * 2;
                }
                else
                {
                    actualBackTitle.Opacity = 0;
                }
            }

            // Back title on hidden opacity
            View actualBackTitleOnHidden = _customBackTitleOnHidden != null ? _customBackTitleOnHidden : _backTitleOnHidden;
            if (actualBackTitleOnHidden != null)
            {
                if (_customContentOnHidden != null)
                {
                    actualBackTitleOnHidden.Opacity = 0;
                }
                else if (animationProcessWithEasing <= 0.5)
                {
                    actualBackTitleOnHidden.Opacity = 0;
                }
                else
                {
                    actualBackTitleOnHidden.Opacity = (animationProcessWithEasing - 0.5) * 2;
                }
            }

            // Title on center opacity
            View actualTitleOnCenter = _customTitleOnCenter != null ? _customTitleOnCenter : _titleOnCenter;
            if (actualTitleOnCenter != null)
            {
                if (_customContent != null)
                {
                    actualTitleOnCenter.Opacity = 0;
                }
                else if (animationProcessWithEasing <= 0.5)
                {
                    actualTitleOnCenter.Opacity = 1 - animationProcessWithEasing * 2;
                }
                else
                {
                    actualTitleOnCenter.Opacity = 0;
                }
            }

            // Title on hidden opacity
            View actualTitleOnHidden = _customTitleOnHidden != null ? _customTitleOnHidden : _titleOnHidden;
            if (actualTitleOnHidden != null)
            {
                if (_customContentOnHidden != null)
                {
                    actualTitleOnHidden.Opacity = 0;
                }
                else if (animationProcessWithEasing <= 0.5)
                {
                    actualTitleOnHidden.Opacity = 0;
                }
                else
                {
                    actualTitleOnHidden.Opacity = (animationProcessWithEasing - 0.5) * 2 * animationProcessWithEasing;
                }
            }

            if (_rightViewOnHidden != null)
            {
                if (animationProcessWithEasing <= 0.5)
                {
                    _rightViewOnHidden.Opacity = 0;
                }
                else
                {
                    _rightViewOnHidden.Opacity = (animationProcessWithEasing - 0.5) * 2;
                }
            }
            if (_rightView != null)
            {
                if (animationProcessWithEasing <= 0.5)
                {
                    _rightView.Opacity = 1 - animationProcessWithEasing * 2;
                }
                else
                {
                    _rightView.Opacity = 0;
                }
            }

            if (_customContentOnHidden != null)
            {
                if (animationProcessWithEasing <= 0.5)
                {
                    _customContentOnHidden.Opacity = 0;
                }
                else
                {
                    _customContentOnHidden.Opacity = (animationProcessWithEasing - 0.5) * 2;
                }
            }
            if (_customContent != null)
            {
                if (animationProcessWithEasing <= 0.5)
                {
                    _customContent.Opacity = 1 - animationProcessWithEasing * 2;
                }
                else
                {
                    _customContent.Opacity = 0;
                }
            }
        }

        /// <summary>
        /// Update part locations without animation. Set hidden parts to same location than visible.
        /// </summary>
        private void InitializeComponents(double width, ContentPage currentPage)
        {
            if (currentPage == null)
            {
                return;
            }

            BackgroundColor = GetBackgroundColor(currentPage);

            _bottomLine.Opacity = GetIsLineVisible(currentPage) ? 1 : 0;
            _bottomLine.InputTransparent = GetIsLineVisible(currentPage) ? false : true;
            _bottomLine.BackgroundColor = GetLineColor(CurrentPage);
            _shadowView.Opacity = GetIsShadowVisible(CurrentPage) ? 1 : 0;

            // Back button

            Size actualBackButtonSize = new Size();
            if (_backButton != null)
            {
                if (IsBackButtonVisible(currentPage))
                {
                    _backButton.Opacity = 1;
                    _backButton.InputTransparent = false;
                    actualBackButtonSize = _backButtonSize;
                }
                else
                {
                    _backButton.Opacity = 0;
                    _backButton.InputTransparent = true;
                }

                _backButton.TranslationX = 0;
            }

            // Back title

            Size actualBackTitleSize = new Size();
            if (_customBackTitle != null)
            {
                if (GetIsBackTitleVisible(currentPage) && NavigationHistory.Count > 1 && GetIsSearchMode(currentPage) == false && _customContent == null)
                {
                    _customBackTitle.Opacity = 1;
                    _customBackTitle.InputTransparent = false;
                    actualBackTitleSize = _customBackTitleSize;
                }
                else
                {
                    _customBackTitle.Opacity = 0;
                    _customBackTitle.InputTransparent = true;
                }

                _customBackTitle.TranslationX = actualBackButtonSize.Width;
            }

            if (_customBackTitleOnHidden != null)
            {
                _customBackTitleOnHidden.Opacity = 0;
                _customBackTitleOnHidden.InputTransparent = true;
                _customBackTitleOnHidden.TranslationX = actualBackButtonSize.Width;
            }

            if (_backTitle != null)
            {
                if (GetIsBackTitleVisible(currentPage) &&
                    NavigationHistory.Count > 1 &&
                    _customBackTitle == null &&
                    GetIsSearchMode(currentPage) == false &&
                    _customContent == null)
                {
                    _backTitle.Opacity = 1;
                    _backTitle.InputTransparent = false;
                    actualBackTitleSize = _backTitleSize;
                }
                else
                {
                    _backTitle.Opacity = 0;
                    _backTitle.InputTransparent = true;
                }

                _backTitle.TranslationX = actualBackButtonSize.Width;
            }

            if (_backTitleOnHidden != null)
            {
                _backTitleOnHidden.Opacity = 0;
                _backTitleOnHidden.InputTransparent = true;
                _backTitleOnHidden.TranslationX = actualBackButtonSize.Width;
            }

            // Title on center

            if (_customTitleOnCenter != null)
            {
                if (GetIsTitleVisible(currentPage) && GetIsSearchMode(currentPage) == false && _customContent == null)
                {
                    _customTitleOnCenter.Opacity = 1;
                    _customTitleOnCenter.InputTransparent = false;
                }
                else
                {
                    _customTitleOnCenter.Opacity = 0;
                    _customTitleOnCenter.InputTransparent = true;
                }

                if (TitleAlignment == TitleAlignments.Left)
                {
                    _customTitleOnCenter.TranslationX = actualBackButtonSize.Width + actualBackTitleSize.Width;
                }
                else
                {
                    _customTitleOnCenter.TranslationX = (width - _customTitleOnCenterSize.Width) / 2;
                }
            }

            if (_customTitleOnHidden != null)
            {
                _customTitleOnHidden.Opacity = 0;
                _customTitleOnHidden.InputTransparent = true;

                if (TitleAlignment == TitleAlignments.Left)
                {
                    _customTitleOnHidden.TranslationX = actualBackButtonSize.Width + actualBackTitleSize.Width;
                }
                else
                {
                    _customTitleOnHidden.TranslationX = (width - _customTitleOnHiddenSize.Width) / 2;
                }
            }

            if (_titleOnCenter != null)
            {
                if (GetIsTitleVisible(currentPage) &&
                    _customTitleOnCenter == null &&
                    GetIsSearchMode(currentPage) == false &&
                    _customContent == null)
                {
                    _titleOnCenter.Opacity = 1;
                    _titleOnCenter.InputTransparent = false;
                }
                else
                {
                    _titleOnCenter.Opacity = 0;
                    _titleOnCenter.InputTransparent = true;
                }

                if (TitleAlignment == TitleAlignments.Left)
                {
                    _titleOnCenter.TranslationX = actualBackButtonSize.Width + actualBackTitleSize.Width;
                }
                else
                {
                    _titleOnCenter.TranslationX = (width - _titleOnCenterSize.Width) / 2;
                }
            }

            if (_titleOnHidden != null)
            {
                _titleOnHidden.Opacity = 0;
                _titleOnHidden.InputTransparent = true;

                if (TitleAlignment == TitleAlignments.Left)
                {
                    _titleOnHidden.TranslationX = actualBackButtonSize.Width + actualBackTitleSize.Width;
                }
                else
                {
                    _titleOnHidden.TranslationX = (width - _titleOnHiddenSize.Width) / 2;
                }
            }
        }

        #endregion

        private bool IsBackButtonVisible(ContentPage page)
        {
            if (page == null || _backButton == null)
            {
                return false;
            }

            int pageIndex = NavigationHistory.IndexOf(page);

            return ((pageIndex > 0 || pageIndex == -1) && GetBackButtonMode(page) != BackButtonModes.Hidden) ||
                   (pageIndex == 0 && (GetBackButtonMode(page) == BackButtonModes.Close || GetBackButtonMode(page) == BackButtonModes.Menu));
        }

        /// <summary>
        /// Create horizontal line
        /// </summary>
        private BoxView CreateLine()
        {
            BoxView b = new BoxView();
            b.HorizontalOptions = LayoutOptions.Fill;
            b.VerticalOptions = LayoutOptions.End;

            Binding bind = new Binding(LineHeightProperty.PropertyName);
            bind.Source = this;
            b.SetBinding(BoxView.HeightRequestProperty, bind);

            return b;
        }

        /// <summary>
        /// Swichs the hidden and centre title pointers
        /// </summary>
        private void Swap(ref View a, ref View b)
        {
            View tmp = a;
            a = b;
            b = tmp;
        }

        /// <summary>
        /// Swichs the hidden and centre title size pointers
        /// </summary>
        private void Swap(ref Size a, ref Size b)
        {
            Size tmp = a;
            a = b;
            b = tmp;
        }

        /// <summary>
        /// Set binding context when view is not a child. Avoid measure invalidation in Android! Not working on UWP.
        /// </summary>
        private void SetBindingContext(View view, object bindingContext)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                Children.Remove(view);
            }

            view.BindingContext = bindingContext;

            if (Device.RuntimePlatform == Device.Android)
            {
                Children.Add(view);
            }
        }
    }

    /// <summary>
    /// NavigationBar visibility modes
    /// </summary>
    public enum NavigationBarVisibility { Visible, Hidden, Scroll, SmoothScroll }

    /// <summary>
    /// NavigationBar title alignment in available space
    /// </summary>
    public enum TitleAlignments { Center, Left }

    /// <summary>
    /// NavigationBar back button modes
    /// </summary>
    public enum BackButtonModes { Close, Menu, Back, Hidden }

    /// <summary>
    /// Interface for NavigationBar back button
    /// </summary>
    public interface IBackButton
    {
        BackButtonModes Mode { get; set; }
    }
}