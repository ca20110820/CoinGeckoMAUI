using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Services
{
    public class APIService
    {
        private const string baseUrl = "https://api.coingecko.com/api/v3/";
        private string endpointFullUrl = string.Empty;

        public string Endpoint { get => endpointFullUrl; }

        public APIService() { }

        public APIService(string endpoint)
        {
            endpointFullUrl = baseUrl + endpoint + "/";  // Example: https://api.coingecko.com/api/v3/simple/ [supported_vs_currencies][?parameters]
        }

        public string GetFullUrl(string requestCategory, string? requestParams)
        {
            /* Examples:
             * https://api.coingecko.com/api/v3/coins/bitcoin?tickers=false&market_data=false&community_data=false&developer_data=false&sparkline=true
             * https://api.coingecko.com/api/v3/coins/list?include_platform=true
             * https://api.coingecko.com/api/v3/coins/bitcoin/market_chart/range?vs_currency=usd&from=1392577232&to=1422577232&precision=full
             */
            string tempParams = !string.IsNullOrEmpty(requestParams) ? requestParams : string.Empty;
            return Endpoint + requestCategory + "?" + tempParams;
        }

        public async Task<string?> GetDataAsStringCustomURLAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
            }
        }
    }
}
