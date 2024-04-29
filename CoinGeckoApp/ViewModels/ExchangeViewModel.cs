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
using System.Windows.Input;

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

        private string _exchangeId;
        public string ExchangeId
        {
            get => _exchangeId;
            set
            {
                _exchangeId = value;
                OnPropertyChanged(nameof(ExchangeId));

                // Also Update the ExchangeModel object and notify listeners
                Exchange = new ExchangeModel(value);
            }
        }


        /* ======================== Constructors */
        public ExchangeViewModel()
        {
            InitCommands();
        }
        public ExchangeViewModel(ExchangePage exchangePage)
        {
            _view = exchangePage;
            InitCommands();
        }


        /* ======================== Commands */
        private void InitCommands()
        {
            ChangeExchangeIdCommand = new Command<string>(ExecChangeExchangeIdCommand);
        }

        public ICommand ChangeExchangeIdCommand { get; set; }
        private async void ExecChangeExchangeIdCommand(string id)
        {
            try
            {
                await ShowTickers(id);
            }
            catch (HttpRequestException ex)
            {
                // Too many http request error
                if (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests) return;

                // TODO: Handle no internet connection
            }
        }


        /* ======================== Getters, Fetchers, Updaters, Refreshers */
        public async Task RefreshTickers(string? exchangeid = null)
        {
            /* The Process is too slow if the "Tickers" property is binded to CollectionView */

            // Set the Title based from Exchange ID User Preference
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string exchangeId = exchangeid == null? Preferences.Get("exchangeid", "binance") : exchangeid;
            exchangeId = textInfo.ToTitleCase(exchangeId.ToLower());  // Warn: It capitalizes the first character
            //_view.labelExchangeId.Text = exchangeId;  // Set to binance if there is some error
            SetExchangeId(exchangeId);

            // Re-instantiate the Exchange Service with the current state of "exchangeid"
            Exchange = new ExchangeModel(exchangeid == null ? Preferences.Get("exchangeid", "binance") : exchangeid);  // Update Exchange observable property
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

        public async Task ShowTickers(string? exchangeid = null)
        {
            // Try to retreive tickers from local json file
            string exchangeId = exchangeid == null ? Preferences.Get("exchangeid", "binance") : exchangeid;
            Exchange = new ExchangeModel(exchangeId);
            SetExchangeId(exchangeId);
            exchangeService = new(Exchange);  // Get the exchange service based on current state of exchangeid
            
            Ticker[] tempArr = await Task.Run(() => exchangeService.GetTickersFromJsonAsync());

            // If not available, fetch from api and save
            if (tempArr.Length < 1)
            {
                await RefreshTickers(exchangeId);
                return;
            }

            Tickers = new(tempArr);
        }

        public void SetExchangeId(string exchangeId)
        {
            ExchangeId = exchangeId;
        }






        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
