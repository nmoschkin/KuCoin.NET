﻿using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Market
{
    public class MarkPrice
    {


        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Granularity (milisecond)
        /// </summary>
        [JsonProperty("granularity")]
        public long Granularity { get; set; }


        /// <summary>
        /// Time point (milisecond)
        /// </summary>
        [JsonProperty("timePoint")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime TimePoint { get; set; }


        /// <summary>
        /// Mark price
        /// </summary>
        [JsonProperty("value")]
        public decimal Value { get; set; }


        /// <summary>
        /// Index price
        /// </summary>
        [JsonProperty("indexPrice")]
        public decimal IndexPrice { get; set; }

    }


}