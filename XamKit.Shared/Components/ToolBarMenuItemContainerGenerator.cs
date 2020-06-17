using System;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// Default implementation for toolbar menu item generator
    /// </summary>
    public class ToolBarMenuItemContainerGenerator : IMenuItemContainerGenerator
    {
        public DataTemplate MenuItemButtonTemplate { get; set; }

        public virtual View GenerateContainer(object item)
        {
            if (item is Button button)
            {
                Button menuItemButton = MenuItemButtonTemplate.CreateContent() as Button;
                InitializeBinding(item, menuItemButton);
                return menuItemButton;
            }
            else if (item is IMenuItem)
            {
                Button menuItemButton = new Button();

                object style = null;
                if (Application.Current.Resources.TryGetValue("Button.MenuStyle", out style))
                {
                    menuItemButton.Style = style as Style;
                }

                InitializeBinding(item, menuItemButton);
                return menuItemButton;
            }
            /*else if (item is ButtonDefaultModel)
            {
            }*/

            // throw new NotImplementedException("No support for item type!");
            return null;
        }

        private void InitializeBinding(object source, Button target)
        {
            Binding bind = new Binding("Text");
            bind.Source = source;
            bind.Mode = BindingMode.TwoWay;
            target.SetBinding(Button.TextProperty, bind);

            bind = new Binding("ExtraText");
            bind.Source = source;
            bind.Mode = BindingMode.TwoWay;
            target.SetBinding(Button.ExtraTextProperty, bind);

            bind = new Binding("Command");
            bind.Source = source;
            bind.Mode = BindingMode.TwoWay;
            target.SetBinding(Button.CommandProperty, bind);

            bind = new Binding("IconResourceKey");
            bind.Source = source;
            bind.Mode = BindingMode.TwoWay;
            target.SetBinding(Button.IconResourceKeyProperty, bind);

            bind = new Binding("IconAssemblyName");
            bind.Source = source;
            bind.Mode = BindingMode.TwoWay;
            target.SetBinding(Button.IconAssemblyNameProperty, bind);
        }
    }
}
