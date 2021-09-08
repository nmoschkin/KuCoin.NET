using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.Order
{
    public class OrderDetails : OrderBase
    {
        /// <summary>
        /// orderid
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        ///  operation type: DEAL
        /// </summary>
        [JsonProperty("opType")]
        public string OpType { get; set; }

        /// <summary>
        ///  deal funds
        /// </summary>
        [JsonProperty("dealFunds")]
        public decimal? DealFunds { get; set; }

        /// <summary>
        ///  deal quantity
        /// </summary>
        [JsonProperty("dealSize")]
        public decimal? DealSize { get; set; }

        /// <summary>
        ///  fee
        /// </summary>
        [JsonProperty("fee")]
        public decimal? Fee { get; set; }

        /// <summary>
        ///  charge fee currency
        /// </summary>
        [JsonProperty("feeCurrency")]
        public string FeeCurrency { get; set; }

        /// <summary>
        ///  stop type
        /// </summary>
        [JsonProperty("stop")]
        public StopType? Stop { get; set; }

        /// <summary>
        ///  stop order is triggered
        /// </summary>
        [JsonProperty("stopTriggered")]
        public bool? StopTriggered { get; set; }

        /// <summary>
        ///  stop price
        /// </summary>
        [JsonProperty("stopPrice")]
        public decimal? StopPrice { get; set; }

        /// <summary>
        ///  hidden order
        /// </summary>
        [JsonProperty("hidden")]
        public bool? Hidden { get; set; }

        /// <summary>
        ///  order source
        /// </summary>
        [JsonProperty("channel")]
        public string Channel { get; set; }

        /// <summary>
        ///  tag order source        
        /// </summary>
        [JsonProperty("tags")]
        public string Tags { get; set; }

        /// <summary>
        ///  status before unfilled or uncanceled
        /// </summary>
        [JsonProperty("isActive")]
        public bool? IsActive { get; set; }

        /// <summary>
        ///  order cancellation transaction record
        /// </summary>
        [JsonProperty("cancelExist")]
        public bool? CancelExist { get; set; }

        /// <summary>
        ///  create time
        /// </summary>
        [JsonProperty("createdAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CreatedAt { get; set; }

    }
}
