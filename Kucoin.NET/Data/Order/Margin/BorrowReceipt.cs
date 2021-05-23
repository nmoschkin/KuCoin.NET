using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
    public class BorrowReceipt
    {

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}
