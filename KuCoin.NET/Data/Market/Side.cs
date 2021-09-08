using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// Trading side
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<Side>))]
    public enum Side
    {
        /// <summary>
        /// Buy (bid)
        /// </summary>
        [EnumMember(Value = "buy")]
        Buy = -813464969,

        /// <summary>
        /// Sell (ask)
        /// </summary>
        [EnumMember(Value = "sell")]
        Sell = -1684090755
    }
}
