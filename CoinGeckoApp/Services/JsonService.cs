using JsonFlatFileDataStore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Services
{
    public class JsonService
    {
        public string JsonFilePath { get; set; }  // Full Path to the Json File

        public JsonService() { }
        public JsonService(string jsonFilePath)
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

        public static void CreateEmptyJson(string jsonPath)
        {
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
    public class JsonItemDBService
    {
        public string JsonFilePath { get; set; }

        public JsonItemDBService() { }
        public JsonItemDBService(string jsonFilePath)
        {
            JsonFilePath = jsonFilePath;
            JsonService.CreateEmptyJson(JsonFilePath);  // Create a Json File with empty dictionary content, if file does not exist
        }

        public async Task<T> GetObjAsync<T>(string key)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                return await Task.Run(() => store.GetItem<T>(key));
            }
        }

        public async Task<List<T>?> GetItemsAsListAsync<T>()
        {
            // Warn: This is assuming that the Json values are of the same type.

            using (StreamReader file = await Task.Run(() => File.OpenText(JsonFilePath)))
            {
                // Get the the content of the Json file as string.
                string jsonString = await file.ReadToEndAsync();

                // Deserialize the Json File as a List<T> and return.
                return await Task.Run(() => JsonConvert.DeserializeObject<List<T>>(jsonString));
            }
        }

        // TODO: Implement GetItemsAsDictAsync  Dictionary<string, object>

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

    public class JsonCollectionDBService
    {
        public string JsonFilePath { get; set; }
        public string CollectionName { get; set; }

        public JsonCollectionDBService(string jsonFilePath, string collectionName)
        {
            JsonFilePath = jsonFilePath;
            CollectionName = collectionName;
            JsonService.CreateEmptyJson(jsonFilePath);
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
