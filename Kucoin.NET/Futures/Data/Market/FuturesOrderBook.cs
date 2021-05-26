using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Market
{
    public class FuturesOrderBook<T> : OrderBook<T> where T: IOrderUnit
    {


        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public override DateTime Timestamp { get => base.Timestamp; set => base.Timestamp = value; }
    }
}
