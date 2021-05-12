using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Websockets.User
{
    public class BalanceNotice
    {

        /// <summary>
        /// Total
        /// </summary>
        [JsonProperty("total")]
        public decimal Total { get; set; }

        /// <summary>
        /// Available
        /// </summary>
        [JsonProperty("available")]
        public decimal Available { get; set; }

        /// <summary>
        /// AvailableChange
        /// </summary>
        [JsonProperty("availableChange")]
        public decimal AvailableChange { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Hold
        /// </summary>
        [JsonProperty("hold")]
        public decimal Hold { get; set; }

        /// <summary>
        /// HoldChange
        /// </summary>
        [JsonProperty("holdChange")]
        public decimal HoldChange { get; set; }

        /// <summary>
        /// RelationEvent
        /// </summary>
        [JsonProperty("relationEvent")]
        public string RelationEvent { get; set; }

        /// <summary>
        /// RelationEventId
        /// </summary>
        [JsonProperty("relationEventId")]
        public string RelationEventId { get; set; }

        /// <summary>
        /// RelationContext
        /// </summary>
        [JsonProperty("relationContext")]
        public RelationContextClass RelationContext { get; set; }

        /// <summary>
        /// InternalTime
        /// </summary>
        [JsonProperty("time")]
        internal long InternalTime { get; set; }

        [JsonIgnore]
        public DateTime Time => EpochTime.MillisecondsToDate(InternalTime);


    }

    public class RelationContextClass
    {

        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// TradeId
        /// </summary>
        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        /// <summary>
        /// OrderId
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

    }

}
