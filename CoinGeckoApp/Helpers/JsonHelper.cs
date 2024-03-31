﻿using JsonFlatFileDataStore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Helpers
{
    public class JsonHelper
    {
        public string JsonFilePath { get; set; }  // Full Path to the Json File

        public JsonHelper() { }
        public JsonHelper(string jsonFilePath)
        {
            JsonFilePath = jsonFilePath;
        }

        /// <summary>
        /// Reads content from the Json file and returns the deserialized object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T?> ReadFromFileAsync<T>()
        {
            /* Example: 
             * .ReadFromFileAsync<APIResponse>()
             */
            using (StreamReader file = File.OpenText(JsonFilePath))
            {
                string jsonString = await file.ReadToEndAsync();
                return await Task.Run(() => JsonConvert.DeserializeObject<T>(jsonString));
            }
        }

        /// <summary>
        /// Writes the given data to the Json file asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task WriteToFileAsync<T>(T data)
        {
            /* Notes:
             * The data here is generic, this could be just one instance of an object or a collection of objects.
             * 
             * Examples:
             * .WriteToFileAsync<List<DataModel>>(dataModelList) where dataModelList is List<DataModel>
             */
            using (StreamWriter file = File.CreateText(JsonFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                await using JsonWriter writer = new JsonTextWriter(file);
                await Task.Run(() => serializer.Serialize(writer, data));
            }
        }

        public static async Task<T?> DeserializeStringAsync<T>(string jsonString)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(jsonString));
        }

        public static async Task<string> SerializeObjectAsync<T>(T obj)
        {
            return await Task.Run(() => JsonConvert.SerializeObject(obj));
        }

        public static void CreateEmptyJson(string jsonPath)
        {
            string? parentDir = Path.GetDirectoryName(jsonPath);

            if (parentDir == null) throw new DirectoryNotFoundException($"Parent Directory does not exists for {jsonPath}");

            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            if (!File.Exists(jsonPath))  // If file does not exists, create a Json File with "{}" as an empty dictionary
            {
                File.WriteAllText(jsonPath, "{}");
            }
            else  // If file exists, make sure it's not an empty file.
            {
                string content = File.ReadAllText(jsonPath);

                if (string.IsNullOrWhiteSpace(content))
                {
                    File.WriteAllText(jsonPath, "{}");
                }
            }
        }
    }

    /// <summary>
    /// Service class for performing CRUD operations on a Json Flat File as a Database. This class depends on JsonFlatFileDataStore package.
    /// It focuses on performing CRUD operations on individual item in the Json File.
    /// <para>The Json Flat File as a DB would have the following structure: 
    /// <code>{"key": {...}, "key": {...}, ...}</code>
    /// </para>
    /// <para>Use-cases: Storing different set of configurations, storing properties of special objects, etc.
    /// </para>
    /// </summary>
    public class JsonItemDBHelper
    {
        public string JsonFilePath { get; set; }

        public JsonItemDBHelper() { }
        public JsonItemDBHelper(string jsonFilePath)
        {
            JsonFilePath = jsonFilePath;
            JsonHelper.CreateEmptyJson(JsonFilePath);  // Create a Json File with empty dictionary content, if file does not exist
        }

        public async Task<T> GetObjAsync<T>(string key)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                return await Task.Run(() => store.GetItem<T>(key));
            }
        }

        public async Task<Dictionary<string, T>?> GetItemsAsDictAsync<T>()
        {
            /*
             * Warn: This is assuming that the Json values are of the same type. If its different types, 
             * use dynamic, object or custom class similar to the API Reponse classes.
             */

            using (StreamReader file = await Task.Run(() => File.OpenText(JsonFilePath)))
            {
                // Get the the content of the Json file as string.
                string jsonString = await file.ReadToEndAsync();

                // Deserialize the Json String and Return.
                return await Task.Run(() => JsonConvert.DeserializeObject<Dictionary<string, T>>(jsonString));
            }
        }

        public async Task InsertObjAsync<T>(string key, T obj)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                await store.InsertItemAsync(key, obj);
            }
        }

        public async Task ReplaceObjAsync<T>(string keyToBeReplaced, T newObj)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                await store.ReplaceItemAsync(keyToBeReplaced, newObj);
            }
        }

        public async Task UpdateObjAsync(string key, object anonymousObjProperties)
        {
            // TODO: Implement this as a method to add new properties to a json object.
            /* Example: new {Prty1 = value, Prty2 = value, ...}
             * 
             * Warn: Do not use this method for modifying an object. Use ReplaceObjAsync instead.
             * Remark: It's basically adding new properties instead of actually updating.
             */
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                await store.UpdateItemAsync(key, anonymousObjProperties);
            }
        }

        public async Task DeleteObjAsync(string key)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                await store.DeleteItemAsync(key);
            }
        }
    }

    public class JsonCollectionDBHelper
    {
        public string JsonFilePath { get; set; }
        public string CollectionName { get; set; }

        public JsonCollectionDBHelper(string jsonFilePath, string collectionName)
        {
            JsonFilePath = jsonFilePath;
            CollectionName = collectionName;
            JsonHelper.CreateEmptyJson(jsonFilePath);
        }

        public async Task<List<T>?> QueryItems<T>(Func<dynamic, bool> filter)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                return collection.AsQueryable().Where(filter).ToList() as List<T>;
            }
        }

        public async Task<List<T>> FindItemsFromPatternAsync<T>(string pattern, bool useCaseSensitive = false)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                var matches = collection.Find(pattern);
                IEnumerable<dynamic> caseSensitiveMatches = collection.Find(pattern, useCaseSensitive);

                return caseSensitiveMatches.Select(item => (T)item).ToList();
            }
        }

        public async Task InsertOneItemAsync<T>(T newItem)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                await Task.Run(() =>
                {
                    collection.InsertOne(newItem);
                });
            }
        }

        public async Task InsertManyItemsAsync<T>(IEnumerable<T> newItems)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                await collection.InsertManyAsync(newItems as IEnumerable<dynamic>);
            }
        }

        public async Task ReplaceItemAsync<T>(Predicate<dynamic> filter, T newItem)
        {
            // Note: `key` could be a string or integer, or any hashable object.
            // Reference: https://ttu.github.io/json-flatfile-datastore/#/2.4.2/?id=replace
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                await collection.ReplaceOneAsync(filter, newItem);
            }
        }

        public async Task ReplaceItemsByConditionAsync<T>(Predicate<dynamic> filter, dynamic newProperties)
        {
            // Reference: https://ttu.github.io/json-flatfile-datastore/#/2.4.2/?id=replace
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                await collection.ReplaceManyAsync(filter, newProperties);
            }
        }

        public async Task DeleteItemAsync(Predicate<dynamic> filter)
        {
            // Reference: https://ttu.github.io/json-flatfile-datastore/#/2.4.2/?id=delete
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                await collection.DeleteOneAsync(filter);
            }
        }

        public async Task DeleteItemsAsync(Predicate<dynamic> filter)
        {
            // Reference: https://ttu.github.io/json-flatfile-datastore/#/2.4.2/?id=delete
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                await collection.DeleteManyAsync(filter);
            }
        }
    }
}
