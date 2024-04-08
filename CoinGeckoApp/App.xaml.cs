using System.Diagnostics;
using CoinGeckoApp.Helpers;

namespace CoinGeckoApp
{
    public partial class App : Application
    {
        FileSystemHelper fsHelper = new();
        JsonHelper jsonHelper = new();

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            await InitFileStructure();
            await InitSettings();
        }

        private async Task InitSettings()
        {
            // Try and Read the Settings from config.json, if exist and available
            // ...

            // If not exist or available, then set the default and write to file
            // ...

            // Set the settings
            // ...
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
