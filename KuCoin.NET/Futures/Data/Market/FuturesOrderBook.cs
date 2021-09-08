using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Market
{
    /// <summary>
    /// Standard Futures Level 2 order book implementation
    /// </summary>
    public class FuturesOrderBook : ObservableOrderBook<ObservableOrderUnit>
    {

        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public override DateTime Timestamp { get => base.Timestamp; set => base.Timestamp = value; }
    }
}
