using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Rest;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.User
{

    public class LedgerContext
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        [JsonProperty("txId")]
        public string TransactionId { get; set; }

        public LedgerContext()
        {

        }
    }

    [JsonConverter(typeof(EnumToStringConverter<TransactionDirection>))]
    public enum TransactionDirection
    {
        [EnumMember(Value = "in")]
        In,

        [EnumMember(Value = "out")]
        Out
    }

    public class AccountLedgerItem
    {
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
        /// business type
        /// </summary>
        [JsonProperty("bizType")]
        public string BizType { get; set; }

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
