using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kucoin.NET.Data.Websockets
{
    [JsonConverter(typeof(EnumToStringConverter<DoneReason>))]
    public enum DoneReason
    {
        [EnumMember(Value="filled")]
        Filled,

        [EnumMember(Value = "canceled")]
        Canceled
    }


    /// <summary>
    /// Level 3 Match Engine Update
    /// </summary>
    public struct Level3Update : ILevel3Update
    {
        /// <summary>
        /// Sequence
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

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
        /// Side
        /// </summary>
        [JsonProperty("side")]
        public Side? Side { get; set; }


        /// <summary>
        /// Price
        /// </summary>
        [JsonProperty("price")]
        public decimal? Price { get; set; }


        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public decimal? Size { get; set; }


        /// <summary>
        /// RemainSize
        /// </summary>
        [JsonProperty("remainSize")]
        public decimal? RemainSize { get; set; }


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

        /// <summary>
        /// Done Reason
        /// </summary>
        [JsonProperty("reason")]
        public DoneReason? Reason { get; set; }


        /// <summary>
        /// Order time
        /// </summary>
        [JsonProperty("orderTime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime? OrderTime { get; set; }


        /// <summary>
        /// Time Stamp
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime? Timestamp { get; set; }


        [JsonIgnore]
        public string Subject { get; set; }


    }

}
