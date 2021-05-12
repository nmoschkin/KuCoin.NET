using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Websockets
{
    public class MatchExecution : IReadOnlySymbol
    {


        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Sequence
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; set; }


        /// <summary>
        /// OrderId
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }


        /// <summary>
        /// ClientOid
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOid { get; set; }


        /// <summary>
        /// InternalTs
        /// </summary>
        [JsonProperty("ts")]
        internal long InternalTs { get; set; }

        [JsonIgnore]
        public DateTime Timestamp => EpochTime.NanosecondsToDate(InternalTs);


        /// <summary>
        /// Side
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }


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
        /// InternalOrderTime
        /// </summary>
        [JsonProperty("orderTime")]
        internal long InternalOrderTime { get; set; }

        [JsonIgnore]
        public DateTime OrderTime => EpochTime.NanosecondsToDate(InternalOrderTime);



        /// <summary>
        /// Reason
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }


        /// <summary>
        /// RemainSize
        /// </summary>
        [JsonProperty("remainSize")]
        public decimal RemainSize { get; set; }


        /// <summary>
        /// TakerOrderId
        /// </summary>
        [JsonProperty("takerOrderId")]
        public string TakerOrderId { get; set; }


        /// <summary>
        /// MakerOrderId
        /// </summary>
        [JsonProperty("makerOrderId")]
        public string MakerOrderId { get; set; }


        /// <summary>
        /// TradeId
        /// </summary>
        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

    }


}
