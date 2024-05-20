using CoinGeckoApp.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Models
{
    /// <summary>
    /// Represents an Exchange.
    /// </summary>
    public class ExchangeModel
    {
        
        public string Id { get; set; }


        public ExchangeModel() { }
        public ExchangeModel(string id) 
        {  
            Id = id; 
        }

        /// <summary>
        /// Fetches a list of exchange IDs asynchronously.
        /// </summary>
        /// <returns>The task result contains a list of dictionaries representing exchange IDs and names.</returns>
        public static async Task<List<Dictionary<string, string>>?> FetchExchangeIds()
        {
            /* Reponse Form:
             * [{"id": "<value>", "name": "<value>"}, 
             * {"id": "<value>", "name": "<value>"}, 
             * ...]
             */
            string url = "https://api.coingecko.com/api/v3/exchanges/list";
            return await APIHelper.FetchAndJsonDeserializeAsync<List<Dictionary<string, string>>>(url);
        }

        /// <summary>
        /// Retrieves a list of exchange IDs asynchronously.
        /// </summary>
        /// <returns>The task result contains a list of exchange IDs.</returns>
        public static async Task<List<string>?> GetExchangeIds()
        {
            List<Dictionary<string, string>>? exchanges = await FetchExchangeIds();
            if (exchanges == null) return null;
            return exchanges.Select(exchange => exchange["id"]).ToList();
        }
    }
}
