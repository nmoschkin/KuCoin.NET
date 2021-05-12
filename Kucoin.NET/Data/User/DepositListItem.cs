using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.User
{
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
        public string Status { get; set; }


        /// <summary>
        /// Remark
        /// </summary>
        [JsonProperty("remark")]
        public string Remark { get; set; }


        /// <summary>
        /// InternalCreatedAt
        /// </summary>
        [JsonProperty("createdAt")]
        internal long InternalCreatedAt { get; set; }

        [JsonIgnore]
        public DateTime CreatedAt => EpochTime.MillisecondsToDate(InternalCreatedAt);



        /// <summary>
        /// InternalUpdatedAt
        /// </summary>
        [JsonProperty("updatedAt")]
        internal long InternalUpdatedAt { get; set; }

        [JsonIgnore]
        public DateTime UpdatedAt => EpochTime.MillisecondsToDate(InternalUpdatedAt);


    }


}
