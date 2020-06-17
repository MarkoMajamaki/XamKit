using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

// Xamarin
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(XamKit.Border), typeof(XamKit.UWP.BorderRenderer))]

namespace XamKit.UWP
{
   public class BorderRenderer : ViewRenderer<XamKit.Border, Windows.UI.Xaml.Controls.Border>
	{
        public BorderRenderer()
        {
            AutoPackage = false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Border> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {

            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var border = new Windows.UI.Xaml.Controls.Border();
                    SetNativeControl(border);
                }

                if (e.NewElement.Content != null)
                {
                    Control.Child = Element.Content.GetOrCreateRenderer().ContainerElement;
                }

                this.Background = null;

                Control.BorderBrush = new SolidColorBrush(e.NewElement.BorderColor.ToWindows());
                Control.BorderThickness = new Thickness(e.NewElement.BorderThickness);
                Control.CornerRadius = new CornerRadius(e.NewElement.CornerRadius);
                Control.Background = new SolidColorBrush(e.NewElement.BackgroundColor.ToWindows());
            }
        }
        
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Border.BorderThicknessProperty.PropertyName)
            {
                Control.BorderThickness = new Thickness(Element.BorderThickness);
            }
            else if (e.PropertyName == Border.CornerRadiusProperty.PropertyName)
            {
                Control.CornerRadius = new CornerRadius(Element.CornerRadius);
            }
            else if (e.PropertyName == Border.BorderColorProperty.PropertyName)
            {
                Control.BorderBrush = new SolidColorBrush(Element.BorderColor.ToWindows());
            }
            else if (e.PropertyName == Border.BackgroundColorProperty.PropertyName)
            {
                Control.Background = new SolidColorBrush(Element.BackgroundColor.ToWindows());
            }
            else if (e.PropertyName == Border.ContentProperty.PropertyName)
            {
                if (Element.Content != null)
                {
                    Control.Child = Element.Content.GetOrCreateRenderer().ContainerElement;
                }
                else
                {
                    Control.Child = null;
                }
            }
            else
            {
                base.OnElementPropertyChanged(sender, e);
            }
        }
    }
}
