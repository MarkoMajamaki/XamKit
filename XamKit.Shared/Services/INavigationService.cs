using System;
using System.Threading.Tasks;

namespace XamKit
{
	public interface INavigationService
	{
        /// <summary>
        /// Navigate to new page.
        /// </summary>
        /// <param name="parameter">The navigation parameter</param>
        /// <param name="page">Page class name</param>
        Task NavigateAsync(string page, object parameter = null);

        /// <summary>
        /// Navigate to new modal page. If current page has modal pages visible, then modal pages is closed 
        /// with current page.
        /// </summary>
        /// <param name="page">Page class name</param>
        /// <param name="parameter">The navigation parameter</param>
        /// <returns>Parameter when modal page is navigated back</returns>
        Task<object> NavigateModalAsync(string page, object parameter = null);

        /// <summary>
        /// Navigates back in the navigation history by popping the calling Page off the navigation 
        /// stack. I current page has modal pages visible, then modal pages is closed with current page.
        /// </summary>
        /// <param name="parameter">The navigation parameter to new page</param>
        Task GoBackAsync(object parameter = null);

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

