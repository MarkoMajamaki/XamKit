using System;

using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

using XamKit;
using XamKit.UWP;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Foundation;

[assembly: ExportRenderer(typeof(CarouselLayout), typeof(CarouselLayoutRenderer))]

namespace XamKit.UWP
{
    public class CarouselLayoutRenderer : VisualElementRenderer<CarouselLayout, Panel>
    {
        private Windows.Foundation.Point _start = new Windows.Foundation.Point();
        private bool _touchDown = false;

        private static CarouselLayout _firstLayout;

        public CarouselLayoutRenderer()
        {
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.System;
            AddHandler(ManipulationStartedEvent, new ManipulationStartedEventHandler(OnStart), true);
            AddHandler(ManipulationDeltaEvent, new ManipulationDeltaEventHandler(OnMove), true);
            AddHandler(ManipulationCompletedEvent, new ManipulationCompletedEventHandler(OnStop), true);

            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
        }

        private void OnStart(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (Element is CarouselLayout layout && layout.IsPanEnabled)
            {
                if (_firstLayout != null && layout != _firstLayout)
                {
                    return;
                }

                _firstLayout = layout;

                _start = new Windows.Foundation.Point(e.Position.X, e.Position.Y);

                Xamarin.Forms.Point delta = GetPositionDelta(_start, _start);
                layout.OnPanUpdated(layout, new PanUpdatedEventArgs(GestureStatus.Started, 1, -delta.X, -delta.Y));
                _touchDown = true;
                e.Handled = true;
            }
        }

        private void OnMove(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Element is CarouselLayout layout && _touchDown && layout.IsPanEnabled)
            {
                if (_firstLayout != null && layout != _firstLayout)
                {
                    return;
                }

                Xamarin.Forms.Point delta = GetPositionDelta(_start, e.Position);
                layout.OnPanUpdated(layout, new PanUpdatedEventArgs(GestureStatus.Running, 1, -delta.X, -delta.Y));
                e.Handled = true;
            }
        }

        private void OnStop(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (Element is CarouselLayout layout && _touchDown && layout.IsPanEnabled)
            {
                double swipeVelocityTreshlod = 0.3;

                Xamarin.Forms.Point delta = GetPositionDelta(_start, e.Position);

                if (Math.Abs(e.Velocities.Linear.X) > swipeVelocityTreshlod)
                {
                    layout.OnPanUpdated(layout, new PanUpdatedEventArgs(GestureStatus.Canceled, 1, -delta.X, -delta.Y));
                }
                else
                {
                    layout.OnPanUpdated(layout, new PanUpdatedEventArgs(GestureStatus.Completed, 1, -delta.X, -delta.Y));
                }

                if (e.Velocities.Linear.X > swipeVelocityTreshlod)
                {
                    layout.OnSwiped(SwipeDirection.Right, Math.Abs(e.Velocities.Linear.X));
                }
                else if (e.Velocities.Linear.X < -swipeVelocityTreshlod)
                {
                    layout.OnSwiped(SwipeDirection.Left, Math.Abs(e.Velocities.Linear.X));
                }

                e.Handled = true;
            }

            _touchDown = false;
            _firstLayout = null;
        }

        private Xamarin.Forms.Point GetPositionDelta(Windows.Foundation.Point start, Windows.Foundation.Point end)
        {
            return new Xamarin.Forms.Point(start.X - end.X, start.Y - end.Y);
        }
    }
}