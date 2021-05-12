using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Data.Order
{
    /// <summary>
    /// Order receipt that is generated when an order is created on the system.
    /// </summary>
    public class OrderReceipt
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
