using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CoinGeckoApp.Responses.Coins
{
    /// <summary>
    /// Representation of the API Response for Fetching a List of Coins.
    /// <para>This is coming from https://api.coingecko.com/api/v3/coins/list.</para>
    /// <example>
    /// This API Response will most likely be used to get a list of APICoinsListResponse.
    /// </example>
    /// </summary>
    public class APICoinsListResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("platforms")]
        public Dictionary<string, string>? Platforms { get; set; }
    }
}
