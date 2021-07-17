using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Websockets
{
    /// <summary>
    /// Order book change 
    /// </summary>
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
        /// Daily interest rate
        /// </summary>
        [JsonProperty("dailyIntRate")]
        public decimal DailyIntRate { get; set; }

        /// <summary>
        /// Annual interest rate
        /// </summary>
        [JsonProperty("annualIntRate")]
        public decimal AnnualIntRate { get; set; }

        /// <summary>
        /// Term (in days)
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
        /// Time Stamp
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }


    }


}
