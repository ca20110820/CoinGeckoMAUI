using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CoinGeckoApp.Helpers;

namespace CoinGeckoApp.Settings
{
    /// <summary>
    /// Abstract base class for handling application settings.
    /// </summary>
    public abstract class SettingBase
    {
        protected FileSystemHelper fsHelper = new();
        protected JsonItemDBHelper jsonItemDBHelper = new();

        protected static string configFileName = "config.json";

        /// <summary>
        /// Resets the setting asynchronously.
        /// </summary>
        public abstract Task ResetSettingAsync();

        /// <summary>
        /// Reads the setting asynchronously.
        /// </summary>
        public abstract Task ReadAsync();

        /// <summary>
        /// Writes the setting asynchronously.
        /// </summary>
        public abstract Task WriteAsync();

        /// <summary>
        /// Updates the setting asynchronously.
        /// </summary>
        public abstract Task UpdateAsync();

        /// <summary>
        /// Writes a setting to the configuration file asynchronously.
        /// </summary>
        /// <typeparam name="SettingT">The type of the setting object.</typeparam>
        /// <param name="settingKey">The key of the setting.</param>
        /// <param name="settingObj">The setting object.</param>
        public static async Task WriteSettingAsync<SettingT>(string settingKey, SettingT settingObj)
        {
            FileSystemHelper fsHelper = new();
            JsonItemDBHelper jsonItemDBHelper = new();

            string settingFilePath = Path.Combine(fsHelper.AppDataDir, "Settings", configFileName);
            await fsHelper.CreateDirectoryInAppDataDirAsync("Settings");  // Try and Create the Settings Subdirectory if not exists
            await Task.Run(() => JsonHelper.CreateEmptyJson(settingFilePath));  // Try and create the Json File if not exists
            jsonItemDBHelper.JsonFilePath = settingFilePath;
            await jsonItemDBHelper.InsertObjAsync(settingKey, settingObj);
        }

        /// <summary>
        /// Updates a setting in the configuration file asynchronously.
        /// </summary>
        /// <typeparam name="SettingT">The type of the setting object.</typeparam>
        /// <param name="settingKey">The key of the setting.</param>
        /// <param name="settingObj">The setting object.</param>
        public static async Task WriteUpdateSettingAsync<SettingT>(string settingKey, SettingT settingObj)
        {
            FileSystemHelper fsHelper = new();
            JsonItemDBHelper jsonItemDBHelper = new();

            string settingFilePath = Path.Combine(fsHelper.AppDataDir, "Settings", configFileName);
            await fsHelper.CreateDirectoryInAppDataDirAsync("Settings");
            await Task.Run(() => JsonHelper.CreateEmptyJson(settingFilePath));  // Try and create the Json File if not exists
            jsonItemDBHelper.JsonFilePath = settingFilePath;
            await jsonItemDBHelper.ReplaceObjAsync(settingKey, settingObj);  // Use Replace to completely overwrite the object
        }

        /// <summary>
        /// Reads a setting from the configuration file asynchronously.
        /// </summary>
        /// <typeparam name="SettingT">The type of the setting object.</typeparam>
        /// <param name="settingKey">The key of the setting.</param>
        /// <returns>The setting object.</returns>
        public static async Task<SettingT> ReadSettingAsync<SettingT>(string settingKey)
        {
            FileSystemHelper fsHelper = new();
            JsonItemDBHelper jsonItemDBHelper = new();

            string settingFilePath = Path.Combine(fsHelper.AppDataDir, "Settings", configFileName);
            await fsHelper.CreateDirectoryInAppDataDirAsync("Settings");  // Try and Create the Settings Subdirectory if not exists
            await Task.Run(() => JsonHelper.CreateEmptyJson(settingFilePath));  // Try and create the Json File if not exists
            jsonItemDBHelper.JsonFilePath = settingFilePath;

            return await jsonItemDBHelper.GetObjAsync<SettingT>(settingKey);
        }

        /// <summary>
        /// Fetches the list of supported currencies from CoinGecko asynchronously.
        /// </summary>
        /// <returns>A list of supported currencies.</returns>
        public static async Task<List<string>?> FetchSupportedCurrenciesAsync()
        {
            // Fetch the List of Supported Currencies from CoinGecko
            string url = "https://api.coingecko.com/api/v3/simple/supported_vs_currencies";
            return await APIHelper.FetchAndJsonDeserializeAsync<List<string>>(url);
        }
    }

    /// <summary>
    /// Represents the user settings model.
    /// </summary>
    public class UserSettingModel : SettingBase
    {
        public bool DarkMode { get; set; }
        public string QuoteCurrency { get; set; }
        public int MaxFavourites { get; set; }
        public string ExchangeId { get; set; }


        private string settingKey = "user_setting";

        /// <summary>
        /// Sets the user preferences.
        /// </summary>
        /// <param name="darkMode">The dark mode setting.</param>
        /// <param name="quoteCurrency">The quote currency.</param>
        /// <param name="maxFavourites">The maximum number of favourites.</param>
        /// <param name="exchangeId">The exchange ID.</param>
        public static void SetPreferences(bool darkMode, string quoteCurrency, int maxFavourites, string exchangeId)
        {
            Preferences.Set("darkmode", darkMode);
            Preferences.Set("quotecurrency", quoteCurrency);
            Preferences.Set("maxfavourites", maxFavourites);
            Preferences.Set("exchangeid", exchangeId);
        }

        /// <summary>
        /// Sets the current user settings based on the preferences.
        /// </summary>
        public void SetCurrentUserSettings()
        {
            SetPreferences(DarkMode, QuoteCurrency, MaxFavourites, ExchangeId);
        }

        /// <summary>
        /// Resets the user settings to their default values asynchronously.
        /// </summary>
        public override async Task ResetSettingAsync()
        {
            Preferences.Default.Remove("darkmode");
            Preferences.Default.Remove("quotecurrency");
            Preferences.Default.Remove("exchangeid");
            Preferences.Default.Remove("maxfavourites");

            DarkMode = false;
            QuoteCurrency = "usd";
            MaxFavourites = 15;
            ExchangeId = "binance";

            SetPreferences(DarkMode, QuoteCurrency, MaxFavourites, ExchangeId);

            await WriteAsync();  // Write immediately
        }

        /// <summary>
        /// Reads the user settings asynchronously.
        /// </summary>
        /// <returns>The user setting model.</returns>
        public override async Task<UserSettingModel> ReadAsync()
        {
            return await ReadSettingAsync<UserSettingModel>(settingKey);
        }

        /// <summary>
        /// Writes the user settings asynchronously.
        /// </summary>
        public override async Task WriteAsync()
        {
            await WriteSettingAsync(settingKey, this);
        }

        /// <summary>
        /// Updates the user settings asynchronously.
        /// </summary>
        public override async Task UpdateAsync()
        {
            await WriteUpdateSettingAsync(settingKey, this);
        }

        /// <summary>
        /// Toggles the dark mode setting and updates the preferences asynchronously.
        /// </summary>
        public async Task SwitchDarkMode()
        {
            DarkMode = !DarkMode;
            Preferences.Set("darkmode", DarkMode);
            await UpdateAsync();
        }

        /// <summary>
        /// Changes the quote currency and updates the preferences asynchronously.
        /// </summary>
        /// <param name="newQuoteCurrency">The new quote currency.</param>
        public async Task ChangeQuoteCurrencyTo(string newQuoteCurrency)
        {
            // TODO: Validate from Web Server.
            QuoteCurrency = newQuoteCurrency;
            Preferences.Set("quotecurrency", QuoteCurrency);
            await UpdateAsync();
        }

        /// <summary>
        /// Changes the exchange ID and updates the preferences asynchronously.
        /// </summary>
        /// <param name="newExchangeId">The new exchange ID.</param>
        public async Task ChangeExchangeIdTo(string newExhangeId)
        {
            ExchangeId = newExhangeId;
            Preferences.Set("exchangeid", ExchangeId);
            await UpdateAsync();
        }
    }
}
