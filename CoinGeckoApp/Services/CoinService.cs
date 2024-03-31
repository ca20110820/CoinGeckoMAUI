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
        private APICoinsIdResponse coinIdResponse;

        public CoinModel Coin { get; set; }

        public CoinService() { }
        public CoinService(CoinModel coin)
        {
            Coin = coin;
        }


        /* ==================== Data Getters ==================== */
        private async Task FetchCoinIdResponse()
        {
            // Set to coinIdResponse
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
