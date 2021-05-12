using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Kucoin.NET.Json;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Trading side
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<Side>))]
    public enum Side
    {
        [EnumMember(Value = "buy")]
        Buy,

        [EnumMember(Value = "sell")]
        Sell
    }
}
