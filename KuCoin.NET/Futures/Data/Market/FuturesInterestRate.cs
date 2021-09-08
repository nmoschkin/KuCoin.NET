using KuCoin.NET.Data;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Market
{
    /// <summary>
    /// Futures interest rate history page results
    /// </summary>
    public class InterestHistory : DataObject
    {

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("dataList")]
        public List<FuturesInterestRate> DataList { get; set; }
    }


    /// <summary>
    /// Futures interest rate information
    /// </summary>
    public class FuturesInterestRate : DataObject
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
