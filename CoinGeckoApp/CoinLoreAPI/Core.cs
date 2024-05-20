using CoinGeckoApp.Helpers;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoinGeckoApp.CoinLoreAPI.Core
{
    /// <summary>
    /// Provides methods for generating HTML code for CoinLore widgets.
    /// </summary>
    public class WidgetHTML
    {
        public WidgetHTML() { }

        /// <summary>
        /// Generates HTML code for a cryptocurrency list widget.
        /// </summary>
        /// <param name="includeDataMcap">Determines whether to include market cap data in the widget.</param>
        /// <param name="dataTop">Specifies the number of top cryptocurrencies to display in the widget.</param>
        /// <param name="dataMcurrency">Specifies the currency for market data (default: "usd").</param>
        /// <returns>HTML code for the cryptocurrency list widget.</returns>
        public static string GenerateHtmlCryptoListWidget(bool includeDataMcap = true, int dataTop = 10, string dataMcurrency = "usd")
        {
            int dataMcap = includeDataMcap ? 1 : 0;

            string htmlCode = @"
            <script type=""text/javascript"" src=""https://widget.coinlore.com/widgets/coinlore-list-widget.js""></script>
            <div 
                class=""coinlore-list-widget"" 
                data-mcap=""{0}"" 
                data-mcurrency=""{1}"" 
                data-top=""{2}"" 
                data-cwidth=""100%"" 
                data-bcolor=""#fff"" 
                data-coincolor=""#428bca"" 
                data-pricecolor=""#4c4c4c"" 
                style=""min-height: 382px; width: 100%;""
            ></div>";

            return string.Format(htmlCode, dataMcap, dataMcurrency, dataTop);
        }

        /// <summary>
        /// Generates HTML code for a coin widget.
        /// </summary>
        /// <param name="dataId">The ID of the cryptocurrency for the widget.</param>
        /// <param name="dataMcurrency">Specifies the currency for market data (default: "usd").</param>
        /// <returns>HTML code for the coin widget.</returns>
        public static string GenerateHtmlCoinWidget(string dataId, string dataMcurrency = "usd")
        {
            string htmlCode = @"
            <script type=""text/javascript"" src=""https://widget.coinlore.com/widgets/new-widget.js""></script>
            <div 
                class=""coinlore-coin-widget"" 
                data-mcap=""1"" 
                data-mcurrency=""{0}"" 
                data-d7=""1"" 
                data-cwidth="""" 
                data-rank=""1"" 
                data-vol=""1"" 
                data-id=""{1}"" 
                data-bcolor="""" 
                data-tcolor=""#333"" 
                data-ccolor=""#333"" 
                data-pcolor="""" 
                style=""height:189px""
            ></div>";

            return string.Format(htmlCode, dataMcurrency, dataId);
        }

        /// <summary>
        /// Generates HTML code for a price ticker widget.
        /// </summary>
        /// <param name="dataMcurrency">Specifies the currency for market data (default: "usd").</param>
        /// <returns>HTML code for the price ticker widget.</returns>
        public static string GenerateHtmlPriceTickerWidget(string dataMcurrency = "usd")
        {
            string htmlCode = @"
            <script type=""text/javascript"" src=""https://widget.coinlore.com/widgets/ticker-widget.js""></script>
            <div 
                class=""coinlore-priceticker-widget"" 
                data-mcurrency=""{0}"" 
                data-bcolor="""" 
                data-scolor=""#333"" 
                data-ccolor="""" 
                data-pcolor=""#428bca""
            ></div>";

            return string.Format(htmlCode, dataMcurrency);
        }
    }
}