using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Websockets
{
    public class FuturesStaticMarketDepthUpdate : StaticMarketDepthUpdate
    {

        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public override DateTime Timestamp { get => base.Timestamp; set => base.Timestamp = value; }
    }

    public class ObservableFuturesStaticMarketDepthUpdate : ObservableStaticMarketDepthUpdate
    {

        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public override DateTime Timestamp { get => base.Timestamp; set => base.Timestamp = value; }
    }



}
