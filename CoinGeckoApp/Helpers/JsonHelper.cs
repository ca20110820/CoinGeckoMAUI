using JsonFlatFileDataStore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Helpers
{
    /// <summary>
    /// Provides methods for reading from and writing to JSON files.
    /// </summary>
    public class JsonHelper
    {
        public string JsonFilePath { get; set; }  // Full Path to the Json File

        public JsonHelper() { }
        public JsonHelper(string jsonFilePath)
        {
            JsonFilePath = jsonFilePath;
        }

        /// <summary>
        /// Reads content from the JSON file and returns the deserialized object asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize into.</typeparam>
        /// <returns>The deserialized object.</returns>
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
        /// Writes the given data to the JSON file asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="data">The data to write.</param>
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

        /// <summary>
        /// Deserializes the JSON string asynchronously into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize into.</typeparam>
        /// <param name="jsonString">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static async Task<T?> DeserializeStringAsync<T>(string jsonString)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(jsonString));
        }

        /// <summary>
        /// Serializes the specified object asynchronously into a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The serialized JSON string.</returns>
        public static async Task<string> SerializeObjectAsync<T>(T obj)
        {
            return await Task.Run(() => JsonConvert.SerializeObject(obj));
        }

        /// <summary>
        /// Creates an empty JSON file at the specified path if it does not exist.
        /// </summary>
        /// <param name="jsonPath">The path to the JSON file.</param>
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
    /// Service class for performing CRUD operations on individual items in a JSON flat file database.
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

        /// <summary>
        /// Retrieves an object asynchronously from the JSON database by its key.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve.</typeparam>
        /// <param name="key">The key of the object to retrieve.</param>
        /// <returns>The retrieved object.</returns>
        public async Task<T> GetObjAsync<T>(string key)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                return await Task.Run(() => store.GetItem<T>(key));
            }
        }

        /// <summary>
        /// Retrieves all items from the JSON database as a dictionary asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of items to retrieve.</typeparam>
        /// <returns>A dictionary containing all items in the JSON database.</returns>
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

        /// <summary>
        /// Inserts an object asynchronously into the JSON database with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object to insert.</typeparam>
        /// <param name="key">The key of the object to insert.</param>
        /// <param name="obj">The object to insert.</param>
        public async Task InsertObjAsync<T>(string key, T obj)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                await store.InsertItemAsync(key, obj);
            }
        }

        /// <summary>
        /// Replaces an object asynchronously in the JSON database with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object to replace.</typeparam>
        /// <param name="keyToBeReplaced">The key of the object to be replaced.</param>
        /// <param name="newObj">The new object to replace the existing one.</param>
        public async Task ReplaceObjAsync<T>(string keyToBeReplaced, T newObj)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                await store.ReplaceItemAsync(keyToBeReplaced, newObj);
            }
        }

        /// <summary>
        /// Updates an object asynchronously in the JSON database with additional properties.
        /// </summary>
        /// <param name="key">The key of the object to update.</param>
        /// <param name="anonymousObjProperties">An anonymous object containing additional properties to add.</param>
        /// <remarks>
        /// This method is used to add new properties to a JSON object. It should not be used to modify an existing object.
        /// </remarks>
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

        /// <summary>
        /// Deletes an object asynchronously from the JSON database by its key.
        /// </summary>
        /// <param name="key">The key of the object to delete.</param>
        public async Task DeleteObjAsync(string key)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                await store.DeleteItemAsync(key);
            }
        }
    }

    /// <summary>
    /// Service class for performing CRUD operations on collections of items in a JSON flat file database.
    /// </summary>
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

        /// <summary>
        /// Queries items in the collection asynchronously based on the specified filter.
        /// </summary>
        /// <typeparam name="T">The type of items to query.</typeparam>
        /// <param name="filter">The filter function.</param>
        /// <returns>A list of items that match the filter.</returns>
        public async Task<List<T>?> QueryItems<T>(Func<dynamic, bool> filter)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                return collection.AsQueryable().Where(filter).ToList() as List<T>;
            }
        }

        /// <summary>
        /// Finds items in the collection asynchronously based on the specified pattern.
        /// </summary>
        /// <typeparam name="T">The type of items to find.</typeparam>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="useCaseSensitive">Specifies whether the search should be case-sensitive.</param>
        /// <returns>A list of items that match the pattern.</returns>
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

        /// <summary>
        /// Inserts a single item into the collection asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of item to insert.</typeparam>
        /// <param name="newItem">The item to insert.</param>
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

        /// <summary>
        /// Inserts multiple items into the collection asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of items to insert.</typeparam>
        /// <param name="newItems">The items to insert.</param>
        public async Task InsertManyItemsAsync<T>(IEnumerable<T> newItems)
        {
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                await collection.InsertManyAsync(newItems as IEnumerable<dynamic>);
            }
        }

        /// <summary>
        /// Replaces an item in the collection asynchronously based on the specified filter.
        /// </summary>
        /// <typeparam name="T">The type of item to replace.</typeparam>
        /// <param name="filter">The filter to apply.</param>
        /// <param name="newItem">The new item to replace the existing one.</param>
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

        /// <summary>
        /// Replaces items in the collection asynchronously based on the specified filter and new properties.
        /// </summary>
        /// <typeparam name="T">The type of items to replace.</typeparam>
        /// <param name="filter">The filter to apply.</param>
        /// <param name="newProperties">The new properties to replace with.</param>
        public async Task ReplaceItemsByConditionAsync<T>(Predicate<dynamic> filter, dynamic newProperties)
        {
            // Reference: https://ttu.github.io/json-flatfile-datastore/#/2.4.2/?id=replace
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                await collection.ReplaceManyAsync(filter, newProperties);
            }
        }

        /// <summary>
        /// Deletes a single item from the collection asynchronously based on the specified filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        public async Task DeleteItemAsync(Predicate<dynamic> filter)
        {
            // Reference: https://ttu.github.io/json-flatfile-datastore/#/2.4.2/?id=delete
            using (var store = await Task.Run(() => new DataStore(JsonFilePath)))
            {
                var collection = await Task.Run(() => store.GetCollection(CollectionName));

                await collection.DeleteOneAsync(filter);
            }
        }

        /// <summary>
        /// Deletes multiple items from the collection asynchronously based on the specified filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
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
