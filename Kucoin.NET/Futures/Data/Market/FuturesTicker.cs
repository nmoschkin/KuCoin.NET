using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Market
{
    /// <summary>
    /// Futures symbol ticker
    /// </summary>
    public class FuturesTicker : Ticker
    {
        /// <summary>
        /// Liquidity taker
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }

        /// <summary>
        /// Time Stamp
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public override DateTime Timestamp { get; set; }

    }
}
