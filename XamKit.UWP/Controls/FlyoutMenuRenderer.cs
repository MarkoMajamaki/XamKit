using System;

using Xamarin.Forms.Platform.UWP;

using XamKit;
using XamKit.UWP;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(FlyoutMenu), typeof(FlyoutMenuRenderer))]

namespace XamKit.UWP
{
    public class FlyoutMenuRenderer : VisualElementRenderer<FlyoutMenu, Panel>
    {
        private Point _start = new Point();
        private bool _touchDown = false;
        private static bool _isPressed = false;
        private Point _previousPoint;

        private static FlyoutMenu _firstLayout;

        public FlyoutMenuRenderer()
        {
            AddHandler(PointerPressedEvent, new PointerEventHandler(OnPressed), true);
            AddHandler(PointerReleasedEvent, new PointerEventHandler(OnReleased), true);
            AddHandler(PointerCanceledEvent, new PointerEventHandler(OnCanceled), true);
            AddHandler(PointerMovedEvent, new PointerEventHandler(OnMoved), true);
            AddHandler(PointerExitedEvent, new PointerEventHandler(OnExited), true);
        }

        private void OnPressed(object sender, PointerRoutedEventArgs e)
        {
            _isPressed = true;
            _touchDown = true;

            if (_firstLayout != null && Element != _firstLayout)
            {
                return;
            }

            _firstLayout = Element;
            Point currentPoint = new Point(e.GetCurrentPoint(this).Position.X, e.GetCurrentPoint(this).Position.Y);
            _start = currentPoint;
            _previousPoint = currentPoint;

            Element.OnTouchHandled(new FlyoutMenuTouchEventArgs(TouchActionType.Pressed, currentPoint.X, currentPoint.Y, 0, 0, 0, 0, _isPressed));
        }

        private void OnMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_touchDown) // Do move only if touch down
            {
                if (_firstLayout != null && Element != _firstLayout)
                {
                    return;
                }

                Point currentPoint = new Point(e.GetCurrentPoint(this).Position.X, e.GetCurrentPoint(this).Position.Y);
                Point totalDelta = GetPositionDelta(_start, currentPoint);
                Point previousDelta = GetPositionDelta(_previousPoint, currentPoint);
                _previousPoint = currentPoint;

                Element.OnTouchHandled(new FlyoutMenuTouchEventArgs(TouchActionType.Move, currentPoint.X, currentPoint.Y, -totalDelta.X, -totalDelta.Y, -previousDelta.X, -previousDelta.Y, _isPressed));
            }
        }

        private void OnExited(object sender, PointerRoutedEventArgs e)
        {
            OnReleased(sender, e);
        }

        private void OnCanceled(object sender, PointerRoutedEventArgs e)
        {
            OnReleased(sender, e);
        }

        private void OnReleased(object sender, PointerRoutedEventArgs e)
        {
            _isPressed = false;

            if (_touchDown)
            {
                Point currentPoint = new Point(e.GetCurrentPoint(this).Position.X, e.GetCurrentPoint(this).Position.Y);
                Point totalDelta = GetPositionDelta(_start, currentPoint);
                Point previousDelta = GetPositionDelta(_previousPoint, currentPoint);
                _previousPoint = currentPoint;

                Element.OnTouchHandled(new FlyoutMenuTouchEventArgs(TouchActionType.Released, currentPoint.X, currentPoint.Y, -totalDelta.X, -totalDelta.Y, -previousDelta.X, -previousDelta.Y, _isPressed));
            }

            _touchDown = false;
            _firstLayout = null;
        }

        private Point GetPositionDelta(Point start, Point end)
        {
            return new Point(start.X - end.X, start.Y - end.Y);
        }
    }
}