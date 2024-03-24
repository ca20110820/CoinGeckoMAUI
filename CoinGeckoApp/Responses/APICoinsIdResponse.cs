using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Responses.Coins
{
    public class APICoinsIdResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("web_slug")]
        public string? WebSlug { get; set; }

        [JsonProperty("asset_platform_id")]
        public string? AssetPlatformId { get; set; }

        public Links? links { get; set;}

        [JsonProperty("genesis_date")]
        public string? GenesisDate { get; set; }

        [JsonProperty("sentiment_votes_up_percentage")]
        public double? SentimentVotesUpPerc { get; set; }

        [JsonProperty("sentiment_votes_down_percentage")]
        public double? SentimentVotesDownPerc { get; set; }

        [JsonProperty("market_cap_rank")]
        public int? MarketCapRank { get; set; }

        [JsonProperty("watchlist_portfolio_users")]
        public int? WatchlistPortfolioUsers { get; set; }

        [JsonProperty("market_data")]
        public MarketData? market_data { get; set; }

        [JsonProperty("community_data")]
        public CommunityData? community_data { get; set; }

        [JsonProperty("developer_data")]
        public DeveloperData? developer_data { get; set; }

        public string? last_updated { get; set; }
        public List<Ticker>? tickers { get; set; }
    }

    public class Links
    {
        public List<string>? homepage { get; set; }
        public string? whitepaper { get; set; }
        public List<string>? blockchain_site { get; set; }
        public List<string>? official_forum_url { get; set; }
        public List<string>? chat_url { get; set; }
        public List<string>? announcement_url { get; set; }
        public string? subreddit_url { get; set; }
        public Dictionary<string, List<string>?>? repos_url { get; set; }
    }

    public class MarketData
    {
        public Dictionary<string, double?>? current_price { get; set; }
        public Dictionary<string, double?>? total_value_locked { get; set; }
        public double? mcap_to_tvl_ratio { get; set; }
        public double? fdv_to_tvl_ratio { get; set; }
        public ROI? roi { get; set; }
        public Dictionary<string, double?>? ath { get; set; }
        public Dictionary<string, double?>? ath_change_percentage { get; set; }
        public Dictionary<string, string?>? ath_date { get; set; }
        public Dictionary<string, double?>? atl { get; set; }
        public Dictionary<string, double?>? atl_change_percentage { get; set; }
        public Dictionary<string, string?>? atl_date { get; set; }
        public Dictionary<string, long?>? market_cap { get; set; }
        public int? market_cap_rank { get; set; }
        public Dictionary<string, long?>? fully_diluted_valuation { get; set; }
        public double? market_cap_fdv_ratio { get; set; }
        public Dictionary<string, long?>? total_volume { get; set; }
        public Dictionary<string, double?>? high_24h { get; set; }
        public Dictionary<string, double?>? low_24h { get; set; }
        public double? price_change_24h { get; set; }
        public double? price_change_percentage_24h { get; set; }
        public double? price_change_percentage_7d { get; set; }
        public double? price_change_percentage_14d { get; set; }
        public double? price_change_percentage_30d { get; set; }
        public double? price_change_percentage_60d { get; set; }
        public double? price_change_percentage_200d { get; set; }
        public double? price_change_percentage_1y { get; set; }
        public double? market_cap_change_24h { get; set; }
        public double? market_cap_change_percentage_24h { get; set; }
        public Dictionary<string, double?>? price_change_24h_in_currency { get; set; }
        public Dictionary<string, double?>? price_change_percentage_1h_in_currency { get; set; }
        public Dictionary<string, double>? price_change_percentage_24h_in_currency { get; set; }
        public Dictionary<string, double?>? price_change_percentage_7d_in_currency { get; set; }
        public Dictionary<string, double?>? price_change_percentage_14d_in_currency { get; set; }
        public Dictionary<string, double?>? price_change_percentage_30d_in_currency { get; set; }
        public Dictionary<string, double?>? price_change_percentage_60d_in_currency { get; set; }
        public Dictionary<string, double?>? price_change_percentage_200d_in_currency { get; set; }
        public Dictionary<string, double?>? price_change_percentage_1y_in_currency { get; set; }
        public Dictionary<string, double?>? market_cap_change_24h_in_currency { get; set; }
        public Dictionary<string, double?>? market_cap_change_percentage_24h_in_currency { get; set; }
        public double? total_supply { get; set; }
        public double? max_supply { get; set; }
        public Dictionary<string, List<double>?>? sparkline_7d { get; set; }
        public string? last_updated { get; set; }
    }
    public class ROI
    {
        public double? times { get; set; }
        public string? currency { get; set; }
        public double? percentage { get; set; }
    }

    public class CommunityData
    {
        public long? facebook_likes { get; set; }
        public long? twitter_followers { get; set; }
        public double? reddit_average_posts_48h { get; set; }
        public double? reddit_average_comments_48h { get; set; }
        public long? reddit_subscribers { get; set; }
        public long? reddit_accounts_active_48h { get; set; }
        public long? telegram_channel_user_count { get; set; }
    }

    public class DeveloperData
    {
        public int? forks { get; set; }
        public int? stars { get; set; }
        public int? subscribers { get; set; }
        public int? total_issues { get; set; }
        public int? closed_issues { get; set; }
        public int? pull_requests_merged { get; set; }
        public int? pull_request_contributors { get; set; }
        public Dictionary<string, int?>? code_additions_deletions_4_weeks { get; set; }
        public int? commit_count_4_weeks { get; set; }
    }

    public class Ticker
    {
        public string? Base { get; set; }
        public string? target { get; set; }
        public Market? market { get; set; }
        public double? last { get; set; }
        public double? volume { get; set; }
        public Dictionary<string, double>? converted_last { get; set; }
        public Dictionary<string, double>? converted_volume { get; set; }
        public string? trust_score { get; set; }
        public double? bid_ask_spread_percentage { get; set; }
        public string? timestamp { get; set; }
        public string? last_traded_at { get; set; }
        public string? last_fetch_at { get; set; }
        public bool? is_anomaly { get; set; }
        public bool? is_stale { get; set; }
        public string? trade_url { get; set; }
        public string? token_info_url { get; set; }
        public string? coin_id { get; set; }
        public string? target_coin_id { get; set; }
    }
    public class Market
    {
        public string? name { get; set; }
        public string? identifier { get; set; }
        public bool? has_trading_incentive { get; set; }
    }
}
