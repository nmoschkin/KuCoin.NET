using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Websockets
{
    public enum FundingPointType
    {
        Begin,
        End
    }

    public class FundingPoint : ISymbol
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

    public enum ContractDataType
    {
        MarkIndexPrice,
        FundingRate
    }

    public class ContractMarketData : ISymbol
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
