using System;
using Xamarin.Forms;

namespace XamKit
{
    public class ScrollEffect : RoutingEffect
    {
        public static string Key = "XamKit.ScrollEffect";

        public double ScrollY { get; private set; }

        public double ScrollX { get; private set; }

        /// <summary>
        /// Event when scroll is changed
        /// </summary>
        public EventHandler<ScrolledEventArgs> ScrollChanged;

        public Action<double, double> ScrollToAction;

        public ScrollEffect() : base(Key)
        {
        }

        public void ScrollTo(double scrollX, double scrollY)
        {
            ScrollX = scrollX;
            ScrollY = scrollY;

            if (ScrollToAction != null)
            {
                ScrollToAction(scrollX, scrollY);
            }
        }
    }
}
