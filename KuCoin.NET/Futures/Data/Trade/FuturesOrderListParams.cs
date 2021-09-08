using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using KuCoin.NET.Json;
using KuCoin.NET.Data.Order;
using KuCoin.NET.Helpers;

namespace KuCoin.NET.Futures.Data.Trade
{
    /// <summary>
    /// Order list query parameters object.
    /// </summary>
    public class FuturesOrderListParams : DataObject
    {

        public FuturesOrderListParams()
        {
        }

        public FuturesOrderListParams(
            FuturesOrderStatus? status = null,
            string symbol = null,
            Side? side = null,
            OrderType? type = null,
            DateTime? startAt = null,
            DateTime? endAt = null
        )
        {
            Status = status;
            Symbol = symbol;
            Side = side;
            Type = type;
            StartAt = startAt;
            EndAt = endAt;
        }

        /// <summary>
        /// Order status
        /// </summary>
        [JsonProperty("status")]
        public FuturesOrderStatus? Status { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        /// <summary>
        /// Side (buy or sell)
        /// </summary>
        [JsonProperty("side")]
        public Side? Side { get; set; }

        /// <summary>
        /// Order type
        /// </summary>
        [JsonProperty("type")]
        public OrderType? Type { get; set; }

        /// <summary>
        /// Start time
        /// </summary>
        [JsonProperty("startAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime? StartAt { get; set; }

        /// <summary>
        /// End time
        /// </summary>
        [JsonProperty("endAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime? EndAt { get; set; }

    }

    public class FuturesStopOrderListParams : FuturesOrderListParams
    {


        public FuturesStopOrderListParams()
        {
        }

        public FuturesStopOrderListParams(
            FuturesOrderStatus? status = null,
            string symbol = null,
            Side? side = null,
            FuturesOrderType? type = null,
            DateTime? startAt = null,
            DateTime? endAt = null
        )
        {
            Status = status;
            Symbol = symbol;
            Side = side;
            Type = type;
            StartAt = startAt;
            EndAt = endAt;
        }

        /// <summary>
        /// Order type
        /// </summary>
        [JsonProperty("type")]
        public new FuturesOrderType? Type { get; set; }
    }
}
