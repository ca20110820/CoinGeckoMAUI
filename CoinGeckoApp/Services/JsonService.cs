using JsonFlatFileDataStore;
using Newtonsoft.Json;
using System;
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

        public JsonItemDBService(string jsonFilePath)
        {
            JsonFilePath = jsonFilePath;
            CreateEmptyJson(JsonFilePath).Wait();  // Create a Json File with empty dictionary content, if file does not exist
        }

        public async static Task CreateEmptyJson(string jsonPath)
        {
            await Task.Delay(250);
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

        public async Task<T> GetObjAsync<T>(string key)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                return await Task.Run(() => store.GetItem<T>(key));
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
            /* Example: new {Prty1 = value, Prty2 = value, ...}
             * 
             * Warn: Do not use this method for modifying an object. Use ReplaceObjAsync instead.
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

    }
}
