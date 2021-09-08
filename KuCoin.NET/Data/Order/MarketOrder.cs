using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.Order
{
    /// <summary>
    /// Market order
    /// </summary>
    public class MarketOrder : OrderBase
    {
        public MarketOrder()
        {
            Type = OrderType.Market;
        }

        /// <summary>
        /// [Optional] Desired amount in base currency
        /// </summary>
        [JsonProperty("size")]
        public decimal? Size { get; set; }

        /// <summary>
        /// [Optional] The desired amount of quote currency to use
        /// </summary>
        [JsonProperty("funds")]
        public decimal? Funds { get; set; }
    }

    /// <summary>
    /// Market order with stop order parameters.
    /// </summary>
    public class MarketStopOrder : MarketOrder
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
