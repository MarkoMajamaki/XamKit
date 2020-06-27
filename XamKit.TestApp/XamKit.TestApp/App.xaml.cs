namespace XamKit.TestApp
{
    public partial class App : ShellApplication
    {
        public App()
        {
            InitializeComponent();
            InitializeShellApplication();

            Navigation.PushAsync(new ComponentsPage());
            MainMenuNavigation.PushAsync(new MainMenuPage());
        }
    }
}
