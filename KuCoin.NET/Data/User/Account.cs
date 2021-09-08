using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KuCoin.NET.Localization;
using KuCoin.NET.Observable;

using Newtonsoft.Json;

namespace KuCoin.NET.Data.User
{
    /// <summary>
    /// User account information
    /// </summary>
    public class Account
    {
        public override string ToString()
        {
            return $"{Id} ({Type}): {Currency}: {Balance:#,##0.00#######}";
        }

        /// <summary>
        /// Account Id
        /// </summary>
        [JsonProperty("id")]
        [KeyProperty]
        public string Id { get; set; }

        /// <summary>
        /// The currency held in the account
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// The account type
        /// </summary>
        [JsonProperty("type")]
        public AccountType Type { get; set; }

        /// <summary>
        /// Total balance of funds in the account
        /// </summary>
        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        /// <summary>
        /// Funds available for trade or transfer
        /// </summary>
        [JsonProperty("available")]
        public decimal Available { get; set; }

        /// <summary>
        /// Funds held
        /// </summary>
        [JsonProperty("holds")]
        public decimal Holds { get; set; }

        /// <summary>
        /// Get the account type description
        /// </summary>
        [JsonIgnore]
        public string TypeDescription => AutoLocalizer.Localize(Type, this, nameof(Type));

    }
}
