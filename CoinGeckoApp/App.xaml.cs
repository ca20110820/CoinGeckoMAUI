namespace CoinGeckoApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
        }

        protected override async void OnSleep()
        {
        }

        protected override async void OnResume()
        {
        }
    }
}
