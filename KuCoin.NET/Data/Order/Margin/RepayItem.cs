using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KuCoin.NET.Data.Order.Margin
{
    /// <summary>
    /// Repay sequence types for auto-repay
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<RepaySequence>))]
    public enum RepaySequence
    {
        /// <summary>
        /// Repay soonest to expire first
        /// </summary>
        [EnumMember(Value = "RECENTLY_EXPIRE_FIRST")]
        RecentlyExpiredFirst,

        /// <summary>
        /// Repay highest interest rate first
        /// </summary>
        [EnumMember(Value = "HIGHEST_RATE_FIRST")]
        HighestRateFirst
    }

    public class RepayItem : DataObject
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
        /// Maturity Time
        /// </summary>
        [JsonProperty("maturityTime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime MaturityTime { get; set; }



        /// <summary>
        /// Principal
        /// </summary>
        [JsonProperty("principal")]
        public decimal Principal { get; set; }


        /// <summary>
        /// Size repaid
        /// </summary>
        [JsonProperty("repaidSize")]
        public decimal RepaidSize { get; set; }


        /// <summary>
        /// Term (in days)
        /// </summary>
        [JsonProperty("term")]
        public long Term { get; set; }


        /// <summary>
        /// Created at
        /// </summary>
        [JsonProperty("createdAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CreatedAt { get; set; }


    }


}
