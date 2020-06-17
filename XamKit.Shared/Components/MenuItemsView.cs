using System;
using System.Threading.Tasks;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	/// <summary>
	/// ItemsView for Menu component
	/// </summary>
	public class MenuItemsView : ItemsView
	{
        /// <summary>
        /// Event when any menubutton submenu item is tapped
        /// </summary>
        public event EventHandler SubMenuItemTapped;

        private MenuButton _previousOpenedMenuButton = null;

        /// <summary>
        /// Close all submenus
        /// </summary>
        public void CloseSubMenus()
        {
            for (int i = 0; i < ItemsGenerator.TotalItemsCount; i++)
            {
                if (ItemsGenerator.HasItemViewGenerated(i))
                {
                    View itemContainer = ItemsGenerator.GetItemViewFromIndex(i);

                    if (itemContainer is MenuButton menuButton)
                    {
                        menuButton.IsOpen = false;
                    }
                }
            }
        }

        /// <summary>
        /// Do filter with search key string
        /// </summary>
        public void DoFilter(string searchKey)
        {
            for (int i = 0; i < ItemsGenerator.TotalItemsCount; i++)
            {
                if (ItemsGenerator.HasItemViewGenerated(i))
                {
                    View itemContainer = ItemsGenerator.GetItemViewFromIndex(i);

                    if ((itemContainer is IFilter filterItem && filterItem.IsFiltered(searchKey) == true) ||
                        (itemContainer.BindingContext is IFilter filterItemModel && filterItemModel.IsFiltered(searchKey) == true))
                    {
                        itemContainer.IsVisible = false;
                    }
                    else
                    {
                        itemContainer.IsVisible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Prepare item container with menu open events
        /// </summary>
        protected override void PrepareItemView(View itemContainer)
        {
            if (itemContainer is MenuButton menuButton)
            {
                menuButton.SubMenuItemTapped += OnSubMenuItemTapped;
                menuButton.IsOpenChanged += IsSubMenuOpen;
            }

            if (itemContainer is Button button)
            {
                button.IsMouseOverChanged += OnIsMouseOverItemChaneged;
            }
        }

        /// <summary>
        /// Event when mouse enter or leave over any item
        /// </summary>
        private async void OnIsMouseOverItemChaneged(object sender, bool isMouseOver)
        {
            if (sender is MenuButton menuButton)
            {
                if (isMouseOver)
                {
                    if (menuButton.ItemsSource != null && menuButton.ItemsSource.Count > 0)
                    {
                        if (_previousOpenedMenuButton != null)
                        {
                            _previousOpenedMenuButton.IsOpen = false;
                        }

                        await Task.Delay(300);

                        if (menuButton.IsMouseOver)
                        {
                            menuButton.IsOpen = true;
                        }
                    }
                    else if (_previousOpenedMenuButton != null)
                    {
                        _previousOpenedMenuButton.IsOpen = false;
                    }
                }
            }
        }

        /// <summary>
        /// Event when submenu opened
        /// </summary>
        private void IsSubMenuOpen(object sender, bool isOpen)
        {
            if (_previousOpenedMenuButton != null)
            {
                _previousOpenedMenuButton.IsOpen = false;
            }

            if (isOpen)
            {
                _previousOpenedMenuButton = sender as MenuButton;
            }
            else if (isOpen == false && _previousOpenedMenuButton == sender)
            {
                _previousOpenedMenuButton = null;
            }
        }

        /// <summary>
        /// Event when item container is trapped
        /// </summary>
        protected override void OnItemContainerTapped(View itemContainer)
        {
            base.OnItemContainerTapped(itemContainer);

            for (int i = 0; i < ItemsGenerator.TotalItemsCount; i++)
            {
            }

            if (SubMenuItemTapped != null)
            {
                SubMenuItemTapped(itemContainer, new EventArgs());
            }
        }

        private void OnSubMenuItemTapped(object sender, EventArgs e)
        {
            if (_previousOpenedMenuButton != null)
            {
                _previousOpenedMenuButton.IsToggled = false;
                _previousOpenedMenuButton = null;
            }

            if (SubMenuItemTapped != null)
            {
                SubMenuItemTapped(sender, e);
            }
        }
    }
}
