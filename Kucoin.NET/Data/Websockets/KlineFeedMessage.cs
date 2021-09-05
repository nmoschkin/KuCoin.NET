using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Kucoin.NET.Observable;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

namespace Kucoin.NET.Data.Websockets
{
    /// <summary>
    /// K-Line websocket feed data packet
    /// </summary>
    /// <typeparam name="T">An implementation of <see cref="IFullCandle"/>.</typeparam>
    public class KlineFeedMessage<T> : IStreamableObject where T: IFullCandle, new()
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
