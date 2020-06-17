using System;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using UIKit;
using Foundation;

[assembly: ResolutionGroupName("One")]
[assembly: ExportEffect(typeof(XamKit.iOS.TouchEffect), "TouchEffect")]

namespace XamKit.iOS
{
    public class TouchEffect : PlatformEffect
    {
        private UIView _view;
        private XamKit.TouchEffect _touchEffect = null;

        // Gestures
        private UIPressedGestureRecognizer _pressedGestureRecognizer = null;

        protected override void OnAttached()
        {
            // Get the iOS UIView corresponding to the Element that the effect is attached to
            _view = Control == null ? Container : Control;

            // Get access to the TouchEffect class in the PCL
            _touchEffect = (XamKit.TouchEffect)Element.Effects.FirstOrDefault(e => e is XamKit.TouchEffect);

            if (_view != null)
            {
                _pressedGestureRecognizer = new UIPressedGestureRecognizer();
                _pressedGestureRecognizer.Pressed += OnPressed;
                _pressedGestureRecognizer.Released += OnReleased;
                _pressedGestureRecognizer.Canceled += OnCanceled;
                _view.AddGestureRecognizer(_pressedGestureRecognizer);
            }
        }

        protected override void OnDetached()
        {
            if (_pressedGestureRecognizer != null)
            {
                _view.RemoveGestureRecognizer(_pressedGestureRecognizer);
            }
        }

        private void OnReleased()
        {
            if (_touchEffect != null)
            {
                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(TouchActionType.Released, GetLocalPoint(), GetApplicationPoint(), false));
            }
        }

        private void OnPressed()
        {
            if (_touchEffect != null)
            {
                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(TouchActionType.Pressed, GetLocalPoint(), GetApplicationPoint(), true));
            }
        }

        private void OnCanceled()
        {
            if (_touchEffect != null)
            {
                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(TouchActionType.Cancelled, GetLocalPoint(), GetApplicationPoint(), false));
            }
        }

        private Point GetApplicationPoint()
        {
            var tapPoint = _pressedGestureRecognizer.LocationInView(Control);
            return new Point(tapPoint.X, tapPoint.Y);
        }

        private Point GetLocalPoint()
        {
            var tapPoint = _pressedGestureRecognizer.LocationInView(Control);
            var globalRect = _view.ConvertRectToView(_view.Bounds, UIApplication.SharedApplication.Windows.First());
            return new Point(tapPoint.X - globalRect.X, tapPoint.Y - globalRect.Y);
        }
    }
}