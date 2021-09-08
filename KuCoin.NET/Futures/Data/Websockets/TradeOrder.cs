using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Order;
using KuCoin.NET.Futures.Data.Trade;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KuCoin.NET.Futures.Data.Websockets
{
    /// <summary>
    /// Trade Message Types
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<TradeMessageType>))]
    public enum TradeMessageType
    {
        /// <summary>
        /// Match
        /// </summary>
        [EnumMember(Value = "match")]
        Match,

        /// <summary>
        /// Open
        /// </summary>
        [EnumMember(Value = "open")]
        Open,

        /// <summary>
        /// Filled
        /// </summary>
        [EnumMember(Value = "filled")]
        Filled,

        /// <summary>
        /// Canceled
        /// </summary>
        [EnumMember(Value = "canceled")]
        Canceled,

        /// <summary>
        /// Updated
        /// </summary>
        [EnumMember(Value = "update")]
        Update
    }

    /// <summary>
    /// Futures Trade Order 
    /// </summary>
    public class TradeOrder : DataObject, ISymbol, IStreamableObject
    {
        /// <summary>
        /// Order ID
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }


        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Message Type: "open", "match", "filled", "canceled", "update"
        /// </summary>
        [JsonProperty("type")]
        public TradeMessageType Type { get; set; }


        /// <summary>
        /// Order Status: "match", "open", "done"
        /// </summary>
        [JsonProperty("status")]
        public FuturesOrderStatus Status { get; set; }

        /// <summary>
        /// Match Size (when the type is "match")
        /// </summary>
        [JsonProperty("matchSize")]
        public decimal MatchSize { get; set; }


        /// <summary>
        /// Match Price (when the type is "match")
        /// </summary>
        [JsonProperty("matchPrice")]
        public decimal MatchPrice { get; set; }


        /// <summary>
        /// Order Type, "market" indicates market order, "limit" indicates limit order
        /// </summary>
        [JsonProperty("orderType")]
        public FuturesOrderType OrderType { get; set; }


        /// <summary>
        /// Trading direction,include buy and sell
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }


        /// <summary>
        /// Order Price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }


        /// <summary>
        /// Order Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }


        /// <summary>
        /// Remaining Size for Trading
        /// </summary>
        [JsonProperty("remainSize")]
        public decimal RemainSize { get; set; }


        /// <summary>
        /// Filled Size
        /// </summary>
        [JsonProperty("filledSize")]
        public decimal FilledSize { get; set; }


        /// <summary>
        /// In the update message, the Size of order reduced
        /// </summary>
        [JsonProperty("canceledSize")]
        public decimal CanceledSize { get; set; }


        /// <summary>
        /// Trade ID (when the type is "match")
        /// </summary>
        [JsonProperty("tradeId")]
        public string TradeId { get; set; }


        /// <summary>
        /// ClientOid
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOid { get; set; }


        /// <summary>
        /// Order Time
        /// </summary>
        [JsonProperty("orderTime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime OrderTime { get; set; }


        /// <summary>
        /// Size Before Update (when the type is "update")
        /// </summary>
        [JsonProperty("oldSize ")]
        public decimal OldSize { get; set; }


        /// <summary>
        /// Trading direction, buy or sell in taker
        /// </summary>
        [JsonProperty("liquidity")]
        public LiquidityType Liquidity { get; set; }


        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }

    }

}
