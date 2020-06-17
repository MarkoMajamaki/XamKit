using System;
using System.Linq;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ResolutionGroupName("XamKit")]
[assembly: ExportEffect(typeof(XamKit.UWP.TouchEffect), "TouchEffect")]

namespace XamKit.UWP
{
    public class TouchEffect : PlatformEffect
    {
        private FrameworkElement _view;
        private XamKit.TouchEffect _touchEffect;
        private static bool _isPressed = false;

        protected override void OnAttached()
        {
            // Get the Android View corresponding to the Element that the effect is attached to
            _view = Control == null ? Container : Control;

            // Get access to the TouchEffect class in the PCL
            _touchEffect = (XamKit.TouchEffect)Element.Effects.FirstOrDefault(e => e is XamKit.TouchEffect);

            if (_view != null)
            {
                _view.PointerPressed += OnPressed;
                _view.PointerReleased += OnReleased;
                _view.PointerCanceled += OnCanceled;
                _view.PointerExited += OnPointerExited;
                _view.PointerEntered += OnPointerEntered;
                _view.PointerMoved += OnPointerMoved;
            }
        }

        protected override void OnDetached()
        {
            if (_view != null)
            {
                _view.PointerPressed -= OnPressed;
                _view.PointerReleased -= OnReleased;
                _view.PointerCanceled -= OnCanceled;
                _view.PointerExited -= OnPointerExited;
                _view.PointerEntered -= OnPointerEntered;
                _view.PointerMoved -= OnPointerMoved;
            }
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_touchEffect != null)
            {
                PointerPoint ptrPt = e.GetCurrentPoint(_view);
                Point point = new Point(ptrPt.Position.X, ptrPt.Position.Y);

                PointerPoint applicationPtrPt = e.GetCurrentPoint(Window.Current.Content);
                Point applicationPoint = new Point(applicationPtrPt.Position.X, applicationPtrPt.Position.Y);

                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(TouchActionType.Entered, point, applicationPoint, _isPressed));
            }
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (_touchEffect != null)
            {
                PointerPoint ptrPt = e.GetCurrentPoint(_view);
                Point point = new Point(ptrPt.Position.X, ptrPt.Position.Y);

                PointerPoint applicationPtrPt = e.GetCurrentPoint(Window.Current.Content);
                Point applicationPoint = new Point(applicationPtrPt.Position.X, applicationPtrPt.Position.Y);

                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(TouchActionType.Exited, point, applicationPoint, _isPressed));
            }
        }

        private void OnCanceled(object sender, PointerRoutedEventArgs e)
        {
            _isPressed = false;

            if (_touchEffect != null)
            {
                PointerPoint ptrPt = e.GetCurrentPoint(_view);
                Point point = new Point(ptrPt.Position.X, ptrPt.Position.Y);

                PointerPoint applicationPtrPt = e.GetCurrentPoint(Window.Current.Content);
                Point applicationPoint = new Point(applicationPtrPt.Position.X, applicationPtrPt.Position.Y);

                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(TouchActionType.Cancelled, point, applicationPoint, _isPressed));
            }
        }

        private void OnReleased(object sender, PointerRoutedEventArgs e)
        {
            _isPressed = false;

            if (_touchEffect != null)
            {
                PointerPoint ptrPt = e.GetCurrentPoint(_view);
                Point point = new Point(ptrPt.Position.X, ptrPt.Position.Y);

                PointerPoint applicationPtrPt = e.GetCurrentPoint(Window.Current.Content);
                Point applicationPoint = new Point(applicationPtrPt.Position.X, applicationPtrPt.Position.Y);

                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(TouchActionType.Released, point, applicationPoint, _isPressed));
            }
        }

        private void OnPressed(object sender, PointerRoutedEventArgs e)
        {
            _isPressed = true;

            if (_touchEffect != null)
            {
                PointerPoint ptrPt = e.GetCurrentPoint(_view);
                Point point = new Point(ptrPt.Position.X, ptrPt.Position.Y);

                PointerPoint applicationPtrPt = e.GetCurrentPoint(Window.Current.Content);
                Point applicationPoint = new Point(applicationPtrPt.Position.X, applicationPtrPt.Position.Y);

                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(TouchActionType.Pressed, point, applicationPoint, _isPressed));
            }
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_touchEffect != null)
            {
                PointerPoint ptrPt = e.GetCurrentPoint(_view);
                Point point = new Point(ptrPt.Position.X, ptrPt.Position.Y);

                PointerPoint applicationPtrPt = e.GetCurrentPoint(Window.Current.Content);
                Point applicationPoint = new Point(applicationPtrPt.Position.X, applicationPtrPt.Position.Y);

                _touchEffect.OnTouchAction(Element, new TouchActionEventArgs(TouchActionType.Move, point, applicationPoint, _isPressed));
            }
        }
    }
}
