using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.Order
{
    /// <summary>
    /// Order receipt that is generated when an order is created on the system.
    /// </summary>
    public class OrderReceipt : DataObject
    {
        /// <summary>
        /// The system Id of the newly-created order.
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        /// <summary>
        /// The amount that was borrowed (for margin orders)
        /// </summary>
        [JsonProperty("borrowSize")]
        public decimal? BorrowSize { get; set; }


        /// <summary>
        /// The loan application id (for borrowed funds)
        /// </summary>
        [JsonProperty("loanApplyId")]
        public string LoanApplyId { get; set; }

    }
}
