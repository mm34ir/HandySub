using HandySub.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HandySub.Common
{
    public static class FavoriteHelper
    {
        public static async Task<ObservableCollection<FavoriteKeyModel>> LoadFavorites()
        {
            List<FavoriteKeyModel> favorites = new List<FavoriteKeyModel>();
            if (File.Exists(Constants.FavoritePath))
            {
                var json = await File.ReadAllTextAsync(Constants.FavoritePath);
                favorites = JsonConvert.DeserializeObject<List<FavoriteKeyModel>>(json);
            }

            return new ObservableCollection<FavoriteKeyModel>(favorites);
        }

        public static async Task<bool> IsFavoriteExist(string key)
        {
            List<FavoriteKeyModel> favorites = new List<FavoriteKeyModel>();
            if (File.Exists(Constants.FavoritePath))
            {
                var json = await File.ReadAllTextAsync(Constants.FavoritePath);
                favorites = JsonConvert.DeserializeObject<List<FavoriteKeyModel>>(json);
            }

            return favorites.Where(x => x.Key == key).Any();
        }
        public static async void AddToFavorite(double rate, FavoriteKeyModel favorite)
        {
            List<FavoriteKeyModel> favorites = new List<FavoriteKeyModel>();
            if (File.Exists(Constants.FavoritePath))
            {
                var json = await File.ReadAllTextAsync(Constants.FavoritePath);
                favorites = JsonConvert.DeserializeObject<List<FavoriteKeyModel>>(json);
            }

            var currentItem = favorites.Where(item => item.Key.Equals(favorite.Key));

            if (rate == 1)
            {
                if (!currentItem.Any())
                {
                    favorites.Add(favorite);
                }
            }
            else
            {
                if (currentItem.Any())
                {
                    favorites.Remove(currentItem.SingleOrDefault());
                }
            }

            string serialize = JsonConvert.SerializeObject(favorites, Formatting.Indented);
            await File.WriteAllTextAsync(Constants.FavoritePath, serialize);
        }
    }
}
