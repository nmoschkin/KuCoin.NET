using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
    public class MarginAccountSummary
    {


        /// <summary>
        /// Accounts
        /// </summary>
        [JsonProperty("accounts")]
        public List<MarginAccount> Accounts { get; set; }


        /// <summary>
        /// DebtRatio
        /// </summary>
        [JsonProperty("debtRatio")]
        public decimal DebtRatio { get; set; }

    }

    public class MarginAccount
    {


        /// <summary>
        /// AvailableBalance
        /// </summary>
        [JsonProperty("availableBalance")]
        public decimal AvailableBalance { get; set; }


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// HoldBalance
        /// </summary>
        [JsonProperty("holdBalance")]
        public decimal HoldBalance { get; set; }


        /// <summary>
        /// Liability
        /// </summary>
        [JsonProperty("liability")]
        public decimal Liability { get; set; }


        /// <summary>
        /// MaxBorrowSize
        /// </summary>
        [JsonProperty("maxBorrowSize")]
        public decimal MaxBorrowSize { get; set; }


        /// <summary>
        /// TotalBalance
        /// </summary>
        [JsonProperty("totalBalance")]
        public decimal TotalBalance { get; set; }

    }

}
