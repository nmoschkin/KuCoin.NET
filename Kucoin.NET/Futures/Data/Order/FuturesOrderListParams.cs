using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Kucoin.NET.Json;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Futures.Data.Order
{
    /// <summary>
    /// Order list query parameters object.
    /// </summary>
    public class FuturesOrderListParams : JsonDictBase
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
        /// 
        /// </summary>
        [JsonProperty("status")]
        public FuturesOrderStatus? Status { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("side")]
        public Side? Side { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("type")]
        public OrderType? Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("startAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime? StartAt { get; set; }

        /// <summary>
        /// 
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

        [JsonProperty("type")]
        public new FuturesOrderType? Type { get; set; }
    }
}
