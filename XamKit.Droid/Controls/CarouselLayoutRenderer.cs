using System;

using Android.Content;
using Android.Views;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using XamKit;
using XamKit.Droid;

[assembly: ExportRenderer(typeof(CarouselLayout), typeof(CarouselLayoutRenderer))]

namespace XamKit.Droid
{
    public class CarouselLayoutRenderer : VisualElementRenderer<CarouselLayout>
    {
        private static Guid? _lastTouchHandlerId;
        private Guid _elementId;
        private int _gestureId;

        private float? _startX;
        private float? _startY;
        private GestureDetector _gestureDetector;

        private float _displayDensity = 0;

        private bool _didSwipe = false;

        public CarouselLayoutRenderer(Context context) : base(context)
        {
            _gestureDetector = new GestureDetector(Context, new CarouselSwipeListener(OnSwiped));
            _displayDensity = Context.Resources.DisplayMetrics.Density;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CarouselLayout> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                _elementId = Guid.NewGuid();
            }
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            _gestureDetector.OnTouchEvent(e);

            if (Element.IsPanEnabled == false || e.PointerCount > 1)
            {
                base.OnInterceptTouchEvent(e);
                return false;
            }  
            
            if (e.ActionMasked == MotionEventActions.Move)
            {
                if (_lastTouchHandlerId.HasValue && _lastTouchHandlerId != _elementId)
                {
                    return false;
                }

                _gestureDetector.OnTouchEvent(e);

                bool isHandled = CheckIsTouchHandled(e);
                Parent?.RequestDisallowInterceptTouchEvent(isHandled);
                return isHandled;
            }

            HandleDownUpCancelEvents(e);

            return false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            double xDelta = GetTotalX(e);
            double yDelta = GetTotalY(e);

            _gestureDetector.OnTouchEvent(e);

            if (e.ActionMasked == MotionEventActions.Move)
            {
                bool isHandled = CheckIsTouchHandled(e);
                Parent?.RequestDisallowInterceptTouchEvent(isHandled);

                if (isHandled)
                {
                    Element.OnPanUpdated(Element, new PanUpdatedEventArgs(GestureStatus.Running, _gestureId, xDelta, yDelta));
                }
            }

            HandleDownUpCancelEvents(e);
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

        private void HandleDownUpCancelEvents(MotionEvent e)
        {
            if (e.ActionMasked == MotionEventActions.Down)
            {
                HandleDownEvent(e);
            }

            if (Element.IsPanEnabled && (e.ActionMasked == MotionEventActions.Up || e.ActionMasked == MotionEventActions.Cancel))
            {
                HandleUpCancelEvent(e);
            }
        }

        private void HandleUpCancelEvent(MotionEvent e)
        {
            var xDelta = GetTotalX(e);
            var yDelta = GetTotalY(e);

            GestureStatus status = e.ActionMasked == MotionEventActions.Up && _didSwipe == false ? GestureStatus.Completed : GestureStatus.Canceled;
            Element.OnPanUpdated(Element, new PanUpdatedEventArgs(status, _gestureId, xDelta, yDelta));
            _lastTouchHandlerId = null;

            Parent?.RequestDisallowInterceptTouchEvent(false);

            _startX = null;
            _startY = null;
            _didSwipe = false;
        }

        private void HandleDownEvent(MotionEvent ev)
        {
            _gestureId = new Random().Next();
            _startX = ev.GetX();
            _startY = ev.GetY();
            Element.OnPanUpdated(Element, new PanUpdatedEventArgs(GestureStatus.Started, _gestureId, 0, 0));
            _lastTouchHandlerId = _elementId;
        }

        private float GetTotalX(MotionEvent ev)
        {
            return (ev.GetX() - _startX.GetValueOrDefault()) / _displayDensity;
        }

        private float GetTotalY(MotionEvent ev)
        {
            return (ev.GetY() - _startY.GetValueOrDefault()) / _displayDensity;
        }

        private void OnSwiped(SwipeArgs args)
        {
            _didSwipe = true;
            Element.OnSwiped(args.Direction, args.Velocity);
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
        private class CarouselSwipeListener : GestureDetector.SimpleOnGestureListener
        {
            public double SwipeThreshold { get; set; } = 0;
            public double SwipeVelocityThreshold { get; set; } = 1200;

            private readonly Action<SwipeArgs> _onSwiped;

            public CarouselSwipeListener(Action<SwipeArgs> onSwiped)
            {
                _onSwiped = onSwiped;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="e1"></param>
            /// <param name="e2"></param>
            /// <param name="velocityX"></param>
            /// <param name="velocityY"></param>
            /// <returns></returns>
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