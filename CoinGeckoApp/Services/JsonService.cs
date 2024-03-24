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
}
