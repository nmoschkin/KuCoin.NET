using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Mark price structure.
    /// </summary>
    public class Price
    {

        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// Granularity
        /// </summary>
        [JsonProperty("granularity")]
        public long Granularity { get; set; }

        /// <summary>
        /// InternalTimestamp
        /// </summary>
        [JsonProperty("timestamp")]
        internal long? InternalTimestamp { get; set; }

        /// <summary>
        /// InternalTimestamp
        /// </summary>
        [JsonProperty("timePoint")]
        internal long? InternalTimepoint { get; set; }

        [JsonIgnore]
        public DateTime Timestamp => EpochTime.MillisecondsToDate(InternalTimestamp ?? InternalTimepoint ?? 0);


        /// <summary>
        /// Value
        /// </summary>
        [JsonProperty("value")]
        public decimal Value { get; set; }

    }
}
