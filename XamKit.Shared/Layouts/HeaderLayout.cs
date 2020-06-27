using System;
using System.Linq;
using System.Runtime.CompilerServices;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    [ContentProperty("Content")]
	public class HeaderLayout : Layout<View>
    {
        private SizeRequest _contentSize = new SizeRequest();
        private SizeRequest _headerSize = new SizeRequest();
        private SizeRequest _stickyHeaderSize = new SizeRequest();

        private View _contentElement = null;
		private View _headerElement = null;
		private View _stickyHeaderElement = null;

        // Layout parents cache
        private RelatedViews _relatedViews;

        // Content TabView
        private TabView _tabView;

        // Previous scroll source Y offset
        private double _scrollY = 0;

        /// <summary>
        /// Event when IsHeaderVisible value changes
        /// </summary>
        public event EventHandler<bool> IsHeaderVisibleChanged;

        #region Properties

        public double HeaderTranslationY
        {
            get
            {
                if (_headerElement != null)
                {
                    return _headerElement.TranslationY;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double StickyHeaderTranslationY
        {
            get
            {
                if (_stickyHeaderElement != null)
                {
                    return _stickyHeaderElement.TranslationY;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double HeaderHeight
        {
            get
            {
                return _headerSize.Request.Height;
            }
        }

        public double StickyHeaderHeight
        {
            get
            {
                return _stickyHeaderSize.Request.Height;
            }
        }

        /// <summary>
        /// Previous scroll source Y offset. Updated internally from ScrollSource.
        /// </summary>
        public double ScrollY
        {
            get
            {
                return _scrollY;
            }
            internal set
            {
                _scrollY = value;
            }
        }

        #endregion

        #region Binding Properties

        /// <summary>
        /// Scroll viewer
        /// </summary>
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create("Content", typeof(View), typeof(HeaderLayout), null);

        public View Content
        {
            get { return (View)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Template to create header
        /// </summary>
        public static readonly BindableProperty HeaderTemplateProperty =
            BindableProperty.Create("HeaderTemplate", typeof(DataTemplate), typeof(HeaderLayout), null);

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Header view
        /// </summary>
        public static readonly BindableProperty HeaderProperty =
            BindableProperty.Create("Header", typeof(View), typeof(HeaderLayout), null);

        public View Header
        {
            get { return (View)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Template to create sticky header
        /// </summary>
        public static readonly BindableProperty StickyHeaderTemplateProperty =
            BindableProperty.Create("StickyHeaderTemplate", typeof(DataTemplate), typeof(HeaderLayout), null);

        public DataTemplate StickyHeaderTemplate
        {
            get { return (DataTemplate)GetValue(StickyHeaderTemplateProperty); }
            set { SetValue(StickyHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Sticky Header view
        /// </summary>
        public static readonly BindableProperty StickyHeaderProperty =
            BindableProperty.Create("StickyHeader", typeof(View), typeof(HeaderLayout), null);

        public View StickyHeader
        {
            get { return (View)GetValue(StickyHeaderProperty); }
            set { SetValue(StickyHeaderProperty, value); }
        }

        /// <summary>
        /// Is header scrolled out: True = yes or header is null, False = no
        /// </summary>
        public static readonly BindableProperty IsHeaderVisibleProperty =
            BindableProperty.Create("IsHeaderVisible", typeof(bool), typeof(HeaderLayout), false);

        public bool IsHeaderVisible
        {
            get { return (bool)GetValue(IsHeaderVisibleProperty); }
            set { SetValue(IsHeaderVisibleProperty, value); }
        }

        #endregion

        public HeaderLayout()
        {
        }

        #region Property changes

        /// <summary>
        /// Called when any property change
        /// </summary>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == ContentProperty.PropertyName)
            {
                OnContentChanged(Content);
            }
            else if (propertyName == HeaderProperty.PropertyName)
            {
                OnHeaderChanged(Header);
            }
            else if (propertyName == HeaderTemplateProperty.PropertyName)
            {
                OnHeaderTemplateChanged(HeaderTemplate);
            }
            else if (propertyName == StickyHeaderProperty.PropertyName)
            {
                OnStickyHeaderChanged(StickyHeader);
            }
            else if (propertyName == StickyHeaderTemplateProperty.PropertyName)
            {
                OnStickyHeaderTemplateChanged(StickyHeaderTemplate);
            }
            else if (propertyName == IsHeaderVisibleProperty.PropertyName)
            {
                IsHeaderVisibleChanged?.Invoke(this, IsHeaderVisible);
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        private void OnContentChanged(object newContent)
        {
            if (newContent is View content)
            {
                // Remove old content from children
                if (_contentElement != null && Children.Contains(_contentElement))
                {
                    Children.Remove(_contentElement);
                }

                // Add new content to children
                _contentElement = content;
                Children.Insert(0, _contentElement);

                // Remove TabView event listeners
                if (_tabView != null)
                {
                    _tabView.PanChanged -= OnTabViewPanChanged;
                }

                // Try find TabView from content
                if (content != null)
                {
                    _tabView = content as TabView;
                    if (_tabView == null)
                    {
                        _tabView = VisualTreeHelper.FindVisualChildren<TabView>(content).FirstOrDefault();
                    }
                    if (_tabView != null)
                    {
                        _tabView.PanChanged += OnTabViewPanChanged;
                    }
                }
            }
            else if (_contentElement != null)
            {
                _contentElement.BindingContext = newContent;
            }
        }

        private void OnHeaderChanged(object newHeader)
        {
            if (newHeader is View header)
            {
                if (_headerElement != null && Children.Contains(_headerElement))
                {
                    Children.Remove(_headerElement);
                }

                _headerElement = header;
                Children.Add(_headerElement);
                IsHeaderVisible = true;
            }
            else if (_headerElement != null)
            {
                _headerElement.BindingContext = newHeader;
            }
        }

        private void OnHeaderTemplateChanged(DataTemplate newHeaderTemplate)
        {
            if (newHeaderTemplate != null && Header is View == false)
            {
                if (_headerElement != null && Children.Contains(_headerElement))
                {
                    Children.Remove(_headerElement);
                }

                _headerElement = newHeaderTemplate.CreateContent() as View;
                Children.Add(_headerElement);
                IsHeaderVisible = true;

                if (Header != null)
                {
                    _headerElement.BindingContext = Header;
                }
            }
        }

        private void OnStickyHeaderChanged(object newStickyHeader)
        {
            if (newStickyHeader is View stickyHeader)
            {
                if (_stickyHeaderElement != null && Children.Contains(_stickyHeaderElement))
                {
                    Children.Remove(_stickyHeaderElement);
                }

                _stickyHeaderElement = stickyHeader;
                Children.Add(_stickyHeaderElement);
            }
            else if (_stickyHeaderElement != null)
            {
                _stickyHeaderElement.BindingContext = newStickyHeader;
            }
        }

        private void OnStickyHeaderTemplateChanged(DataTemplate newStickyHeaderTemplate)
        {
            if (newStickyHeaderTemplate != null && StickyHeader is View == false)
            {
                if (_stickyHeaderElement != null && Children.Contains(_stickyHeaderElement))
                {
                    Children.Remove(_stickyHeaderElement);
                }

                _stickyHeaderElement = newStickyHeaderTemplate.CreateContent() as View;
                Children.Add(_stickyHeaderElement);

                if (StickyHeader != null)
                {
                    _stickyHeaderElement.BindingContext = StickyHeader;
                }
            }
        }

        #endregion

        #region Measure / Layout

        /// <summary>
        /// Measure children
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Size totalSize = new Size();
            _contentSize = new SizeRequest();

            MeasureHeaders(widthConstraint, heightConstraint);

            if (HorizontalOptions.Alignment == LayoutAlignment.Fill && VerticalOptions.Alignment == LayoutAlignment.Fill && double.IsInfinity(widthConstraint) == false && double.IsInfinity(heightConstraint) == false)
            {
                totalSize.Width = widthConstraint;
                totalSize.Height = heightConstraint;

                if (_contentElement != null && Children.Contains(_contentElement))
                {
                    _contentSize = new SizeRequest(totalSize, totalSize);
                }
            }
            else if (_contentElement != null && Children.Contains(_contentElement))
            {
                _contentSize = _contentElement.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);

                totalSize.Width = Math.Max(totalSize.Width, _contentSize.Request.Width);
                totalSize.Height = Math.Max(totalSize.Height, _contentSize.Request.Height);
            }

            return new SizeRequest(totalSize, totalSize);
        }

        /// <summary>
        /// Layout all children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            MeasureHeaders(width, height);
 
            LayoutChild(_headerElement, new Rectangle(x, y, width, _headerSize.Request.Height));
            LayoutChild(_stickyHeaderElement, new Rectangle(x, y, width, _stickyHeaderSize.Request.Height));

            if (_relatedViews == null)
            {
                _relatedViews = new RelatedViews(this);
            }

            ScrollSource.TryGetNavigationBarProperties(_relatedViews.ContentPage, _relatedViews.NavigationPage, out double navigationBarHeight, out double navigationBarY, out bool isNavigationBarFloating, out bool isNavigationBarScrollable);

            // Update headers translationY based on previous scroll
            UpdateHeadersTranslationY(_scrollY, navigationBarHeight, navigationBarY, isNavigationBarFloating, isNavigationBarScrollable);

            bool isHeaderLayoutInScrollViewer = VisualTreeHelper.GetParent<ScrollView>(this, typeof(NavigationPage)) != null;

            if (isHeaderLayoutInScrollViewer)
            {
                double heightToAdd = 0;
                if (_relatedViews.ContentPage != null && _relatedViews.NavigationPage != null && isNavigationBarFloating == false && isNavigationBarScrollable == true)
                {
                    heightToAdd = navigationBarHeight;
                }

                LayoutChild(_contentElement, new Rectangle(x, y + _headerSize.Request.Height + _stickyHeaderSize.Request.Height + heightToAdd, width, height));
            }
            else
            {
                LayoutChild(_contentElement, new Rectangle(x, RootPage.Instance.SafeAreaInsest.Top, width, height));
            }
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

        /// <summary>
        /// Measure headers
        /// </summary>
        private void MeasureHeaders(double widthConstraint, double heightConstraint)
        {
            _headerSize = new SizeRequest();
            _stickyHeaderSize = new SizeRequest();

            if (_headerElement != null && Children.Contains(_headerElement))
            {
                _headerSize = Header.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
            }

            if (_stickyHeaderElement != null && Children.Contains(_stickyHeaderElement))
            {
                _stickyHeaderSize = StickyHeader.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
            }
        }

        #endregion


        private void OnTabViewPanChanged(object sender, PanChangedArgs args)
        {
            if (sender is CarouselView carouselView && carouselView.ItemsLayout is CarouselLayout carouselLayout)
            {
                if (_relatedViews == null)
                {
                    _relatedViews = new RelatedViews(this);
                }

                foreach (View child in carouselLayout.Children)
                {
                    ScrollView scrollView = VisualTreeHelper.FindVisualChildren<ScrollView>(child).FirstOrDefault();

                    if (scrollView != null && scrollView.ScrollY != _scrollY)
                    {
                        ScrollSource.UpdateScrollViewStart(this, _relatedViews.ContentPage, _relatedViews.NavigationPage, scrollView, _scrollY);
                    }
                }
            }
        }

        /// <summary>
        /// Update headers position based on scroll
        /// </summary>
        public void UpdateHeadersTranslationY(double scrollY, double navigationBarHeight, double navigationBarY, bool isNavigationBarFloating, bool isNavigationBarScrollable)
        {
            double headerBottom = 0;
            double navigationBarBottom = navigationBarY + navigationBarHeight;

            bool isHeaderLayoutInScrollViewer = VisualTreeHelper.GetParent<ScrollView>(this, typeof(NavigationPage)) != null;

            if (isNavigationBarFloating == false) // TODO
            {
                headerBottom = navigationBarBottom;
            }

            // Update header element translation
            if (_headerElement != null)
            {
                if (isHeaderLayoutInScrollViewer)
                {
                    // If navigation bar is floating or scrolled out
                    if (isNavigationBarFloating)
                    {
                        _headerElement.TranslationY = 0;
                    }
                    else
                    {
                        if (isNavigationBarScrollable)
                        {
                            _headerElement.TranslationY = navigationBarHeight;
                        }
                        else
                        {
                            _headerElement.TranslationY = 0;
                        }
                    }
                }
                else
                {
                    // If navigation bar is floating or scrolled out
                    if (isNavigationBarFloating)
                    {
                        _headerElement.TranslationY = -Math.Min(scrollY, _headerSize.Request.Height + navigationBarHeight);
                    }
                    else
                    {
                        if (isNavigationBarScrollable)
                        {
                            _headerElement.TranslationY = navigationBarHeight - scrollY;
                        }
                        else
                        {
                            _headerElement.TranslationY = -scrollY;
                        }
                    }
                }

                headerBottom = _headerElement.TranslationY + _headerSize.Request.Height;
            }

            // Update sticky element translation
            if (_stickyHeaderElement != null)
            {
                if (isHeaderLayoutInScrollViewer)
                {
                    if (isNavigationBarFloating == false && isNavigationBarScrollable && scrollY > headerBottom - navigationBarBottom)
                    {
                        _stickyHeaderElement.TranslationY = scrollY + navigationBarBottom;
                    }
                    else if (isNavigationBarFloating == false && isNavigationBarScrollable == false && scrollY > headerBottom)
                    {
                        _stickyHeaderElement.TranslationY = scrollY;
                    }
                    else if (isNavigationBarFloating && isNavigationBarScrollable && scrollY > headerBottom - navigationBarBottom)
                    {
                        _stickyHeaderElement.TranslationY = scrollY + navigationBarBottom;
                    }
                    else if (isNavigationBarFloating && isNavigationBarScrollable == false && scrollY > headerBottom - navigationBarBottom)
                    {
                        _stickyHeaderElement.TranslationY = scrollY + navigationBarHeight;
                    }
                    else
                    {
                        _stickyHeaderElement.TranslationY = headerBottom;
                    }
                }
                else
                {
                    if (isNavigationBarScrollable)
                    {
                        _stickyHeaderElement.TranslationY = Math.Max(navigationBarBottom, headerBottom);
                    }
                    else
                    {
                        if (isNavigationBarFloating)
                        {
                            _stickyHeaderElement.TranslationY = Math.Max(navigationBarBottom, headerBottom);
                        }
                        else
                        {
                            _stickyHeaderElement.TranslationY = 0; // Math.Max(0, headerBottom);
                        }
                    }
                }
            }

            if (isNavigationBarFloating || isNavigationBarScrollable)
            {
                IsHeaderVisible = scrollY <= _headerSize.Request.Height - navigationBarHeight;
            }
            else
            {
                IsHeaderVisible = scrollY <= _headerSize.Request.Height;
            }
        }
    }
}
