using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order
{
    public class FeeRate
    {


        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// TakerFeeRate
        /// </summary>
        [JsonProperty("takerFeeRate")]
        public decimal TakerFeeRate { get; set; }


        /// <summary>
        /// MakerFeeRate
        /// </summary>
        [JsonProperty("makerFeeRate")]
        public decimal MakerFeeRate { get; set; }

    }


}
