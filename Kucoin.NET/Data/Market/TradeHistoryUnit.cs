using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using Kucoin.NET.Json;

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
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"{Side}: {Price} ({Size}) - {Timestamp}";
        }

    }


}
