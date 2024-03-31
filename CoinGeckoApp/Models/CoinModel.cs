﻿using CoinGeckoApp.Helpers;
using CoinGeckoApp.Responses.Coins;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                CoinModel coin = new(reader.GetString(0), reader.GetBoolean(1));
                outCoin.Add(coin);
            };

            string query = $"select * from {dbTableName} where id='{coinId}'";
            await sqlHelper.ExecuteSelectDataReaderAsync(query, readerAction);

            // Raise Error if there is no coin from the 
            if (outCoin.Count != 1) throw new Exception($"CoinId={coinId} does not exist in {dbTableName} table!\nQuery is {query}");

            return outCoin[0];
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
}