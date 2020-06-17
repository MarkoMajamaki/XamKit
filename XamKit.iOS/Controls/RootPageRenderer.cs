// Xamarin forms
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;

// iOS
using UIKit;

[assembly: ExportRenderer(typeof(XamKit.RootPage), typeof(XamKit.iOS.RootPageRenderer))]

namespace XamKit.iOS
{
    public class RootPageRenderer : PageRenderer
    {
        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            UIView v = View;
            while (v.Superview != null)
            {
                v = v.Superview;
                v.GestureRecognizers = null;
            }
        }
    }
}
