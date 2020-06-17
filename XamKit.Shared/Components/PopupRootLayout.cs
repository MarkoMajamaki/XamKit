using System;
using System.Runtime.CompilerServices;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    [ContentProperty("Content")]
    public class PopupRootLayout : Layout<View>
    {
        private Border _border = null;
        private ShadowView _shadowBorder = null;
        private Container _contentContainer = null;

        #region Binding properties

        /// <summary>
        /// Layout content
        /// </summary>
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create("Content", typeof(View), typeof(PopupRootLayout), null);

        public View Content
        {
            get { return (View)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Owner popup
        /// </summary>
        public static readonly BindableProperty PopupProperty =
            BindableProperty.Create("Popup", typeof(Popup), typeof(PopupRootLayout), null);

        public Popup Popup
        {
            get { return (Popup)GetValue(PopupProperty); }
            set { SetValue(PopupProperty, value); }
        }

        #endregion

        public PopupRootLayout()
        {
            _shadowBorder = CreateShadowBorder();
            Children.Add(_shadowBorder);

            _border = CreateBorder();
            Children.Add(_border);

            // Create container for content
            _contentContainer = new Container();
            CompressedLayout.SetIsHeadless(_contentContainer, true);
            _border.Content = _contentContainer;
        }

        /// <summary>
        /// Clip popup to location related to popup
        /// </summary>
        internal void Clip(Rectangle location)
        {
            foreach (View child in Children)
            {
                LayoutChildIntoBoundingRegion(child, location);
            }
        }

        /// <summary>
        /// Measure children total size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return _contentContainer.Measure(Math.Min(widthConstraint, Popup.MaxWidthRequest), Math.Max(heightConstraint, Popup.MaxHeightRequest), MeasureFlags.IncludeMargins);
        }

        /// <summary>
        /// Layout all children to max size
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            foreach (View child in Children)
            {
                LayoutChildIntoBoundingRegion(child, new Rectangle(x, y, width, height));
            }
        }

        /// <summary>
        /// Handle properties changes
        /// </summary>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == ContentProperty.PropertyName)
            {
                if (_contentContainer != null)
                {
                    _contentContainer.Children.Clear();
                    _contentContainer.Children.Add(Content);
                }
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Create border which clip child container
        /// </summary>
        private Border CreateBorder()
        {
            Border border = new Border();

            Binding bind = new Binding("Popup.BorderColor");
            bind.Source = this;
            border.SetBinding(Border.BorderColorProperty, bind);

            bind = new Binding("Popup.CornerRadius");
            bind.Source = this;
            border.SetBinding(Border.CornerRadiusProperty, bind);

            bind = new Binding("Popup.BorderThickness");
            bind.Source = this;
            border.SetBinding(Border.BorderThicknessProperty, bind);

            border.HorizontalOptions = LayoutOptions.Center;
            border.VerticalOptions = LayoutOptions.Center;

            return border;
        }

        /// <summary>
        /// Create shadow border which is behind child container
        /// </summary>
        private ShadowView CreateShadowBorder()
        {
            ShadowView shadowView = new ShadowView();

            Binding bind = new Binding("Popup.BackgroundColor");
            bind.Source = this;
            shadowView.SetBinding(ShadowView.BorderBackgroundColorProperty, bind);

            bind = new Binding("Popup.CornerRadius");
            bind.Source = this;
            shadowView.SetBinding(ShadowView.CornerRadiusProperty, bind);

            bind = new Binding("Popup.ShadowColor");
            bind.Source = this;
            shadowView.SetBinding(ShadowView.ShadowColorProperty, bind);

            bind = new Binding("Popup.ShadowOpacity");
            bind.Source = this;
            shadowView.SetBinding(ShadowView.ShadowOpacityProperty, bind);

            bind = new Binding("Popup.ShadowLenght");
            bind.Source = this;
            shadowView.SetBinding(ShadowView.ShadowLenghtProperty, bind);

            bind = new Binding("Popup.IsShadowEnabled");
            bind.Source = this;
            shadowView.SetBinding(ShadowView.IsShadowEnabledProperty, bind);

            return shadowView;
        }

        /// <summary>
        /// Container for resize animation optimization
        /// </summary>
        private class Container : Layout<View>
        {
            private Size _childSize = new Size();

            /// <summary>
            /// Measure all child based on popup layout size NOT container available size
            /// </summary>
            protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
            {
                Size totalSize = new Size();

                PopupLayout popupLayout = VisualTreeHelper.GetParent<PopupLayout>(this);

                foreach (View child in Children)
                {
                    if (child.HorizontalOptions.Alignment == LayoutAlignment.Fill && child.VerticalOptions.Alignment == LayoutAlignment.Fill)
                    {
                        totalSize = new Size(popupLayout.Width, popupLayout.Height);
                        _childSize = totalSize;
                        break;
                    }

                    _childSize = child.Measure(popupLayout.Width, popupLayout.Height, MeasureFlags.IncludeMargins).Request;

                    if (child.HorizontalOptions.Alignment == LayoutAlignment.Fill)
                    {
                        totalSize.Width = Math.Max(popupLayout.Width, _childSize.Width);
                    }
                    else
                    {
                        totalSize.Width = Math.Max(totalSize.Width, _childSize.Width);
                    }

                    if (child.VerticalOptions.Alignment == LayoutAlignment.Fill)
                    {
                        totalSize.Height = Math.Max(totalSize.Height, popupLayout.Height);
                    }
                    else
                    {
                        totalSize.Height = Math.Max(totalSize.Height, _childSize.Height);
                    }
                }

                return new SizeRequest(totalSize, totalSize);
            }

            /// <summary>
            /// Layout all children based on available size NOT container size
            /// </summary>
            protected override void LayoutChildren(double x, double y, double width, double height)
            {
                foreach (View child in Children)
                {
                    LayoutChildIntoBoundingRegion(child, new Rectangle(0, 0, _childSize.Width, _childSize.Height));
                }
            }
        }
    }
}
