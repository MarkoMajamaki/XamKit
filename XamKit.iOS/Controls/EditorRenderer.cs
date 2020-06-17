using System;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

// iOS
using UIKit;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(XamKit.Editor), typeof(XamKit.iOS.EditorRenderer))]

namespace XamKit.iOS
{
    public class EditorRenderer : Xamarin.Forms.Platform.iOS.EditorRenderer
    {
        public EditorRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.ScrollEnabled = false;
                Control.BackgroundColor = null;
                Control.TextContainer.LineFragmentPadding = 0;
            }

            if (Element is Editor entry)
            {
                SetPadding(entry.Padding);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Element is Editor entry)
            {
                if (e.PropertyName == Editor.PaddingProperty.PropertyName)
                {
                    SetPadding(entry.Padding);
                }
            }
        }

        private void SetPadding(Thickness padding)
        {
            Control.TextContainerInset = new UIEdgeInsets((nfloat)padding.Top, (nfloat)padding.Left, (nfloat)padding.Bottom, (nfloat)padding.Right);
        }
    }
}