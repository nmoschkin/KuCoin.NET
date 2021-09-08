using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Order;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Trade
{
    /// <summary>
    /// Futures Order Details
    /// </summary>
    public class FuturesOrderDetails : DataObject
    {
        /// <summary>
        /// Order ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }


        /// <summary>
        /// Ticker symbol of the contract
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Order type, market order or limit order
        /// </summary>
        [JsonProperty("type")]
        public OrderType Type { get; set; }


        /// <summary>
        /// Transaction side
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }


        /// <summary>
        /// Order price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }


        /// <summary>
        /// Order quantity
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }


        /// <summary>
        /// Order value
        /// </summary>
        [JsonProperty("value")]
        public decimal Value { get; set; }


        /// <summary>
        /// Value of the executed orders
        /// </summary>
        [JsonProperty("filledValue")]
        public decimal FilledValue { get; set; }


        /// <summary>
        /// Executed order quantity
        /// </summary>
        [JsonProperty("filledSize")]
        public decimal FilledSize { get; set; }


        /// <summary>
        /// Self trade prevention types
        /// </summary>
        [JsonProperty("stp")]
        public StpMode Stp { get; set; }

        /// <summary>
        /// Stop order type (stop limit or stop market)
        /// </summary>
        [JsonProperty("stop")]
        public string Stop { get; set; }


        /// <summary>
        /// Trigger price type of stop orders
        /// </summary>
        [JsonProperty("stopPriceType")]
        public FuturesStopPriceType StopPriceType { get; set; }


        /// <summary>
        /// Mark to show whether the stop order is triggered
        /// </summary>
        [JsonProperty("stopTriggered")]
        public bool StopTriggered { get; set; }


        /// <summary>
        /// Trigger price of stop orders
        /// </summary>
        [JsonProperty("stopPrice")]
        public decimal StopPrice { get; set; }


        /// <summary>
        /// Time in force policy type
        /// </summary>
        [JsonProperty("timeInForce")]
        public string TimeInForce { get; set; }


        /// <summary>
        /// Mark of post only
        /// </summary>
        [JsonProperty("postOnly")]
        public bool PostOnly { get; set; }


        /// <summary>
        /// Mark of the hidden order
        /// </summary>
        [JsonProperty("hidden")]
        public bool Hidden { get; set; }


        /// <summary>
        /// Mark of the iceberg order
        /// </summary>
        [JsonProperty("iceberg")]
        public bool Iceberg { get; set; }


        /// <summary>
        /// Visible size of the iceberg order
        /// </summary>
        [JsonProperty("visibleSize")]
        public object VisibleSize { get; set; }


        /// <summary>
        /// Leverage of the order
        /// </summary>
        [JsonProperty("leverage")]
        public decimal Leverage { get; set; }


        /// <summary>
        /// A mark to forcely hold the funds for an order
        /// </summary>
        [JsonProperty("forceHold")]
        public bool ForceHold { get; set; }


        /// <summary>
        /// A mark to close the position
        /// </summary>
        [JsonProperty("closeOrder")]
        public bool CloseOrder { get; set; }


        /// <summary>
        /// A mark to reduce the position size only
        /// </summary>
        [JsonProperty("reduceOnly")]
        public bool ReduceOnly { get; set; }


        /// <summary>
        /// Unique order id created by users to identify their orders
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOid { get; set; }


        /// <summary>
        /// Remark of the order
        /// </summary>
        [JsonProperty("remark")]
        public object Remark { get; set; }


        /// <summary>
        /// Mark of the active orders
        /// </summary>
        [JsonProperty("isActive")]
        public bool IsActive { get; set; }


        /// <summary>
        /// Mark of the canceled orders
        /// </summary>
        [JsonProperty("cancelExist")]
        public bool CancelExist { get; set; }


        /// <summary>
        /// Time the order created
        /// </summary>
        [JsonProperty("createdAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CreatedAt { get; set; }


        /// <summary>
        /// Settlement currency
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleCurrency { get; set; }


        /// <summary>
        /// Order status: “open” or “done”
        /// </summary>
        [JsonProperty("status")]
        public FuturesOrderStatus Status { get; set; }


        /// <summary>
        /// Last update time
        /// </summary>
        [JsonProperty("updatedAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime UpdatedAt { get; set; }


        /// <summary>
        /// Order create time in nanosecond
        /// </summary>
        [JsonProperty("orderTime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime OrderTime { get; set; }

    }


}
