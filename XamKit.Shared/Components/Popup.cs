using System;
using System.Collections.Generic;
using System.Linq;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public class Popup : View
    {
        private static List<Popup> _openPopups = new List<Popup>();

        private const string _openingAnimationName = "openingAnimationName";
        private const string _closingAnimationName = "closingAnimationName";
        
        private Animation _closingAnimationGroup = null;
        private Animation _openingAnimationGroup = null;
        
        // Panel where popup is added
        private PopupRootLayout _popupRootLayout = null;
        private View _actualContent = null;

        /// <summary>
        /// Raised when 'IsOpen' value changes
        /// </summary>
        public event EventHandler<bool> IsOpenChanged;

        /// <summary>
        /// Raised when open / close animation is finished
        /// </summary>
        public event EventHandler<bool> AnimationFinished;

        /// <summary>
        /// Raise when ActualPlacement changes
        /// </summary>
        public event EventHandler<PopupPlacements> ActualPlacementChanged;

        /// <summary>
        /// Popups layout for app
        /// </summary>
        public static PopupLayout PopupLayout { get; set; }

        public static bool IsAnyPopupOpen
        {
            get
            {
                return _openPopups.Count > 0;
            }
        }

        #region Properties

        /// <summary>
        /// Popup content. If view, then add it to popup content. If not, then create content from 'ContentTemplate' and 
        /// add this to it's BindingContext.
        /// </summary>
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create("Content", typeof(object), typeof(Popup), null);

        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Template to create content
        /// </summary>
        public static readonly BindableProperty ContentTemplateProperty =
            BindableProperty.Create("ContentTemplate", typeof(DataTemplate), typeof(Popup), default(DataTemplate));

        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        /// <summary>
        /// Space between placement target and popup
        /// </summary>
        public static readonly BindableProperty SpacingProperty =
            BindableProperty.Create("Spacing", typeof(double), typeof(Popup), 0.0);

        public double Spacing
        {
            get { return (double)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        /// <summary>
        /// Popup placement
        /// </summary>
        public static readonly BindableProperty PlacementProperty =
            BindableProperty.Create("Placement", typeof(PopupPlacements), typeof(Popup), PopupPlacements.TopLeft);

        public PopupPlacements Placement
        {
            get { return (PopupPlacements)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        /// <summary>
        /// Actual popup placement which depends available space. Reverse to Placement if no enought available space.
        /// </summary>
        public static readonly BindableProperty ActualPlacementProperty =
            BindableProperty.Create("ActualPlacement", typeof(PopupPlacements), typeof(Popup), PopupPlacements.TopLeft, propertyChanged: OnActualPlacementChanged);

        private static void OnActualPlacementChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((Popup)bindable).OnActualPlacementChanged((PopupPlacements)oldValue, (PopupPlacements)newValue);
        }

        public PopupPlacements ActualPlacement
        {
            get { return (PopupPlacements)GetValue(ActualPlacementProperty); }
            set { SetValue(ActualPlacementProperty, value); }
        }

        /// <summary>
        /// Placement location inside placement target
        /// </summary>
        public static readonly BindableProperty PlacementRectangleProperty =
            BindableProperty.Create("PlacementRectangle", typeof(Rectangle), typeof(Popup), null);

        public Rectangle PlacementRectangle
        {
            get { return (Rectangle)GetValue(PlacementRectangleProperty); }
            set { SetValue(PlacementRectangleProperty, value); }
        }

        /// <summary>
        /// Popup placement target element
        /// </summary>
        public static readonly BindableProperty PlacementTargetProperty =
            BindableProperty.Create("PlacementTarget", typeof(View), typeof(Popup), null, propertyChanged: OnPlacementChanged);

        private static void OnPlacementChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((Popup)bindable).OnInternalPlacementTargetChanged(oldValue as View, newValue as View);
        }

        public View PlacementTarget
        {
            get { return (View)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

        /// <summary>
        /// Extra horizontal offset
        /// </summary>
        public static readonly BindableProperty HorizontalOffsetProperty =
            BindableProperty.Create("HorizontalOffset", typeof(double), typeof(Popup), 0.0);

        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        /// <summary>
        /// Extra vertical offset
        /// </summary>
        public static readonly BindableProperty VerticalOffsetProperty =
            BindableProperty.Create("VerticalOffset", typeof(double), typeof(Popup), 0.0);

        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        /// <summary>
        /// Popup min width
        /// </summary>
        public static readonly BindableProperty MinWidthRequestProperty =
            BindableProperty.Create("MinWidthRequest", typeof(double), typeof(Popup), 0.0);

        public double MinWidthRequest
        {
            get { return (double)GetValue(MinWidthRequestProperty); }
            set { SetValue(MinWidthRequestProperty, value); }
        }

        /// <summary>
        /// Popup min height
        /// </summary>
        public static readonly BindableProperty MinHeightRequestProperty =
            BindableProperty.Create("MinHeightRequest", typeof(double), typeof(Popup), 0.0);

        public double MinHeightRequest
        {
            get { return (double)GetValue(MinHeightRequestProperty); }
            set { SetValue(MinHeightRequestProperty, value); }
        }

        /// <summary>
        /// Popup max width
        /// </summary>
        public static readonly BindableProperty MaxWidthRequestProperty =
            BindableProperty.Create("MaxWidthRequest", typeof(double), typeof(Popup), double.MaxValue);

        public double MaxWidthRequest
        {
            get { return (double)GetValue(MaxWidthRequestProperty); }
            set { SetValue(MaxWidthRequestProperty, value); }
        }

        /// <summary>
        /// Popup max height
        /// </summary>
        public static readonly BindableProperty MaxHeightRequestProperty =
            BindableProperty.Create("MaxHeightRequest", typeof(double), typeof(Popup), double.MaxValue);

        public double MaxHeightRequest
        {
            get { return (double)GetValue(MaxHeightRequestProperty); }
            set { SetValue(MaxHeightRequestProperty, value); }
        }

        /// <summary>
        /// Is popup open
        /// </summary>
        public static readonly BindableProperty IsOpenProperty =
            BindableProperty.Create("IsOpen", typeof(bool), typeof(Popup), false, propertyChanged: OnIsOpenChanged);

        private static void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((Popup)bindable).OnIsOpenChanged((bool)oldValue, (bool)newValue);
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Is app background grayed when opened
        /// </summary>
        public static readonly BindableProperty HasModalBackgroundProperty =
            BindableProperty.Create("HasModalBackground", typeof(bool), typeof(Popup), true);

        public bool HasModalBackground
        {
            get { return (bool)GetValue(HasModalBackgroundProperty); }
            set { SetValue(HasModalBackgroundProperty, value); }
        }
        
        /// <summary>
        /// Is popup arrow enabled
        /// </summary>
        public static readonly BindableProperty IsArrowEnabledProperty =
            BindableProperty.Create("IsArrowEnabled", typeof(bool), typeof(Popup), false);

        public bool IsArrowEnabled
        {
            get { return (bool)GetValue(IsArrowEnabledProperty); }
            set { SetValue(IsArrowEnabledProperty, value); }
        }
        
        /// <summary>
        /// Popup corner radius
        /// </summary>
        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create("CornerRadius", typeof(double), typeof(Popup), 0.0);

        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Popup border thickness
        /// </summary>
        public static readonly BindableProperty BorderThicknessProperty =
            BindableProperty.Create("BorderThickness", typeof(double), typeof(Popup), 0.0);

        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        #endregion

        #region Animations

        /// <summary>
        /// Popup opening animation
        /// </summary>
        public static readonly BindableProperty OpeningAnimationProperty =
            BindableProperty.Create("OpeningAnimation", typeof(IAnimation), typeof(Popup), null);

        public IAnimation OpeningAnimation
        {
            get { return (IAnimation)GetValue(OpeningAnimationProperty); }
            set { SetValue(OpeningAnimationProperty, value); }
        }

        /// <summary>
        /// Popup closing animation
        /// </summary>
        public static readonly BindableProperty ClosingAnimationProperty =
            BindableProperty.Create("ClosingAnimation", typeof(IAnimation), typeof(Popup), null);

        public IAnimation ClosingAnimation
        {
            get { return (IAnimation)GetValue(ClosingAnimationProperty); }
            set { SetValue(ClosingAnimationProperty, value); }
        }

        #endregion

        #region Shadow

        /// <summary>
        /// Is shadow enabled
        /// </summary>
        public static readonly BindableProperty IsShadowEnabledProperty =
            BindableProperty.Create("IsShadowEnabled", typeof(bool), typeof(Popup), true);

        public bool IsShadowEnabled
        {
            get { return (bool)GetValue(IsShadowEnabledProperty); }
            set { SetValue(IsShadowEnabledProperty, value); }
        }

        /// <summary>
        /// Shadow Y offset
        /// </summary>
        public static readonly BindableProperty ShadowLenghtProperty =
            BindableProperty.Create("ShadowLenght", typeof(double), typeof(Popup), 0.0);

        public double ShadowLenght
        {
            get { return (double)GetValue(ShadowLenghtProperty); }
            set { SetValue(ShadowLenghtProperty, value); }
        }

        #endregion

        #region Colors

        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create("BorderColor", typeof(Color), typeof(Popup), Color.Transparent);

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public static readonly BindableProperty ShadowColorProperty =
            BindableProperty.Create("ShadowColor", typeof(Color), typeof(Popup), Color.LightGray);

        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }

        public static readonly BindableProperty ShadowOpacityProperty =
            BindableProperty.Create("ShadowOpacity", typeof(double), typeof(Popup), 1.0);

        public double ShadowOpacity
        {
            get { return (double)GetValue(ShadowOpacityProperty); }
            set { SetValue(ShadowOpacityProperty, value); }
        }

        #endregion

        public Popup()
        {
            _popupRootLayout = new PopupRootLayout();
            _popupRootLayout.HorizontalOptions = LayoutOptions.Center;
            _popupRootLayout.VerticalOptions = LayoutOptions.Center;
            _popupRootLayout.Popup = this;
        }

        /// <summary>
        /// Close all open popups
        /// </summary>
        public static void CloseAll()
        {
            foreach (Popup popup in _openPopups.ToList())
            {
                popup.IsOpen = false;
            }
        }

        /// <summary>
        /// Popup opened
        /// </summary>
        protected virtual void OnOpened()
        {
            return;
        }

        /// <summary>
        /// Popup closed
        /// </summary>
        protected virtual void OnClosed()
        {
            return;
        }

        /// <summary>
        /// Popup placement target changed
        /// </summary>
        protected virtual void OnPlacementTargetChanged(View oldValue, View newValue)
        {
            return;
        }

        /// <summary>
        /// Close popup when modal background tapped
        /// </summary>
        private void OnModalAreaTapped(object sender, EventArgs e)
        {
            if (_openPopups != null && _openPopups.Count > 0)
            {
                for (int i = 0; i < _openPopups.Count; i++)
                {
                    Popup p = _openPopups[0];
                    p.IsOpen = false;
                }
            }

            _openPopups.Clear();
        }

        /// <summary>
        /// Called when 'PlacementTarget' value changes
        /// </summary>
        private void OnInternalPlacementTargetChanged(View oldView, View newView)
        {
            if (_popupRootLayout != null)
            {
                Binding bind = new Binding("PlacementTarget.DataContext");
                bind.Mode = BindingMode.OneWay;
                bind.Source = this;
                _popupRootLayout.SetBinding(View.BindingContextProperty, bind);
            }

            OnPlacementTargetChanged(oldView, newView);
        }

        /// <summary>
        /// Called when popup IsOpened changes
        /// </summary>
        private void OnIsOpenChanged(bool oldValue, bool newValue)
        {
            if (PlacementTarget == null || Content == null)
            {
                throw new Exception("PlacementTarget is null");
            }

            if (PopupLayout == null)
            {
                throw new Exception("PopupLayout is null. Set app popup layout when app start.");
            }

            this.AbortAnimation(_openingAnimationName);
            this.AbortAnimation(_closingAnimationName);

            if (newValue)
            {
                // If actual content is not created
                if (_actualContent == null)
                {
                    if (Content is View content)
                    {
                        _actualContent = content;
                    }
                    else if (ContentTemplate != null)
                    {
                        _actualContent = ContentTemplate.CreateContent() as View;

                        if (Content != null)
                        {
                            Binding bind = new Binding("Content");
                            bind.Source = this;
                            bind.Mode = BindingMode.OneWay;
                            _actualContent.SetBinding(View.BindingContextProperty, bind);
                        }
                    }
                }

                if (_popupRootLayout.Content != _actualContent)
                {
                    _popupRootLayout.Content = _actualContent;
                }

                _openPopups.Add(this);

                InitializeForOpeningAnimation();
                SetContentLayoutOptions(Placement);

                // Create opening animation
                _openingAnimationGroup = CreateOpeningAnimation();

                if (PopupLayout.Children.Contains(_popupRootLayout) == false)
                {
                    // Add popup to layout
                    PopupLayout.Children.Add(_popupRootLayout);
                }

                if (_openingAnimationGroup != null)
                {
                    _openingAnimationGroup.Commit(this, _openingAnimationName, 64, (uint)OpeningAnimation.Duration, Easing.Linear, (double p, bool isAborted) =>
                    {
                        AnimationFinished?.Invoke(this, IsOpen);
                    });
                }
                else
                {
                    AnimationFinished?.Invoke(this, IsOpen);
                }

                IsOpenChanged?.Invoke(this, IsOpen);
                OnOpened();
            }
            else
            {
                _openPopups.Remove(this);

                // Create closing animation
                _closingAnimationGroup = CreateClosingAnimation();

                if (_closingAnimationGroup != null)
                {
                    _closingAnimationGroup.Commit(this, _closingAnimationName, 64, (uint)ClosingAnimation.Duration, Easing.Linear, (arg1, arg2) =>
                    {
                        if (arg2 == false)
                        {
                            PopupLayout.Children.Remove(_popupRootLayout);
                        }

                        AnimationFinished?.Invoke(this, IsOpen);
                    });
                }
                else
                {
                    PopupLayout.Children.Remove(_popupRootLayout);
                    AnimationFinished?.Invoke(this, IsOpen);
                }

                IsOpenChanged?.Invoke(this, IsOpen);
                OnClosed();
            }
        }

        #region Popup location

        /// <summary>
        /// Is popup on top
        /// </summary>
        public static bool IsTopPlacement(PopupPlacements placement)
        {
            return placement == PopupPlacements.TopCenter || placement == PopupPlacements.TopLeft || placement == PopupPlacements.TopRight;
        }

        /// <summary>
        /// Is popup on right
        /// </summary>
        public static bool IsRightPlacement(PopupPlacements placement)
        {
            return placement == PopupPlacements.RightBottom || placement == PopupPlacements.RightCenter || placement == PopupPlacements.RightTop;
        }

        private void OnActualPlacementChanged(PopupPlacements oldPlacement, PopupPlacements newPlacement)
        {
            SetContentLayoutOptions(newPlacement);

            // Raise placement change event
            ActualPlacementChanged?.Invoke(this, newPlacement);
        }

        /// <summary>
        /// Set popup content horizontal options based on popup placement
        /// </summary>
        private void SetContentLayoutOptions(PopupPlacements placement)
        {
            if (placement == PopupPlacements.BottomStretch || placement == PopupPlacements.TopStretch)
            {
                _popupRootLayout.Content.HorizontalOptions = LayoutOptions.Fill;
                _popupRootLayout.Content.VerticalOptions = LayoutOptions.Center;
            }
            else if (placement == PopupPlacements.FullScreen)
            {
                _popupRootLayout.Content.HorizontalOptions = LayoutOptions.Fill;
                _popupRootLayout.Content.VerticalOptions = LayoutOptions.Fill;
            }
            else
            {
                _popupRootLayout.Content.HorizontalOptions = LayoutOptions.Center;
                _popupRootLayout.Content.VerticalOptions = LayoutOptions.Center;
            }
        }

        #endregion

        #region Animations

        /// <summary>
        /// Set properties to default values before animation
        /// </summary>
        private void InitializeForOpeningAnimation()
        {
            _actualContent.Opacity = 1;
            _actualContent.TranslationX = 0;
            _actualContent.TranslationY = 0;
            _actualContent.ScaleX = 1;
            _actualContent.ScaleY = 1;
            _actualContent.Scale = 1;
            _actualContent.IsVisible = true;
            _popupRootLayout.Opacity = 1;
            _popupRootLayout.TranslationX = 0;
            _popupRootLayout.TranslationY = 0;
            _popupRootLayout.IsVisible = true;
            _popupRootLayout.ScaleX = 1;
            _popupRootLayout.ScaleY = 1;
            _popupRootLayout.Scale = 1;
        }

        /// <summary>
        /// Create opening animations for modal background and popup
        /// </summary>
        private Animation CreateOpeningAnimation()
        {
            Animation animationGroup = new Animation();

            if (OpeningAnimation != null)
            {
                if (OpeningAnimation is IAnimation popupAnimation)
                {
                    animationGroup.Add(0, 1, popupAnimation.Create(_popupRootLayout));
                }
                else
                {
                    animationGroup.Add(0, 1, OpeningAnimation.Create(_popupRootLayout));
                }

                Animation backgroundAnimation = PopupLayout.CreateModalBackgroundAnimation(true);
                if (backgroundAnimation != null && HasModalBackground)
                {
                    animationGroup.Add(0, 1, backgroundAnimation);
                }
            }

            if (animationGroup.HasSubAnimations())
            {
                return animationGroup;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Create closing animations for modal background and popup
        /// </summary>
        private Animation CreateClosingAnimation()
        {
            Animation animationGroup = new Animation();

            if (ClosingAnimation != null)
            {
                if (ClosingAnimation is IAnimation popupAnimation)
                {
                    animationGroup.Add(0, 1, popupAnimation.Create(_popupRootLayout));
                }
                else
                {
                    animationGroup.Add(0, 1, ClosingAnimation.Create(_popupRootLayout));
                }

                Animation modalAnim = PopupLayout.CreateModalBackgroundAnimation(false);
                if (ClosingAnimation.Duration > 0 && modalAnim != null && HasModalBackground)
                {
                    animationGroup.Add(0, 1, modalAnim);
                }
            }

            if (animationGroup.HasSubAnimations())
            {
                return animationGroup;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }

    public enum PopupPlacements
    {
        TopLeft,
        TopRight,
        TopCenter,
        TopStretch,
        BottomLeft,
        BottomRight,
        BottomCenter,
        BottomStretch,
        LeftCenter,
        LeftTop,
        LeftBottom,
        RightCenter,
        RightTop,
        RightBottom,
        FullScreen
    }
}

