using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
    public class RepayItem
    {


        /// <summary>
        /// TradeId
        /// </summary>
        [JsonProperty("tradeId")]
        public long TradeId { get; set; }


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// AccruedInterest
        /// </summary>
        [JsonProperty("accruedInterest")]
        public decimal AccruedInterest { get; set; }


        /// <summary>
        /// DailyIntRate
        /// </summary>
        [JsonProperty("dailyIntRate")]
        public decimal DailyIntRate { get; set; }


        /// <summary>
        /// Liability
        /// </summary>
        [JsonProperty("liability")]
        public decimal Liability { get; set; }


        /// <summary>
        /// InternalMaturityTime
        /// </summary>
        [JsonProperty("maturityTime")]
        internal long InternalMaturityTime { get; set; }

        [JsonIgnore]
        public DateTime MaturityTime => EpochTime.MillisecondsToDate(InternalMaturityTime);



        /// <summary>
        /// Principal
        /// </summary>
        [JsonProperty("principal")]
        public decimal Principal { get; set; }


        /// <summary>
        /// RepaidSize
        /// </summary>
        [JsonProperty("repaidSize")]
        public decimal RepaidSize { get; set; }


        /// <summary>
        /// Term
        /// </summary>
        [JsonProperty("term")]
        public long Term { get; set; }


        /// <summary>
        /// InternalCreatedAt
        /// </summary>
        [JsonProperty("createdAt")]
        internal long InternalCreatedAt { get; set; }

        [JsonIgnore]
        public DateTime CreatedAt => EpochTime.MillisecondsToDate(InternalCreatedAt);


    }


}
