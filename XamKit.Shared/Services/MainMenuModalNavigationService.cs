using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamKit
{
    /// <summary>
    /// Navigation service for modal views which is over main menu and current page
    /// </summary>
    public class MainMenuModalNavigationService : IMainMenuModalNavigationService
    {
        private Dictionary<ContentPage, TaskCompletionSource<object>> m_modalTaskDictionary = new Dictionary<ContentPage, TaskCompletionSource<object>>();

        private Func<string, ContentPage> m_createPage = null;

        protected INavigation m_navigation = null;

        public MainMenuModalNavigationService(INavigation navigation, Func<string, XamKit.ContentPage> createPage)
        {
            m_navigation = navigation;
            m_createPage = createPage;
        }

        /// <summary>
        /// Navigate to new modal page. If current page has modal pages visible, then modal pages is closed 
        /// with current page.
        /// </summary>
        /// <param name="page">Page class name</param>
        /// <param name="parameter">The navigation parameter</param>
        public Task<object> NavigateModalAsync(string page, object parameter)
        {
            ContentPage currentModalPage = m_navigation.ModalNavigationStack.LastOrDefault();
            ContentPage nextModalPage = m_createPage(page);

            DissapearEventArgs dissapearArgs = new DissapearEventArgs(NavigationDirection.Out);
            AppearEventArgs appearArgs = new AppearEventArgs(NavigationDirection.In, parameter);

            NavigationPage.InvokeOnNavigationAwareElement(currentModalPage, v => v.OnDissapearing(dissapearArgs));
            NavigationPage.InvokeOnNavigationAwareElement(nextModalPage, v => v.OnAppearing(appearArgs));

            m_navigation.PushModalAsync(nextModalPage).ContinueWith((arg) =>
            {
                NavigationPage.InvokeOnNavigationAwareElement(currentModalPage, v => v.OnDissapeared(dissapearArgs));
                NavigationPage.InvokeOnNavigationAwareElement(nextModalPage, v => v.OnAppeared(appearArgs));
            });

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            m_modalTaskDictionary.Add(nextModalPage, tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Navigate back in current page modal navigation stack.
        /// </summary>
        /// <param name="closeModal">Close modal page</param>
        /// <param name="parameter">The navigation paremeter</param>
        public Task GoBackModalAsync(bool closeModal, object parameter = null)
        {
            ContentPage backOutPage = m_navigation.NavigationStack.LastOrDefault();
            ContentPage backInPage = null;

            if (m_navigation.NavigationStack.Count > 2 && closeModal == false)
            {
                backInPage = m_navigation.NavigationStack.ElementAt(m_navigation.NavigationStack.Count - 2);
            }

            DissapearEventArgs dissapearArgs = new DissapearEventArgs(NavigationDirection.Out);
            AppearEventArgs appearArgs = new AppearEventArgs(NavigationDirection.In, parameter);

            NavigationPage.InvokeOnNavigationAwareElement(backOutPage, v => v.OnDissapearing(dissapearArgs));
            NavigationPage.InvokeOnNavigationAwareElement(backInPage, v => v.OnAppearing(appearArgs));

            return m_navigation.PopModalAsync(closeModal).ContinueWith((arg) =>
            {
                NavigationPage.InvokeOnNavigationAwareElement(backOutPage, v => v.OnDissapeared(dissapearArgs));
                NavigationPage.InvokeOnNavigationAwareElement(backInPage, v => v.OnAppeared(appearArgs));

                TaskCompletionSource<object> tcs = m_modalTaskDictionary[backOutPage];
                m_modalTaskDictionary.Remove(backOutPage);
                tcs.SetResult(parameter);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            if (m_navigation.NavigationStack.Count > 0)
            {
                ContentPage currentPage = m_navigation.NavigationStack.Last();

                DissapearEventArgs dissapearArgs = new DissapearEventArgs(NavigationDirection.Out);

                NavigationPage.InvokeOnNavigationAwareElement(currentPage, v => v.OnDissapearing(dissapearArgs));
                NavigationPage.InvokeOnNavigationAwareElement(currentPage, v => v.OnDissapeared(dissapearArgs));
            }

            m_navigation.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CanGoBackModal()
        {
            return m_navigation.ModalNavigationStack.Count > 0;
        }
    }
}
