using CoinGeckoApp.DataVisuals;
using CoinGeckoApp.Helpers;
using CoinGeckoApp.Models;
using CoinGeckoApp.Responses.Coins;
using JsonFlatFileDataStore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Services
{
    /// <summary>
    /// Provides methods for retrieving and processing data related to cryptocurrencies from the CoinGecko API.
    /// </summary>
    public class CoinService
    {
        private URIHelper uriHelper = new("https://api.coingecko.com");

        private string _endpoint = URIHelper.MakeEndpoint("api", "v3", "coins");  // "/api/v3/coins"
        int maxFreeMarketChart = 365;

        // Helpers
        private FileSystemHelper fsHelper = new();
        private JsonHelper jsonHelper;
        private JsonItemDBHelper jsonDbHelper;

        public CoinModel Coin { get; set; }

        public CoinService()
        {
            Init();
        }
        public CoinService(CoinModel coin)
        {
            Coin = coin;
            Init();
        }

        private void Init()
        {
            string jsonFilePath = Path.Combine(fsHelper.AppDataDir, "CoinResponses", "coin_response.json");
            jsonHelper = new(jsonFilePath);
            jsonDbHelper = new(jsonFilePath);
        }


        /* ==================== Data Getters ==================== */
        /// <summary>
        /// Fetches detailed information about a specific coin asynchronously.
        /// </summary>
        /// <returns>The API response containing information about the coin.</returns>
        public async Task<APICoinsIdResponse?> FetchCoinIdResponseAsync()
        {
            string endpoint = _endpoint + $"/{Coin.Id}";  // "/api/v3/coins/<coin-id>"
            string parameters = "tickers=true&market_data=true&community_data=true&developer_data=true&sparkline=true";
            string uri = uriHelper.MakeURI(endpoint, parameters);
            return await APIHelper.FetchAndJsonDeserializeAsync<APICoinsIdResponse>(uri);
        }

        /// <summary>
        /// Fetches historical market chart data of a specific coin asynchronously.
        /// </summary>
        /// <param name="vsCurrency">The currency in which the market data is displayed.</param>
        /// <param name="days">The number of days of historical data to retrieve.</param>
        /// <returns>The API response containing the market chart data.</returns>
        public async Task<APICoinsMarketChartResponse?> FetchFreeMarketChartAsync(string vsCurrency, int days)
        {
            // "Free" meaning max allowed historical data for Market Chart is 365
            if (days >= maxFreeMarketChart) throw new ArgumentOutOfRangeException($"Historical data must be less than {maxFreeMarketChart}");

            string endpoint = _endpoint + $"/{Coin.Id}/market_chart";
            string parameters = $"vs_currency={vsCurrency}&days={days}&precision=full";
            string uri = uriHelper.MakeURI(endpoint, parameters);
            return await APIHelper.FetchAndJsonDeserializeAsync<APICoinsMarketChartResponse>(uri);
        }

        /// <summary>
        /// Fetches the open, high, low, close (OHLC) values of a specific coin asynchronously.
        /// </summary>
        /// <param name="vsCurrency">The currency in which the values are displayed.</param>
        /// <param name="days">The number of days of historical data to retrieve.</param>
        /// <returns>The list of OHLC values.</returns>
        public async Task<List<List<double>>?> FetchOhlcAsync(string vsCurrency, int days)
        {
            /* Notes:
             * The Response would have the following structure/pattern:
             * [[timestamp, open, high, low, close], [timestamp, open, high, low, close], ...] where
             * timestamp, open, high, low, and close are double.
             */
            if (days >= maxFreeMarketChart) throw new ArgumentOutOfRangeException($"Historical data must be less than {maxFreeMarketChart}");

            // Example: https://api.coingecko.com/api/v3/coins/bitcoin/ohlc?vs_currency=usd&days=365&precision=full
            string endpoint = _endpoint + URIHelper.MakeEndpoint($"{Coin.Id}", "ohlc");
            string parameters = $"vs_currency={vsCurrency}&days={days}&precision=full";
            string uri = uriHelper.MakeURI(endpoint, parameters);
            return await APIHelper.FetchAndJsonDeserializeAsync<List<List<double>>>(uri);
        }

        /// <summary>
        /// Returns the market chart data of a coin with given quote currency and number of days.
        /// </summary>
        /// <param name="vsCurrency">The currency in which the market data is displayed.</param>
        /// <param name="days">The number of days of historical data to retrieve.</param>
        /// <returns>The market chart data in the form of a dictionary.</returns>
        public async Task<Dictionary<string, List<KeyValuePair<DateTime, double>>?>?> GetMarketChartAsync(string vsCurrency, int days)
        {
            /* Notes:
             * The Market Chart Data will have the following dictionary structure/pattern:
             * 
             * "prices" -> List<KeyValuePair<DateTime, double>>?
             * "marketcaps" -> List<KeyValuePair<DateTime, double>>?
             * "volumes" -> List<KeyValuePair<DateTime, double>>?
             */
            APICoinsMarketChartResponse? apiResponse = await FetchFreeMarketChartAsync(vsCurrency, days);
            if (apiResponse == null) return null;

            Dictionary<string, List<KeyValuePair<DateTime, double>>?> marketChart = new();  // Output

            PropertyInfo[] properties = apiResponse.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                object? value = property.GetValue(apiResponse);
                if (value is List<List<double>> tempSeries && value != null)
                {
                    List<KeyValuePair<DateTime, double>>?  tempKVPList = await Task.Run(() => tempSeries.Select(xList => Convert2ListToKVP(xList)).ToList());
                    marketChart.Add(property.Name.ToLower(), tempKVPList);
                }
                else
                {
                    marketChart.Add(property.Name.ToLower(), null);
                }
            }

            return marketChart;
        }

        /// <summary>
        /// Gets the OHLC data of a coin asynchronously.
        /// </summary>
        /// <param name="vsCurrency">The currency in which the data is displayed.</param>
        /// <param name="days">The number of days of historical data to retrieve.</param>
        /// <returns>The OHLC data of the coin.</returns>
        public async Task<List<KeyValuePair<DateTime, Tuple<double, double, double, double>>>?> GetOHLCAsync(string vsCurrency, int days)
        {
            /* Notes:
             * ["<datetime>": Tuple<open, high, low, close>, 
             * "<datetime>": Tuple<open, high, low, close>, 
             * ...]
             */
            List<List<double>>? apiResponse = await FetchOhlcAsync(vsCurrency, days);
            if (apiResponse == null) return null;

            return await Task.Run(() => apiResponse.Select(xList => Convert5ListToKVP(xList)).OrderBy(kvp => kvp.Key).ToList());
        }

        /// <summary>
        /// Retrieves images associated with a coin from the API response.
        /// </summary>
        /// <param name="apiResponse">The API response containing coin information.</param>
        /// <returns>A dictionary of image URLs.</returns>
        public static Dictionary<string, string?>? GetImages(APICoinsIdResponse apiResponse)
        {
            if (apiResponse.Images == null) return null;
            return apiResponse.Images;
        }

        /// <summary>
        /// Retrieves the sparkline data of a coin from the API response.
        /// </summary>
        /// <param name="apiResponse">The API response containing coin information.</param>
        /// <returns>A list of sparkline data.</returns>
        public static List<double>? GetSparkLine(APICoinsIdResponse apiResponse)
        {
            if (apiResponse.market_data == null) return null;

            if (apiResponse.market_data.sparkline_7d == null) return null;

            return apiResponse.market_data.sparkline_7d["price"];
        }

        /// <summary>
        /// Retrieves current data of a coin from the API response.
        /// </summary>
        /// <param name="apiResponse">The API response containing coin information.</param>
        /// <param name="quoteCurrency">The currency in which the data is displayed.</param>
        /// <returns>A dictionary of current data.</returns>
        public static Dictionary<string, dynamic?>? GetCurrentData(APICoinsIdResponse apiResponse, string quoteCurrency)
        {
            // User of this method should convert the key's value outside.
            Dictionary<string, dynamic?>? outDict = new();

            if (apiResponse.links != null)
            {
                outDict.Add("homepage", apiResponse.links.homepage);  // List<string>?
            }
            else
            {
                outDict.Add("homepage", apiResponse.links);  // null
            }

            outDict.Add("sentiment_votes_up_percentage", apiResponse.SentimentVotesUpPerc);  // double?
            outDict.Add("sentiment_votes_down_percentage", apiResponse.SentimentVotesDownPerc);  // double?

            if (apiResponse.market_data != null)
            {
                var marketData = apiResponse.market_data;

                outDict.Add("current_price", marketData.current_price != null ? marketData.current_price[quoteCurrency] : null);
                outDict.Add("market_cap_fdv_ratio", marketData.market_cap_fdv_ratio);
                outDict.Add("total_volume", marketData.total_volume != null ? marketData.total_volume[quoteCurrency] : null);
                outDict.Add("high_24h", marketData.high_24h != null ? marketData.high_24h[quoteCurrency] : null);
                outDict.Add("low_24h", marketData.low_24h != null ? marketData.low_24h[quoteCurrency] : null);
                outDict.Add("price_change_percentage_24h", marketData.price_change_percentage_24h);
                outDict.Add("price_change_percentage_7d", marketData.price_change_percentage_7d);
                outDict.Add("price_change_percentage_30d", marketData.price_change_percentage_30d);
                outDict.Add("total_supply", marketData.total_supply);
                outDict.Add("max_supply", marketData.max_supply);
                outDict.Add("circulating_supply", marketData.circulating_supply);
            }

            outDict.Add("last_updated", apiResponse.last_updated != null ? 
                DateTime.ParseExact(apiResponse.last_updated, 
                "yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind) 
                : null);  // e.g. 2024-04-02T05:44:13.549Z

            return outDict;
        }

        /* ==================== QuickChart Methods ==================== */
        /// <summary>
        /// Generates an image source for a volume chart based on the API response.
        /// </summary>
        /// <param name="apiResponse">The API response containing volume data.</param>
        /// <param name="chartLabel">The label for the chart.</param>
        /// <returns>An image source for the volume chart.</returns>
        public async Task<ImageSource> GetVolumeChartImageSource(APICoinsMarketChartResponse apiResponse, string chartLabel = "Volume")
        {
            List<List<double>> volumes = apiResponse.Volumes;  // List of 2-Lists

            // Construct a list of KeyValuePair of DateTime and Volume
            List<KeyValuePair<DateTime, double>> kvps = new();

            // Populate the list with DateTime and Volume pairs
            foreach (List<double> ls in volumes)
            {
                try
                {
                    DateTime dt = DateTimeHelper.UnixTimeStampToDateTime(ls[0]);
                    double v = ls[1];
                    kvps.Add(new KeyValuePair<DateTime, double>(dt, v));
                }
                catch(ArgumentOutOfRangeException ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            // Extract the arrays of DateTime and Volume
            DateTime[] dateArray = kvps.Select(pair => pair.Key).ToArray();
            double[] volumeArray = kvps.Select(pair => pair.Value).ToArray();

            QuickChartVisuals qcVisuals = new();
            string url = qcVisuals.CreateLineChartURL(dateArray, volumeArray, chartLabel, Preferences.Get("quotecurrency", "usd"));

            return await VisualUtility.GetImageSourceAsync(url);
        }

        /// <summary>
        /// Generates an image source for a multi-period price changes chart based on the API response.
        /// </summary>
        /// <param name="apiResponse">The API response containing price change data.</param>
        /// <returns>An image source for the multi-period price changes chart.</returns>
        public async Task<ImageSource> GetPriceChangesMultiPeriodImageSource(APICoinsIdResponse apiResponse)
        {
            Dictionary<string, double> priceChanges = new();

            var marketData = apiResponse.market_data;
            // Iterate over public properties
            foreach (var property in marketData.GetType().GetProperties())
            {
                // Get property name (key)
                string propertyName = property.Name;

                if (!propertyName.StartsWith("price_change_percentage_")) continue;

                // Get property value
                double propertyValue = 0; 
                double.TryParse(property.GetValue(marketData).ToString(), out propertyValue);

                // Clean the Property Name
                string cleanedString = propertyName.Replace("_", " ");
                string[] words = cleanedString.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i]);
                }
                string cleanKey = string.Join(" ", words);

                priceChanges[cleanKey] = propertyValue;
            }


            QuickChartVisuals qcVisuals = new();
            string url = qcVisuals.CreateHorizontalBarChartURL(priceChanges, "Price Changes (%)", "PctChg");

            return await VisualUtility.GetImageSourceAsync(url);
        }

        /// <summary>
        /// Generates an image source for a candlestick chart based on the API response.
        /// </summary>
        /// <param name="dohlc">The list of OHLC data.</param>
        /// <returns>An image source for the candlestick chart.</returns>
        public async Task<ImageSource> GetCandlestickChartImageSource(List<List<double>> dohlc)
        {
            // List<KeyValuePair<DateTime, Tuple<double, double, double, double>>>
            DateTime[] dateArray = dohlc.Select(ls => DateTimeHelper.UnixTimeStampToDateTime(ls[0])).ToArray();

            Tuple<double, double, double, double>[] ohlcArray =
                dohlc.Select(ls => new Tuple<double, double, double, double>(ls[1], ls[2], ls[3], ls[4]))
                .ToArray();

            List<KeyValuePair<DateTime, Tuple<double, double, double, double>>> zippedList =
                dateArray.Zip(ohlcArray, (date, ohlc) => new KeyValuePair<DateTime, Tuple<double, double, double, double>>(date, ohlc))
                     .ToList();

            QuickChartVisuals qcVisuals = new();
            string url = qcVisuals.CreateCandleStickChartURL(zippedList, Coin.Id);

            return await VisualUtility.GetImageSourceAsync(url);
        }

        /// <summary>
        /// Retrieves key-value pairs of specific data about a coin from the API response.
        /// </summary>
        /// <param name="apiResponse">The API response containing coin information.</param>
        /// <returns>A list of key-value pairs of data.</returns>
        public List<KeyValuePair<string, object?>> GetDataKeyValuePairs(APICoinsIdResponse apiResponse)
        {
            List<KeyValuePair<string, object?>> outList = new();

            // SentimentVotesUpPerc
            double? sentimentVotesUpPerc = apiResponse?.SentimentVotesUpPerc ?? null;
            outList.Add(new KeyValuePair<string, object?>("Sentiment Votes Up %", sentimentVotesUpPerc));  // Append to List

            // SentimentVotesDownPerc
            double? sentimentVotesDownPerc = apiResponse?.SentimentVotesDownPerc ?? null;
            outList.Add(new KeyValuePair<string, object?>("Sentiment Votes Down %", sentimentVotesDownPerc));  // Append to List

            // MarketCapRank
            int? marketCapRank = apiResponse?.MarketCapRank ?? null;
            outList.Add(new KeyValuePair<string, object?>("Market Cap Rank", marketCapRank));  // Append to List

            // WatchlistPortfolioUsers
            int? watchlistPortfolioUsers = apiResponse?.WatchlistPortfolioUsers ?? null;
            outList.Add(new KeyValuePair<string, object?>("Watchlist Portfolio Users", watchlistPortfolioUsers));  // Append to List

            // last_updated
            string? lastUpdated = apiResponse?.last_updated?? null;
            if (lastUpdated != null)
                outList.Add(new KeyValuePair<string, object?>("Last Updated", DateTime.Parse(lastUpdated).ToString("MMM dd HH:mm")));  // Append to List


            return outList;
        }


        /* ==================== Persistent Coin Responses Data ==================== */
        private string GetJsonPath()
        {
            return Path.Combine(fsHelper.AppDataDir, "CoinResponses", "coin_response.json");
        }

        /// <summary>
        /// Retrieves coin responses from the JSON file.
        /// </summary>
        /// <returns>The coin responses retrieved from the JSON file.</returns>
        public async Task<CoinResponses?> GetCoinResponsesFromJson()
        {
            try
            {
                using (var store = await Task.Run(() => new DataStore(GetJsonPath())))
                {
                    return await Task.Run(() => store.GetItem<CoinResponses>(Coin.Id));
                }
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Saves coin responses to the JSON file.
        /// </summary>
        /// <param name="apiCoinsIdResponse">The API response containing coin ID data.</param>
        /// <param name="apiCoinsMarketChartResponse">The API response containing market chart data.</param>
        public async Task SaveCoinResponsesToJson(APICoinsIdResponse apiCoinsIdResponse, APICoinsMarketChartResponse apiCoinsMarketChartResponse)
        {
            CoinResponses coinResponses = new CoinResponses
            {
                ApiCoinIdResponse = apiCoinsIdResponse,
                ApiCoinsMarketChartResponse = apiCoinsMarketChartResponse
            };

            using (var store = await Task.Run(() => new DataStore(GetJsonPath())))
            {
                await store.ReplaceItemAsync(Coin.Id, coinResponses, true);
            }
        }



        /* ==================== Data Cleaner Methods ==================== */

        /// <summary>
        /// Converts a 2-list of double values into a key-value pair where the key is converted from a Unix timestamp to DateTime.
        /// </summary>
        /// <param name="inputList">The input list containing timestamp and value.</param>
        /// <returns>A key-value pair representing timestamp as DateTime and value as double.</returns>
        /// <exception cref="ArgumentException">Thrown when the input list does not contain exactly two elements.</exception>
        public static KeyValuePair<DateTime, double> Convert2ListToKVP(List<double> inputList)
        {
            // [double, double] -> KeyValuePair<DateTime, double>
            if (inputList == null || inputList.Count != 2)
            {
                throw new ArgumentException("Input list must contain exactly two elements.");
            }

            double unixTimestamp = inputList[0];
            double value = inputList[1];

            DateTime dateTime = DateTimeHelper.UnixTimeStampToDateTime(unixTimestamp);
            return new KeyValuePair<DateTime, double>(dateTime, value);
        }

        /// <summary>
        /// Converts a 5-list of double values into a key-value pair where the key is converted from a Unix timestamp to DateTime,
        /// and the value is a tuple of open, high, low, and close values.
        /// </summary>
        /// <param name="inputList">The input list containing timestamp, open, high, low, and close values.</param>
        /// <returns>A key-value pair representing timestamp as DateTime and value as a tuple of open, high, low, and close values.</returns>
        /// <exception cref="ArgumentException">Thrown when the input list does not contain exactly five elements.</exception>
        public static KeyValuePair<DateTime, Tuple<double, double, double, double>> Convert5ListToKVP(List<double> inputList)
        {
            if (inputList == null || inputList.Count != 5)
            {
                throw new ArgumentException("Input list must have exactly 5 elements.");
            }

            double unixTimestamp = inputList[0];
            DateTime dateTime = DateTimeHelper.UnixTimeStampToDateTime(unixTimestamp);

            var tuple = Tuple.Create(inputList[1], inputList[2], inputList[3], inputList[4]);

            return new KeyValuePair<DateTime, Tuple<double, double, double, double>>(dateTime, tuple);
        }
    }

    /// <summary>
    /// Represents coin responses containing API responses for coin ID and market chart data.
    /// </summary>
    public class CoinResponses
    {
        public APICoinsIdResponse? ApiCoinIdResponse { get; set; }
        public APICoinsMarketChartResponse? ApiCoinsMarketChartResponse { get; set; }
    }
}
