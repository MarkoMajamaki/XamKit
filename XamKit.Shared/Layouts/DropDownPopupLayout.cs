using System;
using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XamKit
{
    public class DropDownPopupLayout : Layout<View>
    {
        private SizeRequest m_navigationBarSizeRequest = new SizeRequest();
		private SizeRequest m_headerSizeRequest = new SizeRequest();
		private SizeRequest m_footerSizeRequest = new SizeRequest();
		private SizeRequest m_contentSizeRequest = new SizeRequest();

		#region Property

        public static readonly BindableProperty NavigationBarProperty =
            BindableProperty.Create("NavigationBar", typeof(NavigationBar), typeof(DropDownPopupLayout), null, propertyChanged: OnNavigationBarChanged);

        private static void OnNavigationBarChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DropDownPopupLayout c = bindable as DropDownPopupLayout;

            View oldView = oldValue as View;
            if (oldView != null)
            {
                c.Children.Remove(oldView);
            }
            
            View newView = newValue as View;
            if (newView != null)
            {
                c.Children.Add(newView);
            }
        }

        public NavigationBar NavigationBar
        {
            get { return (NavigationBar)GetValue(NavigationBarProperty); }
            set { SetValue(NavigationBarProperty, value); }
        }

		public static readonly BindableProperty HeaderProperty =
            BindableProperty.Create("Header", typeof(View), typeof(DropDownPopupLayout), null, propertyChanged: OnHeaderChanged);

        private static void OnHeaderChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DropDownPopupLayout c = bindable as DropDownPopupLayout;

			View oldView = oldValue as View;
            if (oldView != null)
            {
				c.Children.Remove(oldView);
			}
			
            View newView = newValue as View;
			if (newView != null)
			{
				c.Children.Add(newView);
			}
        }

        public View Header
        {
            get { return (View)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly BindableProperty FooterProperty =
            BindableProperty.Create("Footer", typeof(View), typeof(DropDownPopupLayout), null, propertyChanged: OnFooterChanged);

		private static void OnFooterChanged(BindableObject bindable, object oldValue, object newValue)
		{
            DropDownPopupLayout c = bindable as DropDownPopupLayout;

			View oldView = oldValue as View;
			if (oldView != null)
			{
				c.Children.Remove(oldView);
			}

			View newView = newValue as View;
			if (newView != null)
			{
				c.Children.Add(newView);
			}
        }

		public View Footer
        {
            get { return (View)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create("Content", typeof(View), typeof(DropDownPopupLayout), null, propertyChanged: OnContentChanged);

        private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DropDownPopupLayout c = bindable as DropDownPopupLayout;

			View oldView = oldValue as View;
			if (oldView != null)
			{
				c.Children.Remove(oldView);
			}

			View newView = newValue as View;
			if (newView != null)
			{
				c.Children.Insert(0, newView);
			}
        }

        public View Content
        {
            get { return (View)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        #endregion

        public DropDownPopupLayout()
        {
            IsClippedToBounds = true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Size s = new Size();

            if (NavigationBar != null && NavigationBar.IsVisible)
            {
                m_navigationBarSizeRequest = NavigationBar.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
            }
            else
            {
                m_navigationBarSizeRequest = new SizeRequest();
            }

            if (Header != null && Header.IsVisible)
            {
                m_headerSizeRequest = Header.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
            }
            else
            {
                m_headerSizeRequest = new SizeRequest();
            }

            if (Footer != null && Footer.IsVisible)
			{
				m_footerSizeRequest = Footer.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
			}
            else
            {
                m_footerSizeRequest = new SizeRequest();
			}

			if (Content != null)
			{
                m_contentSizeRequest = Content.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
			}
            else
            {
				m_contentSizeRequest = new SizeRequest();
			}

            if (HorizontalOptions.Alignment == LayoutAlignment.Fill && double.IsInfinity(widthConstraint) == false)
            {
                s.Width = widthConstraint;
            }
            else
            {
                s.Width = m_contentSizeRequest.Request.Width;
            }

            if (VerticalOptions.Alignment == LayoutAlignment.Fill && double.IsInfinity(heightConstraint) == false)
			{
                s.Height = heightConstraint;
			}
            else
            {
                s.Height = m_contentSizeRequest.Request.Height + m_headerSizeRequest.Request.Width + m_navigationBarSizeRequest.Request.Height;
			}

            return new SizeRequest(s, s);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            var i = Children;

            if (NavigationBar != null)
			{
                LayoutChildIntoBoundingRegion(NavigationBar, new Rectangle(0, 0, width, m_navigationBarSizeRequest.Request.Height));
			}

			if (Header != null)
			{
                LayoutChildIntoBoundingRegion(Header, new Rectangle(0, m_navigationBarSizeRequest.Request.Height, width, m_headerSizeRequest.Request.Height));
			}

			if (Footer != null)
			{
                LayoutChildIntoBoundingRegion(Footer, new Rectangle(0, height - m_footerSizeRequest.Request.Height, width, m_footerSizeRequest.Request.Height));
			}

			if (Content != null)
			{
                LayoutChildIntoBoundingRegion(Content, new Rectangle(0, m_navigationBarSizeRequest.Request.Height + m_headerSizeRequest.Request.Height, width, height - (m_navigationBarSizeRequest.Request.Height + m_headerSizeRequest.Request.Height)));
			}
        }
    }
}
