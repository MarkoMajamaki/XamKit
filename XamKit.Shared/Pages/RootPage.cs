using System;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// Root content page which handles back button navigation
    /// </summary>
    public class RootPage : Xamarin.Forms.ContentPage
    {
        /// <summary>
        /// Set action to return true if navigation to out from the app is ignored
        /// </summary>
        public event Func<bool> DeviceBackButtonPressed;

        public RootPage()
        {
        }

        public RootPage(View content)
        {
            Content = content;
        }

        protected override bool OnBackButtonPressed()
        {
            if (DeviceBackButtonPressed != null)
            {
                return DeviceBackButtonPressed();
            }

            return base.OnBackButtonPressed();
        }
    }
}
