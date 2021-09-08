using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.Order.Margin
{
    /// <summary>
    /// Receipt for a borrow order returned from the REST API.
    /// </summary>
    public class BorrowReceipt
    {
        /// <summary>
        /// The newly created order Id.
        /// </summary>

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        /// <summary>
        /// The currency in which the order was placed.
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}
