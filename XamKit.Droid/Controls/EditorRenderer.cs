using System.ComponentModel;

// Android
using Android;
using Android.Content;
using Android.Widget;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(XamKit.Editor), typeof(XamKit.Droid.EditorRenderer))]

namespace XamKit.Droid
{
    public class EditorRenderer : Xamarin.Forms.Platform.Android.EditorRenderer
    {
        public EditorRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Editor> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
				Control.SetBackgroundColor(Color.Transparent.ToAndroid());

                if (Element is Editor editor)
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

            if (Element is Editor editor)
            {
                if (e.PropertyName == Editor.PaddingProperty.PropertyName)
                {
                    SetPadding(editor.Padding, editor);
                }
                else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
                {
                    Control.SetTextColor(editor.TextColor.ToAndroid());
                }
                else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
                {
                    Control.TextSize = (float)editor.FontSize;
                }
            }
        }

        private void SetPadding(Thickness padding, Editor editor)
        {
            int left = (int)ContextExtensions.ToPixels(Control.Context, (int)editor.Padding.Left);
            int top = (int)ContextExtensions.ToPixels(Control.Context, (int)editor.Padding.Top);
            int right = (int)ContextExtensions.ToPixels(Control.Context, (int)editor.Padding.Right);
            int bottom = (int)ContextExtensions.ToPixels(Control.Context, (int)editor.Padding.Bottom);

            Control.SetPadding(left, top, right, bottom);
        }
    }
}
