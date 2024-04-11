using CoinGeckoApp.Models;
using CoinGeckoApp.Helpers;
using CoinGeckoApp.Responses.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Data.Sqlite;

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

        private SQLiteHelper sqlHelper;
        private string sqliteFilePath;

        public ExchangeModel Exchange {  get; set; }

        public ExchangeService()
        {
            InitJsonDB();
            InitSQlite();
        }
        public ExchangeService(ExchangeModel exchange)
        {
            Exchange = exchange;
            InitJsonDB();
            InitSQlite();
        }
        private void InitJsonDB()
        {
            string jsonFilePath = Path.Combine(fsHelper.AppDataDir, "Caches", "exchange_tickers.json");
            jsonHelper = new(jsonFilePath);
            jsonDbHelper = new(jsonFilePath);
        }
        private void InitSQlite()
        {
            sqliteFilePath = Path.Combine(fsHelper.AppDataDir, "Caches", "exchange_tickers.db");
            sqlHelper = new(sqliteFilePath);
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

            await SaveTickersToDBAsync(tickers);
        }

        /* CRUD Operations for Tickers (Cache Data) */
        public async Task<List<Ticker>?> GetTickersFromDBAsync()
        {
            return await jsonDbHelper.GetObjAsync<List<Ticker>>(Exchange.Id);
        }

        public async Task SaveTickersToDBAsync(List<Ticker> tickers)
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


        /* SQLite - CRUD Operations for Tickers */
        public async Task CreateSQLTableAsync()
        {
            await sqlHelper.CreateTableAsync(Exchange.Id,
                "coin_id TEXT PRIMARY KEY NOT NULL UNIQUE",
                "blob_data BLOB");
        }

        public async Task InsertTickersToSQLAsync(List<Ticker> tickers)
        {
            /* References:
             * - https://stackoverflow.com/questions/65017136/c-sharp-sqlite-retrieve-complete-blob-and-insert-complete-blob
             */
            await CreateSQLTableAsync();
            string dbFilePath = Path.Combine(fsHelper.AppDataDir, "Caches", "exchange_tickers.db");
            string connectionString = $"Data Source={dbFilePath}";
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (Ticker ticker in tickers)
                {
                    byte[] blob = await SQLiteHelper.ConvertObjectToByte(ticker);


                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"INSERT OR REPLACE INTO `{Exchange.Id}` (coin_id, blob_data) VALUES ('{ticker.CoinId}', @blob_data)";
                        command.Parameters.Add(new SqliteParameter()
                        {
                            ParameterName = "@blob_data",
                            Value = blob,
                            DbType = System.Data.DbType.Binary
                        });

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<List<Ticker>> GetTickersFromSQLAsync()
        {
            List<Ticker> tickers = new List<Ticker>();
            string dbFilePath = Path.Combine(fsHelper.AppDataDir, "Caches", "exchange_tickers.db");
            string connectionString = $"Data Source={dbFilePath}";

            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync(); // Open the connection

                // Construct SQL command with necessary columns
                string query = $"SELECT blob_data FROM `{Exchange.Id}`";

                // Execute the query
                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Read data in batches
                        while (await reader.ReadAsync())
                        {
                            byte[] blob = (byte[])reader["blob_data"];
                            Ticker? ticker = await SQLiteHelper.ConvertByteToObject<Ticker>(blob);
                            if (ticker != null)
                                tickers.Add(ticker);
                        }
                    }
                }
            }
            return tickers;
        }
    }
}
