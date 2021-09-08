using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace KuCoin.NET.Data.Websockets
{
    /// <summary>
    /// Provides information about the server associated with a websocket feed connection.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Server subscription endpoint.
        /// </summary>
        [JsonProperty("endPoint")]
        public string EndPoint { get; internal set; }

        /// <summary>
        /// Connection protocol.
        /// </summary>
        [JsonProperty("protocol")]
        public string Protocol { get; internal set; }

        /// <summary>
        /// Connection is encrypted.
        /// </summary>
        [JsonProperty("encrypt")]
        public bool Encrypt { get; internal set; }

        /// <summary>
        /// The interval of time that the server expects to pass between keep-alive pings (in milliseconds.)
        /// </summary>
        [JsonProperty("pingInterval")]
        public int PingInterval { get; internal set; }

        /// <summary>
        /// The maximum amount of time the server is willing to wait for a ping before closing the connection (in milliseconds.)
        /// </summary>
        [JsonProperty("pingTimeout")]
        public int PingTimeout { get; internal set; }

    }
}
