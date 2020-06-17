using System;
using System.Windows.Input;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public interface IPopupLayout
    {
        PopupLayout PopupLayout { get; }
    }

    /// <summary>
    /// Layout for application main page for popups
    /// </summary>
    public class PopupLayout : Layout<View>
    {
        private BoxView _background = null;

        private double _backgroundAnimationProcess = 0;

        // Modal background max opacity
        private const double _modalBackgroundOpacity = 0.2;

        #region Properties

        public static readonly BindableProperty IsModalBackgroundEnabledProperty =
            BindableProperty.Create("IsModalBackgroundEnabled", typeof(bool), typeof(PopupLayout), true, propertyChanged: OnIsModalBackgroundEnabledChanged);

        private static void OnIsModalBackgroundEnabledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as PopupLayout).OnIsModalBackgroundEnabledChanged((bool)newValue);
        }

        public bool IsModalBackgroundEnabled
        {
            get { return (bool)GetValue(IsModalBackgroundEnabledProperty); }
            set { SetValue(IsModalBackgroundEnabledProperty, value); }
        }

        public static readonly BindableProperty BackgroundCommandProperty =
            BindableProperty.Create("BackgroundCommand", typeof(ICommand), typeof(PopupLayout), null);

        public ICommand BackgroundCommand
        {
            get { return (ICommand)GetValue(BackgroundCommandProperty); }
            set { SetValue(BackgroundCommandProperty, value); }
        }

        #endregion

        public PopupLayout()
        {
            _background = CreateModalBackground();
            Children.Add(_background);
            InputTransparent = true;
        }

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);

            if (Children.Count > 1)
            {
                InputTransparent = false;
            }
        }

        protected override void OnChildRemoved(Element child)
        {
            base.OnChildRemoved(child);

            if (Children.Count <= 1)
            {
                InputTransparent = true;
            }
        }

        /// <summary>
        /// Set modal background visibility / opacity using animation.
        /// </summary>
        public Animation CreateModalBackgroundAnimation(bool isVisible)
        {
            Animation anim = new Animation();

            if (isVisible && IsModalBackgroundEnabled)
            {
                if (_backgroundAnimationProcess == 1)
                {
                    return null;
                }

                anim = new Animation(d =>
                {
                    _backgroundAnimationProcess = d;
                    _background.Color = AnimationUtils.ColorTransform(d, Color.Transparent, Color.Black.MultiplyAlpha(_modalBackgroundOpacity));
                }, _backgroundAnimationProcess, 1, Easing.Linear);
            }
            else
            {
                if (_backgroundAnimationProcess == 0)
                {
                    return null;
                }

                anim = new Animation(d =>
                {
                    _backgroundAnimationProcess = d;
                    _background.Color = AnimationUtils.ColorTransform(d, Color.Transparent, Color.Black.MultiplyAlpha(_modalBackgroundOpacity));
                }, _backgroundAnimationProcess, 0, Easing.Linear);
            }

            return anim;
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (double.IsNaN(widthConstraint) || double.IsInfinity(widthConstraint) || double.IsNaN(heightConstraint) || double.IsInfinity(heightConstraint))
            {
                throw new Exception("Infinity or NaN dimensions are not allowed for PopupLayout!");
            }
            else
            {
                Size s = new Size(widthConstraint, heightConstraint);
                return new SizeRequest(s, s);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            foreach (View child in Children)
            {
                if (child == _background)
                {
                    LayoutChildIntoBoundingRegion(child, new Rectangle(x, y, width, height));
                }
                else if (child is PopupRootLayout popupRootLayout)
                {

                    UpdatePopupLocation(popupRootLayout, width, height);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Create popup modal background
        /// </summary>
        private BoxView CreateModalBackground()
        {
            BoxView box = new BoxView();
            box.Color = Color.Transparent;

            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += OnModalAreaTapped;
            box.GestureRecognizers.Add(tap);

            return box;
        }

        private void OnModalAreaTapped(object sender, EventArgs e)
        {
            if (BackgroundCommand != null)
            {
                BackgroundCommand.Execute(null);
            }

            Popup.CloseAll();
        }

        private void OnIsModalBackgroundEnabledChanged(bool newValue)
        {
            if (_background != null)
            {
                if (newValue)
                {
                    _background.BackgroundColor = Color.Black;
                }
                else
                {
                    _background.BackgroundColor = Color.Transparent;
                }
            }
        }

        #region Popup location

        /// <summary>
        /// Update popup location
        /// </summary>
        public void UpdatePopupLocation(PopupRootLayout popupRootLayout, double width, double height)
        {
            Size availableSize = new Size(width, height);

            Popup popup = popupRootLayout.Popup;

            // Calculate popup available size based on available size and min and max size
            Size popupAvailableSize = new Size(Math.Min(availableSize.Width, popup.MaxWidthRequest), Math.Min(availableSize.Height, popup.MaxHeightRequest));
            popupAvailableSize = new Size(Math.Max(popupAvailableSize.Width, popup.MinWidthRequest), Math.Max(popupAvailableSize.Height, popup.MinHeightRequest));

            View actualPlacementTarget = popup.PlacementTarget ?? Parent as View;
            if (actualPlacementTarget.IsVisible == false)
            {
                return;
            }

            // Measure popup content size
            SizeRequest popupSizeRequest = popupRootLayout.Measure(popupAvailableSize.Width, popupAvailableSize.Height, MeasureFlags.IncludeMargins);

            // Get placement target absolute location in main window
            Point placementTargetTopLeftPoint = GetAbsolutePosition(actualPlacementTarget);
            Rectangle placementTargetLocation = new Rectangle(placementTargetTopLeftPoint.X, placementTargetTopLeftPoint.Y, actualPlacementTarget.Width, actualPlacementTarget.Height);

            // If PlacementRectange is setted, modify actual placement target location.
            if (popup.PlacementRectangle != null && popup.PlacementRectangle.IsEmpty == false)
            {
                placementTargetLocation.X += popup.PlacementRectangle.X;
                placementTargetLocation.Y += popup.PlacementRectangle.Y;
                placementTargetLocation.Width = popup.PlacementRectangle.Width;
                placementTargetLocation.Height = popup.PlacementRectangle.Height;
            }

            // Get popup actual location
            Rectangle popupLocation = GetPopupActualLocation(popupRootLayout, popupSizeRequest.Request, placementTargetLocation, availableSize, out PopupPlacements actualPlacement);

            // Update final actual placement
            popup.ActualPlacement = actualPlacement;

            // Layout popup to correct location
            LayoutChildIntoBoundingRegion(popupRootLayout, popupLocation);
        }

        /// <summary>
        /// Get popup actual location
        /// </summary>
        private Rectangle GetPopupActualLocation(PopupRootLayout popupRootLayout, Size popupSize, Rectangle placementTargetLocation, Size availableSize, out PopupPlacements actualPlacement)
        {
            Rectangle location = new Rectangle();

            actualPlacement = popupRootLayout.Popup.Placement;

            double spacing = popupRootLayout.Popup.Spacing;
            double horizontalOffset = popupRootLayout.Popup.HorizontalOffset;
            double verticalOffset = popupRootLayout.Popup.VerticalOffset;

            // Set location coordinates based on 'Placement'
            switch (popupRootLayout.Popup.Placement)
            {
                case PopupPlacements.BottomCenter:
                    {
                        location = GetPopupLocation(PopupPlacements.BottomCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.TopCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.TopCenter;

                            if (location.Top < 0)
                            {
                                location = GetPopupLocation(PopupPlacements.BottomCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                actualPlacement = PopupPlacements.BottomCenter;
                                location.Y = availableSize.Height - popupSize.Height;
                            }
                        }

                        if (location.Right > availableSize.Width)
                        {
                            actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopRight : PopupPlacements.BottomRight;

                            double y = location.Y;
                            location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            location.Y = y;

                            if (location.Right > availableSize.Width)
                            {
                                location.X = availableSize.Width - popupSize.Width;
                            }
                        }

                        if (location.Left < 0)
                        {
                            actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopLeft : PopupPlacements.BottomLeft;

                            double y = location.Y;
                            location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            location.Y = y;

                            if (location.Left < 0)
                            {
                                location.X = 0;
                            }
                        }

                        break;
                    }
                case PopupPlacements.BottomLeft:
                    {
                        location = GetPopupLocation(PopupPlacements.BottomLeft, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.TopLeft, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.TopLeft;

                            if (location.Top < 0)
                            {
                                location = GetPopupLocation(PopupPlacements.BottomLeft, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                actualPlacement = PopupPlacements.BottomLeft;
                                location.Y = availableSize.Height - popupSize.Height;
                            }
                        }

                        if (location.Right > availableSize.Width)
                        {
                            actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopRight : PopupPlacements.BottomRight;

                            double y = location.Y;
                            location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            location.Y = y;

                            if (location.Left < 0)
                            {
                                actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopLeft : PopupPlacements.BottomLeft;

                                y = location.Y;
                                location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                location.Y = y;

                                location.X = availableSize.Width - popupSize.Width;
                            }
                            else if (location.Right > availableSize.Width)
                            {
                                location.X = availableSize.Width - popupSize.Width;
                            }
                        }
                        else if (location.Left < 0)
                        {
                            location.X = 0;
                        }

                        break;
                    }
                case PopupPlacements.BottomRight:
                    {
                        location = GetPopupLocation(PopupPlacements.BottomRight, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.TopRight, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.TopRight;

                            if (location.Top < 0)
                            {
                                location = GetPopupLocation(PopupPlacements.BottomRight, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                actualPlacement = PopupPlacements.BottomRight;
                                location.Y = availableSize.Height - popupSize.Height;
                            }
                        }

                        if (location.Left < 0)
                        {
                            actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopLeft : PopupPlacements.BottomLeft;

                            double y = location.Y;
                            location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            location.Y = y;

                            if (location.Right > availableSize.Width)
                            {
                                actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopRight : PopupPlacements.BottomRight;

                                y = location.Y;
                                location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                location.Y = y;
                                location.X = 0;
                            }
                            else if (location.Left < 0)
                            {
                                location.X = 0;
                            }
                        }
                        else if (location.Right > availableSize.Width)
                        {
                            location.X = availableSize.Width - popupSize.Width;
                        }

                        break;
                    }
                case PopupPlacements.BottomStretch:
                    {
                        location = GetPopupLocation(PopupPlacements.BottomStretch, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.TopStretch, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.TopStretch;

                            if (location.Top < 0)
                            {
                                location = GetPopupLocation(PopupPlacements.BottomStretch, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                actualPlacement = PopupPlacements.BottomStretch;
                                location.Y = availableSize.Height - popupSize.Height;
                            }
                        }

                        break;
                    }
                case PopupPlacements.LeftCenter:
                    {
                        location = GetPopupLocation(PopupPlacements.LeftCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftBottom;
                        }
                        else if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftTop;
                        }

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftTop;
                            location.Y = Math.Max(0, location.Y);
                        }
                        else if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftBottom;
                            location.Y = availableSize.Height - popupSize.Height;
                        }

                        if (location.Left < 0)
                        {
                            Rectangle rightLocationCandidate = new Rectangle();
                            PopupPlacements rightPlacementCandidate = PopupPlacements.RightBottom;

                            if (actualPlacement == PopupPlacements.LeftBottom)
                            {
                                rightLocationCandidate = GetPopupLocation(PopupPlacements.RightBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                rightPlacementCandidate = PopupPlacements.RightBottom;
                            }
                            else if (actualPlacement == PopupPlacements.LeftCenter)
                            {
                                rightLocationCandidate = GetPopupLocation(PopupPlacements.RightCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                rightPlacementCandidate = PopupPlacements.RightCenter;
                            }
                            else // LeftTop
                            {
                                rightLocationCandidate = GetPopupLocation(PopupPlacements.RightTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                rightPlacementCandidate = PopupPlacements.RightTop;
                            }

                            if (rightLocationCandidate.Right < availableSize.Width)
                            {
                                double y = location.Y;
                                location = rightLocationCandidate;
                                location.Y = y;
                                actualPlacement = rightPlacementCandidate;
                            }
                            else
                            {
                                location.X = 0;
                            }
                        }

                        break;
                    }
                case PopupPlacements.LeftTop:
                    {
                        location = GetPopupLocation(PopupPlacements.LeftTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftBottom;
                        }

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftTop;
                            location.Y = Math.Max(0, location.Y);
                        }
                        else if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftBottom;
                            location.Y = availableSize.Height - popupSize.Height;
                        }

                        if (location.Left < 0)
                        {
                            Rectangle rightLocationCandidate = location;
                            PopupPlacements rightPlacementCandidate = PopupPlacements.RightBottom;

                            if (actualPlacement == PopupPlacements.LeftBottom)
                            {
                                rightLocationCandidate = GetPopupLocation(PopupPlacements.RightBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                rightPlacementCandidate = PopupPlacements.RightBottom;
                            }
                            else // Left top
                            {
                                rightLocationCandidate = GetPopupLocation(PopupPlacements.RightTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                rightPlacementCandidate = PopupPlacements.RightTop;
                            }

                            if (rightLocationCandidate.Right < availableSize.Width)
                            {
                                double y = location.Y;
                                location = rightLocationCandidate;
                                location.Y = y;
                                actualPlacement = rightPlacementCandidate;
                            }
                            else
                            {
                                location.X = 0;
                            }
                        }

                        break;
                    }
                case PopupPlacements.LeftBottom:
                    {
                        location = GetPopupLocation(PopupPlacements.LeftBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftTop;
                        }

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftTop;
                            location.Y = Math.Max(0, location.Y);
                        }
                        else if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.LeftBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.LeftBottom;
                            location.Y = availableSize.Height - popupSize.Height;
                        }

                        if (location.Left < 0)
                        {
                            Rectangle rightLocationCandidate = location;
                            PopupPlacements rightPlacementCandidate = PopupPlacements.RightBottom;

                            if (actualPlacement == PopupPlacements.LeftBottom)
                            {
                                rightLocationCandidate = GetPopupLocation(PopupPlacements.RightBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                rightPlacementCandidate = PopupPlacements.RightBottom;
                            }
                            else // LeftTop
                            {
                                rightLocationCandidate = GetPopupLocation(PopupPlacements.RightTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                rightPlacementCandidate = PopupPlacements.RightTop;
                            }

                            if (rightLocationCandidate.Right < availableSize.Width)
                            {
                                double y = location.Y;
                                location = rightLocationCandidate;
                                location.Y = y;
                                actualPlacement = rightPlacementCandidate;
                            }
                            else
                            {
                                location.X = 0;
                            }
                        }

                        break;
                    }
                case PopupPlacements.RightCenter:
                    {
                        location = GetPopupLocation(PopupPlacements.RightCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.RightBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightBottom;
                        }
                        else if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.RightTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightTop;
                        }

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.RightCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightCenter;
                            location.Y = Math.Max(0, location.Y);
                        }
                        else if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.RightBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightBottom;
                            location.Y = availableSize.Height - popupSize.Height;
                        }

                        if (location.Right > availableSize.Width)
                        {
                            Rectangle leftLocationCandidate = new Rectangle();
                            PopupPlacements leftPlacementCandidate = PopupPlacements.LeftBottom;

                            if (actualPlacement == PopupPlacements.RightBottom)
                            {
                                leftLocationCandidate = GetPopupLocation(PopupPlacements.LeftBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                leftPlacementCandidate = PopupPlacements.LeftBottom;
                            }
                            else if (actualPlacement == PopupPlacements.RightCenter)
                            {
                                leftLocationCandidate = GetPopupLocation(PopupPlacements.LeftCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                leftPlacementCandidate = PopupPlacements.LeftCenter;
                            }
                            else // RightTop
                            {
                                leftLocationCandidate = GetPopupLocation(PopupPlacements.LeftTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                leftPlacementCandidate = PopupPlacements.LeftTop;
                            }

                            if (leftLocationCandidate.Left > 0)
                            {
                                double y = location.Y;
                                location = leftLocationCandidate;
                                location.Y = y;
                                actualPlacement = leftPlacementCandidate;
                            }
                            else
                            {
                                location.X = availableSize.Width - popupSize.Width;
                            }
                        }

                        break;
                    }
                case PopupPlacements.RightTop:
                    {
                        location = GetPopupLocation(PopupPlacements.RightTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.RightBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightBottom;
                        }

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.RightTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightTop;
                            location.Y = Math.Max(0, location.Y);
                        }
                        else if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.RightBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightBottom;
                            // location.Y = availableSize.Height - popupSize.Height;
                        }

                        if (location.Right > availableSize.Width)
                        {
                            Rectangle leftLocationCandidate = location;
                            PopupPlacements leftPlacementCandidate = PopupPlacements.LeftBottom;

                            if (actualPlacement == PopupPlacements.RightBottom)
                            {
                                leftLocationCandidate = GetPopupLocation(PopupPlacements.LeftBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                leftPlacementCandidate = PopupPlacements.LeftBottom;
                            }
                            else // Right top
                            {
                                leftLocationCandidate = GetPopupLocation(PopupPlacements.LeftTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                leftPlacementCandidate = PopupPlacements.LeftTop;
                            }

                            if (leftLocationCandidate.Left > 0)
                            {
                                double y = location.Y;
                                location = leftLocationCandidate;
                                location.Y = y;
                                actualPlacement = leftPlacementCandidate;
                            }
                            else
                            {
                                location.X = availableSize.Width - popupSize.Width;
                            }
                        }

                        break;
                    }
                case PopupPlacements.RightBottom:
                    {
                        location = GetPopupLocation(PopupPlacements.RightBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.RightTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightTop;
                        }

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.RightTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightTop;
                            location.Y = Math.Max(0, location.Y);
                        }
                        else if (location.Bottom > availableSize.Height)
                        {
                            location = GetPopupLocation(PopupPlacements.RightBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.RightBottom;
                            location.Y = availableSize.Height - popupSize.Height;
                        }

                        if (location.Right > availableSize.Width)
                        {
                            Rectangle leftLocationCandidate = location;
                            PopupPlacements leftPlacementCandidate = PopupPlacements.LeftBottom;

                            if (actualPlacement == PopupPlacements.RightBottom)
                            {
                                leftLocationCandidate = GetPopupLocation(PopupPlacements.LeftBottom, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                leftPlacementCandidate = PopupPlacements.LeftBottom;
                            }
                            else // Right top
                            {
                                leftLocationCandidate = GetPopupLocation(PopupPlacements.LeftTop, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                leftPlacementCandidate = PopupPlacements.LeftTop;
                            }

                            if (leftLocationCandidate.Left > 0)
                            {
                                double y = location.Y;
                                location = leftLocationCandidate;
                                location.Y = y;
                                actualPlacement = leftPlacementCandidate;
                            }
                            else
                            {
                                location.X = availableSize.Width - popupSize.Width;
                            }
                        }

                        break;
                    }
                case PopupPlacements.TopCenter:
                    {
                        location = GetPopupLocation(PopupPlacements.TopCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.BottomCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.BottomCenter;

                            if (location.Bottom > availableSize.Height)
                            {
                                location = GetPopupLocation(PopupPlacements.TopCenter, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                actualPlacement = PopupPlacements.TopCenter;
                                location.Y = Math.Max(0, location.Y);
                            }
                        }

                        if (location.Right > availableSize.Width)
                        {
                            actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopRight : PopupPlacements.BottomRight;

                            double y = location.Y;
                            location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            location.Y = y;

                            if (location.Right > availableSize.Width)
                            {
                                location.X = availableSize.Width - popupSize.Width;
                            }
                        }

                        if (location.Left < 0)
                        {
                            location.X = 0;
                        }

                        break;
                    }
                case PopupPlacements.TopLeft:
                    {
                        location = GetPopupLocation(PopupPlacements.TopLeft, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.BottomLeft, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.BottomLeft;

                            if (location.Bottom > availableSize.Height)
                            {
                                location = GetPopupLocation(PopupPlacements.TopLeft, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                actualPlacement = PopupPlacements.TopLeft;
                                location.Y = Math.Max(0, location.Y);
                            }
                        }

                        if (location.Right > availableSize.Width)
                        {
                            actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopRight : PopupPlacements.BottomRight;

                            double y = location.Y;
                            location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            location.Y = y;

                            if (location.Left < 0)
                            {
                                actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopLeft : PopupPlacements.BottomLeft;

                                y = location.Y;
                                location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                location.Y = y;

                                location.X = availableSize.Width - popupSize.Width;
                            }
                            else if (location.Right > availableSize.Width)
                            {
                                location.X = availableSize.Width - popupSize.Width;
                            }
                        }
                        else if (location.Left < 0)
                        {
                            location.X = 0;
                        }

                        break;
                    }
                case PopupPlacements.TopRight:
                    {
                        location = GetPopupLocation(PopupPlacements.TopRight, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.BottomRight, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.BottomRight;

                            if (location.Bottom > availableSize.Height)
                            {
                                location = GetPopupLocation(PopupPlacements.TopRight, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                actualPlacement = PopupPlacements.TopRight;
                                location.Y = Math.Max(0, location.Y);
                            }
                        }

                        if (location.Left < 0)
                        {
                            actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopLeft : PopupPlacements.BottomLeft;

                            double y = location.Y;
                            location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            location.Y = y;

                            if (location.Right > availableSize.Width)
                            {
                                actualPlacement = Popup.IsTopPlacement(actualPlacement) ? PopupPlacements.TopRight : PopupPlacements.BottomRight;

                                y = location.Y;
                                location = GetPopupLocation(actualPlacement, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                location.Y = y;
                                location.X = 0;
                            }
                            else if (location.Left < 0)
                            {
                                location.X = 0;
                            }
                        }
                        else if (location.Right > availableSize.Width)
                        {
                            location.X = availableSize.Width - popupSize.Width;
                        }

                        break;
                    }
                case PopupPlacements.TopStretch:
                    {
                        location = GetPopupLocation(PopupPlacements.TopStretch, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);

                        if (location.Top < 0)
                        {
                            location = GetPopupLocation(PopupPlacements.BottomStretch, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                            actualPlacement = PopupPlacements.BottomStretch;

                            if (location.Bottom > availableSize.Height)
                            {
                                location = GetPopupLocation(PopupPlacements.TopStretch, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                                actualPlacement = PopupPlacements.TopStretch;
                                location.Y = Math.Max(0, location.Y);
                            }
                        }
                        break;
                    }
                case PopupPlacements.FullScreen:
                    {
                        location = GetPopupLocation(PopupPlacements.FullScreen, popupSize, placementTargetLocation, availableSize, spacing, horizontalOffset, verticalOffset);
                        break;
                    }
                default:
                    break;
            }

            popupRootLayout.Popup.ActualPlacement = actualPlacement;

            return location;
        }

        /// <summary>
        /// Get popup location in giving placement. Do not check is there enought available space.
        /// </summary>
        private Rectangle GetPopupLocation(PopupPlacements placement, Size popupSize, Rectangle placementTargetLocation, Size availableSize, double spacing, double verticalOffset, double horizontalOffset)
        {
            Rectangle location = new Rectangle();
            location.Width = popupSize.Width;
            location.Height = popupSize.Height;

            switch (placement)
            {
                case PopupPlacements.BottomCenter:
                    {
                        location.Y = placementTargetLocation.Y + placementTargetLocation.Height + spacing;
                        location.X = placementTargetLocation.X - (popupSize.Width / 2) + (placementTargetLocation.Width / 2);
                        break;
                    }
                case PopupPlacements.BottomRight:
                    {
                        location.Y = placementTargetLocation.Y + placementTargetLocation.Height + spacing;
                        location.X = placementTargetLocation.X - popupSize.Width + placementTargetLocation.Width;
                        break;
                    }
                case PopupPlacements.BottomLeft:
                    {
                        location.Y = placementTargetLocation.Y + placementTargetLocation.Height + spacing;
                        location.X = placementTargetLocation.X;
                        break;
                    }
                case PopupPlacements.BottomStretch:
                    {
                        location.Y = placementTargetLocation.Y + placementTargetLocation.Height + spacing;
                        location.X = 0;
                        location.Width = availableSize.Width;
                        location.Height = popupSize.Height;
                        break;
                    }
                case PopupPlacements.TopCenter:
                    {
                        location.Y = placementTargetLocation.Y - popupSize.Height - spacing;
                        location.X = placementTargetLocation.X - (popupSize.Width / 2) + (placementTargetLocation.Width / 2);
                        break;
                    }
                case PopupPlacements.TopLeft:
                    {
                        location.Y = placementTargetLocation.Y - popupSize.Height - spacing;
                        location.X = placementTargetLocation.X;
                        break;
                    }
                case PopupPlacements.TopRight:
                    {
                        location.Y = placementTargetLocation.Y - popupSize.Height - spacing;
                        location.X = placementTargetLocation.X - popupSize.Width + placementTargetLocation.Width;
                        break;
                    }
                case PopupPlacements.TopStretch:
                    {
                        location.Y = placementTargetLocation.Y - popupSize.Height - spacing;
                        location.X = 0;
                        location.Width = availableSize.Width;
                        location.Height = popupSize.Height;
                        break;
                    }
                case PopupPlacements.RightCenter:
                    {
                        location.Y = placementTargetLocation.Y - (popupSize.Height / 2) + (placementTargetLocation.Height / 2);
                        location.X = placementTargetLocation.X + placementTargetLocation.Width + spacing;
                        break;
                    }
                case PopupPlacements.RightTop:
                    {
                        location.Y = placementTargetLocation.Y;
                        location.X = placementTargetLocation.X + placementTargetLocation.Width + spacing;
                        break;
                    }
                case PopupPlacements.RightBottom:
                    {
                        location.Y = placementTargetLocation.Y - popupSize.Height + placementTargetLocation.Height;
                        location.X = placementTargetLocation.X + placementTargetLocation.Width + spacing;
                        break;
                    }
                case PopupPlacements.LeftCenter:
                    {
                        location.Y = placementTargetLocation.Y - (location.Height / 2) + (placementTargetLocation.Height / 2);
                        location.X = placementTargetLocation.X - popupSize.Width - spacing;
                        break;
                    }
                case PopupPlacements.LeftTop:
                    {
                        location.Y = placementTargetLocation.Y;
                        location.X = placementTargetLocation.X - popupSize.Width - spacing;
                        break;
                    }
                case PopupPlacements.LeftBottom:
                    {
                        location.Y = placementTargetLocation.Y - popupSize.Height + placementTargetLocation.Height;
                        location.X = placementTargetLocation.X - popupSize.Width - spacing;
                        break;
                    }
                case PopupPlacements.FullScreen:
                    {
                        location.Y = 0;
                        location.X = 0;
                        location.Width = availableSize.Width;
                        location.Height = availableSize.Height;
                        break;
                    }
                default:
                    break;
            }

            location.Y += verticalOffset;
            location.X += horizontalOffset;

            return location;
        }

        /// <summary>
        /// Get placement target absolute position on app window
        /// </summary>
        private Point GetAbsolutePosition(VisualElement element)
        {
            Point point = new Point();
            point.Y = element.Y + element.TranslationY;
            point.X = element.X + element.TranslationX;

            VisualElement parent = element.Parent as VisualElement;

            while (parent != null)
            {
                point.Y += parent.Y + parent.TranslationY;
                point.X += parent.X + parent.TranslationX;
                parent = parent.Parent as VisualElement;

                if (parent is Xamarin.Forms.ScrollView)
                {
                    Xamarin.Forms.ScrollView s = parent as Xamarin.Forms.ScrollView;
                    point.Y -= s.ScrollY;
                    point.X -= s.ScrollX;
                }
            }

            return point;
        }

        #endregion
    }
}
