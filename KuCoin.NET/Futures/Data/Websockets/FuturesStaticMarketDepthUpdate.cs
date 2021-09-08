using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Websockets
{
    /// <summary>
    /// Static market depth update for KuCoin Futures Level 2 static depth market feeds
    /// </summary>
    public class FuturesStaticMarketDepthUpdate : StaticMarketDepthUpdate
    {

        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public override DateTime Timestamp { get => base.Timestamp; set => base.Timestamp = value; }
    }

    /// <summary>
    /// Observable static market depth update for KuCoin Futures Level 2 static depth market feeds
    /// </summary>
    public class ObservableFuturesStaticMarketDepthUpdate : ObservableStaticMarketDepthUpdate
    {

        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public override DateTime Timestamp { get => base.Timestamp; set => base.Timestamp = value; }
    }



}
