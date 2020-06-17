using System;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

// iOS
using CoreAnimation;
using CoreGraphics;
using UIKit;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using XamKit;
using XamKit.iOS;
using Foundation;
using System.Linq;

[assembly: ExportRenderer(typeof(FlyoutMenu), typeof(FlyoutMenuRenderer))]

namespace XamKit.iOS
{
    public class FlyoutMenuRenderer : VisualElementRenderer<FlyoutMenu>
    {
        private static bool _isPressed = false;

        private double _startX;
        private double _startY;
        private Point _previousPoint;

        private bool _didSwipe = false;
        private float _displayDensity = 0;

        private UIPressedGestureRecognizer _pressedGestureRecognizer;

        protected override void OnElementChanged(ElementChangedEventArgs<FlyoutMenu> e)
        {
            base.OnElementChanged(e);

            _pressedGestureRecognizer = new UIPressedGestureRecognizer();
            _pressedGestureRecognizer.Pressed += OnPressed;
            _pressedGestureRecognizer.Released += OnReleased;
            _pressedGestureRecognizer.Canceled += OnCanceled;
            _pressedGestureRecognizer.Moved += OnMoved;
            NativeView.AddGestureRecognizer(_pressedGestureRecognizer);
        }

        private void OnMoved()
        {
            Element.OnTouchHandled(CreateArgs(TouchActionType.Move, true));
            _previousPoint = GetLocalPoint();
        }

        private void OnPressed()
        {
            var localPoint = GetLocalPoint();
            _startX = localPoint.X;
            _startY = localPoint.Y;
            _previousPoint = localPoint;

            Element.OnTouchHandled(CreateArgs(TouchActionType.Pressed, true));
        }

        private void OnReleased()
        {
            Element.OnTouchHandled(CreateArgs(TouchActionType.Released, false));

            Point localPoint = GetLocalPoint();

            double velocity = Math.Abs(_previousPoint.X - localPoint.X) / 5;

            if (velocity > 1.5)
            {
                Element.OnSwiped(SwipeDirection.Left, velocity, localPoint);
            }
        }

        private void OnCanceled()
        {
            Element.OnTouchHandled(CreateArgs(TouchActionType.Released, false));
        }

        private Point GetLocalPoint()
        {
            var tapPoint = _pressedGestureRecognizer.LocationInView(NativeView);
            var globalRect = NativeView.ConvertRectToView(NativeView.Bounds, UIApplication.SharedApplication.Windows.First());
            return new Point(tapPoint.X - globalRect.X, tapPoint.Y - globalRect.Y);
        }

        private FlyoutMenuTouchEventArgs CreateArgs(TouchActionType type, bool isPressed)
        {
            var localPoint = GetLocalPoint();
            double totalX = localPoint.X - _startX;
            double totalY = localPoint.Y - _startY;
            double deltaX = localPoint.X - _previousPoint.X;
            double deltaY = localPoint.Y - _previousPoint.Y;

            return new FlyoutMenuTouchEventArgs(type, localPoint.X, localPoint.Y, totalX, totalY, deltaX, deltaY, isPressed);
        }
    }
}