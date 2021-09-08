using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace KuCoin.NET.Data.Websockets
{
    /// <summary>
    /// Subscription data 
    /// </summary>
    public class SubscriptionData
    {
        /// <summary>
        /// List of available servers
        /// </summary>
        [JsonProperty("instanceServers")]
        public IReadOnlyList<Server> Servers;

        /// <summary>
        /// The unique token for subscribing new feeds
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; internal set; }
    }

    /// <summary>
    /// Token application receipt
    /// </summary>
    public class TokenApplication
    {
        [JsonProperty("code")]
        public int Code { get; internal set; }

        /// <summary>
        /// Subscription data
        /// </summary>
        [JsonProperty("data")]
        public SubscriptionData Data { get; internal set; }
    }
}
