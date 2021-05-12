using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kucoin.NET.Data.Websockets
{
    public class SubscriptionData
    {
        [JsonProperty("instanceServers")]
        public IReadOnlyList<Server> Servers;

        [JsonProperty("token")]
        public string Token { get; internal set; }
    }

    public class TokenApplication
    {
        [JsonProperty("code")]
        public int Code { get; internal set; }

        [JsonProperty("data")]
        public SubscriptionData Data { get; internal set; }
    }
}
