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
    public class SettingModel
    {
        private FileSystemHelper fsService = new();
        private JsonItemDBHelper jsonItemDBService = new();

        public bool DarkMode { get; set; }
        public string QuoteCurrency { get; set; }
        public int MaxFavourites { get; set; }
        public string ExchangeId { get; set; }

        public SettingModel() {}

        public async Task ResetSetting()
        {
            DarkMode = false;
            QuoteCurrency = "usd";
            MaxFavourites = 15;
            ExchangeId = "binance";

            // Write
            await WriteSetting();
        }

        public async Task WriteSetting()
        {
            string settingFilePath = Path.Combine(fsService.AppDataDir, "Settings", "config.json");
            await Task.Run(() => JsonHelper.CreateEmptyJson(settingFilePath));  // Try and create the Json File if not exists
            jsonItemDBService.JsonFilePath = settingFilePath;

            /* Example:
             * --------
             * "user_setting": {"darkmode": false, "quotecurrency": "usd", "maxfavourites": 15, exchangeid: "binance"}
             */
            await jsonItemDBService.ReplaceObjAsync("user_setting", this);
        }

        private async Task WriteSettingWithKeyAndObj<T>(string configKey, T obj)
        {
            string settingFilePath = Path.Combine(fsService.AppDataDir, "Settings", "config.json");
            await Task.Run(() => JsonHelper.CreateEmptyJson(settingFilePath));  // Try and create the Json File if not exists
            jsonItemDBService.JsonFilePath = settingFilePath;

            /* Example:
             * --------
             * "user_setting": {"darkmode": false, "quotecurrency": "usd", "maxfavourites": 15, exchangeid: "binance"}
             */
            await jsonItemDBService.ReplaceObjAsync(configKey, obj);
        }

        public static async Task<SettingModel> ReadSetting()
        {
            FileSystemHelper fsService = new();
            JsonItemDBHelper jsonItemDBService = new();

            string settingFilePath = Path.Combine(fsService.AppDataDir, "Settings", "config.json");
            await Task.Run(() => JsonHelper.CreateEmptyJson(settingFilePath));  // Try and create the Json File if not exists
            jsonItemDBService.JsonFilePath = settingFilePath;

            return await jsonItemDBService.GetObjAsync<SettingModel>("user_setting");
        }

        public async Task SwitchDarkMode()
        {
            DarkMode = !DarkMode;
            await WriteSetting();
        }

        public async Task ChangeQuoteCurrencyTo(string newQuoteCurrency)
        {
            // TODO: Validate from Web Server.
            QuoteCurrency = newQuoteCurrency;
            
            await WriteSetting();
        }

        public async Task ChangeExchangeIdTo(string newExhangeId)
        {
            // TODO: Validate from Web Server.
            ExchangeId = newExhangeId;
            
            await WriteSetting();
        }

        public async Task RefreshSupportedCurrencies()
        {
            // TODO: Implement RefreshSupportedCurrencies.

            /* Notes:
             * - Fetch from web server using "https://api.coingecko.com/api/v3/simple/supported_vs_currencies"
             * - Immediately store in config.json with key "supported_currencies"
             */

            List<string>? supportedCurrencies = await APIHelper.FetchAndJsonDeserializeAsync<List<string>>("https://api.coingecko.com/api/v3/simple/supported_vs_currencies");

            if (supportedCurrencies == null)
            {
                // TODO: Log the error
                Trace.WriteLine("The fetched supported currencies is null!");
                return;
            }

            await WriteSettingWithKeyAndObj("supported_currencies", supportedCurrencies);
        }

        public async Task<List<string>> ReadSupportedCurrencies()
        {
            return await jsonItemDBService.GetObjAsync<List<string>>("supported_currencies");
        }
    }
}
