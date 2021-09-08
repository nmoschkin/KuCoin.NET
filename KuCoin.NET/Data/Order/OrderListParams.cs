using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using KuCoin.NET.Json;
using KuCoin.NET.Helpers;

namespace KuCoin.NET.Data.Order
{
    /// <summary>
    /// Order list query parameters object.
    /// </summary>
    public class OrderListParams : DataObject
    {

        public OrderListParams()
        {
        }

        public OrderListParams(
            OrderStatus? status = null,
            string symbol = null,
            Side? side = null,
            OrderType? type = null,
            TradeType? tradeType = null,
            DateTime? startAt = null,
            DateTime? endAt = null
        )
        {
            Status = status;
            Symbol = symbol;
            Side = side;
            Type = type;
            TradeType = tradeType;
            StartAt = startAt;
            EndAt = endAt;
        }

        /// <summary>
        /// Order status
        /// </summary>
        [JsonProperty("status")]
        public OrderStatus? Status { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        
        /// <summary>
        /// Trading side (buy or sell)
        /// </summary>
        [JsonProperty("side")]
        public Side? Side { get; set; }

        /// <summary>
        /// Order type
        /// </summary>
        [JsonProperty("type")]
        public OrderType? Type { get; set; }

        /// <summary>
        /// Trade type
        /// </summary>
        [JsonProperty("tradeType")]
        public TradeType? TradeType { get; set; }
        
        
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
}
