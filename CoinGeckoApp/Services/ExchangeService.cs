using CoinGeckoApp.Models;
using CoinGeckoApp.Helpers;
using CoinGeckoApp.Responses.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Services
{
    public class ExchangeService
    {
        private URIHelper uriHelper = new("https://api.coingecko.com");
        private string _endpoint = URIHelper.MakeEndpoint("api", "v3", "exchanges");

        public ExchangeModel Exchange {  get; set; }

        public ExchangeService() { }
        public ExchangeService(ExchangeModel exchange)
        {
            Exchange = exchange;
        }


        public async Task<APIExchangesIdResponse?> FetchExchangeIdResponseAsync()
        {
            // Example: https://api.coingecko.com/api/v3/exchanges/binance
            string uri = $"https://api.coingecko.com/api/v3/exchanges/{Exchange.Id}";
            return await APIHelper.FetchAndJsonDeserializeAsync<APIExchangesIdResponse>(uri);
        }
        public async Task<APIExchangeIdTickersResponse?> FetchExchangeTickers(int page = 1)
        {
            // Example: https://api.coingecko.com/api/v3/exchanges/<id>/tickers?include_exchange_logo=true&page=<page-num>&depth=true&order=trust_score_desc
            string uri = $"https://api.coingecko.com/api/v3/exchanges/{Exchange.Id}/tickers?include_exchange_logo=true&page={page}&depth=true&order=trust_score_desc";
            return await APIHelper.FetchAndJsonDeserializeAsync<APIExchangeIdTickersResponse>(uri);
        }

        public async Task<List<string>> GetCoinIds()
        {
            List<string> outList = new();

            int i = 1;
            while (true)
            {
                // Fetch the APIExchangeIdTickersResponse
                APIExchangeIdTickersResponse? apiReponse = await FetchExchangeTickers(page: i);

                if (apiReponse == null)
                {
                    return outList;
                }
                else
                {
                    if (apiReponse.Tickers == null)
                    {
                        return outList;
                    }
                    else
                    {
                        // Extract the Coin Ids
                        List<string> coinIds = apiReponse.Tickers.Select(ticker => ticker.CoinId).Distinct().ToList();
                        outList.AddRange(coinIds);
                    }
                }
                i++;
            }
        }
    }
}
