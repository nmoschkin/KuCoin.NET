using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.Order.Margin
{
    /// <summary>
    /// Margin configuration information
    /// </summary>
    public class MarginConfiguration
    {
        /// <summary>
        /// List of currencies
        /// </summary>
        [JsonProperty("currencyList")]
        public List<string> CurrencyList { get; set; }


        /// <summary>
        /// Warning debt ratio
        /// </summary>
        [JsonProperty("warningDebtRatio")]
        public decimal WarningDebtRatio { get; set; }


        /// <summary>
        /// Liquidation debt ratio
        /// </summary>
        [JsonProperty("liqDebtRatio")]
        public decimal LiqDebtRatio { get; set; }


        /// <summary>
        /// Max leverage
        /// </summary>
        [JsonProperty("maxLeverage")]
        public long MaxLeverage { get; set; }

    }


}
