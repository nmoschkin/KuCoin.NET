using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Market
{

    public class TradeHistoryUnit
    {
        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("size")]
        public decimal Size { get; set; }

        [JsonProperty("side")]
        public Side Side { get; set; }

        [JsonProperty("time")]
        internal long Time { get; set; }

        [JsonIgnore]
        public DateTime Timestamp
        {
            get
            {
                return EpochTime.NanosecondsToDate(Time);
            }
        }

        public override string ToString()
        {
            return $"{Side}: {Price} ({Size}) - {Timestamp}";
        }

    }


}
