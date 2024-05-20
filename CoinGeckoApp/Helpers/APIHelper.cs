using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Helpers
{
    /// <summary>
    /// Provides helper methods for making API requests and handling responses.
    /// </summary>
    public class APIHelper
    {
        private const string baseUrl = "https://api.coingecko.com/api/v3/";
        private string endpointFullUrl = string.Empty;

        public string Endpoint { get => endpointFullUrl; }

        public APIHelper() { }

        public APIHelper(string endpoint)
        {
            endpointFullUrl = baseUrl + endpoint + "/";  // Example: https://api.coingecko.com/api/v3/simple/ [supported_vs_currencies][?parameters]
        }

        public string GetFullUrl(string requestCategory, string? requestParams)
        {
            /* Examples:
             * https://api.coingecko.com/api/v3/coins/bitcoin?tickers=false&market_data=false&community_data=false&developer_data=false&sparkline=true
             * https://api.coingecko.com/api/v3/coins/list?include_platform=true
             * https://api.coingecko.com/api/v3/coins/bitcoin/market_chart/range?vs_currency=usd&from=1392577232&to=1422577232&precision=full
             */
            string tempParams = !string.IsNullOrEmpty(requestParams) ? requestParams : string.Empty;
            return Endpoint + requestCategory + "?" + tempParams;
        }

        /// <summary>
        /// Fetches the Json String Response.
        /// <para>Warning: The fetched json string could be really large and may be slow when deserializing.</para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string?> GetDataAsStringCustomURLAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
            }
        }

        /// <summary>
        /// Fetches and deserializes the JSON string asynchronously with the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="url">The URL to fetch data from.</param>
        /// <returns>The deserialized object.</returns>
        public async static Task<T?> FetchAndJsonDeserializeAsync<T>(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (Stream responseString = await client.GetStreamAsync($"{url}"))
                {
                    using (StreamReader streamReader = new StreamReader(responseString))
                    {
                        using (JsonReader reader = new JsonTextReader(streamReader))
                        {
                            JsonSerializer serializer = new JsonSerializer();

                            // read the json from a stream
                            // json size doesn't matter because only a small piece is read at a time from the HTTP request
                            return await Task.Run(() => serializer.Deserialize<T>(reader));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Provides helper methods for constructing URIs and query strings.
    /// </summary>
    public class URIHelper
    {
        public string RootURI {  get; set; }

        public URIHelper(string rootURI)
        {
            RootURI = rootURI;  // e.g. "https://api.coingecko.com"
        }

        /// <summary>
        /// Gets a new instance of UriBuilder using the root URI.
        /// </summary>
        /// <returns>A new UriBuilder instance.</returns>
        public UriBuilder GetNewBuilder()
        {
            return new UriBuilder(RootURI);
        }

        /// <summary>
        /// Combines the specified endpoints to form a complete endpoint.
        /// </summary>
        /// <param name="endpoints">The endpoints to combine.</param>
        /// <returns>The complete endpoint.</returns>
        public static string MakeEndpoint(params string[] endpoints)
        {
            // e.g. Input: ["api", "v3", "some_endpoint"] ==> "/api/v3/some_endpoint"
            string outResult = "";
            foreach (string endpoint in endpoints)
            {
                outResult += $"/{endpoint}";
            }

            return outResult;
        }

        /// <summary>
        /// Creates a query string from the specified key-value tuples.
        /// </summary>
        /// <param name="parameterTuples">The key-value tuples.</param>
        /// <returns>The query string.</returns>
        public static string MakeQueryFromTuples(params Tuple<string, string>[] parameterTuples)
        {
            /* Example:
             * Given [("k1", "v1"), ("k2", "v2")], we want "k1=v1&k2=v2"
             * 
             * In C#:
             * URIHelper.MakeQueryFromTuples(Tuple.Create("k1", "v1"), Tuple.Create("k2", "v2"));
             */
            StringBuilder queryBuilder = new StringBuilder();

            foreach (Tuple<string, string> tuple in parameterTuples)
            {
                if (queryBuilder.Length > 0)
                    queryBuilder.Append("&");

                queryBuilder.Append($"{tuple.Item1}={tuple.Item2}");
            }

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Constructs a URI with the specified endpoint and query strings.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="query">The query strings.</param>
        /// <returns>The constructed URI.</returns>
        public string MakeURI(string endpoint, params string[] query)
        {
            /* Example:
             * MakeURI("/api/v3/coins/list", "key1=value1", "key2=value2", "key3=value3", ...)
             * <==>
             * <root-uri>/api/v3/someendpoint?key1=value1&key1=value1
             */
            UriBuilder builder = GetNewBuilder();
            builder.Path = endpoint;  // Append the Endpoint to root uri
            builder.Query = string.Join('&', query);  // Append the key-value pair queries
            return builder.Uri.ToString();
        }

        /// <summary>
        /// Constructs a URI with the specified endpoint and query string.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="query">The query string.</param>
        /// <returns>The constructed URI.</returns>
        public string MakeURI(string endpoint, string query)
        {
            // Overload where query is a string "k1=v1&k2=v2"
            UriBuilder builder = GetNewBuilder();
            builder.Path = endpoint;  // Append the Endpoint to root uri
            builder.Query = query;  // Append the key-value pair queries
            return builder.Uri.ToString();
        }
    }
}
