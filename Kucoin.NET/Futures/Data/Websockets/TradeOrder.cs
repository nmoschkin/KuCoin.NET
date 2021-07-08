﻿using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Futures.Data.Trade;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kucoin.NET.Futures.Data.Websockets
{
    [JsonConverter(typeof(EnumToStringConverter<TradeMessageType>))]
    public enum TradeMessageType
    {
        [EnumMember(Value = "match")]
        Match,

        [EnumMember(Value = "open")]
        Open,

        [EnumMember(Value = "filled")]
        Filled,

        [EnumMember(Value = "canceled")]
        Canceled,

        [EnumMember(Value = "update")]
        Update
    }

    /// <summary>
    /// Futures Trade Order 
    /// </summary>
    public class TradeOrder : ISymbol
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