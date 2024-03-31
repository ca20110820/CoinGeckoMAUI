using CoinGeckoApp.Helpers;
using CoinGeckoApp.Responses.Coins;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Models
{
    public class CoinModel
    {
        private static string dbPath = Path.Combine("Databases","favourites.db");  // File Path in App Data Directory;
        private static string dbTableName = "favourites";  // Table[ID: string, Favourite: bool]

        /* Public Properties */
        public string Id { get; set; }

        private bool _favourite = false;
        public bool Favourite
        {
            get => _favourite;
            set
            {
                _favourite = value;
            }
        }

        public CoinModel() { }
        public CoinModel(string id)
        {
            Id = id;
            Favourite = IsFavourite().Result;
        }
        public CoinModel(string id, bool favourite)
        {
            Id = id;
            Favourite = favourite;
        }


        /* ==================== Methods ==================== */
        /// <summary>
        /// Instantiating CoinModel from the Favourites SQLite Database.
        /// </summary>
        /// <param name="coinId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async static Task<CoinModel> ReadFromFavouritesDB(string coinId)
        {
            FileSystemHelper fsHelper = new();
            SQLiteHelper sqlHelper = new(Path.Combine(fsHelper.AppDataDir, dbPath));

            // Assume that the Database and Table is created already.
            List<CoinModel> outCoin = new();
            Action<SqliteDataReader> readerAction = (SqliteDataReader reader) =>
            {
                string id = reader.GetString(0);
                bool isFavourite = reader.GetBoolean(1);

                if (!isFavourite) throw new CoinNotFavouriteException($"{id} has Favourite={isFavourite}!");

                CoinModel coin = new(id, isFavourite);
                outCoin.Add(coin);
            };

            string query = $"select * from {dbTableName} where id='{coinId}'";
            await sqlHelper.ExecuteSelectDataReaderAsync(query, readerAction);

            // Raise Error if there is no coin from the 
            if (outCoin.Count != 1) throw new CoinNotFavouriteException($"CoinId={coinId} does not exist in {dbTableName} table!\nQuery is {query}");

            return outCoin[0];
        }

        public async Task<bool> IsFavourite()
        {
            try
            {
                var coin = await ReadFromFavouritesDB(Id);
                if (coin is CoinModel) return true;

                return false;
            }
            catch (CoinNotFavouriteException ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
        }

        public async void UpdateFavourite()
        {
            Favourite = !Favourite;
            /* TODO:
             * If Favourite = true, then Insert to Database.
             * If Favourite = false, then Remove from Database.
             */
            if (Favourite)
            {
                // Add to DB
                await AddToFavouritesDB();
            }
            else
            {
                // Remove from DB
                await RemoveFromFavouritesDB();
            }
        }
        public async Task AddToFavouritesDB()
        {

            if (!Favourite)  // i.e. Favourite == False
            {
                throw new Exception($"Adding {Id} to {dbTableName} when Favourite={Favourite}!");
            }

            FileSystemHelper fsHelper = new();
            SQLiteHelper sqlHelper = new(Path.Combine(fsHelper.AppDataDir, dbPath));

            // Assume that the Database and Table is created already.
            string query = $"INSERT INTO {dbTableName} (id, favourite) VALUES ('{Id}', {Favourite})";
            await sqlHelper.ExecuteNonQueryAsync(query);
        }
        public async Task RemoveFromFavouritesDB()
        {
            FileSystemHelper fsHelper = new();
            SQLiteHelper sqlHelper = new(Path.Combine(fsHelper.AppDataDir, dbPath));

            // Assume that the Database and Table is created already.
            string query = $"DELETE FROM {dbTableName} WHERE ID = '{Id}'";
            await sqlHelper.ExecuteNonQueryAsync(query);
        }
    }


    public class CoinException : Exception
    {
        // Constructor
        public CoinException() : base() { }

        // Constructor with message
        public CoinException(string message) : base(message) { }

        // Constructor with message and inner exception
        public CoinException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class CoinNotFavouriteException : CoinException
    {
        // Constructor
        public CoinNotFavouriteException() : base() { }

        // Constructor with message
        public CoinNotFavouriteException(string message) : base(message) { }

        // Constructor with message and inner exception
        public CoinNotFavouriteException(string message, Exception innerException) : base(message, innerException) { }
    }
}