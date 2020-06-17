using System;
using System.ComponentModel;
using System.Drawing;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

// iOS
using UIKit;

[assembly: ExportRenderer(typeof(XamKit.Entry), typeof(XamKit.iOS.EntryRenderer))]

namespace XamKit.iOS
{
    public class EntryRenderer : Xamarin.Forms.Platform.iOS.EntryRenderer
    {
        public EntryRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Entry> e)
        {
            base.OnElementChanged(e);

            if (Element is Entry entry)
            {
                Control.BackgroundColor = null;
                Control.BorderStyle = UITextBorderStyle.None;

                SetPadding(entry.Padding);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Element is Entry entry)
            {
                if (e.PropertyName == Entry.PaddingProperty.PropertyName)
                {
                    SetPadding(entry.Padding);
                }
            }
        }

        private void SetPadding(Thickness padding)
        {
            UIView paddingLeftView = new UIView(new RectangleF(0, 0, (float)padding.Left, 1));
            Control.LeftView = paddingLeftView;
            Control.LeftViewMode = UITextFieldViewMode.Always;

            UIView paddingRightView = new UIView(new RectangleF(0, 0, (float)padding.Right, 1));
            Control.RightView = paddingRightView;
            Control.RightViewMode = UITextFieldViewMode.Always;
        }
    }
}