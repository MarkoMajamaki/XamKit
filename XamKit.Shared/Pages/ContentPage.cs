using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	/// <summary>
	/// Base class for content page
	/// </summary>
	[ContentProperty("Content")]
    public class ContentPage : Layout<View>, INavigationAware
    {   
        /// <summary>
        /// View states
        /// </summary>
        public enum LifecycleStates { UnLoaded, Appearing, Appeared, Dissapearing, Dissapeared }
        
        /// <summary>
        /// Event when page content is created.
        /// </summary>
        public enum ContentCreateEvents { Appearing, Appeared }
        
        protected SizeRequest _contentSize = new SizeRequest();
        protected SizeRequest _footerSize = new SizeRequest();

        private View _contentElement = null;
        private View _footerElement = null;

        /// <summary>
        /// Event when page lifecycle state changes
        /// </summary>
        public event EventHandler<LifecycleStates> LifecycleStateChanged;

        /// <summary>
        /// Event when page is reandered and ready
        /// </summary>
        public event EventHandler Rendered;

        /// <summary>
        /// Is debug text enabled
        /// </summary>
        public bool IsDebugEnabled { get; set; } = true;

		/// <summary>
		/// Navigation object (setted from XamKit.NavigationPage). Null if root page is NOT NavigationPage.
		/// </summary>
		public new INavigation Navigation { get; internal set; }
        
		#region Binding properties
        
		/// <summary>
		/// Page NavigationAnimations. Used inside NavigationPage.
		/// </summary>
		public static readonly BindableProperty NavigationAnimationGroupProperty =
			BindableProperty.Create("NavigationAnimationGroup", typeof(NavigationAnimationGroup), typeof(ContentPage), new SlideAnimationGroup());

		public NavigationAnimationGroup NavigationAnimationGroup
		{
			get { return (NavigationAnimationGroup)GetValue(NavigationAnimationGroupProperty); }
			set { SetValue(NavigationAnimationGroupProperty, value); }
		}

		/// <summary>
		/// Page content
		/// </summary>
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create("Content", typeof(object), typeof(ContentPage), null);

		public object Content
		{
			get { return (object)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		/// <summary>
		/// Page content template which creates page content
		/// </summary>
        public static readonly BindableProperty ContentTemplateProperty =
            BindableProperty.Create("ContentTemplate", typeof(DataTemplate), typeof(ContentPage), null);

        public DataTemplate ContentTemplate
		{
			get { return (DataTemplate)GetValue(ContentTemplateProperty); }
			set { SetValue(ContentTemplateProperty, value); }
		}

        /// <summary>
        /// Event when page content is created. Works only if ContentTemplate is used!
        /// </summary>
        public static readonly BindableProperty ContentCreateEventProperty =
			BindableProperty.Create("ContentCreateEvent", typeof(ContentCreateEvents), typeof(ContentPage), ContentCreateEvents.Appearing);

        public ContentCreateEvents ContentCreateEvent
		{
			get { return (ContentCreateEvents)GetValue(ContentCreateEventProperty); }
			set { SetValue(ContentCreateEventProperty, value); }
		}

		/// <summary>
		/// Page current state
		/// </summary>
		public static readonly BindableProperty LifecycleStateProperty =
			BindableProperty.Create("LifecycleState", typeof(LifecycleStates), typeof(ContentPage), LifecycleStates.UnLoaded, propertyChanged: OnLifecycleStateChanged);

		private static void OnLifecycleStateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			ContentPage p = bindable as ContentPage;

			if (p.LifecycleStateChanged != null)
			{
				p.LifecycleStateChanged(p, (LifecycleStates)newValue);
			}
        }

		public LifecycleStates LifecycleState
		{
			get { return (LifecycleStates)GetValue(LifecycleStateProperty); }
			private set { SetValue(LifecycleStateProperty, value); }
		}

        #endregion

        #region Footer

        /// <summary>
        /// Page footer
        /// </summary>
        public static readonly BindableProperty FooterProperty =
            BindableProperty.Create("Footer", typeof(object), typeof(ContentPage), null);

        public object Footer
        {
            get { return (object)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        /// <summary>
        /// Page footer template which creates page content
        /// </summary>
        public static readonly BindableProperty FooterTemplateProperty =
            BindableProperty.Create("FooterTemplate", typeof(DataTemplate), typeof(ContentPage), null);

        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate)GetValue(FooterTemplateProperty); }
            set { SetValue(FooterTemplateProperty, value); }
        }

        #endregion

        public ContentPage()
		{
            IsClippedToBounds = true;
        }

        /// <summary>
        /// Called when device back button is pressed
        /// </summary>
        /// <returns>True if back button pressed is ignored</returns>
        public virtual bool OnDeviceBackButtonPressed()
        {
            return false;
        }

        /// <summary>
        /// Called when properties changes
        /// </summary>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == ContentProperty.PropertyName)
            {
                if (Content is View content)
                {
                    if (_contentElement != null && Children.Contains(_contentElement))
                    {
                        Children.Remove(_contentElement);
                    }

                    _contentElement = content;
                    Children.Insert(0, _contentElement);
                }
                else if (_contentElement != null)
                {
                    _contentElement.BindingContext = Content;
                }
            }
            else if (propertyName == ContentTemplateProperty.PropertyName && ContentCreateEvent == ContentCreateEvents.Appearing)
            {
                if (ContentTemplate != null && Content is View == false)
                {
                    if (_contentElement != null && Children.Contains(_contentElement))
                    {
                        Children.Remove(_contentElement);
                    }

                    _contentElement = ContentTemplate.CreateContent() as View;
                    Children.Insert(0, _contentElement);

                    if (Content != null)
                    {
                        _contentElement.BindingContext = Content;
                    }
                }
            }
            else if (propertyName == FooterProperty.PropertyName)
            {
                if (Footer is View footer)
                {
                    if (_footerElement != null && Children.Contains(_footerElement))
                    {
                        Children.Remove(_footerElement);
                    }

                    _footerElement = footer;
                    Children.Insert(0, _footerElement);
                }
                else if (_footerElement != null)
                {
                    _footerElement.BindingContext = Footer;
                }
            }
            else if (propertyName == FooterTemplateProperty.PropertyName)
            {
                if (FooterTemplate != null && Footer is View == false)
                {
                    if (_footerElement != null && Children.Contains(_footerElement))
                    {
                        Children.Remove(_footerElement);
                    }

                    _footerElement = ContentTemplate.CreateContent() as View;
                    Children.Insert(0, _footerElement);

                    if (Footer != null)
                    {
                        _footerElement.BindingContext = Footer;
                    }
                }
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        #region Measure / Layout

        /// <summary>
        /// Measure whole page size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
            if (IsDebugEnabled) { Debug.WriteLine("ContentPage.OnMeasure: (" + widthConstraint + "," + heightConstraint + ") - " + NavigationBar.GetTitle(this)); }

			Size s = MeasureChildren(widthConstraint, heightConstraint);

			return new SizeRequest(s, s);
		}

		/// <summary>
		/// Layout all children
		/// </summary>
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
            if (IsDebugEnabled) { Debug.WriteLine("ContentPage.LayoutChildren: (" + width + "," + height + ") - " + NavigationBar.GetTitle(this)); }

			MeasureChildren(width, height);

            Rectangle contentLocation = new Rectangle(0, 0, width, height - _footerSize.Request.Height);

            if (_contentElement != null && _contentElement.IsVisible && Children.Contains(_contentElement) && _contentElement.Bounds != contentLocation)
            {
                LayoutChildIntoBoundingRegion(_contentElement, contentLocation);
            }

            Rectangle footerLocation = new Rectangle(0, height - _footerSize.Request.Height, width, _footerSize.Request.Height);
            if (_footerElement != null && _footerElement.IsVisible && Children.Contains(_footerElement) && _footerElement.Bounds != footerLocation)
            {
                LayoutChildIntoBoundingRegion(_footerElement, footerLocation);
            }
        }

        /// <summary>
        /// Do children measure
        /// </summary>
        private Size MeasureChildren(double width, double height)
        {
            _footerSize = new SizeRequest();
            if (_footerElement != null && Children.Contains(_footerElement) && _footerElement.IsVisible)
            {
                _footerSize = _footerElement.Measure(width, height, MeasureFlags.IncludeMargins);
            }

            if ((HorizontalOptions.Alignment == LayoutAlignment.Fill && VerticalOptions.Alignment == LayoutAlignment.Fill) ||
                (WidthRequest >= width && HeightRequest >= height))
            {
                Size s = new Size(width, height - _footerSize.Request.Height);
                _contentSize = new SizeRequest(s, s);
                return s;
            }
            else if (WidthRequest >= 0 && WidthRequest <= width && HeightRequest >= 0 && HeightRequest <= height)
            {
                Size s = new Size(WidthRequest, HeightRequest - _footerSize.Request.Height);
                _contentSize = new SizeRequest(s, s);
                return s;
            }
            else if (_contentElement != null && _contentElement.IsVisible && Children.Contains(_contentElement))
            {
                _contentSize = _contentElement.Measure(width, height - _footerSize.Request.Height, MeasureFlags.IncludeMargins);
                return _contentSize.Request;
            }
            else
            {
                return new Size(0, 0);
            }            
        }

        #endregion
        
        #region INavigationAware

        /// <summary>
        /// Page is started to appearing on the screen with animation
        /// </summary>
		public virtual void OnAppearing(AppearEventArgs args)
		{
            if (IsDebugEnabled) { Debug.WriteLine(NavigationBar.GetTitle(this) + ": OnAppearing"); }

			LifecycleState = LifecycleStates.Appearing;
		}

        /// <summary>
        /// Page is appeared to screen and animation is finished
        /// </summary>
		public virtual void OnAppeared(AppearEventArgs args)
		{
            if (IsDebugEnabled) { Debug.WriteLine(NavigationBar.GetTitle(this) + ": OnAppeared"); }

            LifecycleState = LifecycleStates.Appeared;

			if (ContentCreateEvent == ContentCreateEvents.Appeared && ContentTemplate != null && Content is View == false && _contentElement == null)
			{
                if (_contentElement != null && Children.Contains(_contentElement))
                {
                    Children.Remove(_contentElement);
                }

                _contentElement = ContentTemplate.CreateContent() as View;
                Children.Insert(0, _contentElement);

                if (Content != null)
                {
                    _contentElement.BindingContext = Content;
                }
            }
		}

        /// <summary>
        /// Page is started dissapearing on the screen with animation
        /// </summary>
		public virtual void OnDissapearing(DissapearEventArgs args)
		{
            if (IsDebugEnabled) { Debug.WriteLine(NavigationBar.GetTitle(this) + ": OnDissepearing"); }

            LifecycleState = LifecycleStates.Dissapearing;
		}

        /// <summary>
        /// Page is dissapeared from the screen and animation is finished
        /// </summary>
		public virtual void OnDissapeared(DissapearEventArgs args)
        {
            if (IsDebugEnabled) { Debug.WriteLine(NavigationBar.GetTitle(this) + ": OnDissepeared"); }

            LifecycleState = LifecycleStates.Dissapeared;
		}

        #endregion

        #region NavigationBar search

        /// <summary>
        /// Called from NavigationBar when search text changes
        /// </summary>
        public virtual void OnSearchTextChanged(TextChangedEventArgs args)
        {
            return;
        }

        #endregion

        public void OnRendered()
        {
            Rendered?.Invoke(this, new EventArgs());
        }
    }
}
