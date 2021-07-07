using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kucoin.NET.Futures.Data.Trade
{

    [JsonConverter(typeof(EnumToStringConverter<FuturesOrderType>))]
    public enum FuturesOrderType
    {
        [EnumMember(Value = "market")]
        Market,

        [EnumMember(Value = "limit")]
        Limit

    }

    [JsonConverter(typeof(EnumToStringConverter<FuturesStopType>))]
    public enum FuturesStopType
    {
        [EnumMember(Value = "up")]
        Up,

        [EnumMember(Value = "down")]
        Down
    }


    [JsonConverter(typeof(EnumToStringConverter<FuturesStopPriceType>))]
    public enum FuturesStopPriceType
    {
        [EnumMember(Value = "TP")]
        TP,

        [EnumMember(Value = "IP")]
        IP,

        [EnumMember(Value = "MP")]
        MP
    }



    /// <summary>
    /// Time-In-Force 
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<FuturesTimeInForce>))]
    public enum FuturesTimeInForce
    {
        /// <summary>
        /// Good Till Canceled orders remain open on the book until canceled. This is the default behavior if no policy is specified.
        /// </summary>
        [EnumMember(Value = "GTC")]
        GoodTillCanceled,

        /// <summary>
        /// Immediate Or Cancel orders instantly cancel the remaining size of the limit order instead of opening it on the book.
        /// </summary>
        [EnumMember(Value = "IOC")]
        ImmediateOrCancel

    }

    [JsonConverter(typeof(EnumToStringConverter<FuturesOrderStatus>))]
    public enum FuturesOrderStatus
    {
        [EnumMember(Value = "done")]
        Done,

        [EnumMember(Value = "open")]
        Open
    }


    public enum FuturesTradeType
    {
        // (trade, liquidation, ADL or settlement

        [EnumMember(Value = "trade")]
        Trade,

        [EnumMember(Value = "liquidation")]
        Liquidation,

        [EnumMember(Value = "ADL")]
        ADL,

        [EnumMember(Value = "settlement")]
        Settlement

    }
}
