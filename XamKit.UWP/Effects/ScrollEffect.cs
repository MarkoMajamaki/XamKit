using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportEffect(typeof(XamKit.UWP.ScrollEffect), "ScrollEffect")]

namespace XamKit.UWP
{
    public class ScrollEffect : PlatformEffect
    {
        private XamKit.ScrollEffect m_scrollEffect;
        
        protected override void OnAttached()
        {
            m_scrollEffect = Element.Effects.FirstOrDefault(e => e is XamKit.ScrollEffect) as XamKit.ScrollEffect;

            if (m_scrollEffect != null)
            {
                m_scrollEffect.ScrollToAction = new Action<double, double>(OnScrollChanged);
            }

            if (Control is ScrollViewer scrollViewer)
            {
                scrollViewer.ViewChanging -= OnViewChanging;
                scrollViewer.ViewChanging += OnViewChanging;
            }
        }
        
        protected override void OnDetached()
        {
        }

        private void OnViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (m_scrollEffect != null && m_scrollEffect.ScrollChanged != null)
            {
                ScrollViewer s = sender as ScrollViewer;
                m_scrollEffect.ScrollChanged(Element, new ScrolledEventArgs(e.NextView.HorizontalOffset, e.NextView.VerticalOffset));
            }
        }

        private void OnScrollChanged(double scrollX, double scrollY)
        {
            ScrollViewer scrollViewer = Control as ScrollViewer;

            if (scrollViewer != null)
            {
                scrollViewer.ChangeView(scrollX, scrollY, null, true);
            }
        }
    }
}
