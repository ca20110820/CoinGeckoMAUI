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

    public abstract class SettingBase
    {
        protected FileSystemHelper fsHelper = new();
        protected JsonItemDBHelper jsonItemDBHelper = new();

        protected static string configFileName = "config.json";
        //protected string settingKey;

        //public SettingBase(string settingKey)
        //{
        //    this.settingKey = settingKey;
        //}

        public abstract Task ResetSettingAsync();
        public abstract Task ReadAsync();
        public abstract Task WriteAsync();
        public abstract Task UpdateAsync();


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
    }

    public class UserSetting : SettingBase
    {
        public bool DarkMode { get; set; }
        public string QuoteCurrency { get; set; }
        public int MaxFavourites { get; set; }
        public string ExchangeId { get; set; }


        private string settingKey = "user_setting";

        public override async Task ResetSettingAsync()
        {
            DarkMode = false;
            QuoteCurrency = "usd";
            MaxFavourites = 15;
            ExchangeId = "binance";

            Preferences.Set("darkmode", DarkMode);
            Preferences.Set("quotecurrency", QuoteCurrency);
            Preferences.Set("maxfavourites", MaxFavourites);
            Preferences.Set("exchangeid", ExchangeId);
            //Preferences.Set("maxdays", 365);  // This should be done in App.xaml.cs

            await WriteAsync();  // Write immediately
        }

        public override async Task<UserSetting> ReadAsync()
        {
            return await ReadSettingAsync<UserSetting>(settingKey);
        }

        public override async Task WriteAsync()
        {
            await WriteSettingAsync(settingKey, this);
        }

        public override async Task UpdateAsync()
        {
            await WriteUpdateSettingAsync(settingKey, this);
        }

        public async Task SwitchDarkMode()
        {
            DarkMode = !DarkMode;
            Preferences.Set("darkmode", DarkMode);
            await UpdateAsync();
        }

        public async Task ChangeQuoteCurrencyTo(string newQuoteCurrency)
        {
            // TODO: Validate from Web Server.
            QuoteCurrency = newQuoteCurrency;
            Preferences.Set("quotecurrency", QuoteCurrency);
            await UpdateAsync();
        }

        public async Task ChangeExchangeIdTo(string newExhangeId)
        {
            // TODO: Validate from Web Server.
            ExchangeId = newExhangeId;
            Preferences.Set("exchangeid", ExchangeId);
            await UpdateAsync();
        }
    }
}
