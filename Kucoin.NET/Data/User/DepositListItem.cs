using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kucoin.NET.Data.User
{
    [JsonConverter(typeof(EnumToStringConverter<DepositStatus>))]
    public enum DepositStatus
    {
        [EnumMember(Value = "SUCCESS")]
        Success = 0,

        [EnumMember(Value = "FAILURE")]
        Failure = 1,

        [EnumMember(Value = "PROCESSING")]
        Processing = 2
    }

    public enum WithdrawalStatus 
    {

        [EnumMember(Value = "SUCCESS")]
        Success = 0,

        [EnumMember(Value = "FAILURE")]
        Failure = 1,

        [EnumMember(Value = "PROCESSING")]
        Processing = 2,

        [EnumMember(Value = "ACCOUNT_PROCESSING")]
        AccountProcessing = 3
    }


    public class DepositListItem
    {
        /// <summary>
        /// Address
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }


        /// <summary>
        /// Memo
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
        /// IsInner
        /// </summary>
        [JsonProperty("isInner")]
        public bool IsInner { get; set; }


        /// <summary>
        /// WalletTxId
        /// </summary>
        [JsonProperty("walletTxId")]
        public string WalletTxId { get; set; }


        /// <summary>
        /// Status
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
