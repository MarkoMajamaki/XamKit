using Xamarin.Forms;

namespace XamKit
{
    public class Editor : Xamarin.Forms.Editor
    {
        public static readonly BindableProperty PaddingProperty =
            BindableProperty.Create("Padding", typeof(Thickness), typeof(Editor), new Thickness(0));

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public Editor()
        {
        }
    }
}
