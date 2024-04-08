using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Responses.Exchanges
{
    public class APIExchangeIdTickersResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tickers")]
        public List<Ticker>? Tickers { get; set; }
    }

    public class Ticker
    {
        [JsonProperty("base")]
        public string? Base { get; set; }

        [JsonProperty("target")]
        public string? Target { get; set; }

        [JsonProperty("market")]
        public Market? Market { get; set; }

        [JsonProperty("last")]
        public double? Last { get; set; }

        [JsonProperty("volume")]
        public double? Volume { get; set; }

        [JsonProperty("cost_to_move_up_usd")]
        public double? CostToMoveUpUsd { get; set; }

        [JsonProperty("cost_to_move_down_usd")]
        public double? CostToMoveDownUsd { get; set; }

        [JsonProperty("converted_last")]
        public Dictionary<string, double?>? ConvertedLast { get; set; }

        [JsonProperty("converted_volume")]
        public Dictionary<string, double?>? ConvertedVolume { get; set; }

        [JsonProperty("trust_score")]
        public string? TrustScore { get; set; }

        [JsonProperty("bid_ask_spread_percentage")]
        public double? BidAskSpreadPercentage { get; set; }

        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; }

        [JsonProperty("last_traded_at")]
        public DateTime? LastTradedAt { get; set; }

        [JsonProperty("last_fetch_at")]
        public DateTime? LastFetchAt { get; set; }

        [JsonProperty("is_anomaly")]
        public bool? IsAnomaly { get; set; }

        [JsonProperty("is_stale")]
        public bool? IsStale { get; set; }

        [JsonProperty("trade_url")]
        public string? TradeUrl { get; set; }

        [JsonProperty("token_info_url")]
        public string? TokenInfoUrl { get; set; }

        [JsonProperty("coin_id")]
        public string? CoinId { get; set; }

        [JsonProperty("target_coin_id")]
        public string? TargetCoinId { get; set; }
    }
    

    public class Market
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("identifier")]
        public string? Identifier { get; set; }

        [JsonProperty("has_trading_incentive")]
        public bool? HasTradingIncentive { get; set; }

        [JsonProperty("logo")]
        public string? Logo { get; set; }
    }
}
