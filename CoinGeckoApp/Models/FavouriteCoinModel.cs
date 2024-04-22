using CoinGeckoApp.DataVisuals;
using CoinGeckoApp.Responses.Coins;
using CoinGeckoApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Models
{
    public class FavouriteCoinModel : INotifyPropertyChanged
    {
        private CoinModel _coinModel;
        public CoinModel Coin
        {
            get => _coinModel;
            set
            {
                _coinModel = value;
                OnPropertyChanged(nameof(Coin));
            }
        }

        private APICoinsIdResponse _apiCoinsIdResponse;
        public APICoinsIdResponse ApiCoinsIdResponse
        {
            get => _apiCoinsIdResponse;
            set
            {
                _apiCoinsIdResponse = value;
                OnPropertyChanged(nameof(ApiCoinsIdResponse));
            }
        }

        private List<KeyValuePair<string, object?>> statsKVP;
        public List<KeyValuePair<string, object?>> StatsKVP
        {
            get => statsKVP;
            set
            {
                statsKVP = value;
                OnPropertyChanged(nameof(StatsKVP));
            }
        }

        private ImageSource _sparklineImageSource;
        public ImageSource SparklineImageSource
        {
            get => _sparklineImageSource;
            set
            {
                _sparklineImageSource = value;
                OnPropertyChanged(nameof(SparklineImageSource));
            }
        }


        private CoinService coinService = new();


        public FavouriteCoinModel() { }
        public FavouriteCoinModel(CoinModel coin)
        {
            Coin = coin;
            coinService = new(coin);
        }
        public FavouriteCoinModel(CoinModel coin, APICoinsIdResponse apiCoinsIdResponse)
        {
            Coin = coin;
            ApiCoinsIdResponse = apiCoinsIdResponse;
            coinService = new(coin);
        }


        public async Task LoadCoinData()
        {
            // Get tje CoinResponses from CoinResponses/coin_response.json from AppData
            CoinResponses? coinResponses = await coinService.GetCoinResponsesFromJson();
            if (coinResponses == null) return;

            if (coinResponses.ApiCoinIdResponse != null)
                ApiCoinsIdResponse = coinResponses.ApiCoinIdResponse;
        }

        public void SetStatsKVP()
        {
            if (ApiCoinsIdResponse == null) return;

            List<KeyValuePair<string, object?>> outList = new();

            string? lastUpdated = ApiCoinsIdResponse.market_data?.last_updated ?? string.Empty;
            outList.Add(new KeyValuePair<string, object?>("Last Updated", lastUpdated));  // Append to List

            var latestPriceDict = ApiCoinsIdResponse.market_data?.current_price;
            if (latestPriceDict != null)
            {
                string quoteCurrency = Preferences.Get("quotecurrency", "usd");
                outList.Add(new KeyValuePair<string, object?>($"Latest Price ({quoteCurrency})", latestPriceDict[quoteCurrency]));  // Append to List
            }

            string? marketCapRank = ApiCoinsIdResponse.market_data?.market_cap_rank?.ToString() ?? string.Empty;
            outList.Add(new KeyValuePair<string, object?>("Market Cap Rank", marketCapRank));  // Append to List
        }

        public async Task SetSparklineImageSource()
        {
            Dictionary<string, List<double>?>? sparklineDict = ApiCoinsIdResponse?.market_data?.sparkline_7d ?? null;
            if (sparklineDict == null) return;
            if (sparklineDict.ContainsKey("price"))
            {
                List<double>? sparkline = sparklineDict["price"];
                QuickChartVisuals qcVisuals = new();
                if (sparkline != null)
                {
                    double? priceChange24h = ApiCoinsIdResponse?.market_data?.price_change_24h;
                    string? bgColor = null;
                    if (priceChange24h != null)
                    {
                        if (priceChange24h >= 0)
                        {
                            bgColor = "green";
                        }
                        else
                        {
                            bgColor = "red";
                        }
                    }
                    string url = qcVisuals.CreateSparkLineURL(sparkline.ToArray(), borderColor:"black", backgroundColor: bgColor);
                    SparklineImageSource = await VisualUtility.GetImageSourceAsync(url);
                }
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
