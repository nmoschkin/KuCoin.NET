using KuCoin.NET.Data;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Websockets
{
    /// <summary>
    /// Funding Settlement Data
    /// </summary>
    public class FundingSettlement : DataObject, IStreamableObject
    {


        /// <summary>
        /// Funding time
        /// </summary>
        [JsonProperty("fundingTime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime FundingTime { get; set; }


        /// <summary>
        /// Position size
        /// </summary>
        [JsonProperty("qty")]
        public decimal Qty { get; set; }


        /// <summary>
        /// Settlement price
        /// </summary>
        [JsonProperty("markPrice")]
        public decimal MarkPrice { get; set; }


        /// <summary>
        /// Funding rate
        /// </summary>
        [JsonProperty("fundingRate")]
        public decimal FundingRate { get; set; }


        /// <summary>
        /// Funding fees
        /// </summary>
        [JsonProperty("fundingFee")]
        public decimal FundingFee { get; set; }


        /// <summary>
        /// Current time (nanosecond)
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }


        /// <summary>
        /// Currency used to clear and settle the trades
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleCurrency { get; set; }

    }

}
