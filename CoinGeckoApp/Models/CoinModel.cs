using CoinGeckoApp.Helpers;
using CoinGeckoApp.Responses.Coins;
using JsonFlatFileDataStore;
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
    /// <summary>
    /// Represents a Cryptocurrency Coin.
    /// </summary>
    public class CoinModel
    {
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
        }
        public CoinModel(string id, bool favourite)
        {
            Id = id;
            Favourite = favourite;
        }


        /* ==================== Methods ==================== */
        /// <summary>
        /// Gets the path of the JSON file storing favorite coins.
        /// </summary>
        /// <returns>The path of the JSON file.</returns>
        private static string GetJsonPath()
        {
            FileSystemHelper fsHelper = new();
            return Path.Combine(fsHelper.AppDataDir, "Databases", "favourites.json");
        }

        /// <summary>
        /// Checks whether the cryptocurrency is marked as a favorite asynchronously.
        /// </summary>
        /// <returns>The task result contains a value indicating whether the cryptocurrency is marked as a favorite.</returns>
        public async Task<bool> IsFavouriteAsync()
        {
            try
            {
                using (var store = new DataStore(GetJsonPath()))
                {
                    CoinModel coin = await Task.Run(() => store.GetItem<CoinModel>(Id));
                    Favourite = coin.Favourite; // Set the Favourite Property automatically
                    if (!Favourite) throw new CoinNotFavouriteException($"Coin {Id} is not a favourite!");
                }
                return true;
            }
            catch (KeyNotFoundException ex)
            {
                Favourite = false;
                return Favourite;
            }
        }

        /// <summary>
        /// Adds the cryptocurrency to favorites asynchronously.
        /// </summary>
        public async Task AddToFavouritesAsync()
        {
            using (var store = new DataStore(GetJsonPath()))
            {
                Favourite = true;
                await store.ReplaceItemAsync(Id, this, true);
            }
        }

        /// <summary>
        /// Removes the cryptocurrency from favorites asynchronously.
        /// </summary>
        /// <returns>The task result contains a value indicating whether the removal was successful.</returns>
        public async Task<bool> RemoveFromFavouritesAsync()
        {
            using (var store = new DataStore(GetJsonPath()))
            {
                Favourite = false;
                return await store.DeleteItemAsync(Id);
            }
        }
    }

    /// <summary>
    /// Represents an exception that occurs in the <see cref="CoinModel"/> class.
    /// </summary>
    public class CoinException : Exception
    {
        // Constructor
        public CoinException() : base() { }

        // Constructor with message
        public CoinException(string message) : base(message) { }

        // Constructor with message and inner exception
        public CoinException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents an exception that occurs when a cryptocurrency coin is not marked as a favorite.
    /// </summary>
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