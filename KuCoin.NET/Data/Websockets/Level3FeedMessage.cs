using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Data.Websockets
{

    /// <summary>
    /// Websocket feed packet object
    /// </summary>
    public struct Level3FeedMessage : IDataObject
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
        public Level3Update Data { get; set; }


        public Dictionary<string, object> ToDict() => DataObject.ToDict(this);
    }
}
