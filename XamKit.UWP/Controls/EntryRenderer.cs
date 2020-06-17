using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(XamKit.Entry), typeof(XamKit.UWP.EntryRenderer))]

namespace XamKit.UWP
{
    public class EntryRenderer : Xamarin.Forms.Platform.UWP.EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                Control.BackgroundFocusBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
                Control.BorderThickness = new Thickness(0);
                Control.Padding = (Element as Entry).Padding.ToWindows();
                Control.Margin = new Thickness(0);
                Control.FontSize = e.NewElement.FontSize;
                Control.Foreground = new SolidColorBrush(e.NewElement.TextColor.ToWindows());
                Control.MinHeight = 0; 
                Control.Style = CreateSimpleSTyle(); 
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Entry.PaddingProperty.PropertyName)
            {
                Control.Padding = (Element as Entry).Padding.ToWindows();
            }
            else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
            {
                Control.FontSize = Element.FontSize;
            }
            else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
            {
                Control.Foreground = new SolidColorBrush(Element.TextColor.ToWindows());
            }
        }

        public static Style CreateSimpleSTyle()
        {
            const string xaml =
                @"<Style x:Key=""SimpleTextBoxStyle"" TargetType=""TextBox""
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                    <Setter Property=""Template"">
                        <Setter.Value>
                            <ControlTemplate TargetType=""TextBox"">
                                <ScrollViewer
                                    x:Name=""ContentElement""
                                    AutomationProperties.AccessibilityView=""Raw""
                                    HorizontalScrollBarVisibility=""{TemplateBinding ScrollViewer.HorizontalScrollBarVisibility}""
                                    HorizontalScrollMode=""{TemplateBinding ScrollViewer.HorizontalScrollMode}""
                                    IsDeferredScrollingEnabled=""{TemplateBinding ScrollViewer.IsDeferredScrollingEnabled}""
                                    IsHorizontalRailEnabled=""{TemplateBinding ScrollViewer.IsHorizontalRailEnabled}""
                                    IsTabStop=""False""
                                    IsVerticalRailEnabled=""{TemplateBinding ScrollViewer.IsVerticalRailEnabled}""
                                    Padding=""{TemplateBinding Padding}""
                                    VerticalScrollMode=""{TemplateBinding ScrollViewer.VerticalScrollMode}""
                                    VerticalScrollBarVisibility=""{TemplateBinding ScrollViewer.VerticalScrollBarVisibility}""
                                    ZoomMode=""Disabled"">
                                </ScrollViewer>
                            </ControlTemplate>
                        </Setter.Value>
                    </Setter>
                </Style>";
    
            var сt = (Style)XamlReader.Load(xaml);
            return сt;
        }
    }
}
