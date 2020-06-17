using System;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

// iOS
using CoreAnimation;
using CoreGraphics;
using UIKit;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

// One
using XamKit;

[assembly: ExportRenderer(typeof(Border), typeof(XamKit.iOS.BorderRenderer))]

namespace XamKit.iOS
{
    public class BorderRenderer : VisualElementRenderer<Border>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Border> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                return;
            }

            NativeView.ClipsToBounds = true;
            NativeView.Layer.AllowsEdgeAntialiasing = true;
            NativeView.Layer.EdgeAntialiasingMask = CAEdgeAntialiasingMask.All;
            SetCornerRadius();
            SetBorders();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Border.CornerRadiusProperty.PropertyName)
            {
                SetCornerRadius();
            }
            else if (e.PropertyName == Border.BorderThicknessProperty.PropertyName ||
                     e.PropertyName == Border.BorderColorProperty.PropertyName)
            {
                SetBorders();
            }
        }

        private void SetCornerRadius()
        {
            NativeView.Layer.CornerRadius = new nfloat(Element.CornerRadius);
        }

        private void SetBorders()
        {
            NativeView.Layer.BorderWidth = new nfloat(Element.BorderThickness);
            NativeView.Layer.BorderColor = Element.BorderColor.ToCGColor();
        }
    }
}