using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Websockets
{
    /// <summary>
    /// Funding Point Event Types
    /// </summary>
    public enum FundingPointType
    {
        /// <summary>
        /// Funding Cycle Begin
        /// </summary>
        Begin,

        /// <summary>
        /// Funding Cycle End
        /// </summary>
        End
    }

    /// <summary>
    /// Funding Point (Begin or End)
    /// </summary>
    public class FundingPoint : DataObject, ISymbol, IStreamableObject
    {


        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        [JsonIgnore]
        public FundingPointType Type { get; set; }

        /// <summary>
        /// Funding time
        /// </summary>
        [JsonProperty("fundingTime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime FundingTime { get; set; }


        /// <summary>
        /// Funding rate
        /// </summary>
        [JsonProperty("fundingRate")]
        public decimal FundingRate { get; set; }


        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }

    }

    /// <summary>
    /// Contract data type
    /// </summary>
    public enum ContractDataType
    {
        /// <summary>
        /// Mark Index Price
        /// </summary>
        MarkIndexPrice,

        /// <summary>
        /// Funding Rate
        /// </summary>
        FundingRate
    }

    /// <summary>
    /// Contract Market Data
    /// </summary>
    public class ContractMarketData : DataObject, ISymbol, IStreamableObject
    {
        /// <summary>
        /// The symbol for the data being reported.
        /// </summary>
        [JsonIgnore]
        public string Symbol { get; set; }

        /// <summary>
        /// The type of data being reported.
        /// </summary>
        [JsonIgnore]
        public ContractDataType Type { get; set; }

        /// <summary>
        /// Granularity
        /// </summary>
        [JsonProperty("granularity")]
        public decimal Granularity { get; set; }


        /// <summary>
        /// Index price
        /// </summary>
        [JsonProperty("indexPrice")]
        public decimal? IndexPrice { get; set; }


        /// <summary>
        /// Mark price
        /// </summary>
        [JsonProperty("markPrice")]
        public decimal? MarkPrice { get; set; }


        /// <summary>
        /// Funding rate
        /// </summary>
        [JsonProperty("fundingRate")]
        public decimal? FundingRate { get; set; }


        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }

    }



}
