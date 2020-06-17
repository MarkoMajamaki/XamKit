using System;

using Android.Content;
using Android.Views;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using XamKit;
using XamKit.Droid;

[assembly: ExportRenderer(typeof(XamKit.ContentPage), typeof(ContentPageRenderer))]

namespace XamKit.Droid
{
    public class ContentPageRenderer : VisualElementRenderer<XamKit.ContentPage>
    {
        private bool _firstTime = true;

        public ContentPageRenderer(Context context) : base(context)
        {

        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            if (_firstTime)
            {
                Element.OnRendered();
                _firstTime = false;
            }
        }
    }
}