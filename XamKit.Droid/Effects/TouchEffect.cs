using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Android.Views;

[assembly: ResolutionGroupName("XamKit")]
[assembly: ExportEffect(typeof(XamKit.Droid.TouchEffect), "TouchEffect")]

namespace XamKit.Droid
{
    public class TouchEffect : PlatformEffect
    {
        private static bool _isPressed = false;
        private global::Android.Views.View _view;
        private XamKit.TouchEffect _touchEffect;
        private float _displayDensity = 0;

        protected override void OnAttached()
        {
            // Get the Android View corresponding to the Element that the effect is attached to
            _view = Control == null ? Container : Control;

            // Get access to the TouchEffect class in the PCL
            _touchEffect = (XamKit.TouchEffect)Element.Effects.FirstOrDefault(e => e is XamKit.TouchEffect);

            if (_view != null)
            {
                _displayDensity = _view.Context.Resources.DisplayMetrics.Density;

                // Set event handler on View
                _view.Touch += OnTouch;
            }
        }

        protected override void OnDetached()
        {
            if (_view != null)
            {
                _view.Touch -= OnTouch;
            }
        }

        private void OnTouch(object sender, global::Android.Views.View.TouchEventArgs args)
        {
            TouchActionType? type = null;

            if (args.Event.Action == MotionEventActions.Down)
            {
                _isPressed = true;
                type = TouchActionType.Pressed;
            }
            else if (args.Event.Action == MotionEventActions.Up)
            {
                _isPressed = false;
                type = TouchActionType.Released;
            }
            else if (args.Event.Action == MotionEventActions.Cancel)
            {
                _isPressed = false;
                type = TouchActionType.Cancelled;
            }
            else if (args.Event.Action == MotionEventActions.Outside)
            {
                type = TouchActionType.Cancelled;
            }
            else if (args.Event.Action == MotionEventActions.Move)
            {
                type = TouchActionType.Move;
            }
            else if (args.Event.Action == MotionEventActions.Move && (args.Event.GetX() < 0 || args.Event.GetY() < 0 || args.Event.GetX() > _view.Width || args.Event.GetY() > _view.Height))
            {
                type = TouchActionType.Cancelled;
            }

            if (_touchEffect != null && type.HasValue)
            {
                Point point = PxToDp(new Point(args.Event.GetX(), args.Event.GetY()));
                Point applicationPoint = PxToDp(new Point(args.Event.RawX, args.Event.RawY));

                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(type.Value, point, applicationPoint, _isPressed));
            }
        }

        private Point PxToDp(Point point)
        {
            point.X = point.X / _displayDensity;
            point.Y = point.Y / _displayDensity;
            return point;
        }
    }
}
