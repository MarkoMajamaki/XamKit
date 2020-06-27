using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// Navigation page for navigation between content pages
    /// </summary>
    public class NavigationPage : Layout<View>, INavigation
    {
        private Dictionary<ContentPage, TaskCompletionSource<object>> _modalTaskDictionary = new Dictionary<ContentPage, TaskCompletionSource<object>>();

        // Animation names
        private const string _pushAnimationName = "pushAnimation";
        private const string _popAnimationName = "popAnimation";
        private const string _modalOutAnimationName = "modalOutAnimation";
        private const string _modalInAnimationName = "modalInAnimation";
        private const string _panToCurrentPageAnimationName = "panToCurrentPageAnimation";

        // Part names in templates
        private const string _navigationBarName = "NavigationBar";

        // Navigation stack which includes modal and normal views
        private List<PageInfo> _navigationStack = new List<PageInfo>();

        // List for navigation control navigation history
        private ObservableCollection<ContentPage> _navigationBarStack = null;
        private ObservableCollection<ContentPage> _tmpNavigationBarStack = null;

        private const uint _animationRatio = 64;
        private const double _backPanBoxWidth = 20;
        private double _backPanTotalX = 0;

        //
        // Parts
        //

        // Page containers which contains: Page, ModalPage (and ToolBar and NavgiationControl sometimes during animation)
        private Container _topContainer = null;
        private Container _bottomContainer = null;

        private NavigationBarInfo _topNavigationBar = null;
        private NavigationBarInfo _bottomNavigationBar = null;
        private ModalNavigationPageInfo _topModalNavigationPage = null;
        private ModalNavigationPageInfo _bottomModalNavigationPage = null;

        private BoxView _navigationDarkOverlay = null;       // Over everything. Between containers.
        private BoxView _navigationBarModalDarkness = null;  // Over NavigationBar
        private BoxView _backPanAreaBoxView = null;          // On left

        private bool _isInvalidationIgnored = false;

        public bool IsDebugEnabled { get; set; } = false;

        #region Properties

        /// <summary>
        /// Get current page modal views stack
        /// </summary>
        public IReadOnlyList<ContentPage> ModalNavigationStack
        {
            get
            {
                if (_navigationStack != null && _navigationStack.Count > 0)
                {
                    return new ReadOnlyCollection<ContentPage>(_navigationStack.LastOrDefault().ModalPages.ToList());
                }
                else
                {
                    return new List<ContentPage>();
                }
            }
        }

        /// <summary>
        /// Get all views stack
        /// </summary>
        public IReadOnlyList<ContentPage> NavigationStack
        {
            get
            {
                return new ReadOnlyCollection<ContentPage>(_navigationStack.Select(c => c.Page).ToList());
            }
        }

        /// <summary>
        /// Has any pages in navigation view
        /// </summary>
        public EventHandler<bool> HasPagesChanged { get; set; }

        /// <summary>
        /// Event when navigation bar close button is tapped
        /// </summary>
        public EventHandler CloseButtonTapped { get; set; }

        /// <summary>
        /// Event when navigation bar menu button is tapped
        /// </summary>
        public EventHandler MenuButtonTapped { get; set; }

        /// <summary>
        /// Event when navigation bar back button is tapped
        /// </summary>
        public EventHandler BackButtonTapped { get; set; }

        /// <summary>
        /// Event when navigation bar modal darkness layer is tapped
        /// </summary>
        internal event EventHandler NavigationBarModalDarknessLayerTapped;

        #endregion

        #region Binding properties

        /// <summary>
        /// Modal background color
        /// </summary>
        public static readonly BindableProperty HasPagesProperty =
            BindableProperty.Create("HasPages", typeof(bool), typeof(NavigationPage), false, propertyChanged: OnHasPagesChanged);

        private static void OnHasPagesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            NavigationPage page = bindable as NavigationPage;

            if (page.HasPagesChanged != null)
            {
                page.HasPagesChanged(page, (bool)newValue);
            }
        }

        public bool HasPages
        {
            get { return (bool)GetValue(HasPagesProperty); }
            protected set { SetValue(HasPagesProperty, value); }
        }

        /// <summary>
        /// Corner radius if page is modal
        /// </summary>
        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create("CornerRadius", typeof(double), typeof(NavigationPage), 0.0);

        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        #endregion

        #region Binding properties - Templates

        /// <summary>
        /// Template to create common navigation bar for all pages. Template must contain INavigationBar element which name is "NavigationBar"!
        /// </summary>
        public static readonly BindableProperty NavigationBarTemplateProperty =
            BindableProperty.Create("NavigationBarTemplate", typeof(DataTemplate), typeof(NavigationPage), null);

        public DataTemplate NavigationBarTemplate
        {
            get { return (DataTemplate)GetValue(NavigationBarTemplateProperty); }
            set { SetValue(NavigationBarTemplateProperty, value); }
        }

        /// <summary>
        /// Style for modal navigation page
        /// </summary>
        public static readonly BindableProperty ModalNavigationPageStyleProperty =
            BindableProperty.Create("ModalNavigationPageStyle", typeof(Style), typeof(NavigationPage), null);

        public Style ModalNavigationPageStyle
        {
            get { return (Style)GetValue(ModalNavigationPageStyleProperty); }
            set { SetValue(ModalNavigationPageStyleProperty, value); }
        }

        #endregion

        #region Binding properties - Panning

        /// <summary>
        /// Is back pan enabled
        /// </summary>
        public static readonly BindableProperty IsBackPanEnabledProperty =
            BindableProperty.Create("IsBackPanEnabled", typeof(bool), typeof(NavigationPage), true);

        public bool IsBackPanEnabled
        {
            get { return (bool)GetValue(IsBackPanEnabledProperty); }
            set { SetValue(IsBackPanEnabledProperty, value); }
        }

        /// <summary>
        /// Is panning currently active
        /// </summary>
        public static readonly BindableProperty IsPanningProperty =
            BindableProperty.Create("IsPanning", typeof(bool), typeof(NavigationPage), false);

        public bool IsPanning
        {
            get { return (bool)GetValue(IsPanningProperty); }
            protected set { SetValue(IsPanningProperty, value); }
        }

        #endregion

        #region Binding properties - Colors

        /// <summary>
        /// Modal color when navigated or opened modal page
        /// </summary>
        public static readonly BindableProperty ModalColorProperty =
            BindableProperty.Create("ModalColor", typeof(ModalColors), typeof(NavigationPage), ModalColors.Black);

        public ModalColors ModalColor
        {
            get { return (ModalColors)GetValue(ModalColorProperty); }
            set { SetValue(ModalColorProperty, value); }
        }

        /// <summary>
        /// Modal color opacity when navigated or opened modal page
        /// </summary>
        public static readonly BindableProperty ModalColorOpacityProperty =
            BindableProperty.Create("ModalColorOpacity", typeof(double), typeof(NavigationPage), 0.2);

        public double ModalColorOpacity
        {
            get { return (double)GetValue(ModalColorOpacityProperty); }
            set { SetValue(ModalColorOpacityProperty, value); }
        }

        #endregion

        public NavigationPage()
        {
            _isInvalidationIgnored = true;

            _navigationBarStack = new ObservableCollection<ContentPage>();

            IsClippedToBounds = true;

            //
            // Bottom page container
            //

            _bottomContainer = new Container();
            _bottomContainer.ModalPageBackgroundTapped += async (s, a) => await PopModalAsync(null, true);
            _bottomContainer.ModalPageBackgroundColor = ModalColor;
            Children.Add(_bottomContainer);

            Binding bind = new Binding(BackgroundColorProperty.PropertyName);
            bind.Source = this;
            bind.Mode = BindingMode.OneWay;
            _bottomContainer.SetBinding(BackgroundColorProperty, bind);

            // 
            // Navigation animation dark overlay
            //
            _navigationDarkOverlay = new BoxView();
            _navigationDarkOverlay.Opacity = 0;
            _navigationDarkOverlay.InputTransparent = true;
            _navigationDarkOverlay.Color = ModalColor == ModalColors.Black ? Color.Black : Color.White;
            Children.Add(_navigationDarkOverlay);

            //
            // Top page container
            //

            _topContainer = new Container();
            _topContainer.ModalPageBackgroundTapped += async (s, a) => await PopModalAsync(null, true);
            _topContainer.ModalPageBackgroundColor = ModalColor;
            Children.Add(_topContainer);

            bind = new Binding(BackgroundColorProperty.PropertyName);
            bind.Source = this;
            bind.Mode = BindingMode.OneWay;
            _topContainer.SetBinding(BackgroundColorProperty, bind);

            // 
            // Back pan area box
            //

            _backPanAreaBoxView = new BoxView();
            _backPanAreaBoxView.Color = Color.Transparent;
            _backPanAreaBoxView.InputTransparent = true;
            Children.Add(_backPanAreaBoxView);

            PanGestureRecognizer pan = new PanGestureRecognizer();
            pan.PanUpdated += OnBackPan;
            _backPanAreaBoxView.GestureRecognizers.Add(pan);

            bind = new Binding(IsBackPanEnabledProperty.PropertyName);
            bind.Source = this;
            _backPanAreaBoxView.SetBinding(IsVisibleProperty, bind);

            // 
            // NavigationBar
            //

            if (_topNavigationBar == null && NavigationBarTemplate != null)
            {
                _topNavigationBar = CreateNavigationBar();
            }

            // 
            // Toolbar modal background (over NavigationBar always)
            //

            _navigationBarModalDarkness = new BoxView();
            _navigationBarModalDarkness.InputTransparent = true;
            _navigationBarModalDarkness.Opacity = 0;
            Children.Add(_navigationBarModalDarkness);

            TapGestureRecognizer tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnNavigationBarModalDarknessLayerTapped;
            _navigationBarModalDarkness.GestureRecognizers.Add(tapGesture);

            // Quick and dirty: Prevent tapping throught
            if (Device.RuntimePlatform == Device.Android)
            {
                this.GestureRecognizers.Add(new TapGestureRecognizer());
            }

            _isInvalidationIgnored = false;
        }

        /// <summary>
        /// Update toolbar, navigationbar, modal navigation page 
        /// </summary>
        /// <param name="propertyName"></param>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == NavigationBarTemplateProperty.PropertyName)
            {
                _isInvalidationIgnored = Width < 0;

                if (NavigationBarTemplate == null)
                {
                    _topContainer.NavigationBar = null;
                    _bottomContainer.NavigationBar = null;
                    _topNavigationBar = null;
                    _bottomNavigationBar = null;
                }
                else
                {
                    if (_bottomContainer != null)
                    {
                        _bottomContainer.NavigationBar = null;

                        if (_bottomNavigationBar != null && _bottomNavigationBar.View != null)
                        {
                            Children.Remove(_bottomNavigationBar.View);
                        }

                        _bottomNavigationBar = null;
                    }

                    if (_topNavigationBar != null)
                    {
                        _topContainer.NavigationBar = null;

                        if (_topNavigationBar != null && _topNavigationBar.View != null)
                        {
                            Children.Remove(_topNavigationBar.View);
                        }

                        _topNavigationBar = null;
                    }

                    _topNavigationBar = CreateNavigationBar();
                    _topNavigationBar.NavigationBar?.Initialize(_navigationBarStack);

                    if (_topContainer != null)
                    {
                        if (_navigationStack.Count > 0 && NavigationBar.GetVisibility(_navigationStack.Last().Page) == NavigationBarVisibility.Hidden)
                        {
                            // Do nothing...
                        }
                        else
                        {
                            Children.Add(_topNavigationBar.View);
                        }
                    }
                }

                _isInvalidationIgnored = true;
            }
            else if (propertyName == ModalColorProperty.PropertyName)
            {
                if (_navigationDarkOverlay != null)
                {
                    _navigationDarkOverlay.Color = ModalColor == ModalColors.Black ? Color.Black : Color.White;
                }
                if (_navigationBarModalDarkness != null)
                {
                    _navigationBarModalDarkness.Color = ModalColor == ModalColors.Black ? Color.Black : Color.White;
                }
            }
            else if (propertyName == ModalNavigationPageStyleProperty.PropertyName)
            {
                // TODO
                _bottomModalNavigationPage = null;
                _topModalNavigationPage = null;
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Called when back button is pressed.
        /// </summary>
        /// <returns>True if navigation out from the app is ignored</returns>
        public bool OnDeviceBackButtonPressed()
        {
            if (ModalNavigationStack.Count > 0)
            {
                Task task = PopModalAsync(null, false);
                return true;
            }
            else if (NavigationStack.Count > 1)
            {
                // Check is device back button pressed ignored in active page
                if (NavigationStack.Last().OnDeviceBackButtonPressed())
                {
                    return true;
                }
                else
                {
                    Task task = PopAsync();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Measure invalidation
        
        /// <summary>
        /// Should invalidate measure and arrange when child is added
        /// </summary>
        protected override bool ShouldInvalidateOnChildAdded(View child)
        {
            return base.ShouldInvalidateOnChildAdded(child) && !_isInvalidationIgnored;
        }

        /// <summary>
        /// Should invalidate measure and arrange when child is removed
        /// </summary>
        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
            return base.ShouldInvalidateOnChildRemoved(child) && !_isInvalidationIgnored;
        }

        protected override void OnChildMeasureInvalidated()
        {
            if (_isInvalidationIgnored == false)
            {
                base.OnChildMeasureInvalidated();
            }
        }

        protected override void InvalidateLayout()
        {
            if (_isInvalidationIgnored == false)
            {
                base.InvalidateLayout();
            }
        }

        protected override void InvalidateMeasure()
        {
            if (_isInvalidationIgnored == false)
            {
                base.InvalidateMeasure();
            }
        }
        
        #endregion

        #region Layout / measure

        /// <summary>
        /// Measure all children
        /// </summary>
        /// <returns>Page total size</returns>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("NavigationPage.OnMeasure: Top page = " + (NavigationStack.Count > 0 ? XamKit.NavigationBar.GetTitle(NavigationStack.Last()) : "no pages")); }

            bool isCurrentPageTop = NavigationStack == null || NavigationStack.Count == 0 || NavigationStack.Last() == _topContainer.Page;

            Size size = new Size();

            // Measure container size we need width to measure other parts height based on page width. Total size is based 
            // on current active page. Only current active page is measured.
            if (isCurrentPageTop)
            {
                size = MeasureSize(widthConstraint, heightConstraint, _topContainer, _topNavigationBar);
            }
            else
            {
                size = MeasureSize(widthConstraint, heightConstraint, _bottomContainer, _bottomNavigationBar);
            }

            return new SizeRequest(size, size);
        }

        /// <summary>
        /// Measure container size with navigation bar
        /// </summary>
        private Size MeasureSize(double widthConstraint, double heightConstraint, Container container, NavigationBarInfo navigationBar)
        {
            Size size = new Size();

            bool isFullWidthRequest =
                container.Page == null ||
                container.Page.WidthRequest >= widthConstraint ||
                (container.Page.HorizontalOptions.Alignment == LayoutAlignment.Fill || container.Page.WidthRequest < 0);

            bool isFullHeightRequest =
                container.Page == null ||
                container.Page.HeightRequest >= heightConstraint ||
                (container.Page.VerticalOptions.Alignment == LayoutAlignment.Fill && container.Page.HeightRequest < 0);

            // Is fully stretch
            if (isFullWidthRequest && isFullHeightRequest)
            {
                size = new Size(widthConstraint, heightConstraint);
            }
            // Height is stretch and width is fixed
            else if (isFullWidthRequest == false && isFullHeightRequest == true && container.Page.WidthRequest > 0)
            {
                size = container.Measure(container.Page.WidthRequest, heightConstraint, MeasureFlags.IncludeMargins).Request;
            }
            // Width is strecth and height is fixed
            else if (isFullWidthRequest == true && isFullHeightRequest == false && container.Page.HeightRequest > 0)
            {
                size = container.Measure(widthConstraint, container.Page.HeightRequest, MeasureFlags.IncludeMargins).Request;
            }
            // Width and height are fixed
            else if (container.Page.WidthRequest > 0 && container.Page.HeightRequest > 0)
            {
                size = new Size(container.Page.WidthRequest, container.Page.HeightRequest);
            }
            // Other scenarios
            else
            {
                size = container.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins).Request;
            }

            return size;
        }

        /// <summary>
        /// Layout all children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("NavigationPage.LayoutChildren: Top page = " + (NavigationStack.Count > 0 ? XamKit.NavigationBar.GetTitle(NavigationStack.Last()) : "no pages")); }

            if (_topNavigationBar != null && _topNavigationBar.View != null && Children.Contains(_topNavigationBar.View))
            {
                _topContainer.NavigationBarSize = _topNavigationBar.View.Measure(width, height, MeasureFlags.IncludeMargins);
                LayoutChild(_topNavigationBar.View, new Rectangle(0, 0, width, _topContainer.NavigationBarSize.Request.Height));
            }
            if (_bottomNavigationBar != null && _bottomNavigationBar.View != null && Children.Contains(_bottomNavigationBar.View))
            {
                _bottomContainer.NavigationBarSize = _bottomNavigationBar.View.Measure(width, height, MeasureFlags.IncludeMargins);
                LayoutChild(_bottomNavigationBar.View, new Rectangle(0, 0, width, _bottomContainer.NavigationBarSize.Request.Height));
            }

            Rectangle fullLayoutLocation = new Rectangle(0, 0, width, height);
            LayoutChild(_navigationDarkOverlay, fullLayoutLocation);

            if (_topContainer.Page != null)
            {
                LayoutChild(_topContainer, fullLayoutLocation);
            }
            if (_bottomContainer.Page != null)
            {
                LayoutChild(_bottomContainer, fullLayoutLocation);
            }

            LayoutChild(_navigationBarModalDarkness, new Rectangle(0, 0, width, _topContainer.NavigationBarSize.Request.Height));

            LayoutBackPanBox();
        }

        /// <summary>
        /// Layout back pan box to correct position by navigation control and toolbar
        /// </summary>
        private void LayoutBackPanBox()
        {
            if (_navigationStack.Count <= 1)
            {
                LayoutChild(_backPanAreaBoxView, new Rectangle(0, 0, 0, 0));
            }
            else
            {
                ContentPage lastPage = _navigationStack.LastOrDefault().Page;

                double navigationBarHeight = 0;
                double toolBarHeightHeight = 0;

                if (_topContainer.Page == lastPage)
                {
                    if (_topNavigationBar != null)
                    {
                        navigationBarHeight = _topContainer.NavigationBarSize.Request.Height;
                    }
                    else if (_bottomNavigationBar != null)
                    {
                        navigationBarHeight = _bottomContainer.NavigationBarSize.Request.Height;
                    }
                }
                else
                {
                    if (_bottomNavigationBar != null)
                    {
                        navigationBarHeight = _bottomContainer.NavigationBarSize.Request.Height;
                    }
                    else if (_topNavigationBar != null)
                    {
                        navigationBarHeight = _topContainer.NavigationBarSize.Request.Height;
                    }
                }

                LayoutChild(_backPanAreaBoxView, new Rectangle(0, navigationBarHeight, _backPanBoxWidth, Bounds.Height - navigationBarHeight - toolBarHeightHeight));
            }
        }

        /// <summary>
        /// Layout view if needed (optimization)
        /// </summary>
        private void LayoutChild(View child, Rectangle newLocation)
        {
            /*
            if (child != null && child.Bounds != newLocation && Children.Contains(child))
            {
                LayoutChildIntoBoundingRegion(child, newLocation);
            }
            */

            LayoutChildIntoBoundingRegion(child, newLocation);
        }

        /// <summary>
        /// Update children Z order in layout
        /// </summary>
        private void UpdateChildrenOrder()
        {
            RaiseChild(_bottomContainer);

            if (_bottomNavigationBar != null && Children.Contains(_bottomNavigationBar.View))
            {
                RaiseChild(_bottomNavigationBar.View);
            }

            RaiseChild(_navigationDarkOverlay);
            RaiseChild(_topContainer);
            RaiseChild(_backPanAreaBoxView);

            if (_topNavigationBar != null && Children.Contains(_topNavigationBar.View))
            {
                RaiseChild(_topNavigationBar.View);
            }

            RaiseChild(_navigationBarModalDarkness);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize with single page
        /// </summary>
        public void Initialize(ContentPage page)
        {
            Initialize(new List<ContentPage>() { page });
        }

        /// <summary>
        /// Initialize navigation history
        /// </summary>
        /// <param name="pages">Navigation history pages</param>
        public void Initialize(List<ContentPage> pages)
        {
            _isInvalidationIgnored = true;

            //
            // Clear and remove bottom parts
            //

            _bottomContainer.NavigationBarSize = new SizeRequest();
            _bottomContainer.Page = null;
            _bottomContainer.NavigationBar = null;
            _bottomContainer.ModalNavigationPage = null;

            if (_bottomNavigationBar != null)
            {
                _bottomNavigationBar.NavigationBar?.Clear();
                if (_bottomNavigationBar.View != null && Children.Contains(_bottomNavigationBar.View))
                {
                    Children.Remove(_bottomNavigationBar.View);
                }
            }
            if (_bottomModalNavigationPage != null)
            {
                _bottomModalNavigationPage.NavigationPage.Clear();
                if (Children.Contains(_bottomModalNavigationPage.NavigationPage))
                {
                    Children.Remove(_bottomModalNavigationPage.NavigationPage);
                }
            }

            //
            // Init top parts with pages
            //

            ContentPage lastPage = pages.LastOrDefault();

            // Add new page as navigation controller
            lastPage.Navigation = this;

            // Create navigation stack
            _navigationBarStack = new ObservableCollection<ContentPage>(pages);
            foreach (ContentPage page in pages)
            {
                _navigationStack.Add(new PageInfo(page));
            }

            // Create top navigation bar or swap it from the bottom
            if (_topNavigationBar == null && _bottomNavigationBar != null)
            {
                Swap<NavigationBarInfo>(ref _topNavigationBar, ref _bottomNavigationBar);
            }
            else if (_topNavigationBar == null && _bottomNavigationBar == null)
            {
                _topNavigationBar = CreateNavigationBar();
            }

            // Init top navigation bar
            if (_topNavigationBar != null)
            {
                // Remove from container
                _topContainer.NavigationBar = null;

                if (NavigationBar.GetVisibility(lastPage) != NavigationBarVisibility.Hidden)
                {
                    // Add to child
                    if (Children.Contains(_topNavigationBar.View) == false)
                    {
                        Children.Add(_topNavigationBar.View);
                    }
                }
                else
                {
                    // Remove also from child
                    if (Children.Contains(_topNavigationBar.View))
                    {
                        Children.Remove(_topNavigationBar.View);
                    }
                }

                // Init with navigation stack
                _topNavigationBar.NavigationBar.Initialize(_navigationBarStack);
            }
            
            // TODO: Init modal pages

            UpdateChildrenOrder();

            _isInvalidationIgnored = false;

            // Set latest page to top container and raise measure invalidation
            _topContainer.Page = lastPage;

            HasPages = lastPage != null;
        }

        #endregion

        #region Push

        /// <summary>
        /// Push new page
        /// </summary>
        public Task PushAsync(ContentPage page, object parameter = null)
        {
            return InternalPushAsync(page, false, parameter);
        }

        /// <summary>
        /// Push new page as root page
        /// </summary>
        /// <param name="page">New page</param>
        /// <param name="isAnimated">Is page change animated</param>
        public async Task PushRootAsync(ContentPage page, object parameter = null, bool isAnimated = true)
        {
            await InternalPushAsync(page, true, parameter, isAnimated);

            // Clear navigation stack except last item
            int originalCount = _navigationStack.Count;
            for (int i = 0; i < originalCount - 1; i++)
            {
                _navigationStack.RemoveAt(0);
            }
        }

        /// <summary>
        /// Internal implementation for push
        /// </summary>
        private async Task InternalPushAsync(ContentPage page, bool pushToRoot, object parameter = null, bool isAnimated = true)
        {
            _isInvalidationIgnored = true;

            // Set new page container modal page to empty
            if (_bottomContainer.ModalNavigationPage != null)
            {
                _bottomContainer.ModalNavigationPage = null;
            }

            // Get pages to remove and add
            PageInfo pageToAdd = new PageInfo(page);
            PageInfo pageToRemove = _navigationStack.LastOrDefault();

            // Add new page as navigation controller
            page.Navigation = this;

            bool isFirst = _navigationStack.Count == 0;

            // Add new page to navigation stack
            _navigationStack.Add(pageToAdd);

            if (_topContainer.Page != null)
            {
                // Swap top and bottom page container. Current page is located at the bottom parts container.
                Swap<Container>(ref _topContainer, ref _bottomContainer);
            }

            // Animation groups for in and out animations
            Animation animationGroup = new Animation();

            IAnimation inAnimation = null;
            IAnimation outAnimation = null;

            bool isDarkOverlayeEnabled = true;
            bool isPageToAddNavigationShadowVisible = false;
            bool isPageToRemoveNavigationShadowVisible = false;
            bool addToNavigationStackWithAnimation = false;

            if (isFirst || isAnimated == false)
            {
                if (_topNavigationBar != null)
                {
                    // Init navigation bar stack
                    _navigationBarStack = new ObservableCollection<ContentPage>();
                    _navigationBarStack.Add(page);
                    _topNavigationBar.NavigationBar?.Initialize(_navigationBarStack);

                    if (_topContainer.NavigationBar == _topNavigationBar.View)
                    {
                        _topNavigationBar.NavigationBar = null;
                    }

                    // Add top navigation bar to children if needed
                    if (NavigationBar.GetVisibility(page) != NavigationBarVisibility.Hidden && Children.Contains(_topNavigationBar.View) == false)
                    {
                        Children.Add(_topNavigationBar.View);
                    }
                    // Remove from children if needed
                    else if (NavigationBar.GetVisibility(page) == NavigationBarVisibility.Hidden && Children.Contains(_topNavigationBar.View) == true)
                    {
                        Children.Remove(_topNavigationBar.View);
                    }
                }

                // Init container properties which could change on previous animations
                InitializeContainerProperties(_topContainer);
            }
            else
            {
                _backPanAreaBoxView.InputTransparent = false;

                // If new page has navigation bar hidden and current visible (visible -> hidden)
                if (NavigationBar.GetVisibility(pageToAdd.Page) == NavigationBarVisibility.Hidden && NavigationBar.GetVisibility(pageToRemove.Page) != NavigationBarVisibility.Hidden)
                {
                    // Add current navigation bar to bottom page container
                    if (Children.Contains(_topNavigationBar.View))
                    {
                        Children.Remove(_topNavigationBar.View);
                    }
                    // If current page container has not top navigation bar as child
                    if (_bottomContainer.NavigationBar != _topNavigationBar.View)
                    {
                        _bottomContainer.NavigationBar = _topNavigationBar.View;
                    }

                    // Remove another navigation bar (bottom)
                    if (_bottomNavigationBar != null && Children.Contains(_bottomNavigationBar.View))
                    {
                        Children.Remove(_bottomNavigationBar.View);
                    }
                    if (_topContainer.NavigationBar != null)
                    {
                        _topContainer.NavigationBar = null;
                    }

                    // Current active navigation bar is on top, set it to bottom
                    Swap<NavigationBarInfo>(ref _topNavigationBar, ref _bottomNavigationBar);

                    // Create new navigation stack and add new page to top of it
                    if (pushToRoot)
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>();
                    }
                    else
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    }
                    _navigationBarStack.Add(page);
                }
                // If new page has navigation control visible and current hidden (hidden -> visible)
                else if (NavigationBar.GetVisibility(pageToAdd.Page) != NavigationBarVisibility.Hidden && NavigationBar.GetVisibility(pageToRemove.Page) == NavigationBarVisibility.Hidden)
                {
                    // Add new page navigation bar to top page container for animations

                    // Create top navigation bar if any navigation bar is created
                    if (_topNavigationBar == null && _bottomNavigationBar == null)
                    {
                        _topNavigationBar = CreateNavigationBar();
                    }
                    // If bottom navigation bar is created, then swap it to top
                    else if (_topNavigationBar == null && _bottomNavigationBar != null)
                    {
                        Swap<NavigationBarInfo>(ref _bottomNavigationBar, ref _topNavigationBar);
                    }

                    // Remove navigation bar from current page container if it contains it or from children (should be removed already!)
                    if (_bottomContainer.NavigationBar == _topNavigationBar.View)
                    {
                        _bottomContainer.NavigationBar = null;
                    }
                    else if (Children.Contains(_topNavigationBar.View))
                    {
                        Children.Remove(_topNavigationBar.View);
                    }

                    // Add current navigation control to new page container
                    if (_topContainer.NavigationBar != _topNavigationBar.View)
                    {
                        _topContainer.NavigationBar = _topNavigationBar.View;
                    }

                    // Create new navigation stack and add new page top of it
                    if (pushToRoot)
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>();
                    }
                    else
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    }
                    _navigationBarStack.Add(page);

                    // Initialize top navigation bar with new navigation stack without animation
                    _topNavigationBar.NavigationBar?.Initialize(_navigationBarStack);
                }
                // If both page has navigation bar hidden
                else if (NavigationBar.GetVisibility(pageToAdd.Page) == NavigationBarVisibility.Hidden && NavigationBar.GetVisibility(pageToRemove.Page) == NavigationBarVisibility.Hidden)
                {
                    _topContainer.NavigationBar = null;

                    // Keep navigation stack updated
                    if (pushToRoot)
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>();
                    }
                    else
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    }
                    _navigationBarStack.Add(page);
                }
                // If current page has modal pages, then add current navigation bar to page container which is removed. Because 
                // page has navigation bar visible, then create it if needed and add to page container which contains new page.
                else if (pageToRemove.ModalPages.Count > 0)
                {
                    // Swap active navigation control to bottom
                    if (_topNavigationBar != null)
                    {
                        Swap<NavigationBarInfo>(ref _topNavigationBar, ref _bottomNavigationBar);
                    }

                    // Create new top navigation bar if not created or remove from children
                    if (_topNavigationBar == null)
                    {
                        _topNavigationBar = CreateNavigationBar();
                    }
                    else if (Children.Contains(_topNavigationBar.View))
                    {
                        Children.Add(_topNavigationBar.View);
                    }

                    // Create new stack for new page navigation bar
                    if (pushToRoot)
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>();
                    }
                    else
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    }
                    _navigationBarStack.Add(page);

                    // Init top navigation bar without animation
                    _topNavigationBar.NavigationBar?.Initialize(_navigationBarStack);

                    // Add top navigation bar to new page container
                    if (_topContainer.NavigationBar != _topNavigationBar.View)
                    {
                        _topContainer.NavigationBar = _topNavigationBar.View;
                    }
                }
                // If navigationbar is scrolled out
                else if (_topNavigationBar != null && _topNavigationBar.View.TranslationY < 0)
                {
                    if (_bottomNavigationBar == null)
                    {
                        _bottomNavigationBar = CreateNavigationBar();
                    }

                    if (_topNavigationBar != null)
                    {
                        if (Children.Contains(_topNavigationBar.View))
                        {
                            Children.Remove(_topNavigationBar.View);
                        }
                        if (_bottomContainer.NavigationBar != _topNavigationBar.View)
                        {
                            _bottomContainer.NavigationBar = _topNavigationBar.View;
                        }
                    }

                    Swap<NavigationBarInfo>(ref _topNavigationBar, ref _bottomNavigationBar);

                    // Create new stack for new page navigation bar
                    if (pushToRoot)
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>();
                    }
                    else
                    {
                        _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    }
                    _navigationBarStack.Add(page);

                    // Init top navigation bar without animation
                    _topNavigationBar.NavigationBar?.Initialize(_navigationBarStack);

                    // Add top navigation bar to new page container
                    if (_topContainer.NavigationBar != _topNavigationBar.View)
                    {
                        _topContainer.NavigationBar = _topNavigationBar.View;
                    }
                }
                // If current and new page has navigation bar visible
                else
                {
                    // Add top navigation bar to children if it is locaited in current part container (page containers are swapped)
                    if (_bottomContainer.NavigationBar != null)
                    {
                        _bottomContainer.NavigationBar = null;
                    }
                    if (_topNavigationBar != null && Children.Contains(_topNavigationBar.View) == false)
                    {
                        Children.Add(_topNavigationBar.View);
                    }

                    // Update navigation bar with animation
                    addToNavigationStackWithAnimation = true;
                }

                if (_topNavigationBar != null)
                {
                    _topNavigationBar.View.TranslationY = 0;
                    _navigationBarModalDarkness.TranslationY = 0;
                    if (Device.RuntimePlatform == Device.UWP)
                    {
                        _topNavigationBar.View.TranslationY = 0.001;
                        _navigationBarModalDarkness.TranslationY = 0.091;
                    }
                }

                // Add navigate in animation
                if (pageToAdd.Page.NavigationAnimationGroup != null && pageToAdd.Page.NavigationAnimationGroup.In != null)
                {
                    inAnimation = pageToAdd.Page.NavigationAnimationGroup.In;
                    isDarkOverlayeEnabled = pageToAdd.Page.NavigationAnimationGroup.IsDarkOverlayEnabled;
                    isPageToAddNavigationShadowVisible = pageToAdd.Page.NavigationAnimationGroup.IsShadowEnabled;
                }
                if (pageToRemove != null)
                {
                    isPageToRemoveNavigationShadowVisible = pageToRemove.Page.NavigationAnimationGroup.IsShadowEnabled;

                    if (pageToAdd.Page.NavigationAnimationGroup != null && pageToAdd.Page.NavigationAnimationGroup.PreviousPageOutOverride != null)
                    {
                        outAnimation = pageToAdd.Page.NavigationAnimationGroup.PreviousPageOutOverride;
                    }
                    else if (pageToRemove.Page.NavigationAnimationGroup != null && pageToRemove.Page.NavigationAnimationGroup.Out != null)
                    {
                        outAnimation = pageToRemove.Page.NavigationAnimationGroup.Out;
                    }
                }
            }

            // Initialize container properties for animation
            InitializeContainerProperties(_topContainer);

            // Update children Z-order
            UpdateChildrenOrder();

            // Set layout invalidation NOT ignored
            _isInvalidationIgnored = false;

            // If has animations (no animations if first page)
            if (inAnimation != null || outAnimation != null)
            {
                _navigationDarkOverlay.Opacity = 0;
                if (isDarkOverlayeEnabled)
                {
                    animationGroup.Add(0, 1, new Animation(d => _navigationDarkOverlay.Opacity = d, 0, ModalColorOpacity));
                }

                _topContainer.IsNavigationShadowVisible(isPageToAddNavigationShadowVisible);
                _bottomContainer.IsNavigationShadowVisible(isPageToRemoveNavigationShadowVisible);

                SetPageInputTransparent(true);

                uint outDuration = outAnimation.Duration;
                uint inDuration = inAnimation.Duration;

                uint actualDuration = Math.Max(inDuration, outDuration);
                double inEnd = 1;
                double outEnd = 1;

                if (inDuration > outDuration)
                {
                    outEnd = (double)outDuration / (double)inDuration;
                }
                else
                {
                    outEnd = (double)inDuration / (double)outDuration;
                }

                // Create and initialize animations
                if (inAnimation != null)
                {
                    animationGroup.Add(0, inEnd, inAnimation.Create(_topContainer));
                }
                if (outAnimation != null)
                {
                    animationGroup.Add(0, outEnd, outAnimation.Create(_bottomContainer));
                }

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                ExecuteWhenReady(page, () =>
                {
                    if (addToNavigationStackWithAnimation)
                    {
                        if (pushToRoot)
                        {
                            _topNavigationBar.NavigationBar.PushToRoot(page);
                        }
                        else
                        {
                            _topNavigationBar.NavigationBar.Push(page);
                        }
                    }

                    animationGroup.Commit(
                        this,
                        _pushAnimationName,
                        _animationRatio,
                        actualDuration,
                        Easing.Linear,
                        (d, a) =>
                        {
                            _topContainer.IsNavigationShadowVisible(false);
                            _bottomContainer.IsNavigationShadowVisible(false);
                            tcs.SetResult(a);
                        });
                });

                _topContainer.Page = page;

                HasPages = true;

                InvokeOnNavigationAwareElement(pageToRemove?.Page, p => p.OnDissapearing(new DissapearEventArgs(NavigationDirection.Out)));
                InvokeOnNavigationAwareElement(pageToAdd.Page, p => p.OnAppearing(new AppearEventArgs(NavigationDirection.In, parameter)));

                bool isAborted = await tcs.Task;

                if (isAborted == false)
                {
                    _bottomContainer.Page = null;
                    SetPageInputTransparent(false);
                    UpdateChildrenOrder();
                }
            }
            else
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                ExecuteWhenReady(page, () =>
                {
                    tcs.SetResult(true);
                });

                _topContainer.Page = page;

                HasPages = true;

                InvokeOnNavigationAwareElement(pageToRemove?.Page, p => p.OnDissapearing(new DissapearEventArgs(NavigationDirection.Out)));
                InvokeOnNavigationAwareElement(pageToAdd.Page, p => p.OnAppearing(new AppearEventArgs(NavigationDirection.In, parameter)));

                await tcs.Task;
            }

            LayoutBackPanBox();

            InvokeOnNavigationAwareElement(pageToRemove?.Page, p => p.OnDissapeared(new DissapearEventArgs(NavigationDirection.Out)));
            InvokeOnNavigationAwareElement(pageToAdd.Page, p => p.OnAppeared(new AppearEventArgs(NavigationDirection.In, parameter)));
        }

        #endregion

        #region Push modal

        /// <summary>
        /// Push new modal page
        /// </summary>
        /// <param name="page">New modal page</param>
        /// <param name="parameter">New modal page parameter</param>
        public Task<object> PushModalAsync(ContentPage page, object parameter = null)
        {
            this.AbortAnimation(_modalInAnimationName);
            this.AbortAnimation(_modalOutAnimationName);

            HasPages = true;
            bool hasModalNavigationPage = _topModalNavigationPage != null;

            // Initialize page navigation object
            page.Navigation = this;

            _isInvalidationIgnored = true;

            PageInfo currentPage = null;

            if (_navigationStack.Count > 0)
            {
                currentPage = _navigationStack.LastOrDefault();
            }
            else
            {
                // TODO: If first page is modal page
                PageInfo info = new PageInfo(null);
                _navigationStack.Add(info);
                currentPage = info;
            }

            // Use top modal navigation page
            if (_topModalNavigationPage == null && _bottomModalNavigationPage != null)
            {
                Swap<ModalNavigationPageInfo>(ref _topModalNavigationPage, ref _bottomModalNavigationPage);
            }
            else if (_topModalNavigationPage == null)
            {
                _topModalNavigationPage = CreateModalNavigationPage();
            }

            // Remove top modal navigation page from bottom container
            if (_bottomContainer.ModalNavigationPage == _topModalNavigationPage.NavigationPage)
            {
                _bottomContainer.ModalNavigationPage = null;
            }

            // Set active navigation bar to page container

            if (_topNavigationBar != null)
            {
                if (Children.Contains(_topNavigationBar.View))
                {
                    Children.Remove(_topNavigationBar.View);
                }
                if (_topContainer.Page != null && NavigationBar.GetVisibility(_topContainer.Page) != NavigationBarVisibility.Hidden && _topContainer.NavigationBar != _topNavigationBar.View)
                {
                    _topContainer.NavigationBar = _topNavigationBar.View;
                }
            }

            // Update current page modal navigation stack
            currentPage.ModalPages.Add(page);

            _isInvalidationIgnored = false;

            // If first modal page
            if (ModalNavigationStack.Count == 1)
            {
                InvokeOnNavigationAwareElement(page, p => p.OnAppearing(new AppearEventArgs(NavigationDirection.In, parameter)));

                // If aimation has animation
                if (page != null && page.NavigationAnimationGroup != page.NavigationAnimationGroup.ModalIn)
                {
                    // Init modal navigation page
                    _topModalNavigationPage.NavigationPage.Initialize(page);

                    // Add modal navigation page to container
                    _topContainer.ModalNavigationPage = _topModalNavigationPage.NavigationPage;
                    _topContainer.IsModalPageVisible(false);
                    SetPageInputTransparent(true);
                    ExecuteWhenReady(page, () =>
                    {
                        _topContainer.IsModalPageVisible(true);
                        Animation animation = _topContainer.CreateShowModalAnimation(page.NavigationAnimationGroup.ModalIn);

                        animation.Commit(this, _modalInAnimationName, _animationRatio, page.NavigationAnimationGroup.ModalIn.Duration, Easing.Linear, finished: (arg1, isAborted) =>
                        {
                            if (isAborted == false)
                            {
                                SetPageInputTransparent(false);
                                InvokeOnNavigationAwareElement(page, p => p.OnAppeared(new AppearEventArgs(NavigationDirection.In, parameter)));
                            }
                        });
                    });
                }
                else
                {
                    _topContainer.SetModalBackgroundVisibility(true);
                    _topModalNavigationPage.NavigationPage.Initialize(page);
                    _topContainer.ModalNavigationPage = _topModalNavigationPage.NavigationPage;
                }
            }
            else
            {
                _topModalNavigationPage.NavigationPage.PushAsync(page, parameter);
            }

            // Set this task ready when modal page is closed
            TaskCompletionSource<object> modalPageClosedTask = new TaskCompletionSource<object>();
            _modalTaskDictionary.Add(page, modalPageClosedTask);

            // Return task for awaiting
            return modalPageClosedTask.Task;
        }

        /// <summary>
        /// Initialize modal navigation layout
        /// </summary>
        private ModalNavigationPageInfo CreateModalNavigationPage()
        {
            NavigationPage modalNavigationPage = new NavigationPage();
            modalNavigationPage.Style = ModalNavigationPageStyle;
            modalNavigationPage.CloseButtonTapped += async (sender, e) =>
            {
                await PopModalAsync(null, true);
            };

            return new ModalNavigationPageInfo(modalNavigationPage);
        }

        #endregion

        #region Pop

        /// <summary>
        /// Pop last page
        /// </summary>
        public async Task PopAsync(object parameter = null)
        {
            // Do nothing if last page
            if (_navigationStack.Count < 2)
            {
                return;
            }

            if (AnimationExtensions.AnimationIsRunning(this, _pushAnimationName))
            {
                AnimationExtensions.AbortAnimation(this, _pushAnimationName);
            }
            if (AnimationExtensions.AnimationIsRunning(this, _popAnimationName))
            {
                AnimationExtensions.AbortAnimation(this, _popAnimationName);
            }

            _isInvalidationIgnored = true;

            // Current page which is going to be removed
            PageInfo pageToRemove = _navigationStack.Last();

            // Remove page from navigation stack
            _navigationStack.Remove(pageToRemove);

            // Previous page which is going to be added
            PageInfo pageToAdd = _navigationStack.Last();

            _backPanAreaBoxView.InputTransparent = _navigationStack.Count == 1;

            bool removeFromNavigationStackWithAnimation = false;

            // If new page has navigation control hidden and current visible (hidden <- visible)
            if (NavigationBar.GetVisibility(pageToAdd.Page) == NavigationBarVisibility.Hidden && NavigationBar.GetVisibility(pageToRemove.Page) != NavigationBarVisibility.Hidden)
            {
                // Remove top current navigation control from children and add to top page container
                if (Children.Contains(_topNavigationBar.View))
                {
                    Children.Remove(_topNavigationBar.View);
                }
                if (_topContainer.NavigationBar != _topNavigationBar.View)
                {
                    _topContainer.NavigationBar = _topNavigationBar.View;
                }

                // Remove bottom navigation control from everywhere
                if (_bottomNavigationBar != null && Children.Contains(_bottomNavigationBar.View))
                {
                    Children.Remove(_bottomNavigationBar.View);
                }
                if (_bottomContainer.NavigationBar != null)
                {
                    _bottomContainer.NavigationBar = null;
                }

                _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                _navigationBarStack.RemoveAt(_navigationBarStack.Count - 1);
            }
            // If new page has navigation control visible and current hidden (visible <- hidden)
            else if (NavigationBar.GetVisibility(pageToAdd.Page) != NavigationBarVisibility.Hidden && NavigationBar.GetVisibility(pageToRemove.Page) == NavigationBarVisibility.Hidden)
            {
                // If bottom navigation bar is created but top is not
                if (_topNavigationBar == null && _bottomNavigationBar != null)
                {
                    // Swap bottom to top
                    Swap<NavigationBarInfo>(ref _topNavigationBar, ref _bottomNavigationBar);
                }
                // If top and bottom is not created
                else if (_topNavigationBar == null && _bottomNavigationBar == null)
                {
                    // Create top for previous page
                    _topNavigationBar = CreateNavigationBar();
                }
                // If has top navigation bar
                else if (_topNavigationBar != null && Children.Contains(_topNavigationBar.View))
                {
                    // Remove from children and add to container
                    Children.Remove(_topNavigationBar.View);
                }

                // Container top/bottom is swappd after animation when correct container contais correct top/bottom navigation control
                if (_bottomContainer.NavigationBar != _topNavigationBar.View)
                {
                    _bottomContainer.NavigationBar = _topNavigationBar.View;
                }

                _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                _navigationBarStack.RemoveAt(_navigationBarStack.Count - 1);
                _topNavigationBar.NavigationBar?.Initialize(_navigationBarStack);
            }
            // If both page has navigation bar hidden
            else if (NavigationBar.GetVisibility(pageToAdd.Page) == NavigationBarVisibility.Hidden && NavigationBar.GetVisibility(pageToRemove.Page) == NavigationBarVisibility.Hidden)
            {
                _bottomContainer.NavigationBar = null;
                _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                _navigationBarStack.RemoveAt(_navigationBarStack.Count - 1);
            }
            else if (pageToAdd.ModalPages.Count > 0)
            {
                if (_bottomNavigationBar == null)
                {
                    _bottomNavigationBar = CreateNavigationBar();
                }
                else if (Children.Contains(_bottomNavigationBar.View))
                {
                    Children.Remove(_bottomNavigationBar.View);
                }

                _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                _navigationBarStack.RemoveAt(_navigationBarStack.Count - 1);

                _bottomNavigationBar.NavigationBar?.Initialize(_navigationBarStack);

                if (_bottomContainer.NavigationBar != _bottomNavigationBar.View)
                {
                    _bottomContainer.NavigationBar = _bottomNavigationBar.View;
                }

                Swap<NavigationBarInfo>(ref _topNavigationBar, ref _bottomNavigationBar);
            }
            else if (_topNavigationBar != null && _topNavigationBar.View.TranslationY < 0)
            {
                if (_bottomNavigationBar == null)
                {
                    _bottomNavigationBar = CreateNavigationBar();
                }

                if (_topNavigationBar.View != null && _topContainer.NavigationBar != _topNavigationBar.View)
                {
                    Children.Remove(_topNavigationBar.View);
                    _topContainer.NavigationBar = _topNavigationBar.View;
                }

                Swap<NavigationBarInfo>(ref _topNavigationBar, ref _bottomNavigationBar);

                _topNavigationBar.View.TranslationY = 0;
                _navigationBarModalDarkness.TranslationY = 0;
                if (Device.RuntimePlatform == Device.UWP)
                {
                    _topNavigationBar.View.TranslationY = 0.01;
                    _navigationBarModalDarkness.TranslationY = 0.01;
                }

                // Create new stack for new page navigation bar
                _navigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                _navigationBarStack.RemoveAt(_navigationBarStack.Count - 1);

                // Init top navigation bar
                _topNavigationBar.NavigationBar?.Initialize(_navigationBarStack);

                // Add top navigation bar to new page container
                if (_bottomContainer.NavigationBar != _topNavigationBar.View)
                {
                    Children.Remove(_topNavigationBar.View);
                    _bottomContainer.NavigationBar = _topNavigationBar.View;
                }
            }
            // If current and new page has navigation control visible
            else
            {
                // If navigation bar is still located in current page container...
                if (_bottomContainer.NavigationBar != null)
                {
                    _bottomContainer.NavigationBar = null;
                }
                if (_topContainer.NavigationBar != null)
                {
                    _topContainer.NavigationBar = null;
                }

                // ... Add to children
                if (_topNavigationBar != null && Children.Contains(_topNavigationBar.View) == false)
                {
                    Children.Add(_topNavigationBar.View);
                }

                removeFromNavigationStackWithAnimation = true;
            }

            if (_topNavigationBar != null)
            {
                if (Device.RuntimePlatform == Device.UWP)
                {
                    _topNavigationBar.View.TranslationY = 0.001;
                    _navigationBarModalDarkness.TranslationY = 0.001;
                }
                else
                {
                    _topNavigationBar.View.TranslationY = 0;
                    _navigationBarModalDarkness.TranslationY = 0;
                }
            }

            // Initialize actual modal page if bottom page
            if (pageToAdd.ModalPages.Count > 0)
            {
                // If no modal pages created
                if (_bottomModalNavigationPage == null && _topModalNavigationPage == null)
                {
                    _bottomModalNavigationPage = CreateModalNavigationPage();
                }
                // If only top modal page is created but it is currently used in page which is navigated back out
                else if (_bottomModalNavigationPage == null && _topModalNavigationPage != null && pageToRemove.ModalPages.Count > 0)
                {
                    _bottomModalNavigationPage = CreateModalNavigationPage();
                }
                // If top modal page is created but bottom not (and current page has no modal pages)
                else if (_bottomModalNavigationPage == null && _topModalNavigationPage != null && pageToRemove.ModalPages.Count == 0)
                {
                    Swap<ModalNavigationPageInfo>(ref _bottomModalNavigationPage, ref _topModalNavigationPage);
                }

                // Remove bottom modal navigation page from top (if it contains it) and add to bottom page container
                if (_topContainer.ModalNavigationPage == _bottomModalNavigationPage.NavigationPage)
                {
                    _topContainer.ModalNavigationPage = null;
                }
                if (_bottomContainer.ModalNavigationPage != _bottomModalNavigationPage.NavigationPage)
                {
                    _bottomContainer.ModalNavigationPage = _bottomModalNavigationPage.NavigationPage;
                }

                // Initialize bottom modal page and set it to visible immediatley
                _bottomModalNavigationPage.NavigationPage.Initialize(pageToAdd.ModalPages);
                _bottomModalNavigationPage.NavigationPage.IsVisible = true;

                // Swap bottom page back to top because it will be it after aniamtion
                Swap<ModalNavigationPageInfo>(ref _bottomModalNavigationPage, ref _topModalNavigationPage);
            }

            IAnimation inAnimation = null;
            IAnimation outAnimation = null;
            bool isDarkOverlayEnabled = true;
            bool isPageToAddNavigationShadowVisible = false;
            bool isPageToRemoveNavigationShadowVisible = false;

            // NavigationBar
            if (pageToAdd.Page.NavigationAnimationGroup != null)
            {
                if (pageToRemove != null && pageToRemove.Page.NavigationAnimationGroup != null && pageToRemove.Page.NavigationAnimationGroup.PreviousPageBackInOverride != null)
                {
                    inAnimation = pageToRemove.Page.NavigationAnimationGroup.PreviousPageBackInOverride;
                }
                else if (pageToAdd.Page.NavigationAnimationGroup.BackIn != null)
                {
                    inAnimation = pageToAdd.Page.NavigationAnimationGroup.BackIn;
                }

                isDarkOverlayEnabled = pageToAdd.Page.NavigationAnimationGroup.IsDarkOverlayEnabled;
                isPageToAddNavigationShadowVisible = pageToAdd.Page.NavigationAnimationGroup.IsShadowEnabled;
            }
            if (pageToRemove != null && pageToRemove.Page.NavigationAnimationGroup != null && pageToRemove.Page.NavigationAnimationGroup.Out != null)
            {
                isPageToRemoveNavigationShadowVisible = pageToRemove.Page.NavigationAnimationGroup.IsShadowEnabled;
                outAnimation = pageToRemove.Page.NavigationAnimationGroup.BackOut;
            }

            InitializeContainerProperties(_bottomContainer);

            _isInvalidationIgnored = false;

            InvokeOnNavigationAwareElement(pageToRemove.Page, p => p.OnDissapearing(new DissapearEventArgs(NavigationDirection.BackOut)));
            InvokeOnNavigationAwareElement(pageToAdd.Page, p => p.OnAppearing(new AppearEventArgs(NavigationDirection.BackIn, parameter)));

            // If has animations
            if (inAnimation != null || outAnimation != null)
            {
                Animation animationGroup = new Animation();

                if (isDarkOverlayEnabled)
                {
                    _navigationDarkOverlay.Opacity = ModalColorOpacity;
                    animationGroup.Add(0, 1, new Animation(d => _navigationDarkOverlay.Opacity = d, ModalColorOpacity, 0));
                }
                else
                {
                    _navigationDarkOverlay.Opacity = 0;
                }

                _topContainer.IsNavigationShadowVisible(isPageToRemoveNavigationShadowVisible);
                _bottomContainer.IsNavigationShadowVisible(isPageToAddNavigationShadowVisible);
                SetPageInputTransparent(true);

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                uint outDuration = outAnimation.Duration;
                uint inDuration = inAnimation.Duration;

                uint actualDuration = Math.Max(inDuration, outDuration);
                double backInEnd = 1;
                double backOutEnd = 1;

                if (inDuration > outDuration)
                {
                    backOutEnd = (double)outDuration / (double)inDuration;
                }
                else
                {
                    backOutEnd = (double)inDuration / (double)outDuration;
                }

                if (inAnimation != null)
                {
                    animationGroup.Add(0, backInEnd, inAnimation.Create(_bottomContainer));
                }
                if (outAnimation != null)
                {
                    animationGroup.Add(0, backOutEnd, outAnimation.Create(_topContainer));
                }

                // Add previous page to bottom page container
                _bottomContainer.Page = pageToAdd.Page;

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (removeFromNavigationStackWithAnimation && _navigationBarStack.Count > 0)
                    {
                        _topNavigationBar.NavigationBar.Pop();
                    }

                    animationGroup.Commit(
                        this,
                        _popAnimationName,
                        _animationRatio,
                        actualDuration,
                        Easing.Linear,
                        (d, b) =>
                        {
                            tcs.SetResult(true);
                            _topContainer.IsNavigationShadowVisible(false);
                            _bottomContainer.IsNavigationShadowVisible(false);
                        });
                });

                await tcs.Task;

                _topContainer.Page = null;
                Swap<Container>(ref _topContainer, ref _bottomContainer);
                UpdateChildrenOrder();
                SetPageInputTransparent(false);
                LayoutBackPanBox();
            }
            else
            {
                // Add previous page to bottom page container
                _bottomContainer.Page = pageToAdd.Page;
                LayoutBackPanBox();
            }

            InvokeOnNavigationAwareElement(pageToRemove.Page, p => p.OnDissapeared(new DissapearEventArgs(NavigationDirection.BackOut)));
            InvokeOnNavigationAwareElement(pageToAdd.Page, p => p.OnAppeared(new AppearEventArgs(NavigationDirection.BackIn, parameter)));

            HasPages = ModalNavigationStack.Count > 0 || NavigationStack.Count > 0;
        }

        #endregion

        #region Pop modal

        /// <summary>
        /// Pop last modal page
        /// </summary>
        /// <param name="parameter">Parameter to previous page</param>
        /// <param name="popAll">True if pop all modal pages</param>
        public async Task PopModalAsync(object parameter = null, bool popAll = false)
        {
            if (ModalNavigationStack.Count == 0)
            {
                return;
            }

            PageInfo currentPage = _navigationStack.Last();
            ContentPage modalPageToRemove = _topModalNavigationPage.NavigationPage.NavigationStack.FirstOrDefault();

            if (currentPage.ModalPages.Count == 1 || popAll)
            {
                this.AbortAnimation(_modalInAnimationName);
                this.AbortAnimation(_modalOutAnimationName);

                InvokeOnNavigationAwareElement(modalPageToRemove, p => p.OnDissapearing(new DissapearEventArgs(NavigationDirection.BackOut)));

                // If has animation
                if (modalPageToRemove.NavigationAnimationGroup != modalPageToRemove.NavigationAnimationGroup.ModalOut)
                {
                    uint animationDuration = modalPageToRemove.NavigationAnimationGroup.ModalOut.Duration;

                    Animation animation = _topContainer.CreateHideModalAnimation(modalPageToRemove.NavigationAnimationGroup.ModalOut);

                    SetPageInputTransparent(true);
                    TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                    animation.Commit(this, _modalOutAnimationName, _animationRatio, animationDuration, Easing.Linear, finished: (arg1, isAborted) =>
                    {
                        if (isAborted == false)
                        {
                            SetPageInputTransparent(false);
                        }

                        tcs.SetResult(true);
                    });

                    await tcs.Task;
                }
                else
                {
                    _topContainer.SetModalBackgroundVisibility(false);
                }

                currentPage.ModalPages.Clear();
                _topContainer.ModalNavigationPage = null;
                _topModalNavigationPage.NavigationPage.Clear();

                if (currentPage.Page == null)
                {
                    _navigationStack.Remove(currentPage);
                }

                InvokeOnNavigationAwareElement(modalPageToRemove, p => p.OnDissapeared(new DissapearEventArgs(NavigationDirection.BackOut)));
            }
            else
            {
                currentPage.ModalPages.RemoveAt(currentPage.ModalPages.Count - 1);
                ContentPage page = currentPage.ModalPages.Last();
                await _topModalNavigationPage.NavigationPage.PopAsync();
            }

            HasPages = ModalNavigationStack.Count > 0 || NavigationStack.Count > 0;

            TaskCompletionSource<object> modalPageClosedTask = _modalTaskDictionary[modalPageToRemove];
            _modalTaskDictionary.Remove(modalPageToRemove);
            modalPageClosedTask.SetResult(parameter);
        }

        #endregion

        #region Clean

        /// <summary>
        /// Clear all content
        /// </summary>
        public void Clear()
        {
            foreach (TaskCompletionSource<object> tcs in _modalTaskDictionary.Values)
            {
                tcs.SetResult(null);
            }
            _modalTaskDictionary.Clear();

            ContentPage modalPageToRemove = null;
            ContentPage pageToRemove = NavigationStack.LastOrDefault();

            if (_topModalNavigationPage != null)
            {
                modalPageToRemove = _topModalNavigationPage.NavigationPage.NavigationStack.FirstOrDefault();
            }

            InvokeOnNavigationAwareElement(modalPageToRemove, p => p.OnDissapearing(new DissapearEventArgs(NavigationDirection.Out)));
            InvokeOnNavigationAwareElement(pageToRemove, p => p.OnDissapearing(new DissapearEventArgs(NavigationDirection.Out)));

            _isInvalidationIgnored = true;

            _backPanAreaBoxView.InputTransparent = true;

            _topContainer.NavigationBarSize = new SizeRequest();
            _bottomContainer.NavigationBarSize = new SizeRequest();

            _topContainer.NavigationBar = null;
            _topContainer.Page = null;

            _bottomContainer.NavigationBar = null;
            _bottomContainer.Page = null;

            if (_topNavigationBar != null && Children.Contains(_topNavigationBar.View))
            {
                Children.Remove(_topNavigationBar.View);
            }
            if (_bottomNavigationBar != null && Children.Contains(_bottomNavigationBar.View))
            {
                Children.Remove(_bottomNavigationBar.View);
            }

            if (_topNavigationBar != null)
            {
                _topNavigationBar.NavigationBar?.Clear();
            }
            if (_bottomNavigationBar != null)
            {
                _bottomNavigationBar.NavigationBar.Clear();
            }
            if (_topModalNavigationPage != null)
            {
                _topModalNavigationPage.NavigationPage.Clear();
            }
            if (_bottomModalNavigationPage != null)
            {
                _bottomModalNavigationPage.NavigationPage.Clear();
            }

            _navigationStack.Clear();

            InvokeOnNavigationAwareElement(modalPageToRemove, p => p.OnDissapeared(new DissapearEventArgs(NavigationDirection.Out)));
            InvokeOnNavigationAwareElement(pageToRemove, p => p.OnDissapeared(new DissapearEventArgs(NavigationDirection.Out)));

            HasPages = false;

            _isInvalidationIgnored = false;

            InvalidateMeasure();
            InvalidateLayout();
        }

        #endregion

        #region Pan

        /// <summary>
        /// Back pan gesture handler
        /// </summary>
        public void OnBackPan(object sender, PanUpdatedEventArgs e)
        {
            if (_navigationStack.Count <= 1 || ModalNavigationStack.Count > 0)
            {
                return;
            }

            // Get previous page and add it to view         
            PageInfo newPreviousPage = _navigationStack.Count > 2 ? _navigationStack[_navigationStack.Count - 3] : null;
            PageInfo previousPage = _navigationStack.ElementAt(_navigationStack.Count - 2);
            PageInfo currentPage = _navigationStack.Last();

            if (e.StatusType == GestureStatus.Started)
            {
                IsPanning = true;
                _tmpNavigationBarStack = null;

                this.AbortAnimation(_panToCurrentPageAnimationName);

                InvokeOnNavigationAwareElement(currentPage.Page, p => p.OnDissapearing(new DissapearEventArgs(NavigationDirection.BackOut)));
                InvokeOnNavigationAwareElement(previousPage.Page, p => p.OnAppearing(new AppearEventArgs(NavigationDirection.BackIn, null)));

                // Add navigation control to correct page container which is panned

                // If previous page navigation control is hidden and current visible (hidden <- visible)
                if (NavigationBar.GetVisibility(currentPage.Page) != NavigationBarVisibility.Hidden && NavigationBar.GetVisibility(previousPage.Page) == NavigationBarVisibility.Hidden)
                {
                    // Remove top current navigation control from children and add to top page container
                    if (Children.Contains(_topNavigationBar.View))
                    {
                        Children.Remove(_topNavigationBar.View);
                    }
                    if (_topContainer.NavigationBar != _topNavigationBar.View)
                    {
                        _topContainer.NavigationBar = _topNavigationBar.View;
                    }

                    // Remove bottom navigation control from everywhere
                    if (_bottomNavigationBar != null && Children.Contains(_bottomNavigationBar.View))
                    {
                        Children.Remove(_bottomNavigationBar.View);
                    }
                    if (_bottomContainer.NavigationBar != null)
                    {
                        _bottomContainer.NavigationBar = null;
                    }

                    _bottomContainer.NavigationBarSize = new SizeRequest();

                    _tmpNavigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    _tmpNavigationBarStack.RemoveAt(_navigationBarStack.Count - 1);
                }
                // If previous page navigation control is visible and current hidden (visible <- hidden)
                else if (NavigationBar.GetVisibility(currentPage.Page) == NavigationBarVisibility.Hidden && NavigationBar.GetVisibility(previousPage.Page) != NavigationBarVisibility.Hidden)
                {
                    // Swap top navigation control to bottom
                    if (_bottomNavigationBar == null && _topNavigationBar != null)
                    {
                        Swap<NavigationBarInfo>(ref _topNavigationBar, ref _bottomNavigationBar);
                    }
                    else if (_bottomNavigationBar == null && _topNavigationBar == null)
                    {
                        _bottomNavigationBar = CreateNavigationBar();
                    }
                    else if (_bottomNavigationBar != null && Children.Contains(_bottomNavigationBar.View))
                    {
                        Children.Remove(_bottomNavigationBar.View);
                    }

                    // Container top/bottom is swappd after animation when correct container contais correct top/bottom navigation control
                    if (_bottomContainer.NavigationBar != _bottomNavigationBar.View)
                    {
                        _bottomContainer.NavigationBar = _bottomNavigationBar.View;
                    }

                    _tmpNavigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    _tmpNavigationBarStack.RemoveAt(_tmpNavigationBarStack.Count - 1);
                    _bottomNavigationBar.NavigationBar?.Initialize(_tmpNavigationBarStack);
                }
                // If both page has navigation bar hidden
                else if (NavigationBar.GetVisibility(currentPage.Page) == NavigationBarVisibility.Hidden && NavigationBar.GetVisibility(previousPage.Page) == NavigationBarVisibility.Hidden)
                {
                    _tmpNavigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    _tmpNavigationBarStack.RemoveAt(_tmpNavigationBarStack.Count - 1);
                    _bottomContainer.NavigationBarSize = new SizeRequest();
                    _bottomContainer.NavigationBar = null;
                }
                // If previous page has modal pages
                else if (previousPage.ModalPages.Count > 0)
                {
                    if (_bottomNavigationBar == null)
                    {
                        _bottomNavigationBar = CreateNavigationBar();
                    }
                    else if (Children.Contains(_bottomNavigationBar.View))
                    {
                        Children.Remove(_bottomNavigationBar.View);
                    }

                    _tmpNavigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    _tmpNavigationBarStack.RemoveAt(_tmpNavigationBarStack.Count - 1);

                    _bottomNavigationBar.NavigationBar?.Initialize(_tmpNavigationBarStack);

                    if (_bottomContainer.NavigationBar != _bottomNavigationBar.View)
                    {
                        _bottomContainer.NavigationBar = _bottomNavigationBar.View;
                    }
                }
                // If current page has navigation bar scrolled out
                else if (_topNavigationBar != null && _topNavigationBar.View.TranslationY < 0)
                {
                    // Create bottom navigation bar if not created
                    if (_bottomNavigationBar == null)
                    {
                        _bottomNavigationBar = CreateNavigationBar();
                    }

                    // Set previous page (top bar) to default location
                    _bottomNavigationBar.View.TranslationY = 0;
                    _navigationBarModalDarkness.TranslationY = 0;
                    if (Device.RuntimePlatform == Device.UWP)
                    {
                        _bottomNavigationBar.View.TranslationY = 0.001;
                        _navigationBarModalDarkness.TranslationY = 0.001;
                    }

                    // Create new stack for new page navigation bar
                    _tmpNavigationBarStack = new ObservableCollection<ContentPage>(_navigationBarStack);
                    _tmpNavigationBarStack.RemoveAt(_navigationBarStack.Count - 1);

                    // Init top navigation bar
                    _bottomNavigationBar.NavigationBar?.Initialize(_tmpNavigationBarStack);

                    // Add new top navigation bar to new page container
                    if (_bottomContainer.NavigationBar != _bottomNavigationBar.View)
                    {
                        _bottomContainer.NavigationBar = _bottomNavigationBar.View;
                    }

                    if (_topNavigationBar != null && _topNavigationBar.View != null && _topContainer.NavigationBar != _topNavigationBar.View)
                    {
                        _topContainer.NavigationBar = _topNavigationBar.View;
                    }
                }
                else
                {
                    // If navigation bar is still located in current page container...
                    if (_bottomContainer.NavigationBar != null)
                    {
                        _bottomContainer.NavigationBar = null;
                    }
                    if (_topContainer.NavigationBar != null)
                    {
                        _topContainer.NavigationBar = null;
                    }

                    // ... Add to children
                    if (_topNavigationBar != null && Children.Contains(_topNavigationBar.View) == false)
                    {
                        Children.Add(_topNavigationBar.View);
                    }
                }

                // Initialize previous page
                _bottomContainer.Page = previousPage.Page;
                _bottomContainer.TranslationX = -Width / 2;
                _bottomContainer.TranslationY = 0;
                _bottomContainer.Opacity = 1;
                _bottomContainer.Scale = 1;

                if (Device.RuntimePlatform == Device.UWP)
                {
                    _bottomContainer.TranslationY = 0.001;
                }

                _topContainer.IsNavigationShadowVisible(true);
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                // Prevent back pan box to go outside of window from left
                if (Device.RuntimePlatform != Device.Android)
                {
                    _backPanAreaBoxView.TranslationX = Math.Min(Width - _backPanAreaBoxView.Width, Math.Max(0, e.TotalX));
                }

                // Pan current page
                _topContainer.TranslationX = Math.Min(Width, Math.Max(0, e.TotalX));

                // Pan previous page
                _bottomContainer.TranslationX = Math.Min(0, -(Width / 2) + (e.TotalX / 2));

                // Update modal darkness by pan percent
                _navigationDarkOverlay.Opacity = ModalColorOpacity * (1 - (e.TotalX / this.Width));

                _backPanTotalX = Math.Max(0, e.TotalX);

                if (_topNavigationBar != null && Children.Contains(_topNavigationBar.View) && _topNavigationBar.View.TranslationY >= 0)
                {
                    _topNavigationBar.NavigationBar?.BackPan(_backPanTotalX);
                }
            }
            else if (e.StatusType == GestureStatus.Completed || e.StatusType == GestureStatus.Canceled)
            {
                _backPanAreaBoxView.TranslationX = 0;
                if (Device.RuntimePlatform == Device.UWP)
                {
                    _backPanAreaBoxView.TranslationX = 0.001;
                }

                Animation animationGroup = new Animation();

                // If pan ended and navigated to previous page
                if (_backPanTotalX > Width / 2)
                {
                    // Animate current page
                    animationGroup.Add(0, 1, new Animation(d =>
                    {
                        _topContainer.TranslationX = d;

                        // Update navigation darkness background
                        _navigationDarkOverlay.Opacity = ModalColorOpacity * (1 - (d / this.Width));

                    }, _topContainer.TranslationX, Width));

                    // Animate previous page
                    animationGroup.Add(0, 1, new Animation(d =>
                    {
                        _bottomContainer.TranslationX = d;

                    }, _bottomContainer.TranslationX, 0));

                    // Calculate animation duration 
                    uint duration = (uint)(400 * ((this.Width - _backPanTotalX) / this.Width));

                    SetPageInputTransparent(true);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (_topNavigationBar != null && Children.Contains(_topNavigationBar.View))
                        {
                            _topNavigationBar.NavigationBar.Pop();
                        }

                        // Run animation
                        animationGroup.Commit(
                            this,
                            _panToCurrentPageAnimationName,
                            _animationRatio,
                            duration,
                            Easing.Linear,
                            (arg1, arg2) =>
                            {
                                // If any navigation bar is in container
                                if (_tmpNavigationBarStack != null)
                                {
                                    _navigationBarStack = _tmpNavigationBarStack;
                                    Swap<NavigationBarInfo>(ref _topNavigationBar, ref _bottomNavigationBar);
                                    _tmpNavigationBarStack = null;
                                }

                                _topContainer.Page = null;

                                Swap<Container>(ref _topContainer, ref _bottomContainer);

                                UpdateChildrenOrder();

                                SetPageInputTransparent(false);

                                LayoutBackPanBox();

                                _bottomContainer.TranslationX = 0;
                                _topContainer.TranslationX = 0;

                                if (Device.RuntimePlatform == Device.UWP)
                                {
                                    _bottomContainer.TranslationX = 0.001;
                                    _topContainer.TranslationX = 0.001;
                                }

                                InvokeOnNavigationAwareElement(currentPage.Page, p => p.OnDissapeared(new DissapearEventArgs(NavigationDirection.BackOut)));
                                InvokeOnNavigationAwareElement(previousPage.Page, p => p.OnAppeared(new AppearEventArgs(NavigationDirection.BackIn, null)));
                            });
                    });

                    // Remove page from navigation stack
                    _navigationStack.Remove(currentPage);
                }
                else
                {
                    // Animate current page
                    animationGroup.Add(0, 1, new Animation(d =>
                    {
                        _topContainer.TranslationX = d;

                        if (_topNavigationBar != null && Children.Contains(_topNavigationBar.View))
                        {
                            _topNavigationBar.NavigationBar?.BackPan(d);
                        }

                        _navigationDarkOverlay.Opacity = ModalColorOpacity * (1 - (d / this.Width));

                    }, _topContainer.TranslationX, 0, Easing.Linear));

                    // Animate previous page
                    animationGroup.Add(0, 1, new Animation(d =>
                    {
                        _bottomContainer.TranslationX = d;

                    }, _bottomContainer.TranslationX, -(Width / 2), Easing.Linear));

                    uint duration = (uint)(400 * (_backPanTotalX / this.Width));

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        // Run animations
                        animationGroup.Commit(this, _panToCurrentPageAnimationName, length: duration, easing: Easing.Linear, finished: (args1, argd2) =>
                        {
                            _bottomContainer.TranslationX = 0;
                            _topContainer.TranslationX = 0;
                            _navigationDarkOverlay.Opacity = 0;

                            if (Device.RuntimePlatform == Device.UWP)
                            {
                                _bottomContainer.TranslationX = 0;
                                _topContainer.TranslationX = 0;
                            }
 
                            InvokeOnNavigationAwareElement(currentPage.Page, p => p.OnAppeared(new AppearEventArgs(NavigationDirection.In, null)));
                            InvokeOnNavigationAwareElement(previousPage.Page, p => p.OnDissapeared(new DissapearEventArgs(NavigationDirection.Out)));
                        });
                    });
                }

                IsPanning = false;
                _topContainer.IsNavigationShadowVisible(false);
            }
        }

        #endregion

        #region NavigationBar

        /// <summary>
        /// Create navigation control from template
        /// </summary>
        private NavigationBarInfo CreateNavigationBar()
        {
            if (NavigationBarTemplate == null)
            {
                return null;
            }
            else
            {
                View view = NavigationBarTemplate.CreateContent() as View;

                NavigationBarInfo info = new NavigationBarInfo(view);

                info.NavigationBar = info.View.FindByName(_navigationBarName) as INavigationBar;

                if (info.NavigationBar != null)
                {
                    info.NavigationBar.BackCommand = new Command(async (arg) =>
                    {
                        if (BackButtonTapped != null)
                        {
                            BackButtonTapped(this, new EventArgs());
                        }

                        await PopAsync();
                    });

                    info.NavigationBar.MenuCommand = new Command((arg) =>
                    {
                        if (MenuButtonTapped != null)
                        {
                            MenuButtonTapped(this, new EventArgs());
                        }
                    });

                    info.NavigationBar.CloseCommand = new Command(async (arg) =>
                    {
                        if (CloseButtonTapped != null)
                        {
                            CloseButtonTapped(this, new EventArgs());
                        }
                        else // Is good desing?
                        {
                            if (BackButtonTapped != null)
                            {
                                BackButtonTapped(this, new EventArgs());
                            }

                            await PopAsync();
                        }
                    });
                }
                else
                {
                    throw new Exception("NavigationBar element not found from NavigationBarTemplate! Give navigation bar element name 'NavigationBar'");
                }

                return info;
            }
        }

        /// <summary>
        /// NavigationBar modal color tapped
        /// </summary>
        private void OnNavigationBarModalDarknessLayerTapped(object sender, EventArgs e)
        {
            NavigationBarModalDarknessLayerTapped?.Invoke(sender, e);
        }

        /// <summary>
        /// Set NavigationBar modal color
        /// </summary>
        internal void SetNavigationBarModalDarkness(double darknessOpacity, ModalColors color)
        {
            if (_topContainer.NavigationBarSize.Request.Height > 0)
            {
                _navigationBarModalDarkness.TranslationY = _topNavigationBar.View.TranslationY;
                _navigationBarModalDarkness.Color = color == ModalColors.Black ? Color.Black : Color.White;
                _navigationBarModalDarkness.Opacity = darknessOpacity;
                _navigationBarModalDarkness.InputTransparent = darknessOpacity <= 0;
            }
        }

        /// <summary>
        /// Get top navigation bar
        /// </summary>
        internal View GetNavigationBar()
        {
            return _topNavigationBar.View;
        }

        /// <summary>
        /// Get navigation bar height
        /// </summary>
        internal double GetNavigationBarHeight()
        {
            if (_topNavigationBar != null && _topContainer.NavigationBar == _topNavigationBar.View)
            {
                return _topContainer.NavigationBarSize.Request.Height;
            }
            else if (_bottomNavigationBar != null && _bottomContainer.NavigationBar == _bottomNavigationBar.View)
            {
                return _bottomContainer.NavigationBarSize.Request.Height;
            }
            else
            {
                return _topContainer.NavigationBarSize.Request.Height;
            }
        }

        #endregion

        #region Common

        private void SetPageInputTransparent(bool isInputTransparent)
        {
            _topContainer.InputTransparent = isInputTransparent;
            _bottomContainer.InputTransparent = isInputTransparent;

            if (_topNavigationBar != null)
            {
                _topNavigationBar.View.InputTransparent = isInputTransparent;
            }
            if (_bottomNavigationBar != null)
            {
                _bottomNavigationBar.View.InputTransparent = isInputTransparent;
            }
        }

        /// <summary>
        /// Swap two object pointers
        /// </summary>
        private void Swap<T>(ref T a, ref T b)
        {
            T c = a;
            a = b;
            b = c;
        }

        /// <summary>
        /// Invoke INavigationAware interface elements
        /// </summary>
        public static void InvokeOnNavigationAwareElement(object item, Action<INavigationAware> invocation)
        {
            var navigationAwareItem = item as INavigationAware;
            if (navigationAwareItem != null)
            {
                invocation(navigationAwareItem);
            }

            var bindableObject = item as BindableObject;
            if (bindableObject != null)
            {
                var navigationAwareDataContext = bindableObject.BindingContext as INavigationAware;
                if (navigationAwareDataContext != null)
                {
                    invocation(navigationAwareDataContext);
                }
            }
        }

        private void InitializeContainerProperties(View container)
        {
            container.TranslationX = 0;
            container.TranslationY = 0;
            container.Scale = 1;
            container.ScaleX = 1;
            container.ScaleY = 1;
            container.Opacity = 1;
            container.WidthRequest = -1;
            container.HeightRequest = -1;

            if (Device.RuntimePlatform == Device.UWP)
            {
                container.TranslationX = 0;
                container.TranslationY = 0;
            }
        }

        private void ExecuteWhenReady(ContentPage page, Action action)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    action.Invoke();
                });
            }
            else
            {
                EventHandler eventHandler = null;
                eventHandler += (s, e) =>
                {
                    page.Rendered -= eventHandler;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        action.Invoke();
                    });
                };
                page.Rendered += eventHandler;
            }
        }

        #endregion

        #region Internal classes

        /// <summary>
        /// Helper layout for ToolBar and NavigationBar animation
        /// </summary>
        private class Container : Layout<View>
        {
            private ContentPage _page = null;
            private NavigationPage _modalNavigationPage = null;
            private View _navigationBar = null;
            private BoxView _modalPageBackground = null;
            private ShadowView _navigationShadowView = null;
            private ShadowView _modalPageShadowView = null;
            private Border _modalPageBorder = null;
            private Size _modalPageSize = new Size();
            private SizeRequest _navigationBarSize;

            private bool _isValidationIgnored = false;

            /// <summary>
            /// Event to execute when modal background is tapped
            /// </summary>
            public EventHandler ModalPageBackgroundTapped { get; set; }

            /// <summary>
            /// Is debug log enabled
            /// </summary>
            public bool IsDebugEnabled { get; set; } = false;

            #region Binding properties

            /// <summary>
            /// Modal background color for modal navigation page
            /// </summary>
            public static readonly BindableProperty ModalPageBackgroundColorProperty =
                BindableProperty.Create("ModalPageBackgroundColor", typeof(ModalColors), typeof(Container), ModalColors.Black);

            public ModalColors ModalPageBackgroundColor
            {
                get { return (ModalColors)GetValue(ModalPageBackgroundColorProperty); }
                set { SetValue(ModalPageBackgroundColorProperty, value); }
            }

            /// <summary>
            /// Modal opacity
            /// </summary>
            public static readonly BindableProperty ModalColorOpacityProperty =
                BindableProperty.Create("ModalColorOpacity", typeof(double), typeof(Container), 0.2);

            public double ModalColorOpacity
            {
                get { return (double)GetValue(ModalColorOpacityProperty); }
                set { SetValue(ModalColorOpacityProperty, value); }
            }

            #endregion

            #region Properties

            /// <summary>
            /// NavigationBar size (measured inside this layout container or parent)
            /// </summary>
            public SizeRequest NavigationBarSize
            {
                get
                {
                    return _navigationBarSize;
                }
                set
                {
                    bool didChanged = value.Request.Height != _navigationBarSize.Request.Height;
                    _navigationBarSize = value;

                    if (didChanged)
                    {
                        InvalidateMeasure();
                        InvalidateLayout();
                    }
                }
            }

            /// <summary>
            /// Container Page
            /// </summary>
            public ContentPage Page
            {
                get
                {
                    return _page;
                }
                set
                {
                    if (_page == value)
                    {
                        return;
                    }

                    if (_page != null)
                    {
                        _page.PropertyChanged -= OnPagePropertyChanged;

                        _isValidationIgnored = value != null;

                        ContentPage tmp = _page;
                        _page = null;
                        Children.Remove(tmp);

                        _isValidationIgnored = false;
                    }

                    _page = value;

                    if (_page != null)
                    {
                        _page.PropertyChanged += OnPagePropertyChanged;
                        Children.Insert(Children.IndexOf(_navigationShadowView) + 1, _page);

                        UpdateNavigationBarVisibility();
                    }
                }
            }

            /// <summary>
            /// Container modal navigation page
            /// </summary>
            public NavigationPage ModalNavigationPage
            {
                get
                {
                    return _modalNavigationPage;
                }
                set
                {
                    if (_modalNavigationPage == value)
                    {
                        return;
                    }

                    _modalNavigationPage = value;

                    _isValidationIgnored = true;

                    if (_modalPageBackground == null)
                    {
                        _modalPageBackground = CreateModalPageBackground();
                        Children.Add(_modalPageBackground);
                    }
                    if (_modalPageBorder == null)
                    {
                        _modalPageBorder = new Border();
                        _modalPageBorder.HorizontalOptions = LayoutOptions.Center;
                        _modalPageBorder.VerticalOptions = LayoutOptions.Center;
                    }
                    if (_modalPageShadowView == null)
                    {
                        _modalPageShadowView = new ShadowView();
                        _modalPageShadowView.HorizontalOptions = LayoutOptions.Center;
                        _modalPageShadowView.VerticalOptions = LayoutOptions.Center;
                        _modalPageShadowView.ShadowColor = Color.Black;
                        _modalPageShadowView.ShadowOpacity = 0.2;
                        _modalPageShadowView.ShadowLenght = 20;
                        _modalPageShadowView.BorderBackgroundColor = Color.White;
                        _modalPageShadowView.Children.Add(_modalPageBorder);
                    }

                    _isValidationIgnored = false;

                    if (_modalNavigationPage != null)
                    {
                        _modalPageBorder.Content = _modalNavigationPage;
                        Children.Add(_modalPageShadowView);
                    }
                    else
                    {
                        Children.Remove(_modalPageShadowView);
                        _modalPageBorder.Content = null;
                    }
                }
            }

            /// <summary>
            /// Container navigation bar
            /// </summary>
            public View NavigationBar
            {
                get
                {
                    return _navigationBar;
                }
                set
                {
                    if (_navigationBar == value)
                    {
                        return;
                    }

                    _isValidationIgnored = true;

                    if (_navigationBar != null)
                    {
                        View tmp = _navigationBar;
                        _navigationBar = null;
                        Children.Remove(tmp);
                    }

                    _navigationBar = value;

                    if (_navigationBar != null)
                    {
                        if (_modalPageBackground != null)
                        {
                            Children.Insert(Children.IndexOf(_modalPageBackground), _navigationBar);
                        }
                        else
                        {
                            Children.Add(_navigationBar);
                        }

                        UpdateNavigationBarVisibility();
                    }

                    _isValidationIgnored = false;
                }
            }

            #endregion

            public Container()
            {
                _navigationShadowView = new ShadowView();
                _navigationShadowView.InputTransparent = true;
                _navigationShadowView.ShadowColor = Color.Black;
                _navigationShadowView.ShadowOpacity = 0.2;
                _navigationShadowView.ShadowLenght = 0;
                _navigationShadowView.Opacity = 0;
                 Children.Add(_navigationShadowView);
            }

            internal void IsNavigationShadowVisible(bool isVisible)
            {
                _navigationShadowView.ShadowLenght = isVisible ? 20 : 0;
                _navigationShadowView.Opacity = isVisible ? 1 : 0;
            }

            internal void IsModalPageVisible(bool isVisible)
            {
                _modalPageShadowView.Opacity = isVisible ? 1 : 0;
            }

            /// <summary>
            /// Set is modal page background visible
            /// </summary>
            public void SetModalBackgroundVisibility(bool isVisible)
            {
                if (isVisible)
                {
                    if (_modalPageBackground == null)
                    {
                        _modalPageBackground = CreateModalPageBackground();
                        Children.Add(_modalPageBackground);
                    }

                    _modalPageBackground.Opacity = ModalColorOpacity;
                    _modalPageBackground.InputTransparent = false;
                }
                else if (_modalPageBackground != null)
                {
                    _modalPageBackground.Opacity = 0;
                    _modalPageBackground.InputTransparent = true;
                }
            }

            /// <summary>
            /// Create modal page background animation
            /// </summary>
            public Animation CreateShowModalAnimation(IAnimation animation)
            {
                if (_modalPageBackground == null)
                {
                    _modalPageBackground = CreateModalPageBackground();
                    Children.Add(_modalPageBackground);
                }


                // Initialize animationpage
                _modalPageShadowView.TranslationX = 0;
                _modalPageShadowView.TranslationY = 0;
                if (Device.RuntimePlatform == Device.UWP)
                {
                    _modalPageShadowView.TranslationX = 0.001;
                    _modalPageShadowView.TranslationY = 0.001;
                }

                _modalPageShadowView.IsVisible = true;
                _modalPageShadowView.Opacity = 1;
                _modalPageShadowView.Scale = 1;
                _modalPageShadowView.ScaleX = 1;
                _modalPageShadowView.ScaleY = 1;

                _modalPageBackground.Opacity = 0;
                _modalPageBackground.InputTransparent = false;

                Animation animationGroup = new Animation();
                animationGroup.Add(0, 1, new Animation(d => _modalPageBackground.Opacity = d, 0, ModalColorOpacity));
                animationGroup.Add(0, 1, animation.Create(_modalPageShadowView));
                return animationGroup;
            }


            /// <summary>
            /// Create modal page background animation
            /// </summary>
            public Animation CreateHideModalAnimation(IAnimation animation)
            {
                Animation animationGroup = new Animation();

                animationGroup.Add(0, 1, new Animation(d => _modalPageBackground.Opacity = d, ModalColorOpacity, 0, finished: () =>
                {
                    _modalPageBackground.InputTransparent = true;
                }));

                animationGroup.Add(0, 1, animation.Create(_modalPageShadowView));

                return animationGroup;
            }

            #region Invalidation

            /// <summary>
            /// Will child add invalidate layout
            /// </summary>
            protected override bool ShouldInvalidateOnChildAdded(View child)
            {
                return base.ShouldInvalidateOnChildAdded(child) && _isValidationIgnored == false;
            }

            /// <summary>
            /// Will child remove invalidate layout
            /// </summary>
            protected override bool ShouldInvalidateOnChildRemoved(View child)
            {
                return base.ShouldInvalidateOnChildRemoved(child) && _isValidationIgnored == false;
            }

            protected override void OnChildMeasureInvalidated()
            {
                if (_isValidationIgnored == false)
                {
                    base.OnChildMeasureInvalidated();
                }
            }

            protected override void InvalidateLayout()
            {
                if (_isValidationIgnored == false)
                {
                    base.InvalidateLayout();
                }
            }

            protected override void InvalidateMeasure()
            {
                if (_isValidationIgnored == false)
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
                if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("Container.OnMeasure: " + (_page != null ? XamKit.NavigationBar.GetTitle(_page) : "")); }

                return MeasureChildren(widthConstraint, heightConstraint);
            }

            /// <summary>
            /// Layout children
            /// </summary>
            protected override void LayoutChildren(double x, double y, double width, double height)
            {
                if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("Container.LayoutChildren: " + (_page != null ? XamKit.NavigationBar.GetTitle(_page) : "")); }

                NavigationBarSize = MeasureNavigationBar(width, height);

                // Do not layout navigationbar if page is null
                if (_page != null && Children.Contains(_page))
                {
                    double heightToTake = 0;
                    if (XamKit.NavigationBar.GetVisibility(_page) == NavigationBarVisibility.Visible && XamKit.NavigationBar.GetIsFloating(_page) == false)
                    {
                        heightToTake = NavigationBarSize.Request.Height;
                    }

                    LayoutChild(_page, new Rectangle(0, heightToTake, width, height - heightToTake));
                    LayoutChild(_navigationBar, new Rectangle(0, 0, width, NavigationBarSize.Request.Height));
                }

                // Layout modal page always to full size
                LayoutChild(_modalPageBackground, new Rectangle(0, 0, width, height));

                // Measure and layout modal page
                if (_modalPageShadowView != null && Children.Contains(_modalPageShadowView))
                {
                    ContentPage lastModalPage = _modalNavigationPage.NavigationStack.Last();

                    if (lastModalPage.HorizontalOptions.Alignment != LayoutAlignment.Fill || 
                        lastModalPage.VerticalOptions.Alignment != LayoutAlignment.Fill ||
                        (lastModalPage.WidthRequest > 0 && lastModalPage.WidthRequest < width) ||
                        (lastModalPage.HeightRequest > 0 && lastModalPage.HeightRequest < height))
                    {
                        _modalPageSize = _modalPageShadowView.Measure(width, height, MeasureFlags.IncludeMargins).Request;

                        if (_modalPageSize.Width < width || _modalPageSize.Height < height)
                        {
                            _modalPageBorder.CornerRadius = _modalNavigationPage.CornerRadius;
                            _modalPageShadowView.CornerRadius = _modalNavigationPage.CornerRadius;
                        }
                        else
                        {
                            _modalPageBorder.CornerRadius = 0;
                            _modalPageShadowView.CornerRadius = 0;
                        }

                        LayoutChild(_modalPageShadowView, new Rectangle((width - _modalPageSize.Width) / 2, (height - _modalPageSize.Height) / 2, _modalPageSize.Width, _modalPageSize.Height));
                    }
                    else
                    {
                        _modalPageBorder.CornerRadius = 0;
                        _modalPageShadowView.CornerRadius = 0;
                        LayoutChild(_modalPageShadowView, new Rectangle(0, 0, width, height));
                    }
                }

                if (_navigationShadowView != null)
                {
                    LayoutChild(_navigationShadowView, new Rectangle(0, 0, width, height));
                }
            }

            /// <summary>
            /// Do actual total size measure
            /// </summary>
            private SizeRequest MeasureChildren(double availableWidth, double availableHeight)
            {
                Size totalSize = new Size();

                // Do measures only if page is not null
                if (_page != null && Children.Contains(_page))
                {
                    bool isFullWidthRequest = 
                        _page.WidthRequest >= availableWidth || 
                        (_page.HorizontalOptions.Alignment == LayoutAlignment.Fill && _page.WidthRequest < 0);

                    bool isFullHeightRequest = 
                        _page.HeightRequest >= availableHeight || 
                        (_page.VerticalOptions.Alignment == LayoutAlignment.Fill && _page.HeightRequest < 0);

                    // Is fully stretch
                    if (isFullWidthRequest && isFullHeightRequest)
                    {
                        NavigationBarSize = MeasureNavigationBar(availableHeight, availableHeight);
                        totalSize = new Size(availableWidth, availableHeight);
                    }
                    // Height is stretch and width is fixed
                    else if (isFullWidthRequest == false && isFullHeightRequest == true && _page.WidthRequest > 0)
                    {
                        NavigationBarSize = MeasureNavigationBar(_page.WidthRequest, availableHeight);
                        totalSize = new Size(_page.WidthRequest, availableHeight);
                    }
                    // Width is strecth and height is fixed
                    else if (isFullWidthRequest == true && isFullHeightRequest == false && _page.HeightRequest > 0)
                    {
                        NavigationBarSize = MeasureNavigationBar(availableWidth, _page.HeightRequest);
                        totalSize = new Size(availableWidth, _page.HeightRequest);
                    }
                    // Width and height are fixed
                    else if (_page.WidthRequest > 0 && _page.HeightRequest > 0)
                    {
                        NavigationBarSize = MeasureNavigationBar(_page.WidthRequest, _page.HeightRequest);
                        totalSize = new Size(_page.WidthRequest, _page.HeightRequest);
                    }
                    // Other scenarios
                    else
                    {
                        double heightToTake = 0;

                        // Is navigation bar going to take height from page
                        if (XamKit.NavigationBar.GetVisibility(_page) == NavigationBarVisibility.Visible && XamKit.NavigationBar.GetIsFloating(_page) == false)
                        {
                            NavigationBarSize = MeasureNavigationBar(availableWidth, availableHeight);
                            heightToTake = NavigationBarSize.Request.Height;
                        }

                        SizeRequest pageSize = _page.Measure(availableWidth, availableHeight - heightToTake, MeasureFlags.IncludeMargins);
                        totalSize = new Size(pageSize.Request.Width, pageSize.Request.Height + heightToTake);
                    }
                }
                else
                {
                    // If page is null, then take all available space
                    totalSize = new Size(availableWidth, availableHeight);
                }

                return new SizeRequest(totalSize, totalSize);
            }

            /// <summary>
            /// Measure navigation bar from this panel or parent
            /// </summary>
            private SizeRequest MeasureNavigationBar(double width, double height)
            {
                if (_navigationBar != null)
                {
                    return _navigationBar.Measure(width, height, MeasureFlags.IncludeMargins);
                }
                else
                {
                    NavigationPage navPage = Parent as NavigationPage;
                    if (this == navPage._topContainer && navPage._topNavigationBar != null && navPage._topNavigationBar.View != null)
                    {
                        return navPage._topNavigationBar.View.Measure(width, height, MeasureFlags.IncludeMargins);
                    }
                    else if (this == navPage._bottomContainer && navPage._bottomNavigationBar != null && navPage._bottomNavigationBar.View != null)
                    {
                        return navPage._bottomNavigationBar.View.Measure(width, height, MeasureFlags.IncludeMargins);
                    }
                    else if (this == navPage._bottomContainer && navPage._topNavigationBar != null && navPage._topNavigationBar.View != null)
                    {
                        return navPage._topNavigationBar.View.Measure(width, height, MeasureFlags.IncludeMargins);
                    }
                }

                return new SizeRequest();
            }

            /// <summary>
            /// Layout view if needed (optimization)
            /// </summary>
            private void LayoutChild(View child, Rectangle newLocation)
            {
                if (child != null && child.Bounds != newLocation && Children.Contains(child))
                {
                    LayoutChildIntoBoundingRegion(child, newLocation);
                }
            }

            #endregion

            /// <summary>
            /// Handle page properties chanes for navigation bar
            /// </summary>
            private void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == XamKit.NavigationBar.VisibilityProperty.PropertyName)
                {
                    UpdateNavigationBarVisibility();
                    InvalidateMeasure();
                    InvalidateLayout();
                }
                else if (e.PropertyName == XamKit.NavigationBar.IsFloatingProperty.PropertyName)
                {
                    InvalidateMeasure();
                    InvalidateLayout();
                }
            }

            private void UpdateNavigationBarVisibility()
            {
                if (_page == null || _navigationBar == null)
                {
                    return;
                }

                if (XamKit.NavigationBar.GetVisibility(_page) == NavigationBarVisibility.Hidden)
                {
                    _navigationBar.Opacity = 0;
                    _navigationBar.InputTransparent = true;
                }
                else
                {
                    _navigationBar.Opacity = 1;
                    _navigationBar.InputTransparent = false;
                }
            }


            /// <summary>
            /// Background tapped event handler
            /// </summary>
            private void OnModalPageBackgroundTapped(object sender, EventArgs e)
            {
                if (ModalPageBackgroundTapped != null)
                {
                    ModalPageBackgroundTapped.Invoke(this, e);
                }
            }

            private BoxView CreateModalPageBackground()
            {
                BoxView modalPageBackground = new BoxView();
                modalPageBackground.Opacity = 0;
                modalPageBackground.Color = ModalPageBackgroundColor == ModalColors.Black ? Color.Black : Color.White;

                TapGestureRecognizer tap = new TapGestureRecognizer();
                tap.Tapped += OnModalPageBackgroundTapped;
                modalPageBackground.GestureRecognizers.Add(tap);

                return modalPageBackground;
            }
        }

        /// <summary>
        /// Helper class for page and its modal pages
        /// </summary>
        private class PageInfo
        {
            public ContentPage Page { get; private set; }
            public List<ContentPage> ModalPages { get; private set; }

            public PageInfo(ContentPage page)
            {
                Page = page;
                ModalPages = new List<ContentPage>();
            }
        }

        private class ModalNavigationPageInfo
        {
            public NavigationPage NavigationPage { get; private set; }
            public SizeRequest Size { get; set; }

            public ModalNavigationPageInfo(NavigationPage navigationPage)
            {
                NavigationPage = navigationPage;
            }
        }

        private class NavigationBarInfo
        {
            public View View { get; private set; }
            public INavigationBar NavigationBar { get; set; }

            public NavigationBarInfo(View view)
            {
                View = view;
            }
        }

        private class NavigationPageInfo
        {
            public double NavigationBarHeight { get; private set; }

            public NavigationPageInfo(double navigationBarHeight)
            {
                NavigationBarHeight = navigationBarHeight;
            }
        }

        #endregion
    }

    public enum ModalColors { Black, White }
}
