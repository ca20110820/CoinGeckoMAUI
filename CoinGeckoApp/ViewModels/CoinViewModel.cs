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
                _coin = value;
                OnPropertyChanged(nameof(Coin));
            }
        }

        private string _isFavouriteImage;
        public string IsFavouriteImage
        {
            get => _isFavouriteImage;
            set
            {
                _isFavouriteImage = value;
                OnPropertyChanged(nameof(IsFavouriteImage));
            }
        }

        private APICoinsIdResponse? _coinsIdApiResponse = null;
        public APICoinsIdResponse? CoinsIdAPIResponse
        {
            get => _coinsIdApiResponse;
            set
            {
                _coinsIdApiResponse = value;
                OnPropertyChanged(nameof(CoinsIdAPIResponse));
            }
        }

        private APICoinsMarketChartResponse? _marketChart = null;
        public APICoinsMarketChartResponse? MarketChart
        {
            get => _marketChart;
            set
            {
                _marketChart = value;
                OnPropertyChanged(nameof(MarketChart));
            }
        }

        private List<double>? _sparkLine = null;
        public List<double>? SparkLine
        {
            get => _sparkLine;
            set
            {
                _sparkLine = value;
                OnPropertyChanged(nameof(SparkLine));
            }
        }


        private double? _currentPrice = null;
        public double? CurrentPrice
        {
            get => _currentPrice;
            set
            {
                _currentPrice = value;
                OnPropertyChanged(nameof(CurrentPrice));
            }
        }


        /* =========== Constructors */
        public CoinViewModel() { }


        /* =========== Setters */
        public async Task SetCoin(CoinModel newCoin)
        {
            Coin = newCoin;
            coinService = new(Coin);  // Pass the new Coin to CoinService object
            CoinsIdAPIResponse = await Task.Run(() => coinService.FetchCoinIdResponseAsync());  // Set the new (if it is) CoinsIdAPIResponse

            // Set MarketChart Property
            string vsCurrency = Preferences.Get("quotecurrency", "usd");  // TODO: Set this in App.xaml.cs
            int maxDays = Preferences.Get("maxdays", 360);  // TODO: Set this in App.xaml.cs
            MarketChart = await Task.Run(() => coinService.FetchFreeMarketChartAsync(vsCurrency, maxDays));

            // Set SparkLine Property
            SparkLine = CoinsIdAPIResponse != null ? CoinService.GetSparkLine(CoinsIdAPIResponse) : null;

            // Set the CurrentPrice Property
            CurrentPrice = CoinsIdAPIResponse.market_data.current_price[Preferences.Get("quotecurrency", "usd")];

            // Set IsFavouriteImage Property
            SetIsFavouriteImage();
        }

        private void SetIsFavouriteImage()
        {
            if (Coin.Favourite)
            {
                IsFavouriteImage = "star_favourite.png";
            }
            else
            {
                IsFavouriteImage = "star_unfavourite.png";
            }
        }

        /* =========== View Updaters */
        /* Notes: 
         * - These methods are for manually updating or rendering the UI if BindingContext is not used.
         * - These are methods are ideal when updating collection data.
         */

        /* =========== Actions and Commands */
        public async Task ChangeFavouriteState()
        {
            CoinModel updatedCoin = new CoinModel(Coin.Id, Coin.Favourite);

            if (updatedCoin.Favourite)  // Current State is Favourite
            {
                await updatedCoin.RemoveFromFavouritesAsync();
            }
            else  // Current State is Not Favourite
            {
                await updatedCoin.AddToFavouritesAsync();
            }

            Coin = updatedCoin;
            
            // Set the path to star image, depending on favourite status
            SetIsFavouriteImage();
        }

        




        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
