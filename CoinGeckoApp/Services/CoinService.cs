using CoinGeckoApp.Helpers;
using CoinGeckoApp.Models;
using CoinGeckoApp.Responses.Coins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Services
{
    public class CoinService
    {
        private URIHelper uriHelper = new("https://api.coingecko.com");

        private string _endpoint = URIHelper.MakeEndpoint("api", "v3", "coins");  // "/api/v3/coins"
        int maxFreeMarketChart = 365;

        public CoinModel Coin { get; set; }

        public CoinService() { }
        public CoinService(CoinModel coin)
        {
            Coin = coin;
        }


        /* ==================== Data Getters ==================== */
        public async Task<APICoinsIdResponse?> FetchCoinIdResponseAsync()
        {
            string endpoint = _endpoint + $"/{Coin.Id}";  // "/api/v3/coins/<coin-id>"
            string parameters = "tickers=true&market_data=true&community_data=true&developer_data=true&sparkline=true";
            string uri = uriHelper.MakeURI(endpoint, parameters);
            return await APIHelper.FetchAndJsonDeserializeAsync<APICoinsIdResponse>(uri);
        }

        public async Task<APICoinsMarketChartResponse?> FetchFreeMarketChartAsync(string vsCurrency, int days)
        {
            // "Free" meaning max allowed historical data for Market Chart is 365
            if (days >= maxFreeMarketChart) throw new ArgumentOutOfRangeException($"Historical data must be less than {maxFreeMarketChart}");

            string endpoint = _endpoint + $"/{Coin.Id}/market_chart";
            string parameters = $"vs_currency={vsCurrency}&days={days}&precision=full";
            string uri = uriHelper.MakeURI(endpoint, parameters);
            return await APIHelper.FetchAndJsonDeserializeAsync<APICoinsMarketChartResponse>(uri);
        }

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
        /// Returns the Market Chart Data of a Coin with given Quote Currency and Number of Days.
        /// <para>For the free API, the maximum number of days is 365.</para>
        /// </summary>
        /// <param name="vsCurrency"></param>
        /// <param name="days"></param>
        /// <returns></returns>
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

        public List<double>? GetSparkLine(APICoinsIdResponse apiResponse)
        {
            if (apiResponse.market_data == null) return null;

            if (apiResponse.market_data.sparkline_7d == null) return null;

            return apiResponse.market_data.sparkline_7d["price"];
        }

        /* ==================== Data Cleaner Methods ==================== */

        /// <summary>
        /// Transform a 2-List<double> into a KeyValuePair where the Key is converted from double (Unix TimeStamp)
        /// into DateTime.
        /// <para>This will be useful for cleaning data fetched from Market Chart.</para>
        /// </summary>
        /// <param name="inputList"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
}
