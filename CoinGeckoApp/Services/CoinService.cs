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

        public CoinModel Coin { get; set; }

        public CoinService() { }
        public CoinService(CoinModel coin)
        {
            Coin = coin;
        }


        /* ==================== Data Getters ==================== */
        private async Task RefreshCoinIdResponse()
        {
            string endpoint = URIHelper.MakeEndpoint("api", "v3", Coin.Id);
            string parameters = "tickers=true&market_data=true&community_data=true&developer_data=true&sparkline=true";
            string uri = uriHelper.MakeURI(endpoint, parameters);
            APICoinsIdResponse? apiResponse = await APIHelper.FetchAndJsonDeserializeAsync<APICoinsIdResponse>(uri);

            if (apiResponse == null) return;

            coinIdResponse = apiResponse;
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
