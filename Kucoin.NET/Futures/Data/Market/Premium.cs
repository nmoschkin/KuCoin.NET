using Kucoin.NET.Data;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Market
{


    /// <summary>
    /// Premium index page
    /// </summary>
    public class PremiumIndex : IDataObject
    {

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("dataList")]
        public List<Premium> DataList { get; set; }
    }

    /// <summary>
    /// Premium index
    /// </summary>
    public class Premium
    {
        /// <summary>
        /// Premium index symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Granularity (milisecond)
        /// </summary>
        [JsonProperty("granularity")]
        public decimal Granularity { get; set; }


        /// <summary>
        /// Time point (milisecond)
        /// </summary>
        [JsonProperty("timePoint")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime TimePoint { get; set; }


        /// <summary>
        /// Premium index
        /// </summary>
        [JsonProperty("value")]
        public decimal Value { get; set; }

    }


}
