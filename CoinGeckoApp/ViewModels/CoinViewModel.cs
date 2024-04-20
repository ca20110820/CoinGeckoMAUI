using CoinGeckoApp.Models;
using CoinGeckoApp.Responses.Coins;
using CoinGeckoApp.Services;
using CoinGeckoApp.DataVisuals;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace CoinGeckoApp.ViewModels
{
    public class CoinViewModel : INotifyPropertyChanged
    {
        private static int maxDataQuickChart = 230;
        private CoinService coinService = new();
        private QuickChartVisuals qcVisuals = new();

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

        private string _priceChangeIndicator;
        public string PriceChangeIndicator
        {
            get => _priceChangeIndicator;
            set
            {
                _priceChangeIndicator = value;
                OnPropertyChanged(nameof(PriceChangeIndicator));
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

        private ImageSource _candlestickChartImageSource;
        public ImageSource CandlestickChartImageSource
        {
            get => _candlestickChartImageSource;
            set
            {
                _candlestickChartImageSource = value;
                OnPropertyChanged(nameof(CandlestickChartImageSource));
            }
        }

        private ImageSource _volumeChartImageSource;
        public ImageSource VolumeChartImageSource
        {
            get => _volumeChartImageSource;
            set
            {
                _volumeChartImageSource = value;
                OnPropertyChanged(nameof(VolumeChartImageSource));
            }
        }

        private ImageSource _priceChangesMultiPeriodImageSource;
        public ImageSource PriceChangesMultiPeriodImageSource
        {
            get => _priceChangesMultiPeriodImageSource;
            set
            {
                _priceChangesMultiPeriodImageSource = value;
                OnPropertyChanged(nameof(PriceChangesMultiPeriodImageSource));
            }
        }

        private List<KeyValuePair<string, object?>> _dataKeyValuePairs;
        public List<KeyValuePair<string, object?>> DataKeyValuePairs
        {
            get => _dataKeyValuePairs;
            set
            {
                _dataKeyValuePairs = value;
                OnPropertyChanged(nameof(DataKeyValuePairs));
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
            string vsCurrency = Preferences.Get("quotecurrency", "usd");
            MarketChart = await Task.Run(() => coinService.FetchFreeMarketChartAsync(vsCurrency, maxDataQuickChart));  // Use max data from QuickChart

            // Set SparkLine Property
            SparkLine = CoinsIdAPIResponse != null ? CoinService.GetSparkLine(CoinsIdAPIResponse) : null;

            // Set the CurrentPrice Property
            CurrentPrice = CoinsIdAPIResponse.market_data.current_price[Preferences.Get("quotecurrency", "usd")];

            // Set IsFavouriteImage Property
            SetIsFavouriteImage();

            // Set PriceChangeIndicator Property
            SetPriceChangeIndicator();

            // Set DataKeyValuePairs
            DataKeyValuePairs = coinService.GetDataKeyValuePairs(CoinsIdAPIResponse);
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

        private void SetPriceChangeIndicator()
        {
            if (MarketChart == null) return;

            try
            {
                if (CoinsIdAPIResponse.market_data.price_change_24h > 0)
                {
                    PriceChangeIndicator = "triangle_up.png";
                }
                else if (CoinsIdAPIResponse.market_data.price_change_24h < 0)
                {
                    PriceChangeIndicator = "triangle_down.png";
                }
            }
            catch(NullReferenceException ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public async Task SetImages()
        {
            //// Set CandlestickChartImageSource
            //List<List<double>>? dohlc = await coinService.FetchOhlcAsync(Preferences.Get("quotecurrency", "usd"), maxDataQuickChart);
            //CandlestickChartImageSource = await coinService.GetCandlestickChartImageSource(dohlc);

            // Set VolumeChartImageSource
            VolumeChartImageSource = await coinService.GetVolumeChartImageSource(MarketChart);

            // Set PriceChangesMultiPeriodImageSource
            PriceChangesMultiPeriodImageSource = await coinService.GetPriceChangesMultiPeriodImageSource(CoinsIdAPIResponse);
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




        /* =========== Helpers */
        /// <summary>
        /// Resets all the public properties.
        /// <para>Particularly important when there's too many https requests in other not to mix the data presentation with
        /// the previous coin viewed.
        /// </para>
        /// </summary>
        public void ResetProperties()
        {
            PropertyInfo[] properties = GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                // Check if the property is writable and not an indexer
                if (property.CanWrite && property.GetIndexParameters().Length == 0)
                {
                    // Set the property value to its default
                    property.SetValue(this, GetDefaultValue(property.PropertyType));
                }
            }
        }
        private object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }




        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
