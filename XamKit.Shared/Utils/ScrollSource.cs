using System;
using System.Linq;
using Xamarin.Forms;

namespace XamKit
{
    public class ScrollSource
    {
        /// <summary>
        /// Is scrolling source for header and navigation bar scrolling
        /// </summary>
        public static readonly BindableProperty IsEnabledProperty =
            BindableProperty.CreateAttached("IsEnabled", typeof(bool), typeof(HeaderLayout), false, propertyChanged: OnIsEnabledChanged);

        public static bool GetIsEnabled(BindableObject view)
        {
            return (bool)view.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(BindableObject view, bool value)
        {
            view.SetValue(IsEnabledProperty, value);
        }

        /// <summary>
        /// Add page content root element attached property list which contains all scroll hosts
        /// </summary>
        private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if ((bool)newValue == true)
            {
                View source = bindable as View;

                RelatedViews relatedViews = new RelatedViews(source);
                double oldScrollY = 0;

                // Size changed
                EventHandler sizeChangedEventHandler = null;
                sizeChangedEventHandler = (object sender, EventArgs e) =>
                {
                    UpdateScrollSourcePadding(relatedViews.HeaderLayout, relatedViews.ContentPage, relatedViews.NavigationPage, source);
                };
                source.SizeChanged += sizeChangedEventHandler;

                if (bindable is ScrollView scrollView)
                {
                    if (Device.RuntimePlatform == Device.UWP)
                    {
                        scrollView.Effects.Add(new ScrollEffect());
                    }

                    // Scrolled
                    EventHandler<ScrolledEventArgs> scrollEventHandler = null;
                    scrollEventHandler = (object sender, ScrolledEventArgs e) =>
                    {
                        HandleScrolling(relatedViews.HeaderLayout, relatedViews.ContentPage, relatedViews.NavigationPage, oldScrollY, e.ScrollY);
                        oldScrollY = e.ScrollY;
                    };
                    scrollView.Scrolled += scrollEventHandler;
                }
                else if (bindable is CollectionView collectionView)
                {
                    // Scrolled
                    EventHandler<ItemsViewScrolledEventArgs> scrollEventHandler = null;
                    scrollEventHandler = (object sender, ItemsViewScrolledEventArgs e) =>
                    {
                        HandleScrolling(relatedViews.HeaderLayout, relatedViews.ContentPage, relatedViews.NavigationPage, oldScrollY, e.VerticalOffset);
                        oldScrollY = e.VerticalOffset;
                    };
                    collectionView.Scrolled += scrollEventHandler;
                }
            }
            else
            {
                // TODO
            }
        }

        /// <summary>
        /// Update ScrollView / CollectionView padding which are header layout content
        /// </summary>
        public static void UpdateScrollSourcePadding(HeaderLayout headerLayout, ContentPage contentPage, NavigationPage navigationPage, View scrollSource)
        {
            bool isHeaderLayoutInScrollViewer = headerLayout != null ? VisualTreeHelper.GetParent<ScrollView>(headerLayout) != null : false;

            // Get navigation page navigation bar height
            double topPadding = 0;
            if (contentPage != null && NavigationBar.GetIsFloating(contentPage) == false &&
                isHeaderLayoutInScrollViewer == false &&
                (NavigationBar.GetVisibility(contentPage) == NavigationBarVisibility.Scroll ||
                 NavigationBar.GetVisibility(contentPage) == NavigationBarVisibility.SmoothScroll))
            {
                topPadding += navigationPage.GetNavigationBarHeight();
            }

            if (headerLayout != null && isHeaderLayoutInScrollViewer == false)
            {
                topPadding += headerLayout.HeaderHeight;
                topPadding += headerLayout.StickyHeaderHeight;
            }

            if (scrollSource is ScrollView scrollView)
            {
                if (topPadding.Equals(scrollView.Padding.Top) == false)
                {
                    scrollView.Padding = new Thickness(scrollView.Padding.Left, topPadding, scrollView.Padding.Right, scrollView.Padding.Bottom);
                }
            }
            else if (scrollSource is CollectionView collectionView)
            {
                if (collectionView.Header == null)
                {
                    BoxView b = new BoxView();
                    b.HeightRequest = 0;
                    collectionView.Header = b;
                }

                if (collectionView.Header is View headerView)
                {
                    headerView.Margin = new Thickness(headerView.Margin.Left, topPadding, headerView.Margin.Right, headerView.Margin.Bottom);
                }
            }
        }

        /// <summary>
        /// Update headers and navigation bar translation Y by scroll
        /// </summary>
        private static void HandleScrolling(HeaderLayout headerLayout, ContentPage contentPage, NavigationPage navigationPage, double oldScrollY, double newScrollY)
        {
            // Save previous scroll Y
            if (headerLayout != null)
            {
                headerLayout.ScrollY = newScrollY;
            }

            TryGetNavigationBarProperties(contentPage, navigationPage, out double navigationBarHeight, out double navigationBarY, out bool isNavigationBarFloating, out bool isNavigationBarScrollable);

            // Scroll delta
            double delta = Math.Max(0, newScrollY) - Math.Max(0, oldScrollY);

            // Update navigation bar translation when it should scroll out smoothly or not
            if (navigationPage != null && contentPage != null &&
                (NavigationBar.GetVisibility(contentPage) == NavigationBarVisibility.Scroll ||
                 NavigationBar.GetVisibility(contentPage) == NavigationBarVisibility.SmoothScroll))
            {
                View navigationBar = navigationPage.GetNavigationBar();

                // Hide navigation bar smoothly
                if (NavigationBar.GetVisibility(contentPage) == NavigationBarVisibility.SmoothScroll && navigationBar != null)
                {
                    // If scrolled enought speed do smooth navigation bar hiding with animation
                    if (Math.Abs(delta) > 5)
                    {
                        string showScrollBarAnimationName = "showScrollBarAnimation";
                        string hideScrollBarAnimationName = "hideScrollBarAnimation";

                        double start = navigationBar.TranslationY;
                        double target = 0;
                        string actualAnimationName;

                        if (delta < 0)
                        {
                            target = 0;
                            actualAnimationName = showScrollBarAnimationName;
                        }
                        else
                        {
                            target = -navigationBarHeight;
                            actualAnimationName = hideScrollBarAnimationName;
                        }

                        if (target != navigationBar.TranslationY && AnimationExtensions.AnimationIsRunning(navigationBar, actualAnimationName) == false)
                        {
                            AnimationExtensions.AbortAnimation(navigationBar, showScrollBarAnimationName);
                            AnimationExtensions.AbortAnimation(navigationBar, hideScrollBarAnimationName);

                            new Animation(d =>
                            {
                                // Animate NavigastionBar TranslationY
                                navigationBar.TranslationY = start + (target - start) * d;

                                // Get NavigationBar properties during animation because they can change
                                TryGetNavigationBarProperties(contentPage, navigationPage, out navigationBarHeight, out navigationBarY, out isNavigationBarFloating, out isNavigationBarScrollable);

                                // Update headers transaltion because navigation bar translation is animated
                                headerLayout?.UpdateHeadersTranslationY(headerLayout.ScrollY, navigationBarHeight, navigationBarY, isNavigationBarFloating, isNavigationBarScrollable);

                            }, 0, 1).Commit(navigationBar, actualAnimationName, 64, 250);
                        }
                        else
                        {
                            headerLayout?.UpdateHeadersTranslationY(newScrollY, navigationBarHeight, navigationBar.TranslationY, isNavigationBarFloating, isNavigationBarScrollable);
                        }
                    }
                    // If not scrolled enought speed, then hide other headers normally
                    else
                    {
                        headerLayout?.UpdateHeadersTranslationY(newScrollY, navigationBarHeight, navigationBar.TranslationY, isNavigationBarFloating, isNavigationBarScrollable);
                    }
                }
                // Hide navigation bar based on scroll only
                else
                {
                    navigationBar.TranslationY = Math.Min(0, Math.Max(-navigationBarHeight, navigationBar.TranslationY - delta));
                    navigationBarY = navigationBar.TranslationY;
                    headerLayout?.UpdateHeadersTranslationY(newScrollY, navigationBarHeight, navigationBarY, isNavigationBarFloating, isNavigationBarScrollable);
                }
            }
            // If navigation bar is visible or hidden
            else
            {
                headerLayout?.UpdateHeadersTranslationY(newScrollY, navigationBarHeight, navigationBarY, isNavigationBarFloating, isNavigationBarScrollable);
            }
        }

        /// <summary>
        /// Helper to get navigation bar properties
        /// </summary>
        public static void TryGetNavigationBarProperties(ContentPage contentPage, NavigationPage navigationPage, out double navigationBarHeight, out double navigationBarY, out bool isNavigationBarFloating, out bool isNavigationBarScrollable)
        {
            if (contentPage != null && navigationPage != null)
            {
                navigationBarHeight = NavigationBar.GetVisibility(contentPage) != NavigationBarVisibility.Hidden ? navigationPage.GetNavigationBarHeight() : 0;
                isNavigationBarFloating = NavigationBar.GetIsFloating(contentPage);
                isNavigationBarScrollable = NavigationBar.GetVisibility(contentPage) == NavigationBarVisibility.Scroll || NavigationBar.GetVisibility(contentPage) == NavigationBarVisibility.SmoothScroll;

                if (navigationPage.GetNavigationBar() is NavigationBar navigationBar)
                {
                    navigationBarY = navigationBar.TranslationY;
                }
                else
                {
                    navigationBarY = 0;
                }
            }
            else
            {
                navigationBarHeight = 0;
                navigationBarY = 0;
                isNavigationBarFloating = false;
                isNavigationBarScrollable = false;
            }
        }

        /// <summary>
        /// Update ScrollView scroll start position based on other scrollview scrolling
        /// </summary>
        public static void UpdateScrollViewStart(HeaderLayout headerLayout, ContentPage contentPage, NavigationPage navigationPage, ScrollView scrollView, double scrollY)
        {
            bool isNavigationBarFloating = false;
            NavigationBarVisibility navBarVisibility = NavigationBarVisibility.Hidden;
            double navigationBarHeight = 0;

            if (contentPage != null)
            {
                isNavigationBarFloating = NavigationBar.GetIsFloating(contentPage);
                navBarVisibility = NavigationBar.GetVisibility(contentPage);
            }

            if (navigationPage != null)
            {
                bool isNavigationBarScrollable = navBarVisibility == NavigationBarVisibility.Scroll || navBarVisibility == NavigationBarVisibility.SmoothScroll;
                if (navigationPage != null && (isNavigationBarFloating || isNavigationBarScrollable))
                {
                    navigationBarHeight = navigationPage.GetNavigationBarHeight();
                }
            }

            if (headerLayout != null)
            {
                // If scrolled header hidden
                if (scrollY >= headerLayout.HeaderHeight + navigationBarHeight)
                {
                    // If scrollview is scrolled less than header height
                    if (scrollView.ScrollY < headerLayout.HeaderHeight + navigationBarHeight)
                    {
                        if (Device.RuntimePlatform == Device.UWP)
                        {
                            ScrollEffect effect = scrollView.Effects.FirstOrDefault(c => c is ScrollEffect) as ScrollEffect;
                            effect.ScrollTo(0, headerLayout.HeaderHeight + navigationBarHeight);
                        }
                        else
                        {
                            scrollView.ScrollToAsync(0, headerLayout.HeaderHeight + navigationBarHeight, false);
                        }
                    }
                }
                // If scrolled less than header height
                else if (scrollView.ScrollY.Equals(scrollY) == false)
                {
                    if (Device.RuntimePlatform == Device.UWP)
                    {
                        ScrollEffect effect = scrollView.Effects.FirstOrDefault(c => c is ScrollEffect) as ScrollEffect;
                        effect.ScrollTo(0, scrollY);
                    }
                    else
                    {
                        scrollView.ScrollToAsync(0, scrollY, false);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Class to have weak references to related parts
    /// </summary>
    internal class RelatedViews
    {
        private View _source = null;

        // Weak references to parents
        public WeakReference<ContentPage> contentPageReference = null;
        public WeakReference<NavigationPage> navigationPageReference = null;
        public WeakReference<HeaderLayout> headerLayoutReference = null;

        public NavigationPage NavigationPage
        {
            get
            {
                NavigationPage navigationPage = null;
                if (navigationPageReference == null || navigationPageReference.TryGetTarget(out navigationPage) == false)
                {
                    navigationPage = VisualTreeHelper.GetParent<NavigationPage>(_source);
                    if (navigationPage != null)
                    {
                        navigationPageReference = new WeakReference<NavigationPage>(navigationPage);
                    }
                    else
                    {
                        navigationPageReference = null;
                    }
                }

                return navigationPage;
            }
        }
        public ContentPage ContentPage
        {
            get
            {
                ContentPage contentPage = null;
                if (contentPageReference == null || contentPageReference.TryGetTarget(out contentPage) == false)
                {
                    contentPage = VisualTreeHelper.GetParent<ContentPage>(_source);
                    if (contentPage != null)
                    {
                        contentPageReference = new WeakReference<ContentPage>(contentPage);
                    }
                    else
                    {
                        contentPageReference = null;
                    }
                }

                return contentPage;
            }
        }

        public HeaderLayout HeaderLayout
        {
            get
            {
                HeaderLayout headerLayout = null;

                if (headerLayoutReference == null || headerLayoutReference.TryGetTarget(out headerLayout) == false)
                {
                    headerLayout = VisualTreeHelper.GetParent<HeaderLayout>(_source, typeof(NavigationPage));
                    if (headerLayout != null)
                    {
                        headerLayoutReference = new WeakReference<HeaderLayout>(headerLayout);
                    }
                    else
                    {
                        headerLayoutReference = null;
                    }

                    if (headerLayout == null)
                    {
                        headerLayout = VisualTreeHelper.FindVisualChildren<HeaderLayout>(_source).FirstOrDefault();
                    }
                }

                return headerLayout;
            }
        }

        public RelatedViews(View source)
        {
            _source = source;
        }
    }
}
