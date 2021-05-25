﻿using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
    public class LendingMarketData
    {

        /// <summary>
        /// Daily Interest Rate
        /// </summary>
        [JsonProperty("dailyIntRate")]
        public decimal DailyInterestRate { get; set; }

        /// <summary>
        /// Term (in Days)
        /// </summary>
        [JsonProperty("term")]
        public long Term { get; set; }


        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }

    }


}
