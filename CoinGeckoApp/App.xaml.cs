using System.Diagnostics;
using CoinGeckoApp.Helpers;
using CoinGeckoApp.Models;
using CoinGeckoApp.Settings;

namespace CoinGeckoApp
{
    public partial class App : Application
    {
        FileSystemHelper fsHelper = new();
        JsonHelper jsonHelper = new();

        public List<string>? ExchangeIds { get; set; }
        public List<string>? SupportedCurrencies { get; set; }

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            //await CleanUpTesting();
            await InitFileStructure();
            await InitSettings();
        }

        private async Task CleanUpTesting()
        {
            /* Remove Directories and Files for testing.
             */
            await fsHelper.RemoveDirectoryContentsFromAppDataDir("Settings");  // Cleanup AppDirectory for Testing
        }

        private async Task InitSettings()
        {
            // Initialize Supported Currencies
            await InitSupportedCurrencies();

            // Initialize Exchange Ids
            await InitExchangeIds();

            // Initialize User Settings
            await InitUserSetting();

            // Initialize other Default Preferences not from config.json
            Preferences.Set("application_name", "CoinGeckoMAUIApp");
            Preferences.Set("maxdays", 360);  // max historical data from CoinGecko for free API
        }
        public async Task InitUserSetting()
        {
            // Try and Read the Settings from config.json, if exist and available
            UserSettingModel userSetting = new();
            try
            {
                // Try and read the settings from config.json
                userSetting = await userSetting.ReadAsync();
            }
            catch (KeyNotFoundException ex)
            {
                // Set the User Settings to Default and then Write to config.json
                await userSetting.ResetSettingAsync();
            }

            // Set the settings to Preferences
            userSetting.SetCurrentUserSettings();
        }
        private async Task InitSupportedCurrencies()
        {
            // Get Supported Currencies
            SupportedCurrencies = await Task.Run(() => SettingBase.FetchSupportedCurrenciesAsync());

            // Write to config.json
            try
            {
                await SettingBase.WriteSettingAsync("supported_currencies", SupportedCurrencies);
            }
            catch (KeyNotFoundException ex)
            {
                await SettingBase.WriteUpdateSettingAsync("supported_currencies", SupportedCurrencies);
            }
        }
        private async Task InitExchangeIds()
        {
            // Get Exhange Ids
            ExchangeIds = await ExchangeModel.GetExchangeIds();

            // Write to config.json
            try
            {
                await SettingBase.WriteSettingAsync("exchangeids", ExchangeIds);
            }
            catch (KeyNotFoundException ex)
            {
                await SettingBase.WriteUpdateSettingAsync("exchangeids", ExchangeIds);
            }
        }


        private async Task InitFileStructure()
        {
            Trace.WriteLine("Initializing CoinGecko MAUI App File Structure ...");
            
            // Create Subdirectories in AppData
            await fsHelper.CreateDirectoryInAppDataDirAsync("Settings");  // Try to create the "Settings Subdirectory"
            await fsHelper.CreateDirectoryInAppDataDirAsync("Databases");  // Try to create the "Database Subdirectory"
            await fsHelper.CreateDirectoryInAppDataDirAsync("Logs");  // Try to create the "Logs Subdirectory"

            // Create Subdirectories in Cache
            // ...

            
            // Initialize config.json
            string configJsonPath = Path.Combine(fsHelper.AppDataDir, "Settings", "config.json");
            await Task.Run(() => {
                JsonHelper.CreateEmptyJson(configJsonPath);
            });

            // Initialize favourites.db
            string favouritesDbPath = Path.Combine(fsHelper.AppDataDir, "Databases", "favourites.db");
            SQLiteHelper sqlHelper = new(favouritesDbPath);
            await sqlHelper.CreateTableAsync("favourites",
                "id TEXT PRIMARY KEY NOT NULL UNIQUE",
                "favourite INTEGER NOT NULL CHECK (favourite IN (0, 1))");
        }

        protected override async void OnSleep()
        {
        }

        protected override async void OnResume()
        {
        }
    }
}
