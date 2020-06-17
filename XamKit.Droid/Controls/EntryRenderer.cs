using System.ComponentModel;

// Android
using Android.Content;
using Android.Widget;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(XamKit.Entry), typeof(XamKit.Droid.EntryRenderer))]

namespace XamKit.Droid
{
    public class EntryRenderer : Xamarin.Forms.Platform.Android.EntryRenderer
    {
        public EntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.SetBackgroundColor(Color.Transparent.ToAndroid());

                if (Element is Entry editor)
                {
                    SetPadding(editor.Padding, editor);
                    Control.SetTextColor(editor.TextColor.ToAndroid());
                    Control.TextSize = (float)editor.FontSize;
                }
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Element is Entry editor)
            {
                if (e.PropertyName == Entry.PaddingProperty.PropertyName)
                {
                    SetPadding(editor.Padding, editor);
                }
                else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
                {
                    Control.SetTextColor(editor.TextColor.ToAndroid());
                }
                else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
                {
                    Control.TextSize = (float)editor.FontSize;
                }
            }
        }

        private void SetPadding(Thickness padding, Entry editor)
        {
            int left = (int)ContextExtensions.ToPixels(Control.Context, (int)editor.Padding.Left);
            int top = (int)ContextExtensions.ToPixels(Control.Context, (int)editor.Padding.Top);
            int right = (int)ContextExtensions.ToPixels(Control.Context, (int)editor.Padding.Right);
            int bottom = (int)ContextExtensions.ToPixels(Control.Context, (int)editor.Padding.Bottom);

            Control.SetPadding(left, top, right, bottom);
        }
    }
}
