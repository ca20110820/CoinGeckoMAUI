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
        public static async Task<List<string>?> FetchSupportedCurrencies()
        {
            APIService apiService = new();
            string? responseString = await apiService.GetDataAsStringCustomURLAsync("https://api.coingecko.com/api/v3/simple/supported_vs_currencies");
            if (responseString != null)
            {
                return await Task.Run(() => JsonConvert.DeserializeObject<List<string>>(responseString));
            }
            
            return null;
        }
    }
}
