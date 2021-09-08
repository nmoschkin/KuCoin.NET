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
    /// Lend order status
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<LendStatus>))]
    public enum LendStatus
    {
        /// <summary>
        /// Filled
        /// </summary>
        [EnumMember(Value = "FILLED")]
        Filled,

        /// <summary>
        /// Canceled
        /// </summary>
        [EnumMember(Value = "CANCELED")]
        Canceled
    }

    /// <summary>
    /// Lending object base class
    /// </summary>
    public class LendBase : DataObject, IDataObject
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
    
    /// <summary>
    /// Lending order request parameters
    /// </summary>
    public class LendParams : LendBase
    {

        /// <summary>
        /// Total size
        /// </summary>
        [JsonProperty("size")]
        public decimal? Size { get; set; }

    }

    /// <summary>
    /// Auto lending request parameters
    /// </summary>
    public class AutoLendParams : LendBase
    {
        [JsonProperty("isEnable")]
        public bool? IsEnabled { get; set; }

        [JsonProperty("retainSize")]
        public decimal? RetainSize { get; set; }
    }

    /// <summary>
    /// Lend order information
    /// </summary>
    public class LendOrder : LendParams
    {
        /// <summary>
        /// Order Id
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        /// <summary>
        /// The filled size (if any)
        /// </summary>
        [JsonProperty("filledSize")]
        public decimal? FilledSize { get; set; }
        
        /// <summary>
        /// The created time stamp
        /// </summary>
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>
    /// Historical lend order information
    /// </summary>
    public class HistoricalLendOrder : LendOrder
    {
        /// <summary>
        /// Status of the order (if any)
        /// </summary>
        [JsonProperty("status")]
        public LendStatus? Status { get; set; }

    }

    /// <summary>
    /// Information about unsettled lend orders
    /// </summary>
    public class UnsettledLendOrder : LendBase
    {
        /// <summary>
        /// The trade Id
        /// </summary>

        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        /// <summary>
        /// The size of the order
        /// </summary>
        [JsonProperty("size")]
        public decimal? Size { get; set; }

        /// <summary>
        /// The amount repaid
        /// </summary>
        [JsonProperty("repaid")]
        public decimal? Repaid { get; set; }

        /// <summary>
        /// The accrued interest
        /// </summary>
        [JsonProperty("accruedInterest")]
        public decimal? AccruedInterest { get; set; }

        /// <summary>
        /// Maturity time
        /// </summary>
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        [JsonProperty("maturityTime")]
        public DateTime MaturityTime { get; set; }
    }


    /// <summary>
    /// Information about settled lend orders
    /// </summary>
    public class SettledLendOrder : LendBase
    {
        /// <summary>
        /// The trade Id
        /// </summary>

        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        /// <summary>
        /// The size of the order
        /// </summary>
        [JsonProperty("size")]
        public decimal? Size { get; set; }

        /// <summary>
        /// The amount repaid
        /// </summary>
        [JsonProperty("repaid")]
        public decimal? Repaid { get; set; }

        /// <summary>
        /// The interest paid
        /// </summary>
        [JsonProperty("interest")]
        public decimal? Interest { get; set; }

        /// <summary>
        /// The settled at time stamp
        /// </summary>
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        [JsonProperty("settledAt")]
        public DateTime SettledAt { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        [JsonProperty("note")]
        public string Note { get; set; }
    }

    /// <summary>
    /// Account lend record item
    /// </summary>
    public class AccountLendRecordItem : DataObject
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
