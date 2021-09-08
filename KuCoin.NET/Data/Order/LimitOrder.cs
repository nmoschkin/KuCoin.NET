using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.Order
{
    /// <summary>
    /// Limit order
    /// </summary>
    public class LimitOrder : OrderBase
    {
        public LimitOrder()
        {
            Type = OrderType.Limit;
        }

        /// <summary>
        /// Price per base currency
        /// </summary>
        [JsonProperty("price")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Amount of base currency to buy or sell
        /// </summary>
        [JsonProperty("size")]
        public decimal? Size { get; set; }


        /// <summary>
        /// [Optional] GTC, GTT, IOC, or FOK (default is GTC), read Time In Force.
        /// </summary>
        [JsonProperty("timeInForce")]
        public TimeInForce? TimeInForce { get; set; } = Order.TimeInForce.GoodTillCanceled;

        /// <summary>
        /// [Optional] cancel after n seconds, requires timeInForce to be GTT
        /// </summary>
        [JsonProperty("cancelAfter")]
        public TimeSpan? CancelAfter { get; set; }

        /// <summary>
        /// [Optional] Post only flag, invalid when timeInForce is IOC or FOK
        /// </summary>
        [JsonProperty("postOnly")]
        public bool? PostOnly { get; set; }

        /// <summary>
        /// [Optional] Order will not be displayed in the order book
        /// </summary>
        [JsonProperty("hidden")]
        public bool? Hidden { get; set; }

        /// <summary>
        /// [Optional] Only aportion of the order is displayed in the order book
        /// </summary>
        [JsonProperty("iceberg")]
        public bool? Iceberg { get; set; }

        /// <summary>
        /// [Optional] The maximum visible size of an iceberg order
        /// </summary>
        [JsonProperty("visibleSize")]
        public decimal? VisibleSize { get; set; }

    }

    /// <summary>
    /// Limit order with stop order parameters
    /// </summary>
    public class LimitStopOrder : LimitOrder
    {
        /// <summary>
        ///  stop type
        /// </summary>
        [JsonProperty("stop")]
        public StopType Stop { get; set; }

        /// <summary>
        ///  stop price
        /// </summary>
        [JsonProperty("stopPrice")]
        public decimal StopPrice { get; set; }
    
    }


}
