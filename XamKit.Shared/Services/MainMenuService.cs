using System;

namespace XamKit
{
	/// <summary>
	/// Service for MainMenu application
	/// </summary>
	public class MainMenuService : IMainMenuService
    {
        private IFlyoutMenu m_flyoutMenu = null;

        /// <summary>
        /// Navigation service for main menu
        /// </summary>
        public INavigationService MainMenuNavigationService { get; set; }

        /// <summary>
        /// Navigation service for submenu
        /// </summary>
        public INavigationService SubMenuNavigationService { get; set; }

        /// <summary>
        /// Navigation service for modal views which is over main menu and current page
        /// </summary>
        public IMainMenuModalNavigationService ModalNavigationService { get; set; }

        /// <summary>
        /// MainMenu open/close change animation is started
        /// </summary>
        public event EventHandler<bool> IsMainMenuOpenChanging;

        /// <summary>
        /// MainMenu open/close change animation is finished
        /// </summary>
        public event EventHandler<bool> IsMainMenuOpenChanged;

        /// <summary>
        /// SubMenu open/close change animation is started
        /// </summary>
        public event EventHandler<bool> IsSubMenuOpenChanging;

        /// <summary>
        /// SubMenu open/close change animation is finished
        /// </summary>
        public event EventHandler<bool> IsSubMenuOpenChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainMenuService(INavigation mainPage, INavigation subPage, INavigation fullScreenPage, IFlyoutMenu flyoutMenu, Func<string, XamKit.ContentPage> createPage)
        {
            m_flyoutMenu = flyoutMenu;

            flyoutMenu.IsMainMenuOpenChanging += OnIsMainMenuOpenChanging;
            flyoutMenu.IsMainMenuOpenChanged += OnIsMainMenuOpenChanged;
            flyoutMenu.IsSubMenuOpenChanging += OnIsSubMenuOpenChanging;
            flyoutMenu.IsSubMenuOpenChanged += OnIsSubMenuOpenChanged;

            MainMenuNavigationService = new NavigationService(mainPage, createPage);
            SubMenuNavigationService = new NavigationService(subPage, createPage);
            ModalNavigationService = new MainMenuModalNavigationService(fullScreenPage, createPage);
        }

        private void OnIsMainMenuOpenChanged(object sender, bool e)
        {
            IsMainMenuOpenChanged?.Invoke(sender, e);
        }

        private void OnIsMainMenuOpenChanging(object sender, bool e)
        {
            IsMainMenuOpenChanging?.Invoke(sender, e);
        }

        private void OnIsSubMenuOpenChanged(object sender, bool e)
        {
            IsSubMenuOpenChanged?.Invoke(sender, e);
        }

        private void OnIsSubMenuOpenChanging(object sender, bool e)
        {
            IsSubMenuOpenChanging?.Invoke(sender, e);
        }

        /// <summary>
        /// Is menu opened
        /// </summary>
        public bool IsMainMenuOpen
        {
            get { return m_flyoutMenu.IsMainMenuOpen; }
            set { m_flyoutMenu.IsMainMenuOpen = value; }
        }

        /// <summary>
        /// Is submenu opened
        /// </summary>
        public bool IsSubMenuOpen
        {
            get { return m_flyoutMenu.IsSubMenuOpen; }
            set { m_flyoutMenu.IsSubMenuOpen = value; }
        }
    }
}