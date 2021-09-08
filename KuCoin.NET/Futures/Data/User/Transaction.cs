using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using KuCoin.NET.Data;

namespace KuCoin.NET.Futures.Data.User
{
    /// <summary>
    /// Transaction type
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<TransactionType>))]
    public enum TransactionType
    {
        /// <summary>
        /// Realised Profit and Loss
        /// </summary>
        [EnumMember(Value = "RealisedPNL")]
        RealisedPNL,

        /// <summary>
        /// Deposit
        /// </summary>
        [EnumMember(Value = "Deposit")]
        Deposit,

        /// <summary>
        /// Withdrawal
        /// </summary>
        [EnumMember(Value = "Withdrawal")]
        Withdrawal,

        /// <summary>
        /// Transfer in from KuCoin
        /// </summary>
        [EnumMember(Value = "TransferIn")]
        TransferIn,

        /// <summary>
        /// Transfer out to KuCoin
        /// </summary>
        [EnumMember(Value = "TransferOut")]
        TransferOut

    }

    /// <summary>
    /// Transaction status
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<TransactionStatus>))]
    public enum TransactionStatus
    {
        [EnumMember(Value = "Completed")]
        Completed,
        [EnumMember(Value = "Pending")]
        Pending
    }

    public class TransactionHistory : DataObject
    {

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("dataList")]
        public List<Transaction> DataList { get; set; }
    }

    /// <summary>
    /// Represents a single transaction in the futures account.
    /// </summary>
    public class Transaction : DataObject
    {


        /// <summary>
        /// Event time
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }


        /// <summary>
        /// Type
        /// </summary>
        [JsonProperty("type")]
        public TransactionType Type { get; set; }


        /// <summary>
        /// Transaction amount
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }


        /// <summary>
        /// Fees
        /// </summary>
        [JsonProperty("fee")]
        public object Fee { get; set; }


        /// <summary>
        /// Account equity
        /// </summary>
        [JsonProperty("accountEquity")]
        public decimal AccountEquity { get; set; }


        /// <summary>
        /// Status. If you have held a position in the current 8-hour settlement period.
        /// </summary>
        [JsonProperty("status")]
        public TransactionStatus Status { get; set; }


        /// <summary>
        /// Ticker symbol of the contract
        /// </summary>
        [JsonProperty("remark")]
        public string Remark { get; set; }


        /// <summary>
        /// Offset
        /// </summary>
        [JsonProperty("offset")]
        public decimal Offset { get; set; }


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

    }

}
