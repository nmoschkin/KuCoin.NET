using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.Order.Margin
{
    /// <summary>
    /// Margin accounts summary
    /// </summary>
    public class MarginAccountSummary
    {
        /// <summary>
        /// List of margin accounts
        /// </summary>
        [JsonProperty("accounts")]
        public List<MarginAccount> Accounts { get; set; }


        /// <summary>
        /// Current debt ratio
        /// </summary>
        [JsonProperty("debtRatio")]
        public decimal DebtRatio { get; set; }

    }

    /// <summary>
    /// Margin account information
    /// </summary>
    public class MarginAccount
    {


        /// <summary>
        /// Available balance
        /// </summary>
        [JsonProperty("availableBalance")]
        public decimal AvailableBalance { get; set; }


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Held balance
        /// </summary>
        [JsonProperty("holdBalance")]
        public decimal HoldBalance { get; set; }


        /// <summary>
        /// Current liability
        /// </summary>
        [JsonProperty("liability")]
        public decimal Liability { get; set; }


        /// <summary>
        /// Maximum borrow size
        /// </summary>
        [JsonProperty("maxBorrowSize")]
        public decimal MaxBorrowSize { get; set; }


        /// <summary>
        /// Total balance
        /// </summary>
        [JsonProperty("totalBalance")]
        public decimal TotalBalance { get; set; }

    }

}
