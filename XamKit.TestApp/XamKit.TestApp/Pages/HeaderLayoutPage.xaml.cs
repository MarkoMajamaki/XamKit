using System;
using Xamarin.Forms;

namespace XamKit.TestApp
{
    public partial class HeaderLayoutPage : ContentPage
    {
        private View _scrollViewInsideHeaderLayout;
        private View _headerLayoutInsideScrollView;

        public HeaderLayoutPage()
        {
            InitializeComponent();

            _scrollViewInsideHeaderLayout = (Resources["ScrollViewInsideHeaderLayout"] as DataTemplate).CreateContent() as View;
            _headerLayoutInsideScrollView = (Resources["HeaderLayoutInsideScrollView"] as DataTemplate).CreateContent() as View;

            Content = _scrollViewInsideHeaderLayout;

            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            _scrollView.HeightRequest = Height / 2;
        }

        private void HeaderLayoutModeChanged(object sender, bool e)
        {
            if (sender is RadioButton button)
            {
                if (button.Tag == "1")
                {
                    Content = _scrollViewInsideHeaderLayout;
                }
                else
                {
                    Content = _headerLayoutInsideScrollView;
                }
            }
        }

        private void NavigationBarVisibilityChanged(object sender, bool e)
        {
            if (sender is RadioButton button)
            {
                if (button.Tag == "1")
                {
                    NavigationBar.SetVisibility(this, NavigationBarVisibility.Visible);
                }
                else if (button.Tag == "2")
                {
                    NavigationBar.SetVisibility(this, NavigationBarVisibility.Hidden);
                }
                else if (button.Tag == "3")
                {
                    NavigationBar.SetVisibility(this, NavigationBarVisibility.Scroll);
                }
                else if (button.Tag == "4")
                {
                    NavigationBar.SetVisibility(this, NavigationBarVisibility.SmoothScroll);
                }
            }
        }

        private void IsNavigationBarFloatingChanged(object sender, bool e)
        {
            NavigationBar.SetIsFloating(this, e);
        }
    }
}
