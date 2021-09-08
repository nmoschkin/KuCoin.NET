using KuCoin.NET.Data;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Trade
{

    public class FundingHistory : DataObject
    {

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("dataList")]
        public List<FundingDetails> DataList { get; set; }
    }

    /// <summary>
    /// Funding Details
    /// </summary>
    public class FundingDetails : DataObject
    {


        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("id")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Id { get; set; }


        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Time point (millisecond)
        /// </summary>
        [JsonProperty("timePoint")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime TimePoint { get; set; }


        /// <summary>
        /// Funding rate
        /// </summary>
        [JsonProperty("fundingRate")]
        public decimal FundingRate { get; set; }


        /// <summary>
        /// Mark price
        /// </summary>
        [JsonProperty("markPrice")]
        public decimal MarkPrice { get; set; }


        /// <summary>
        /// Position size
        /// </summary>
        [JsonProperty("positionQty")]
        public decimal PositionQty { get; set; }


        /// <summary>
        /// Position value at settlement period
        /// </summary>
        [JsonProperty("positionCost")]
        public decimal PositionCost { get; set; }


        /// <summary>
        /// Settled funding fees. A positive number means that the user received the funding fee, and vice versa.
        /// </summary>
        [JsonProperty("funding")]
        public decimal Funding { get; set; }


        /// <summary>
        /// Settlement currency
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleCurrency { get; set; }

    }

}
