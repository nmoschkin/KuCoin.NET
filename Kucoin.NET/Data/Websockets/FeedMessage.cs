using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kucoin.NET.Json;

namespace Kucoin.NET.Data.Websockets
{
    public class FeedMessage : JsonDictBase
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("privateChannel")]
        public bool? PrivateChannel { get; set; }

        [JsonProperty("response")]
        public bool? Response { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("tunnelId")]
        public string TunnelId { get; set; }

        [JsonProperty("newTunnelId")]
        public string NewTunnelId { get; set; }

        [JsonProperty("data")] 
        public JToken Data { get; set; }

    }
}
