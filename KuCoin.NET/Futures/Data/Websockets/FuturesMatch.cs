using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Websockets
{
    /// <summary>
    /// Futures Order Match
    /// </summary>
    public class FuturesMatch : DataObject, ISymbol, IStreamableObject
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Sequence number which is used to judge the continuity of the pushed messages
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; set; }


        /// <summary>
        /// Side of liquidity taker
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }


        /// <summary>
        /// Filled quantity
        /// </summary>
        [JsonProperty("matchSize")]
        public decimal MatchSize { get; set; }


        /// <summary>
        /// UnFilled quantity
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }


        /// <summary>
        /// Filled price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }


        /// <summary>
        /// Taker order ID
        /// </summary>
        [JsonProperty("takerOrderId")]
        public string TakerOrderId { get; set; }


        /// <summary>
        /// Filled time - nanosecond
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }


        /// <summary>
        /// Maker order ID
        /// </summary>
        [JsonProperty("makerOrderId")]
        public string MakerOrderId { get; set; }


        /// <summary>
        /// Transaction ID
        /// </summary>
        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

    }


}
