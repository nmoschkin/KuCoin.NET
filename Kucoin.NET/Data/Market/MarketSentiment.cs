using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Kucoin.NET.Json;

namespace Kucoin.NET.Data.Market
{

    [JsonConverter(typeof(EnumToStringConverter<MarketSentiment>))]
    public enum MarketSentiment
    {
        [EnumMember(Value = "up")]
        Bullish,

        [EnumMember(Value = "down")]
        Bearish
    }

}
