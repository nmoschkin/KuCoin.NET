using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Order;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KuCoin.NET.Data.Websockets.User
{

    /// <summary>
    /// Order change event type
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<OrderEventType>))]
    public enum OrderEventType
    {
        /// <summary>
        /// When the order enters into the order book
        /// </summary>
        [EnumMember(Value = "open")]
        Open,


        /// <summary>
        /// When the order has been executed
        /// </summary>
        [EnumMember(Value = "match")]
        Match,


        /// <summary>
        /// When the order has been executed and its status was changed into DONE
        /// </summary>
        [EnumMember(Value = "filled")]
        Filled,


        /// <summary>
        /// When the order has been cancelled and its status was changed into DONE
        /// </summary>
        [EnumMember(Value = "canceled")]
        Canceled,


        /// <summary>
        /// When the order has been updated
        /// </summary>
        [EnumMember(Value = "update")]
        Update

    }

    /// <summary>
    /// User order change information.
    /// </summary>
    public class OrderChange : DataObject, IReadOnlySymbol, ICloneable, IStreamableObject
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Order Type
        /// </summary>
        [JsonProperty("orderType")]
        public OrderType OrderType { get; set; }


        /// <summary>
        /// Side
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }


        /// <summary>
        /// OrderId
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }


        /// <summary>
        /// Event type
        /// </summary>
        [JsonProperty("type")]
        public OrderEventType Type { get; set; }


        /// <summary>
        /// Order Time
        /// </summary>
        [JsonProperty("orderTime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime OrderTime { get; set; }



        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }




        /// <summary>
        /// Price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }


        /// <summary>
        /// Client Oid
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOid { get; set; }


        /// <summary>
        /// Remain Size
        /// </summary>
        [JsonProperty("remainSize")]
        public decimal RemainSize { get; set; }


        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }


        /// <summary>
        /// Time Stamp
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }



        /// <summary>
        /// Liquidity
        /// </summary>
        [JsonProperty("liquidity")]
        public string Liquidity { get; set; }


        /// <summary>
        /// Filled Size
        /// </summary>
        [JsonProperty("filledSize")]
        public decimal FilledSize { get; set; }


        /// <summary>
        /// Match Price
        /// </summary>
        [JsonProperty("matchPrice")]
        public decimal MatchPrice { get; set; }


        /// <summary>
        /// Match Size
        /// </summary>
        [JsonProperty("matchSize")]
        public decimal MatchSize { get; set; }


        /// <summary>
        /// Trade Id
        /// </summary>
        [JsonProperty("tradeId")]
        public string TradeId { get; set; }



        /// <summary>
        /// Old Size
        /// </summary>
        [JsonProperty("oldSize")]
        public decimal OldSize { get; set; }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        public OrderChange Clone()
        {
            return (OrderChange)MemberwiseClone();
        }
    }


}
