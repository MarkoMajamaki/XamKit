using System;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

// iOS
using UIKit;
using System.Linq;

[assembly: ExportRenderer(typeof(XamKit.CarouselLayout), typeof(XamKit.iOS.CarouselLayoutRenderer))]

namespace XamKit.iOS
{
    public class CarouselLayoutRenderer : VisualElementRenderer<CarouselLayout>
    {
        private UIPressedGestureRecognizer _pressedGestureRecognizer;

        private double _startX;
        private double _startY;
        private Point _previousPoint;
        private bool _isPressed = false;

        public CarouselLayoutRenderer()
        {
            _pressedGestureRecognizer = new UIPressedGestureRecognizer();
            _pressedGestureRecognizer.Pressed += OnPressed;
            _pressedGestureRecognizer.Released += OnReleased;
            _pressedGestureRecognizer.Moved += OnMoved;
            _pressedGestureRecognizer.ShouldRecognizeSimultaneously = ShouldRecognizeSimultaneously;

            AddGestureRecognizer(_pressedGestureRecognizer);
        }

        private bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            bool isPanGestureHandled = IsPanGestureHandled();

            if ((isPanGestureHandled && _isPressed) /*|| Element.IsSnapToItemNeeded*/)
            {
                otherGestureRecognizer.State = UIGestureRecognizerState.Cancelled;
            }

            return isPanGestureHandled;
        }        

        private bool IsPanGestureHandled()
        {
            var localPoint = GetLocalPoint();
            double deltaX = localPoint.X - _startX;
            double deltaY = localPoint.Y - _startY;

            return Math.Abs(deltaX) > Math.Abs(deltaY);
        }

        // Events

        private void OnPressed()
        {
            var localPoint = GetLocalPoint();
            _previousPoint = localPoint;
            _startX = localPoint.X;
            _startY = localPoint.Y;
            Element.OnPanUpdated(Element, new PanUpdatedEventArgs(GestureStatus.Started, 0, 0, 0));
            _isPressed = true;
        }

        private void OnMoved()
        {
            var localPoint = GetLocalPoint();
            double totalX = localPoint.X - _startX;
            double totalY = localPoint.Y - _startY;
            Element.OnPanUpdated(Element, new PanUpdatedEventArgs(GestureStatus.Running, 0, totalX, totalY));

            _previousPoint = localPoint;
            _isPressed = true;
        }

        private void OnReleased()
        {
            var localPoint = GetLocalPoint();
            double totalX = localPoint.X - _startX;
            double totalY = localPoint.Y - _startY;

            double velocity = Math.Abs(_previousPoint.X - localPoint.X) / 5;

            if (velocity < 1)
            {
                Element.OnPanUpdated(Element, new PanUpdatedEventArgs(GestureStatus.Completed, 0, totalX, totalY));
            }
            else
            {
                Element.OnPanUpdated(Element, new PanUpdatedEventArgs(GestureStatus.Canceled, 0, totalX, totalY));
                if (_previousPoint.X - localPoint.X > 0)
                {
                    Element.OnSwiped(SwipeDirection.Left, velocity);
                }
                else if (_previousPoint.X - localPoint.X < 0)
                {
                    Element.OnSwiped(SwipeDirection.Right, velocity);
                }
            }

            _isPressed = false;
        }

        private Point GetLocalPoint()
        {
            var tapPoint = _pressedGestureRecognizer.LocationInView(NativeView);
            var globalRect = NativeView.ConvertRectToView(NativeView.Bounds, UIApplication.SharedApplication.Windows.First());
            return new Point(tapPoint.X - globalRect.X, tapPoint.Y - globalRect.Y);
        }
    }
}