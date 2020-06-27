using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;

namespace XamKit.TestApp
{
    [DesignTimeVisible(false)]
    public partial class ComponentsPage : ContentPage
    {
        public ComponentsPage()
        {
            InitializeComponent();

            _tabBar.Source = _tabView;
            _componentsItemsView.ItemsSource = CreateComponentsList();
            _layoutsItemsView.ItemsSource = CreateLayoutList();
            _shellItemsView.ItemsSource = CreateShellList();
        }

        private IEnumerable CreateShellList()
        {
            ObservableCollection<object> list = new ObservableCollection<object>();

            list.Add(new
            {
                Name = "Dialogs",
                Icon = "Icons.texture-24px.svg"
            });

            list.Add(new
            {
                Name = "BackButton",
                Icon = "Icons.texture-24px.svg"
            });

            list.Add(new
            {
                Name = "NavigationBar",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "Navigation",
                Icon = "Icons.texture-24px.svg"
            });

            list.Add(new
            {
                Name = "Tabs",
                Icon = "Icons.texture-24px.svg"
            });

            return list;
        }

        private ObservableCollection<object> CreateLayoutList()
        {
            ObservableCollection<object> list = new ObservableCollection<object>();

            list.Add(new
            {
                Name = "CarouselLayout",
                Icon = "Icons.texture-24px.svg",
                Command = new Command(() => Navigation.PushAsync(new CarouselLayoutPage()))
            });
            list.Add(new
            {
                Name = "DockLayout",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "ExpanderLayout",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "FlyoutMenu",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "HeaderLayout",
                Icon = "Icons.texture-24px.svg",
                Command = new Command(() => Navigation.PushAsync(new HeaderLayoutPage()))
            });
            list.Add(new
            {
                Name = "ParallaxLayout",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "StackLayout",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "WrapLayout",
                Icon = "Icons.texture-24px.svg"
            });

            return list;
        }

        public ObservableCollection<object> CreateComponentsList()
        {
            ObservableCollection<object> list = new ObservableCollection<object>();

            list.Add(new
            {
                Name = "ActivityIndicator",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "Border",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "Buttons",
                Icon = "Icons.texture-24px.svg",
                Command = new Command(() => Navigation.PushAsync(new ButtonsPage()))
            });
            list.Add(new
            {
                Name = "CarouselView",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "CheckBox",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "DropDown",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "MenuButton",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "ParallaxImage",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "Popup",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "RadioButton",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "Separator",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "ShadowView",
                Icon = "Icons.texture-24px.svg",
                Command = new Command(() => Navigation.PushAsync(new ShadowViewPage()))
            });
            list.Add(new
            {
                Name = "SvgImage",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "Switch",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "TextBox",
                Icon = "Icons.texture-24px.svg"
            });
            list.Add(new
            {
                Name = "ToggleButton",
                Icon = "Icons.texture-24px.svg",
                Command = new Command(() => Navigation.PushAsync(new ToggleButtonPage()))
            });
            list.Add(new
            {
                Name = "ToolBar",
                Icon = "Icons.texture-24px.svg"
            });

            return list;
        }
    }
}
