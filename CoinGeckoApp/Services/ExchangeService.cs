using CoinGeckoApp.Models;
using CoinGeckoApp.Helpers;
using CoinGeckoApp.Responses.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CoinGeckoApp.Services
{
    public class ExchangeService
    {
        // API Fields
        private URIHelper uriHelper = new("https://api.coingecko.com");
        private string _endpoint = URIHelper.MakeEndpoint("api", "v3", "exchanges");

        // Helpers
        private FileSystemHelper fsHelper = new();
        private JsonHelper jsonHelper;
        private JsonItemDBHelper jsonDbHelper;

        public ExchangeModel Exchange {  get; set; }

        public ExchangeService()
        {
            InitJsonDB();
        }
        public ExchangeService(ExchangeModel exchange)
        {
            Exchange = exchange;
            InitJsonDB();
        }
        private void InitJsonDB()
        {
            string jsonFilePath = Path.Combine(fsHelper.AppDataDir, "Caches", "exchange_tickers.json");
            jsonHelper = new(jsonFilePath);
            jsonDbHelper = new(jsonFilePath);
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

        public async Task<List<string>> GetCoinIdsAsync()
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

        public async Task FetchAndSaveTickersAsync(int page = 1)
        {
            APIExchangeIdTickersResponse? apiResponse = await Task.Run(() => FetchExchangeTickers(page: page));
            if (apiResponse == null) return;

            List<Ticker>? tickers = apiResponse.Tickers;
            if (tickers == null) return;

            Trace.Assert(tickers.Count > 0);

            await SaveTickersFromDBAsync(tickers);
        }

        /* CRUD Operations for Tickers (Cache Data) */
        public async Task<List<Ticker>?> GetTickersFromDBAsync()
        {
            return await jsonDbHelper.GetObjAsync<List<Ticker>>(Exchange.Id);
        }

        public async Task SaveTickersFromDBAsync(List<Ticker> tickers)
        {
            if (tickers.Count == 0) throw new ArgumentException("The given list of ticker cannot be empty!");

            try
            {
                await jsonDbHelper.ReplaceObjAsync(Exchange.Id, tickers);
            }
            catch (KeyNotFoundException ex)
            {
                // This will happen when the exchange_id does not exists in "ExchangeTickers/exchange_tickers.json"
                await jsonDbHelper.InsertObjAsync(Exchange.Id, tickers);
            }
        }
    }
}
