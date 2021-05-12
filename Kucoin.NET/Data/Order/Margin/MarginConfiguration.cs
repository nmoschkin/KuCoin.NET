using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
    public class MarginConfiguration
    {
        /// <summary>
        /// CurrencyList
        /// </summary>
        [JsonProperty("currencyList")]
        public List<string> CurrencyList { get; set; }


        /// <summary>
        /// WarningDebtRatio
        /// </summary>
        [JsonProperty("warningDebtRatio")]
        public decimal WarningDebtRatio { get; set; }


        /// <summary>
        /// LiqDebtRatio
        /// </summary>
        [JsonProperty("liqDebtRatio")]
        public decimal LiqDebtRatio { get; set; }


        /// <summary>
        /// MaxLeverage
        /// </summary>
        [JsonProperty("maxLeverage")]
        public long MaxLeverage { get; set; }

    }


}
