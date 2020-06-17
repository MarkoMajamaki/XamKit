using System.Threading.Tasks;

namespace XamKit
{
    /// <summary>
    /// Navigation service for dialogs which is over main menu and current page
    /// </summary>
    public interface IMainMenuModalNavigationService
    {
        /// <summary>
        /// Navigate to new modal page. If current page has modal pages visible, then modal pages is closed 
        /// with current page.
        /// </summary>
        /// <param name="page">Page class name</param>
        /// <param name="parameter">The navigation parameter</param>
        Task<object> NavigateModalAsync(string page, object parameter = null);

        /// <summary>
        /// Navigate back in current page modal navigation stack.
        /// </summary>
        /// <param name="closeModal">Close modal page</param>
        /// <param name="parameter">The navigation paremeter</param>
        Task GoBackModalAsync(bool closeModal, object parameter = null);

        /// <summary>
        /// Clear navigation stack and current page immediatley without animation
        /// </summary>
        void Clear();
    }
}
