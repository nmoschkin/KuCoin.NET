using System;
using System.Collections.Generic;
using System.Text;

using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Private;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.Websockets
{
    /// <summary>
    /// Level 2 New Sequence Changes
    /// </summary>
    public class Changes
    {
        /// <summary>
        /// Asks (from sellers)
        /// </summary>
        [JsonProperty("asks")]
        public List<OrderUnit> Asks { get; set; }

        /// <summary>
        /// Bids (from buyers)
        /// </summary>
        [JsonProperty("bids")]
        public List<OrderUnit> Bids { get; set; }
    }

    /// <summary>
    /// Level 2 Static Market Depth Feed (5/50) Changes
    /// </summary>
    public class StaticMarketDepthUpdate : Changes, ISymbol
    {
        public string Symbol { get; set; }

        /// <summary>
        /// Gets the market depth of this update.
        /// </summary>
        public Level2Depth Depth => Asks?.Count == 50 ? Level2Depth.Depth50 : Level2Depth.Depth5;

        void ISymbol.SetSymbol(string symbol)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// InternalTimestamp
        /// </summary>
        [JsonProperty("timestamp")]
        internal long InternalTimestamp { get; set; }

        [JsonIgnore]
        public DateTime Timestamp => EpochTime.MillisecondsToDate(InternalTimestamp);

    }

    /// <summary>
    /// Level 2 Full Market Depth Data Update
    /// </summary>
    public class Level2Update : ISymbol
    {
        void ISymbol.SetSymbol(string symbol)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// Update sequence start
        /// </summary>
        [JsonProperty("sequenceStart")]
        public long SequenceStart { get; set; }

        /// <summary>
        /// Update sequence end
        /// </summary>
        [JsonProperty("sequenceEnd")]
        public long SequenceEnd { get; set; }

        /// <summary>
        /// The symbol for this update
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// The changes for this sequence
        /// </summary>
        [JsonProperty("changes")]
        public Changes Changes { get; set; }

    }
}
