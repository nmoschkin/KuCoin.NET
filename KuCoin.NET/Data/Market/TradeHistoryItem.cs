using KuCoin.NET.Rest;
using KuCoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// Trade history information
    /// </summary>
    public class TradeHistoryItem : DataObject
    {
        /// <summary>
        /// Order sequence
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }

        /// <summary>
        /// Side (buy/sell)
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }

        /// <summary>
        /// Time stamp
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"{Side}: {Price} ({Size}) - {Timestamp}";
        }

    }


}
