using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using KuCoin.NET.Observable;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Rest;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.Websockets
{
    /// <summary>
    /// K-Line websocket feed data packet
    /// </summary>
    /// <typeparam name="T">An implementation of <see cref="IFullCandle"/>.</typeparam>
    public class KlineFeedMessage<T> : DataObject, IStreamableObject where T: IFullCandle, new()
    {
        /// <summary>
        /// Time stamp in local time.
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The candlestick data.
        /// </summary>
        [JsonProperty("candles")]
        public T Candles { get; set; }

        /// <summary>
        /// The trading symbol that this message applies to.
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// Create a new K-Line websocket feed message packet.
        /// </summary>
        public KlineFeedMessage()
        {
        }

    }
}
