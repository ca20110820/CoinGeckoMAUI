using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Responses.Coins
{
    public class APICoinsMarketChartResponse
    {
        [JsonProperty("prices")]
        public List<List<double>>? Prices { get; set; }

        [JsonProperty("market_caps")]
        public List<List<double>>? MarketCaps { get; set; }

        [JsonProperty("total_volumes")]
        public List<List<double>>? Volumes { get; set; }
    }
}
