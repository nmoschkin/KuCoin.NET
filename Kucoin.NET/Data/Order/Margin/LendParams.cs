using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
    [JsonConverter(typeof(EnumToStringConverter<LendStatus>))]
    public enum LendStatus
    {
        [EnumMember(Value = "FILLED")]
        Filled,
        [EnumMember(Value = "CANCELED")]
        Canceled
    }


    public class LendBase : JsonDictBase
    {
        /// <summary>
        /// Currency to lend
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Daily interest rate. e.g. 0.002 is 0.2%
        /// </summary>
        [JsonProperty("dailyIntRate")]
        public decimal? DailyInterestRate { get; set; }

        /// <summary>
        /// Term (Unit: Day)
        /// </summary>
        [JsonProperty("term")]
        public int? Term { get; set; }

    }
    
    public class LendParams : LendBase
    {

        /// <summary>
        /// Total size
        /// </summary>
        [JsonProperty("size")]
        public decimal? Size { get; set; }

    }

    public class AutoLendParams : LendBase
    {
        [JsonProperty("isEnable")]
        public bool? IsEnabled { get; set; }

        [JsonProperty("retainSize")]
        public decimal? RetainSize { get; set; }
    }

    public class LendOrder : LendParams
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("filledSize")]
        public decimal? FilledSize { get; set; }
        
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }
    }

    public class HistoricalLendOrder : LendOrder
    {
        [JsonProperty("status")]
        public LendStatus? Status { get; set; }

    }

    public class UnsettledLendOrder : LendBase
    {

        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        [JsonProperty("size")]
        public decimal? Size { get; set; }

        [JsonProperty("repaid")]
        public decimal? Repaid { get; set; }

        [JsonProperty("accruedInterest")]
        public decimal? AccruedInterest { get; set; }

        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        [JsonProperty("maturityTime")]
        public DateTime MaturityTime { get; set; }
    }


    public class SettledLendOrder : LendBase
    {

        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        [JsonProperty("size")]
        public decimal? Size { get; set; }

        [JsonProperty("repaid")]
        public decimal? Repaid { get; set; }

        [JsonProperty("interest")]
        public decimal? Interest { get; set; }

        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        [JsonProperty("settledAt")]
        public DateTime SettledAt { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }
    }

    public class AccountLendRecordEntry
    {

        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Outstanding
        /// </summary>
        [JsonProperty("outstanding")]
        public decimal Outstanding { get; set; }


        /// <summary>
        /// FilledSize
        /// </summary>
        [JsonProperty("filledSize")]
        public decimal FilledSize { get; set; }


        /// <summary>
        /// AccruedInterest
        /// </summary>
        [JsonProperty("accruedInterest")]
        public decimal AccruedInterest { get; set; }


        /// <summary>
        /// RealizedProfit
        /// </summary>
        [JsonProperty("realizedProfit")]
        public decimal RealizedProfit { get; set; }


        /// <summary>
        /// IsAutoLend
        /// </summary>
        [JsonProperty("isAutoLend")]
        public bool IsAutoLend { get; set; }

    }


}
