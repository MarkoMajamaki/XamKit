using System;
using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XamKit.TestApp
{
    public partial class CarouselLayoutPage : ContentPage
    {
        public CarouselLayoutPage()
        {
            InitializeComponent();

            _carouselView.ItemsSource = CreateItems(100);
            BindableLayout.SetItemsSource(_bindableCarouselLayout, CreateItems(10));
        }

        private IList CreateItems(int count)
        {
            List<string> list = new List<string>();

            for (int i = 0; i < count; i++)
            {
                list.Add(i.ToString());
            }

            return list;
        }
    }
}
