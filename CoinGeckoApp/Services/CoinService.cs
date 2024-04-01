using CoinGeckoApp.Helpers;
using CoinGeckoApp.Models;
using CoinGeckoApp.Responses.Coins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Services
{
    public class CoinService
    {
        private APICoinsIdResponse? coinIdResponse = null;
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
        private async Task RefreshCoinIdResponse()
        {
            string endpoint = _endpoint + $"/{Coin.Id}";  // "/api/v3/coins/<coin-id>"
            string parameters = "tickers=true&market_data=true&community_data=true&developer_data=true&sparkline=true";
            string uri = uriHelper.MakeURI(endpoint, parameters);
            APICoinsIdResponse? apiResponse = await APIHelper.FetchAndJsonDeserializeAsync<APICoinsIdResponse>(uri);

            if (apiResponse == null) return;

            coinIdResponse = apiResponse;
        }


        private async Task<APICoinsMarketChartResponse?> FetchFreeMarketChart(string vsCurrency, int days)
        {
            // "Free" meaning max allowed historical data for Market Chart is 365
            if (days >= maxFreeMarketChart) throw new ArgumentOutOfRangeException($"Historical data must be less than {maxFreeMarketChart}");

            string endpoint = _endpoint + $"/{Coin.Id}/market_chart";
            string parameters = $"vs_currency={vsCurrency}&days={days}&precision=full";
            string uri = uriHelper.MakeURI(endpoint, parameters);
            return await APIHelper.FetchAndJsonDeserializeAsync<APICoinsMarketChartResponse>(uri);
        }


        public async Task GetCoinDetails()
        {
            // Latest and Current Data
        }

        public async Task GetSparkLine()
        {

        }

        public async Task GetHistoricalPrice()
        {

        }

        public async Task GetCoinOHLCV()
        {

        }


        /* ==================== Data Cleaner Methods ==================== */


    }
}
