using System;

using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

using XamKit;
using XamKit.UWP;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Core;

[assembly: ExportRenderer(typeof(XamKit.ContentPage), typeof(ContentPageRenderer))]

namespace XamKit.UWP
{
    public class ContentPageRenderer : VisualElementRenderer<XamKit.ContentPage, Panel>
    {
        public ContentPageRenderer()
        {
            this.SizeChanged += OnSizeChanged2;
        }

        private async void OnSizeChanged2(object sender, SizeChangedEventArgs e)
        {
            this.SizeChanged -= OnSizeChanged2;

            if (Element != null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, Element.OnRendered);
            }
        }
    }
}
