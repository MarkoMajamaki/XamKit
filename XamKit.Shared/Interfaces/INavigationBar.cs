using System.Collections.Generic;
using System.Windows.Input;

namespace XamKit
{
	/// <summary>
    /// Interface for NavigationPage navigation bar
	/// </summary>
	public interface INavigationBar
	{
        /// <summary>
        /// Initialize navigation bar with pages navigation history
        /// </summary>
        /// <param name="navigationHistory">Navigation stack</param>
        void Initialize(IList<ContentPage> navigationHistory);

        /// <summary>
        /// Push page to navigation stack
        /// </summary>
        void Push(ContentPage page);

        /// <summary>
        /// Push page to navigation stack root page
        /// </summary>
        void PushToRoot(ContentPage page);

        /// <summary>
        /// Pop page from navigatio nstack
        /// </summary>
        void Pop();

        /// <summary>
        /// Clear whole navigation history without animation
        /// </summary>
        void Clear();

		/// <summary>
		/// Previous page navigation button pressed command
		/// </summary>
		ICommand BackCommand { get; set; }

        /// <summary>
        /// Close all modal pages button
        /// </summary>
        ICommand CloseCommand { get; set; }

        /// <summary>
        /// Command to open main menu
        /// </summary>
        ICommand MenuCommand { get; set; }

        /// <summary>
        /// 0 -> 1
        /// </summary>
        void BackPan(double panX);
    }
}

