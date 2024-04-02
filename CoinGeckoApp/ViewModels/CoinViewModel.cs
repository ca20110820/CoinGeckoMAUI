using CoinGeckoApp.Models;
using CoinGeckoApp.Responses.Coins;
using CoinGeckoApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.ViewModels
{
    public class CoinViewModel : INotifyPropertyChanged
    {
        private CoinService coinService = new();

        /* =========== Observable Properties */
        // Note: Useful for updating simple scalar data.

        private CoinModel _coin;
        public CoinModel Coin
        {
            get => _coin;
            set
            {
                // We notify if Coin in context changed (i.e. Id) or if the state of "Favourite" changed.
                if ((_coin.Id != value.Id) || (_coin.Id == value.Id && _coin.Favourite != value.Favourite))
                {
                    _coin = value;
                    OnPropertyChanged(nameof(Coin));
                }
            }
        }

        private APICoinsIdResponse? _coinsIdApiResponse = null;
        public APICoinsIdResponse? CoinsIdAPIResponse
        {
            get => _coinsIdApiResponse;
            set
            {
                if (value != null)
                {
                    _coinsIdApiResponse = value;
                    OnPropertyChanged(nameof(CoinsIdAPIResponse));
                }
            }
        }

        private APICoinsMarketChartResponse? _marketChart = null;
        public APICoinsMarketChartResponse? MarketChart
        {
            get => _marketChart;
            set
            {
                if (value != null)
                {
                    MarketChart = value;
                    OnPropertyChanged(nameof(MarketChart));
                }
            }
        }

        
        /* =========== Constructors */
        public CoinViewModel() { }


        /* =========== Setters */
        public async Task SetCoin(CoinModel newCoin)
        {
            Coin = newCoin;  // Set the new Coin
            coinService = new(newCoin);  // Pass the new Coin to CoinService object
            CoinsIdAPIResponse = await coinService.FetchCoinIdResponseAsync();  // Set the new (if it is) CoinsIdAPIResponse

            string vsCurrency = Preferences.Get("quotecurrency", "usd");  // TODO: Set this in App.xaml.cs
            int maxDays = Preferences.Get("maxdays", 365);  // TODO: Set this in App.xaml.cs
            MarketChart = await coinService.FetchFreeMarketChartAsync(vsCurrency, maxDays);


        }

        /* =========== View Updaters */
        /* Notes: 
         * - These methods are for manually updating or rendering the UI if BindingContext is not used.
         * - These are methods are ideal when updating collection data.
         */

        /* =========== Actions and Commands */
        public async Task ChangeFavouriteState()
        {
            CoinModel updatedCoin = Coin;
            await updatedCoin.ChangeFavouriteStatus();  // This will write the changed Coin state to SQLite DB for persistence.
            Coin = updatedCoin;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
