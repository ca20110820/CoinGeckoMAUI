using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinGeckoApp.Services;
using Newtonsoft.Json;


namespace CoinGeckoApp.Helpers
{
    public static class CurrencyHelper
    {
        /// <summary>
        /// Fetch all the supported currency in CoinGecko API.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<string>?> FetchSupportedCurrencies()
        {
            return await APIService.FetchAndJsonDeserializeAsync<List<string>>("https://api.coingecko.com/api/v3/simple/supported_vs_currencies");
        }
    }
}
