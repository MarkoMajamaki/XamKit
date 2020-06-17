using System;

namespace XamKit
{
	/// <summary>
	/// Navigation service for MainMenu application
	/// </summary>
	public interface IMainMenuService
	{
        /// <summary>
        /// Navigation service for main menu
        /// </summary>
        INavigationService MainMenuNavigationService { get; set; }

        /// <summary>
        /// Navigation service for submenu
        /// </summary>
        INavigationService SubMenuNavigationService { get; set; }

        /// <summary>
        /// Navigation service for full screen modal views
        /// </summary>
        IMainMenuModalNavigationService ModalNavigationService { get; set; }

        /// <summary>
        /// MainMenu open/close animation is started
        /// </summary>
        event EventHandler<bool> IsMainMenuOpenChanging;

        /// <summary>
        /// MainMenu open/close animation is finished
        /// </summary>
        event EventHandler<bool> IsMainMenuOpenChanged;

        /// <summary>
        /// SubMenu open/close animation is started
        /// </summary>
        event EventHandler<bool> IsSubMenuOpenChanging;

        /// <summary>
        /// SubMenu open/close animation is finished
        /// </summary>
        event EventHandler<bool> IsSubMenuOpenChanged;

        /// <summary>
        /// Is menu open
        /// </summary>
        bool IsMainMenuOpen { get; set; }

        /// <summary>
        /// Is submenu open
        /// </summary>
        bool IsSubMenuOpen { get; set; }
    }
}

