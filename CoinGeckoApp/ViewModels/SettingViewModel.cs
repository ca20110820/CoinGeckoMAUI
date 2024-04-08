using CoinGeckoApp.Helpers;
using CoinGeckoApp.Models;
using CoinGeckoApp.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.ViewModels
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        private const string applicationName = "CoinGeckoMAUIApp";

        private UserSettingModel _userSetting = new();
        public UserSettingModel UserSetting
        {
            get => _userSetting;
            set
            {
                _userSetting = value;
                OnPropertyChanged(nameof(UserSetting));
            }
        }

        private List<string> _supportedCurrencies = new List<string>();
        public List<string> SupportedCurrencies
        {
            get => _supportedCurrencies;
            set
            {
                _supportedCurrencies = value;
                OnPropertyChanged(nameof(SupportedCurrencies));
            }
        }

        private List<string> _exchangeIds = new List<string>();
        public List<string> ExchangeIds
        {
            get => _exchangeIds;
            set
            {
                _exchangeIds = value;
                OnPropertyChanged(nameof(ExchangeIds));
            }
        }


        /// <summary>
        /// Fetches the latest supported list of currencies from CoinGecko.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshSupportedCurrenciesAsync()
        {
            // Fetch the List of Supported Currencies from CoinGecko
            // https://api.coingecko.com/api/v3/simple/supported_vs_currencies
            string url = "https://api.coingecko.com/api/v3/simple/supported_vs_currencies";
            List<string>? supportedCurrencies = await APIHelper.FetchAndJsonDeserializeAsync<List<string>>(url);

            if (supportedCurrencies == null) return;
            SupportedCurrencies = supportedCurrencies;  // Update the Observable Property

            // If fetched data is valid, write to config.json with "supported_currencies" key as List of string
            // if key exists, write by replacement
            await SettingBase.WriteUpdateSettingAsync("supported_currencies", supportedCurrencies);

            // TODO: if key does not exist, write by insert. Catch the raised error.
        }

        /// <summary>
        /// Fetches the latest list of all Exchange Ids from CoinGecko.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshExchangeIdsAsync()
        {
            List<string>? newExchangeIds = await ExchangeModel.GetExchangeIds();
            if (newExchangeIds == null) return;
            ExchangeIds = newExchangeIds;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
