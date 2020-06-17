using System;
using System.Threading.Tasks;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// Interface for FlyoutMenu
    /// </summary>
    public interface IFlyoutMenu
    {
        View MainMenu { get; set; }
        View SubMenu { get; set; }
        View Content { get; set; }
        
        bool IsSubMenuOpen { get; set; }
		bool IsMainMenuOpen { get; set; }

        Task SetIsMainMenuOpenAsync(bool isOpen);
        Task SetIsSubMenuOpenAsync(bool isOpen);

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
    }
}