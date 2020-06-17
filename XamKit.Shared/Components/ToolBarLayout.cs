using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamKit
{
    public class ToolBarLayout : Layout<View>
    {
        private const string _openMenuAnimationName = "openMenuAnimationName";
        private const string _closeMenuAnimationName = "closeMenuAnimationName";
        private double _menuAnimationProcess;
        private double _maxModalDarknessOpacity = 0.2;

        private NavigationPage _parentNavigationPage;
        private BoxView _modalBackground;
        private ToolBar _toolBar;
        private Size _toolBarSize;

        #region Binding properties

        /// <summary>
        /// Is default items over content or is content below
        /// </summary>
        public static readonly BindableProperty IsOverContentProperty =
            BindableProperty.Create("IsOverContent", typeof(bool), typeof(ToolBarLayout), false);

        public bool IsOverContent
        {
            get { return (bool)GetValue(IsOverContentProperty); }
            set { SetValue(IsOverContentProperty, value); }
        }

        /// <summary>
        /// Modal color bottom menu opened
        /// </summary>
        public static readonly BindableProperty ModalColorProperty =
            BindableProperty.Create("ModalColor", typeof(ModalColors), typeof(ToolBarLayout), ModalColors.Black);

        public ModalColors ModalColor
        {
            get { return (ModalColors)GetValue(ModalColorProperty); }
            set { SetValue(ModalColorProperty, value); }
        }

        public static readonly BindableProperty ToolBarProperty =
            BindableProperty.Create("ToolBar", typeof(ToolBar), typeof(ToolBarLayout), null);

        public ToolBar ToolBar
        {
            get { return (ToolBar)GetValue(ToolBarProperty); }
            set { SetValue(ToolBarProperty, value); }
        }

        #endregion

        public ToolBarLayout()
        {
            _modalBackground = new BoxView();
            _modalBackground.BackgroundColor = ModalColor == ModalColors.Black ? Color.Black : Color.White;
            _modalBackground.InputTransparent = true;
            _modalBackground.Opacity = 0;           
            Children.Add(_modalBackground);

            TapGestureRecognizer tapped = new TapGestureRecognizer();
            tapped.Tapped += OnModalBackgroundTapped;
            _modalBackground.GestureRecognizers.Add(tapped);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == ToolBarProperty.PropertyName)
            {
                if (_toolBar != null)
                {
                    _toolBar.IsMenuOpenChanged -= OnIsToolBarMenuOpenChanged;
                    Children.Remove(_toolBar);
                }

                if (ToolBar != null)
                {
                    _toolBar = ToolBar;
                    _toolBar.IsMenuOpenChanged += OnIsToolBarMenuOpenChanged;
                    Children.Add(_toolBar);
                }
            }
            else if (propertyName == ModalColorProperty.PropertyName)
            {
                if (_modalBackground != null)
                {
                    _modalBackground.BackgroundColor = ModalColor == ModalColors.Black ? Color.Black : Color.White;
                }
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (HorizontalOptions.Alignment != LayoutAlignment.Fill || VerticalOptions.Alignment != LayoutAlignment.Fill)
            {
                throw new Exception("ToolBarLayout must have horizontal and vertical alingment Fill");
            }

            Size size = new Size(widthConstraint, heightConstraint);

            return new SizeRequest(size, size);
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            RaiseChild(_modalBackground);
            LayoutChildIntoBoundingRegion(_modalBackground, new Rectangle(0, 0, width, height));

            if (_toolBar != null)
            {
                _toolBar.BottomMenuMaxHeight = height / 2;
                _toolBarSize = _toolBar.Measure(width, height, MeasureFlags.IncludeMargins).Request;
                RaiseChild(_toolBar);
                LayoutChildIntoBoundingRegion(_toolBar, new Rectangle(0, height - _toolBarSize.Height, width, _toolBarSize.Height));
            }

            foreach (View child in Children)
            {
                if (child is ToolBar == false)
                {
                    if (IsOverContent)
                    {
                        LayoutChildIntoBoundingRegion(child, new Rectangle(0, 0, width, height));
                    }
                    else
                    {
                        LayoutChildIntoBoundingRegion(child, new Rectangle(0, 0, width, height - _toolBarSize.Height));
                    }
                }
            }
        }

        private void OnIsToolBarMenuOpenChanged(object sender, bool e)
        {
            if (_toolBar.IsMenuOpen)
            {
                this.AbortAnimation(_closeMenuAnimationName);

                if (_parentNavigationPage == null)
                {
                    _parentNavigationPage = GetParent<NavigationPage>(this);
                }
                if (_parentNavigationPage != null)
                {
                    _parentNavigationPage.NavigationBarModalDarknessLayerTapped += OnNavigationPageNavigationBarModalDarknessLayerTapped;
                }

                _modalBackground.InputTransparent = false;

                Animation showBottomMenuAnim = new Animation(d =>
                {
                    _menuAnimationProcess = d;

                    if (_parentNavigationPage != null)
                    {
                        _parentNavigationPage.SetNavigationBarModalDarkness(_menuAnimationProcess * _maxModalDarknessOpacity, ModalColor);
                    }
                    if (IsOverContent == false)
                    {
                        LayoutChildren(0, 0, Width, Height);
                    }

                    _modalBackground.Opacity = _maxModalDarknessOpacity * _menuAnimationProcess;

                }, _menuAnimationProcess, 1);

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                showBottomMenuAnim.Commit(this, _openMenuAnimationName, 64, (uint)_toolBar.ShowMenuDuration, Easing.Linear, finished: (p, a) =>
                {
                    tcs.SetResult(true);
                });
            }
            else
            {
                this.AbortAnimation(_openMenuAnimationName);

                if (_parentNavigationPage != null)
                {
                    _parentNavigationPage.NavigationBarModalDarknessLayerTapped -= OnNavigationPageNavigationBarModalDarknessLayerTapped;
                }

                _modalBackground.InputTransparent = true;

                Animation hideBottomMenuAnim = new Animation(d =>
                {
                    _menuAnimationProcess = d;

                    if (_parentNavigationPage != null)
                    {
                        _parentNavigationPage.SetNavigationBarModalDarkness(_menuAnimationProcess * _maxModalDarknessOpacity, ModalColor);
                    }
                    if (IsOverContent == false)
                    {
                        LayoutChildren(0, 0, Width, Height);
                    }

                    _modalBackground.Opacity = _maxModalDarknessOpacity * _menuAnimationProcess;

                }, _menuAnimationProcess, 0);

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                hideBottomMenuAnim.Commit(this, _closeMenuAnimationName, 64, (uint)_toolBar.HideMenuDuration, Easing.Linear, finished: (p, a) =>
                {
                    tcs.SetResult(true);
                });
            }
        }

        private void OnNavigationPageNavigationBarModalDarknessLayerTapped(object sender, EventArgs e)
        {
            _toolBar.IsMenuOpen = false;
        }

        private void OnModalBackgroundTapped(object sender, EventArgs e)
        {
            _toolBar.IsMenuOpen = false;
        }

        /// <summary>
        /// Get parent view by type
        /// </summary>
        private static T GetParent<T>(View source) where T : View
        {
            View p = source.Parent as View;

            while (p != null)
            {
                if (p is T p2)
                {
                    return p2;
                }

                p = p.Parent as View;
            }

            return null;
        }
    }

    /// <summary>
    /// ToolBar visibility when it is empty
    /// </summary>
    public enum ToolBarVisibilityModes { Auto, Hidden, Visible }
}
