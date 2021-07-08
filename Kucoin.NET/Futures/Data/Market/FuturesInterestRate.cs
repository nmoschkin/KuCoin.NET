﻿using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Market
{

    public class InterestHistory
    {

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("dataList")]
        public List<FuturesInterestRate> DataList { get; set; }
    }


    public class FuturesInterestRate
    {
        /// <summary>
        /// Symbol of the Bitcoin Lending Rate
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// 粒Granularity (millisecond)
        /// </summary>
        [JsonProperty("granularity")]
        public long Granularity { get; set; }


        /// <summary>
        /// Time point (millisecond)
        /// </summary>
        [JsonProperty("timePoint")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime TimePoint { get; set; }


        /// <summary>
        /// Interest rate value
        /// </summary>
        [JsonProperty("value")]
        public decimal Value { get; set; }

    }

    public class FundingRate : FuturesInterestRate
    {

        [JsonProperty("predictedValue")]
        public decimal PredictedValue { get; set; }

    }
}