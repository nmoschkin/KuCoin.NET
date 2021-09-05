using Kucoin.NET.Data;
using Kucoin.NET.Data.User;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kucoin.NET.Futures.Data.User
{
    /// <summary>
    /// Deposit
    /// </summary>
    public class Deposit : DataObject
    {
        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Status type: PROCESSING, WALLET_PROCESSING, SUCCESS, FAILURE
        /// </summary>
        [JsonProperty("status")]
        public DepositStatus Status { get; set; }


        /// <summary>
        /// Deposit address
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }


        /// <summary>
        /// Inner transfer or not
        /// </summary>
        [JsonProperty("isInner")]
        public bool IsInner { get; set; }


        /// <summary>
        /// Deposit amount
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }


        /// <summary>
        /// Fees for deposit
        /// </summary>
        [JsonProperty("fee")]
        public decimal Fee { get; set; }


        /// <summary>
        /// Wallet TXID
        /// </summary>
        [JsonProperty("walletTxId")]
        public string WalletTxId { get; set; }


        /// <summary>
        /// Funds deposit time
        /// </summary>
        [JsonProperty("createdAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CreatedAt { get; set; }

    }


}
