using Xamarin.Forms;

namespace XamKit
{
    public class Entry : Xamarin.Forms.Entry
    {
        public static readonly BindableProperty PaddingProperty =
            BindableProperty.Create("Padding", typeof(Thickness), typeof(Entry), new Thickness(0));

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public Entry()
        {
        }
    }
}
