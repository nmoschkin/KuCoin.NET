using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Websockets
{
    public class OrderBookChange
    {

        /// <summary>
        /// Sequence
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// DailyIntRate
        /// </summary>
        [JsonProperty("dailyIntRate")]
        public decimal DailyIntRate { get; set; }

        /// <summary>
        /// AnnualIntRate
        /// </summary>
        [JsonProperty("annualIntRate")]
        public decimal AnnualIntRate { get; set; }

        /// <summary>
        /// Term
        /// </summary>
        [JsonProperty("term")]
        public long Term { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }

        /// <summary>
        /// Side
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }

        /// <summary>
        /// InternalTs
        /// </summary>
        [JsonProperty("ts")]
        internal long InternalTs { get; set; }

        [JsonIgnore]
        public DateTime Ts => EpochTime.NanosecondsToDate(InternalTs);


    }


}
