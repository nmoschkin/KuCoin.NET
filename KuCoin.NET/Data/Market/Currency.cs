using System;
using System.Collections.Generic;
using System.Text;

using KuCoin.NET.Json;
using KuCoin.NET.Observable;

using Newtonsoft.Json;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// Crypto-market currency information.
    /// </summary>
    public class MarketCurrency : DataObject
    {

        /// <summary>
        /// A unique currency code that will never change
        /// </summary>
        [JsonProperty("currency")]
        [KeyProperty]
        public string Currency { get; set; }

        /// <summary>
        /// Currency name, will change after renaming
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Full name of a currency, will change after renaming
        /// </summary>
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        /// <summary>
        /// Currency precision
        /// </summary>
        [JsonProperty("precision")]
        public int Precision { get; set; }

        /// <summary>
        /// Minimum withdrawal amount
        /// </summary>
        [JsonProperty("withdrawalMinSize")]
        public decimal WithdrawalMinSize { get; set; }

        /// <summary>
        /// Minimum fees charged for withdrawal
        /// </summary>
        [JsonProperty("withdrawalMinFee")]
        public decimal WithdrawalMinFee { get; set; }

        /// <summary>
        /// Support withdrawal or not
        /// </summary>
        [JsonProperty("isWithdrawEnabled")]
        public bool IsWithdrawEnabled { get; set; }

        /// <summary>
        /// Support deposit or not
        /// </summary>
        [JsonProperty("isDepositEnabled")]
        public bool IsDepositEnabled { get; set; }

        /// <summary>
        /// Support margin or not
        /// </summary>
        [JsonProperty("isMarginEnabled")]
        public bool IsMarginEnabled { get; set; }

        /// <summary>
        /// Support debit or not
        /// </summary>
        [JsonProperty("isDebitEnabled")]
        public bool IsDebitEnabled { get; set; }

        /// <summary>
        /// Number of block confirmations
        /// </summary>
        [JsonProperty("confirms")]
        public int? Confirms { get; set; }

        public override string ToString()
        {
            
            if (FullName != null && Currency != null)
            {
                return $"{Currency}: {FullName}";
            }
            return FullName ?? Currency ?? base.ToString();
        }

    }
}
