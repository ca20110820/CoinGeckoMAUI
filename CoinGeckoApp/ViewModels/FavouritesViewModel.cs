using CoinGeckoApp.Helpers;
using CoinGeckoApp.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.ViewModels
{
    public class FavouritesViewModel : INotifyPropertyChanged
    {
        private List<FavouriteCoinModel> favouriteCoins;
        public List<FavouriteCoinModel> FavouriteCoins
        {
            get => favouriteCoins;
            set
            {
                favouriteCoins = value;
                OnPropertyChanged(nameof(FavouriteCoins));
            }
        }


        private FavouritesPage view;


        public FavouritesViewModel(FavouritesPage view)
        {
            this.view = view;
        }

        public async Task GetFavouriteCoins()
        {
            FileSystemHelper fsHelper = new();
            JsonHelper jsonHelper = new JsonHelper(Path.Combine(fsHelper.AppDataDir, "Databases", "favourites.json"));
            Dictionary<string, CoinModel>? favouritesDict = await jsonHelper.ReadFromFileAsync<Dictionary<string, CoinModel>>();

            List<FavouriteCoinModel> tempFavourites = new();
            FavouriteCoins = default;
            if (favouritesDict != null)
            {
                foreach (var kvp in favouritesDict)
                {
                    FavouriteCoinModel favourite = new FavouriteCoinModel(kvp.Value);
                    
                    tempFavourites.Add(favourite);
                    
                    await favourite.LoadCoinData();
                    
                    try
                    {
                        favourite.SetStatsKVP();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }

                    await favourite.SetSparklineImageSource();

                    FavouriteCoins = tempFavourites;
                }
            }
        }

        public async Task RemoveCoinFromFavourites(string coinId)
        {
            CoinModel coin = new(coinId);
            await coin.RemoveFromFavouritesAsync();
            FavouriteCoins = FavouriteCoins.Where(x => x.Coin.Id != coinId).ToList();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
