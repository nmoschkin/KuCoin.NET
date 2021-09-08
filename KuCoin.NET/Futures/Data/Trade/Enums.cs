using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KuCoin.NET.Futures.Data.Trade
{

    /// <summary>
    /// Futures order type
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<FuturesOrderType>))]
    public enum FuturesOrderType
    {
        /// <summary>
        /// Market order
        /// </summary>
        [EnumMember(Value = "market")]
        Market,

        /// <summary>
        /// Limit order
        /// </summary>
        [EnumMember(Value = "limit")]
        Limit

    }

    /// <summary>
    /// Futures Stop Type
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<FuturesStopType>))]
    public enum FuturesStopType
    {
        /// <summary>
        /// Up
        /// </summary>
        [EnumMember(Value = "up")]
        Up,

        /// <summary>
        /// Down
        /// </summary>
        [EnumMember(Value = "down")]
        Down
    }

    /// <summary>
    /// Futures Stop Price Type
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<FuturesStopPriceType>))]
    public enum FuturesStopPriceType
    {
        /// <summary>
        /// Trade Price
        /// </summary>
        [EnumMember(Value = "TP")]
        TP,

        /// <summary>
        /// Index Price
        /// </summary>
        [EnumMember(Value = "IP")]
        IP,

        /// <summary>
        /// Mark Price
        /// </summary>
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

    /// <summary>
    /// Futures order status
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<FuturesOrderStatus>))]
    public enum FuturesOrderStatus
    {
        /// <summary>
        /// Match
        /// </summary>
        [EnumMember(Value = "match")]
        Match,

        /// <summary>
        /// Open
        /// </summary>
        [EnumMember(Value = "open")]
        Open,

        /// <summary>
        /// Done
        /// </summary>
        [EnumMember(Value = "done")]
        Done
    }

    /// <summary>
    /// Futures Trade or Transaction Type
    /// </summary>
    public enum FuturesTradeType
    {

        /// <summary>
        /// Trade
        /// </summary>
        [EnumMember(Value = "trade")]
        Trade,

        /// <summary>
        /// Liquidation
        /// </summary>
        [EnumMember(Value = "liquidation")]
        Liquidation,

        /// <summary>
        /// Auto Deleverage
        /// </summary>
        [EnumMember(Value = "ADL")]
        ADL,

        /// <summary>
        /// Settlement
        /// </summary>
        [EnumMember(Value = "settlement")]
        Settlement

    }
}
