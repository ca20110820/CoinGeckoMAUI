using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CoinGeckoApp.Responses
{
    public class APIExchangesIdResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("year_established")]
        public int? YearEstablished { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("image")]
        public string? Image { get; set; }

        [JsonProperty("facebook_url")]
        public string? FacebookUrl { get; set; }

        [JsonProperty("reddit_url")]
        public string? RedditUrl { get; set; }

        [JsonProperty("twitter_handle")]
        public string? TwitterHandle { get; set; }

        [JsonProperty("has_trading_incentive")]
        public bool? HasTradingIncentive { get; set; }

        [JsonProperty("centralized")]
        public bool? Centralized { get; set; }

        [JsonProperty("trust_score")]
        public int? TrustScore { get; set; }

        [JsonProperty("trust_score_rank")]
        public int? TrustScoreRank { get; set; }

        [JsonProperty("trade_volume_24h_btc")]
        public double? TradeVolume24hBtc { get; set; }

        [JsonProperty("trade_volume_24h_btc_normalized")]
        public double? TradeVolume24hBtcNormalized { get; set; }

        [JsonProperty("tickers")]
        public List<APIExchangeResponseTicker>? Tickers { get; set; }
    }


    public class APIExchangeResponseTicker
    {
        [JsonProperty("base")]
        public string? Base { get; set; }

        [JsonProperty("target")]
        public string? Target { get; set; }

        [JsonProperty("coin_id")]
        public string? CoinId { get; set; }

        [JsonProperty("target_coin_id")]
        public string? TargetCoinId { get; set; }

        [JsonProperty("last")]
        public double? Last { get; set; }

        [JsonProperty("volume")]
        public double? Volume { get; set; }

        [JsonProperty("converted_last")]
        public Dictionary<string, double?>? ConvertedLast { get; set; }

        [JsonProperty("converted_volume")]
        public Dictionary<string, double?>? ConvertedVolume { get; set; }

        [JsonProperty("bid_ask_spread_percentage")]
        public double? BidAskSpreadPercentage { get; set; }

        [JsonProperty("timestamp")]
        public string? TimeStamp { get; set; }

        [JsonProperty("last_traded_at")]
        public string? LastTradedAt { get; set; }

        [JsonProperty("last_fetch_at")]
        public string? LastFetchedAt { get; set; }

        [JsonProperty("is_anomaly")]
        public bool? IsAnomaly { get; set; }

        [JsonProperty("is_stale")]
        public bool? IsStale { get; set; }
    }
}
