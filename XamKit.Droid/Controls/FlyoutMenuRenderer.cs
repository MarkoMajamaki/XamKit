using System;

using Android.Content;
using Android.Views;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using XamKit;
using XamKit.Droid;

[assembly: ExportRenderer(typeof(FlyoutMenu), typeof(FlyoutMenuRenderer))]

namespace XamKit.Droid
{
    public class FlyoutMenuRenderer : VisualElementRenderer<FlyoutMenu>
    {
        private static bool _isPressed = false;

        private float? _startX;
        private float? _startY;
        private GestureDetector _gestureDetector;
        private Point _previousPoint;

        private bool _didSwipe = false;
        private float _displayDensity = 0;

        public FlyoutMenuRenderer(Context context) : base(context)
        {
            _gestureDetector = new GestureDetector(Context, new SwipeListener(OnSwiped));
            _displayDensity = Context.Resources.DisplayMetrics.Density;
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            double totalX = GetTotalX(e);
            double totalY = GetTotalY(e);
            Point currentPoint = new Point(GetX(e), GetY(e));

            _gestureDetector.OnTouchEvent(e);

            if (e.ActionMasked == MotionEventActions.Down)
            {
                _startX = e.GetX();
                _startY = e.GetY();
                _previousPoint = new Point();

                _isPressed = true;
                Element.OnTouchHandled(new FlyoutMenuTouchEventArgs(TouchActionType.Pressed, currentPoint.X, currentPoint.Y, 0, 0, 0, 0, _isPressed));
            }
            else if (e.ActionMasked == MotionEventActions.Move && Element.IsMainMenuOpen)
            {
                bool isHandled = CheckIsTouchHandled(e);
                Parent?.RequestDisallowInterceptTouchEvent(isHandled);
                return isHandled;
            }
            else if (e.ActionMasked == MotionEventActions.Up || e.ActionMasked == MotionEventActions.Cancel)
            {
                _isPressed = false;
                TouchActionType type = e.ActionMasked == MotionEventActions.Up && _didSwipe == false ? TouchActionType.Released : TouchActionType.Cancelled;
                Point previousDelta = new Point(currentPoint.X - _previousPoint.X, currentPoint.Y - _previousPoint.Y);
                Element.OnTouchHandled(new FlyoutMenuTouchEventArgs(type, GetX(e), GetY(e), totalX, totalY, previousDelta.X, previousDelta.Y, _isPressed));
            }

            _previousPoint = currentPoint;

            return false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            double totalX = GetTotalX(e);
            double totalY = GetTotalY(e);
            Point currentPoint = new Point(GetX(e), GetY(e));
            Point previousDelta = new Point(currentPoint.X - _previousPoint.X, currentPoint.Y - _previousPoint.Y);

            _gestureDetector.OnTouchEvent(e);

            if (e.ActionMasked == MotionEventActions.Move)
            {
                bool isHandled = CheckIsTouchHandled(e);
                Parent?.RequestDisallowInterceptTouchEvent(isHandled);

                if (isHandled)
                {
                    Element.OnTouchHandled(new FlyoutMenuTouchEventArgs(TouchActionType.Move, currentPoint.X, currentPoint.Y, totalX, totalY, previousDelta.X, previousDelta.Y, _isPressed));
                }
            }
            else if (e.ActionMasked == MotionEventActions.Up || e.ActionMasked == MotionEventActions.Cancel)
            {
                _isPressed = false;
                TouchActionType type = e.ActionMasked == MotionEventActions.Up && _didSwipe == false ? TouchActionType.Released : TouchActionType.Cancelled;
                Element.OnTouchHandled(new FlyoutMenuTouchEventArgs(type, currentPoint.X, currentPoint.Y, totalX, totalY, previousDelta.X, previousDelta.Y, _isPressed));
            }

            _previousPoint = currentPoint;
            return true;
        }

        private bool CheckIsTouchHandled(MotionEvent e)
        {
            var xDeltaAbs = Math.Abs(GetTotalX(e));
            var yDeltaAbs = Math.Abs(GetTotalY(e));

            if (xDeltaAbs > yDeltaAbs)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private float GetTotalX(MotionEvent ev)
        {
            return (ev.GetX() - _startX.GetValueOrDefault()) / _displayDensity;
        }

        private float GetTotalY(MotionEvent ev)
        {
            return (ev.GetY() - _startY.GetValueOrDefault()) / _displayDensity;
        }

        private float GetX(MotionEvent ev)
        {
            return ev.GetX() / _displayDensity;
        }

        private float GetY(MotionEvent ev)
        {
            return ev.GetY() / _displayDensity;
        }

        private void OnSwiped(SwipeArgs args)
        {
            _didSwipe = true;
            Element.OnSwiped(args.Direction, args.Velocity, _previousPoint);
        }

        private class SwipeArgs
        {
            public SwipeDirection Direction { get; private set; }
            public double Velocity { get; private set; }
            public SwipeArgs(SwipeDirection direction, double velocity)
            {
                Direction = direction;
                Velocity = velocity;
            }
        }

        /// <summary>
        /// Custom swipe gesture
        /// </summary>
        private class SwipeListener : GestureDetector.SimpleOnGestureListener
        {
            public double SwipeThreshold { get; set; } = 0;
            public double SwipeVelocityThreshold { get; set; } = 1200;

            private readonly Action<SwipeArgs> _onSwiped;

            public SwipeListener(Action<SwipeArgs> onSwiped)
            {
                _onSwiped = onSwiped;
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                var diffX = (e2?.GetX() ?? 0) - (e1?.GetX() ?? 0);
                var diffY = (e2?.GetY() ?? 0) - (e1?.GetY() ?? 0);

                var absDiffX = Math.Abs(diffX);
                var absDiffY = Math.Abs(diffY);

                if (absDiffX > absDiffY &&
                    absDiffX > SwipeThreshold &&
                    Math.Abs(velocityX) > SwipeVelocityThreshold)
                {
                    _onSwiped?.Invoke(new SwipeArgs(diffX > 0 ? SwipeDirection.Right : SwipeDirection.Left, Math.Abs(velocityX / 1750)));
                    return true;
                }

                return false;
            }
        }
    }
}