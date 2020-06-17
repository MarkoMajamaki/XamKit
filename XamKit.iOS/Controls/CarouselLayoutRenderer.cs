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
            return isPanGestureHandled;
        }        

        private bool IsPanGestureHandled()
        {
            var localPoint = GetLocalPoint();
            double delta = localPoint.X - _startX;
            return Math.Abs(delta) > 10;
        }

        // Events

        private void OnPressed()
        {
            var localPoint = GetLocalPoint();
            _previousPoint = localPoint;
            _startX = localPoint.X;
            _startY = localPoint.Y;
            Element.OnPanUpdated(Element, new PanUpdatedEventArgs(GestureStatus.Started, 0, 0, 0));
        }

        private void OnMoved()
        {
            var localPoint = GetLocalPoint();
            double totalX = localPoint.X - _startX;
            double totalY = localPoint.Y - _startY;
            Element.OnPanUpdated(Element, new PanUpdatedEventArgs(GestureStatus.Running, 0, totalX, totalY));

            _previousPoint = localPoint;
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
        }

        private Point GetLocalPoint()
        {
            var tapPoint = _pressedGestureRecognizer.LocationInView(NativeView);
            var globalRect = NativeView.ConvertRectToView(NativeView.Bounds, UIApplication.SharedApplication.Windows.First());
            return new Point(tapPoint.X - globalRect.X, tapPoint.Y - globalRect.Y);
        }
    }
}