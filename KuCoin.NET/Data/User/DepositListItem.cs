using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KuCoin.NET.Data.User
{
    /// <summary>
    /// Deposit status
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<DepositStatus>))]
    public enum DepositStatus
    {
        /// <summary>
        /// Success
        /// </summary>
        [EnumMember(Value = "SUCCESS")]
        Success = 0,

        /// <summary>
        /// Failure
        /// </summary>
        [EnumMember(Value = "FAILURE")]
        Failure = 1,

        /// <summary>
        /// Processing
        /// </summary>
        [EnumMember(Value = "PROCESSING")]
        Processing = 2,

        /// <summary>
        /// Wallet Processing
        /// </summary>
        [EnumMember(Value = "WALLET_PROCESSING")]
        WalletProcessing = 3


    }

    /// <summary>
    /// Withdrawal status
    /// </summary>
    public enum WithdrawalStatus 
    {
        /// <summary>
        /// Success
        /// </summary>

        [EnumMember(Value = "SUCCESS")]
        Success = 0,

        /// <summary>
        /// Failure
        /// </summary>
        [EnumMember(Value = "FAILURE")]
        Failure = 1,

        /// <summary>
        /// Processing
        /// </summary>
        [EnumMember(Value = "PROCESSING")]
        Processing = 2,

        /// <summary>
        /// Account processing
        /// </summary>
        [EnumMember(Value = "ACCOUNT_PROCESSING")]
        AccountProcessing = 3
    }

    /// <summary>
    /// Deposit item
    /// </summary>
    public class DepositListItem : DataObject
    {
        /// <summary>
        /// Wallet Address
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }


        /// <summary>
        /// Wallet Address Memo (for currencies such as XLM)
        /// </summary>
        [JsonProperty("memo")]
        public string Memo { get; set; }


        /// <summary>
        /// Amount
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }


        /// <summary>
        /// Fee
        /// </summary>
        [JsonProperty("fee")]
        public decimal Fee { get; set; }


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Is inner transfer
        /// </summary>
        [JsonProperty("isInner")]
        public bool IsInner { get; set; }


        /// <summary>
        /// Wallet transaction Id
        /// </summary>
        [JsonProperty("walletTxId")]
        public string WalletTxId { get; set; }


        /// <summary>
        /// Deposit Status
        /// </summary>
        [JsonProperty("status")]
        public DepositStatus Status { get; set; }


        /// <summary>
        /// Remark
        /// </summary>
        [JsonProperty("remark")]
        public string Remark { get; set; }


        /// <summary>
        /// Created At
        /// </summary>
        [JsonProperty("createdAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CreatedAt { get; set; }



        /// <summary>
        /// Updated At
        /// </summary>
        [JsonProperty("updatedAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime UpdatedAt { get; set; }


    }


}
