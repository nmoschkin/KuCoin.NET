using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.Order
{
    /// <summary>
    /// Stop order information
    /// </summary>
    public class StopOrderDetails : LimitStopOrder
    {
        /// <summary>
        /// Order ID, the ID of an order.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// order funds
        /// </summary>
        [JsonProperty("funds")]
        public decimal Funds { get; set; }

        /// <summary>
        /// self trade prevention
        /// </summary>
        [JsonProperty("stp")]
        public StpMode Stp { get; set; }

        /// <summary>
        /// order source
        /// </summary>
        [JsonProperty("channel")]
        public string Channel { get; set; }

        /// <summary>
        /// tag order source
        /// </summary>
        [JsonProperty("tags")]
        public string Tags { get; set; }

        /// <summary>
        /// The currency of the fee
        /// </summary>
        [JsonProperty("feeCurrency")]
        public string FeeCurrency { get; set; }

        /// <summary>
        /// order creation time
        /// </summary>
        [JsonProperty("createdAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CreatedAt { get; set; }



    }
}
