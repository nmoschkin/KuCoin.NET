using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KuCoin.NET.Json;
using System.Runtime.CompilerServices;

namespace KuCoin.NET.Data.Websockets
{
    /// <summary>
    /// Websocket feed packet object
    /// </summary>
    public class FeedMessage : DataObject
    {

        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Topic
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }

        /// <summary>
        /// True if private subscription
        /// </summary>
        [JsonProperty("privateChannel")]
        public bool? PrivateChannel { get; set; }

        /// <summary>
        /// True if response requested
        /// </summary>
        [JsonProperty("response")]
        public bool? Response { get; set; }

        /// <summary>
        /// Subject
        /// </summary>
        [JsonProperty("subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Multiplex tunnel Id
        /// </summary>
        [JsonProperty("tunnelId")]
        public string TunnelId { get; set; }

        /// <summary>
        /// Multiplex create new tunnel Id
        /// </summary>
        [JsonProperty("newTunnelId")]
        public string NewTunnelId { get; set; }

        /// <summary>
        /// Deserialized data contents
        /// </summary>
        [JsonProperty("data")] 
        public virtual JToken Data { get; set; }

    }

    /// <summary>
    /// Websocket feed packet object
    /// </summary>
    public class FeedMessage<T> : FeedMessage
    {
        /// <summary>
        /// Deserialized data contents
        /// </summary>
        [JsonProperty("data")]
        new public virtual T Data { get; set; }

    }


}
