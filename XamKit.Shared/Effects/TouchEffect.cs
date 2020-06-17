using System;
using Xamarin.Forms;

namespace XamKit
{
    public class TouchEffect : RoutingEffect
    {
        public event TouchActionEventHandler TouchAction;

        public TouchEffect() : base("XamKit.TouchEffect")
        {
        }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            TouchAction?.Invoke(element, args);
        }
    }

    public class TouchActionEventArgs : EventArgs
    {
        /// <summary>
        /// Point relative to element
        /// </summary>
        public Point Point { get; private set; }

        /// <summary>
        /// Point relative to application window
        /// </summary>
        public Point ApplicationPoint { get; private set; }

        /// <summary>
        /// Touch action type
        /// </summary>
        public TouchActionType Type { get; private set; }

        /// <summary>
        /// Is touch pressed to any place on the app
        /// </summary>
        public bool IsPressed { get; private set; }

        public TouchActionEventArgs(TouchActionType type, Point point, Point applicationPoint, bool isPressed)
        {
            Point = point;
            Type = type;
            ApplicationPoint = applicationPoint;
            IsPressed = isPressed;
        }
    }

    public delegate void TouchActionEventHandler(object sender, TouchActionEventArgs args);

    public enum TouchActionType
    {
        Entered,
        Pressed,
        Move,
        Released,
        Exited,
        Cancelled
    }
}
