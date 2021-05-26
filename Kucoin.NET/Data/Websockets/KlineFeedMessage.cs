using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Kucoin.NET.Observable;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;
using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Json;

namespace Kucoin.NET.Data.Websockets
{
    public class KlineFeedMessage<T> where T: IWriteableCandle, new()
    {
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }

        [JsonProperty("candles")]
        public T Candles { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }


    }
}
