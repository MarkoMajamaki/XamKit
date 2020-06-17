using System;
using System.Threading.Tasks;

namespace XamKit
{
    public class NavigationService : INavigationService
	{
        private Func<string, ContentPage> _createPageFunc = null;
        protected INavigation _navigation = null;

		/// <summary>
		/// Constructor
		/// </summary>
        public NavigationService(INavigation navigationPage, Func<string, ContentPage> createPageFunc)
		{
            _navigation = navigationPage;
            _createPageFunc = createPageFunc;
        }

        #region INavigationService

        /// <summary>
        /// Navigate to new page.
        /// </summary>
        /// <param name="parameter">The navigation parameter</param>
        /// <param name="page">Page class name</param>
        public Task NavigateAsync(string page, object parameter = null)
		{
            return ProcessNavigation(page, parameter, _navigation);
		}

        /// <summary>
        /// Navigate to new modal page. If current page has modal pages visible, then modal pages is closed 
        /// with current page.
        /// </summary>
        /// <param name="page">Page class name</param>
        /// <param name="parameter">The navigation parameter</param>
        /// <returns>Parameter when modal page is navigated back</returns>
        public Task<object> NavigateModalAsync(string page, object parameter = null)
		{
            return ProcessModalNavigation(page, parameter, _navigation);
		}

        /// <summary>
        /// Navigates back in the navigation history by popping the calling Page off the navigation 
        /// stack. I current page has modal pages visible, then modal pages is closed with current page.
        /// </summary>
        /// <param name="parameter">The navigation parameter to new page</param>
        public Task GoBackAsync(object parameter = null)
        {
            return ProcessBackNavigationAsync(parameter, _navigation);
        }

        /// <summary>
        /// Navigate back in current page modal navigation stack.
        /// </summary>
        /// <param name="closeModal">Close modal page</param>
        /// <param name="parameter">The navigation paremeter</param>
        public Task GoBackModalAsync(bool closeModal, object parameter = null)
        {
            return ProcessModalBackNavigationAsync(closeModal, parameter, _navigation);
        }

        /// <summary>
        /// Clear navigation stack and current page immediatley without animation
        /// </summary>
		public void Clear()
		{
            _navigation.Clear();
        }

		#endregion

        /// <summary>
        /// Process actual navigation
        /// </summary>
        /// <param name="page">Page class name</param>
        /// <param name="parameter">Navigation parameters.</param>
        /// <param name="navigationTarget">Target navigation UI element</param>
        protected Task ProcessNavigation(string page, object parameter, INavigation navigationTarget)
        {
            ContentPage nextPage = _createPageFunc(page);
            return navigationTarget.PushAsync(nextPage, parameter);
        }

        /// <summary>
        /// Process modal navigation
        /// </summary>
        /// <param name="page">Page class name</param>
        /// <param name="parameter">Navigation parameter.</param>
        /// <param name="navigationTarget">Target navigation UI element</param>
        protected Task<object> ProcessModalNavigation(string page, object parameter, INavigation navigationTarget)
        {
            ContentPage nextModalPage = _createPageFunc(page);
            return navigationTarget.PushModalAsync(nextModalPage, parameter);
        }

        /// <summary>
        /// Process back navigation. If modal pages visible then remove all modal pages first.
        /// </summary>
        /// <param name="parameter">Navigation parameter</param>
        /// <param name="navigationTarget">Navigation page for NOT modal pages</param>
        protected Task ProcessBackNavigationAsync(object parameter, INavigation navigationTarget)
		{
			return navigationTarget.PopAsync(parameter);
        }

        /// <summary>
        /// Process modal back navigation
        /// </summary>
        /// <param name="popAllModalPages">Is all modal pages closed</param>
        /// <param name="parameter">Navigation parameter</param>
        /// <param name="navigationTarget">Navigation page for NOT modal pages</param>
        protected Task ProcessModalBackNavigationAsync(bool popAllModalPages, object parameter, INavigation navigationTarget)
        {
            return _navigation.PopModalAsync(parameter, popAllModalPages);
        }
	}
}

