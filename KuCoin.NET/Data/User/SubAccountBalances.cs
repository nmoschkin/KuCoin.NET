using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.User
{
    /// <summary>
    /// Sub-account balance information
    /// </summary>
    public class SubAccountBalances
    {


        /// <summary>
        /// SubUserId
        /// </summary>
        [JsonProperty("subUserId")]
        public string SubUserId { get; set; }


        /// <summary>
        /// SubName
        /// </summary>
        [JsonProperty("subName")]
        public string SubName { get; set; }


        /// <summary>
        /// Main accounts
        /// </summary>
        [JsonProperty("mainAccounts")]
        public List<SubAccountBalance> MainAccounts { get; set; }


        /// <summary>
        /// Trading accounts
        /// </summary>
        [JsonProperty("tradeAccounts")]
        public List<SubAccountBalance> TradeAccounts { get; set; }


        /// <summary>
        /// Margin accounts
        /// </summary>
        [JsonProperty("marginAccounts")]
        public List<SubAccountBalance> MarginAccounts { get; set; }

    }

    /// <summary>
    /// Individual sub-user account balance information
    /// </summary>
    public class SubAccountBalance
    {
        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Balance
        /// </summary>
        [JsonProperty("balance")]
        public decimal Balance { get; set; }


        /// <summary>
        /// Available
        /// </summary>
        [JsonProperty("available")]
        public decimal Available { get; set; }


        /// <summary>
        /// Holds
        /// </summary>
        [JsonProperty("holds")]
        public decimal Holds { get; set; }


        /// <summary>
        /// BaseCurrency
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseCurrency { get; set; }


        /// <summary>
        /// BaseCurrencyPrice
        /// </summary>
        [JsonProperty("baseCurrencyPrice")]
        public decimal BaseCurrencyPrice { get; set; }


        /// <summary>
        /// BaseAmount
        /// </summary>
        [JsonProperty("baseAmount")]
        public decimal BaseAmount { get; set; }

    }

}
