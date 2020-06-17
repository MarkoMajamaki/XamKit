using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// Default menu item container template selector for Menu
    /// </summary>
    public class DefaultMenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MenuItemTemplate { get; set; }

        public DataTemplate MenuItemSeparatorTemplate { get; set; }
        
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is IMenuItemSeparator)
            {
                return MenuItemSeparatorTemplate;
            }
            else
            {
                return MenuItemTemplate;
            }
        }
    }
}
