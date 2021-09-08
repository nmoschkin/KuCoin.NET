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
    /// Index data list page
    /// </summary>
    public class IndexList : DataObject
    {

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("dataList")]
        public List<IndexData> DataList { get; set; }
    }

    /// <summary>
    /// Index decomposition
    /// </summary>
    public class IndexDecomposition : DataObject
    {


        /// <summary>
        /// Exchange
        /// </summary>
        [JsonProperty("exchange")]
        public string Exchange { get; set; }


        /// <summary>
        /// Last traded price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }


        /// <summary>
        /// Weight
        /// </summary>
        [JsonProperty("weight")]
        public decimal Weight { get; set; }

    }

    /// <summary>
    /// Index data
    /// </summary>
    public class IndexData : DataObject
    {


        /// <summary>
        /// Symbol of Bitcoin spot
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Granularity (millisecond)
        /// </summary>
        [JsonProperty("granularity")]
        public decimal Granularity { get; set; }


        /// <summary>
        /// Time point (millisecond)
        /// </summary>
        [JsonProperty("timePoint")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime TimePoint { get; set; }


        /// <summary>
        /// Index Value
        /// </summary>
        [JsonProperty("value")]
        public decimal Value { get; set; }


        /// <summary>
        /// Component List
        /// </summary>
        [JsonProperty("decomposionList")]
        public List<IndexDecomposition> DecomposionList { get; set; }

    }
}
