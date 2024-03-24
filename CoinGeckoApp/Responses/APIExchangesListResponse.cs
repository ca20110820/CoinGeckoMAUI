using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CoinGeckoApp.Responses.Exchanges
{
    /// <summary>
    /// Representation of the API Response for Fetching a List of Exchanges.
    /// <para>This is coming from https://api.coingecko.com/api/v3/exchanges/list.</para>
    /// </summary>
    public class APIExchangesListResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
