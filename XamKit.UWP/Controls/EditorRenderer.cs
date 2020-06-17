using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(XamKit.Editor), typeof(XamKit.UWP.EditorRenderer))]

namespace XamKit.UWP
{
    public class EditorRenderer : Xamarin.Forms.Platform.UWP.EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                Control.BackgroundFocusBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
                Control.BorderThickness = new Thickness(0);
                Control.Padding = (Element as Editor).Padding.ToWindows();
                Control.Margin = new Thickness(0);
                Control.FontSize = e.NewElement.FontSize;
                Control.Foreground = new SolidColorBrush(e.NewElement.TextColor.ToWindows());
                Control.MinHeight = 0;
                Control.Style = EntryRenderer.CreateSimpleSTyle();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Editor.PaddingProperty.PropertyName)
            {
                Control.Padding = (Element as Editor).Padding.ToWindows();
            }
            else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
            {
                Control.FontSize = Element.FontSize;
            }
            else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
            {
                Control.Foreground = new SolidColorBrush(Element.TextColor.ToWindows());
            }
        }
    }
}
