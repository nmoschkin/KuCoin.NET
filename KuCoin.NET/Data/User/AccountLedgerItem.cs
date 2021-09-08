using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using KuCoin.NET.Helpers;
using KuCoin.NET.Json;
using KuCoin.NET.Rest;

using Newtonsoft.Json;

namespace KuCoin.NET.Data.User
{
    /// <summary>
    /// Business core parameters
    /// </summary>
    public class LedgerContext
    {
        /// <summary>
        /// Order Id
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// Trade Id
        /// </summary>
        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        /// <summary>
        /// Transaction Id
        /// </summary>
        [JsonProperty("txId")]
        public string TransactionId { get; set; }

        public LedgerContext()
        {
        }
    }

    /// <summary>
    /// Transaction direction
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<TransactionDirection>))]
    public enum TransactionDirection
    {
        /// <summary>
        /// Deposit / Receive
        /// </summary>
        [EnumMember(Value = "in")]
        In,

        /// <summary>
        /// Withdrawal / Send
        /// </summary>
        [EnumMember(Value = "out")]
        Out
    }

    /// <summary>
    /// Account ledger item (piece of account business)
    /// </summary>
    public class AccountLedgerItem : DataObject
    {
        /// <summary>
        /// The item Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Change amount of the funds
        /// </summary>
        [JsonProperty("amount")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Deposit or withdrawal fee
        /// </summary>
        [JsonProperty("fee")]
        public decimal? Fee { get; set; }

        /// <summary>
        /// Total assets of a currency
        /// </summary>
        [JsonProperty("balance")]
        public decimal? Balance { get; set; }

        /// <summary>
        /// Business type
        /// </summary>
        [JsonProperty("bizType")]
        public BizType? BizType { get; set; }

        /// <summary>
        /// side, in or out
        /// </summary>
        [JsonProperty("direction")]
        public TransactionDirection Direction { get; set; }

        /// <summary>
        /// Creation time
        /// </summary>
        [JsonProperty("createdAt")]
        public long? CreatedAt { get; set; }

        /// <summary>
        /// Business core parameters
        /// </summary>
        [JsonProperty("context")]
        [JsonConverter(typeof(StringToJsonObjectConverter<LedgerContext>))]
        public LedgerContext Context { get; set; }


        /// <summary>
        /// Time stamp of the business
        /// </summary>
        [JsonIgnore]
        public DateTime Timestamp
        {
            get
            {
                if (CreatedAt == null)
                {
                    return DateTime.Now;
                }
                else
                {
                    return EpochTime.MillisecondsToDate((long)CreatedAt);
                }
            }
        }


    }
}
