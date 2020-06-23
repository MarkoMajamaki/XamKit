using System;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace XamKit
{
    /// <summary>
    /// Root content page which handles back button navigation
    /// </summary>
    public class RootPage : Xamarin.Forms.ContentPage
    {
        private static RootPage _rootPage;
        public static RootPage Instance
        {
            get
            {
                if (_rootPage == null)
                {
                    _rootPage = new RootPage();
                }
                return _rootPage;
            }
        }

        public Thickness SafeAreaInsest { get; set; }

        /// <summary>
        /// Set action to return true if navigation to out from the app is ignored
        /// </summary>
        public event Func<bool> DeviceBackButtonPressed;

        /// <summary>
        /// Event when SafeAreaInsest changes
        /// </summary>
        public event EventHandler<Thickness> SafeAreaInsetsChanged;

        private RootPage()
        {
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(false);
            SafeAreaInsest = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
        }

        public RootPage(View content)
        {
            Content = content;
        }

        public void SafeAreaUpdated()
        {
            SafeAreaInsest = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();

            SafeAreaInsetsChanged?.Invoke(this, SafeAreaInsest);
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
