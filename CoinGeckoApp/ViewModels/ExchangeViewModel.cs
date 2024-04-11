using CoinGeckoApp.Models;
using CoinGeckoApp.Responses.Exchanges;
using CoinGeckoApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.ViewModels
{
    public class ExchangeViewModel : INotifyPropertyChanged
    {
        private ExchangeService exchangeService = new();
        private ExchangePage _view;

        private ExchangeModel _exchangeModel;
        public ExchangeModel Exchange
        {
            get => _exchangeModel;
            set
            {
                _exchangeModel = value;
                exchangeService = new(value);  // Update the exchange service
                OnPropertyChanged(nameof(Exchange));
            }
        }

        private Ticker _currentTicker;
        public Ticker CurrentTicker
        {
            get => _currentTicker;
            set
            {
                _currentTicker = value;
                OnPropertyChanged(nameof(CurrentTicker));
            }
        }

        private ObservableCollection<Ticker> _tickers = new();
        public ObservableCollection<Ticker> Tickers
        {
            get => _tickers;
            set
            {
                _tickers = value;
                OnPropertyChanged(nameof(Tickers));
            }
        }


        /* ======================== Constructors */
        public ExchangeViewModel() { }
        public ExchangeViewModel(ExchangePage exchangePage)
        {
            _view = exchangePage;
        }



        /* ======================== Getters, Fetchers, Updaters, Refreshers */
        public async Task RefreshTickers()
        {
            /* The Process is too slow if the "Tickers" property is binded to CollectionView */

            // Set the Title based from Exchange ID User Preference
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string exchangeId = Preferences.Get("exchangeid", "binance");
            exchangeId = textInfo.ToTitleCase(exchangeId.ToLower());  // Warn: It capitalizes the first character
            _view.labelExchangeId.Text = exchangeId;  // Set to binance if there is some error

            // Re-instantiate the Exchange Service with the current state of "exchangeid"
            Exchange = new ExchangeModel(Preferences.Get("exchangeid", "binance"));  // Update Exchange observable property
            exchangeService = new(Exchange);  // Get the exchange service based on current state of exchangeid

            var apiResponse = await Task.Run(() => exchangeService.FetchExchangeTickers());

            if (apiResponse == null) return;

            if (apiResponse.Tickers != null)
            {
                // Save to Json Database
                await exchangeService.InsertTickersToJsonAsync(apiResponse.Tickers.ToArray());
                Tickers = new(apiResponse.Tickers);
            }
        }

        public async Task ShowTickers()
        {
            // Try to retreive tickers from local json file
            Exchange = new ExchangeModel(Preferences.Get("exchangeid", "binance"));
            exchangeService = new(Exchange);  // Get the exchange service based on current state of exchangeid
            
            Ticker[] tempArr = await Task.Run(() => exchangeService.GetTickersFromJsonAsync());

            // If not available, fetch from api and save
            if (tempArr.Length < 1)
            {
                await RefreshTickers();
                //tempArr = await Task.Run(() => exchangeService.GetTickersFromJsonAsync());
                return;
            }

            Tickers = new(tempArr);
        }








        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
