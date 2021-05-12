using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Kucoin.NET.Json;

namespace Kucoin.NET.Data.Order
{
    /// <summary>
    /// Order list query parameters object.
    /// </summary>
    public class OrderListParams : JsonDictBase
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
        /// 
        /// </summary>
        [JsonProperty("status")]
        public OrderStatus? Status { get; set; }

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
        [JsonProperty("tradeType")]
        public TradeType? TradeType { get; set; }
        
        // TO DO: Fix this up
        
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("startAt")]
        public DateTime? StartAt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("endAt")]
        public DateTime? EndAt { get; set; }

    }
}
