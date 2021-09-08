using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.Order.Margin
{
    /// <summary>
    /// Borrow Order
    /// </summary>
    public class BorrowOrder : DataObject, IStreamableObject
    {

        /// <summary>
        /// OrderId
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }


        /// <summary>
        /// Filled
        /// </summary>
        [JsonProperty("filled")]
        public decimal Filled { get; set; }


        /// <summary>
        /// MatchList
        /// </summary>
        [JsonProperty("matchList")]
        public List<BorrowMatch> MatchList { get; set; }


        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

    }

    /// <summary>
    /// Borrow Order Match Data
    /// </summary>
    public class BorrowMatch : DataObject, IStreamableObject
    {
        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Daily intereest rate
        /// </summary>
        [JsonProperty("dailyIntRate")]
        public decimal DailyIntRate { get; set; }


        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }


        /// <summary>
        /// Term (in days)
        /// </summary>
        [JsonProperty("term")]
        public long Term { get; set; }


        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }



        /// <summary>
        /// TradeId
        /// </summary>
        [JsonProperty("tradeId")]
        public long TradeId { get; set; }

    }


}
