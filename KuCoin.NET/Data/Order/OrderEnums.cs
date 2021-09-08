using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.Order
{
    /// <summary>
    /// Trade type
    /// </summary>

    [JsonConverter(typeof(EnumToStringConverter<TradeType>))]
    public enum TradeType
    {
        /// <summary>
        /// Spot trade
        /// </summary>
        [EnumMember(Value = "TRADE")]
        Spot,

        /// <summary>
        /// Margin trade
        /// </summary>
        [EnumMember(Value = "MARGIN_TRADE")]
        Margin

    }

    /// <summary>
    /// Applied liquidity type
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<LiquidityType>))]
    public enum LiquidityType
    {
        /// <summary>
        /// Taker (market orders)
        /// </summary>
        [EnumMember(Value = "taker")]
        Taker,

        /// <summary>
        /// Maker (limit orders)
        /// </summary>
        [EnumMember(Value = "maker")]
        Maker
    }

    /// <summary>
    /// Margin mode
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<MarginMode>))]
    public enum MarginMode
    {
        [EnumMember(Value = "cross")]
        Cross,

        [EnumMember(Value = "isolated")]
        Isolated
    }

    /// <summary>
    /// Time-In-Force 
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<TimeInForce>))]
    public enum TimeInForce
    {
        /// <summary>
        /// Good Till Canceled orders remain open on the book until canceled. This is the default behavior if no policy is specified.
        /// </summary>
        [EnumMember(Value = "GTC")]
        GoodTillCanceled,

        /// <summary>
        /// Good Till Time orders remain open on the book until canceled or the allotted cancelAfter is depleted on the matching engine. GTT orders are guaranteed to cancel before any other order is processed after the cancelAfter seconds placed in order book.
        /// </summary>
        [EnumMember(Value = "GTT")]
        GoodTillTime,

        /// <summary>
        /// Immediate Or Cancel orders instantly cancel the remaining size of the limit order instead of opening it on the book.
        /// </summary>
        [EnumMember(Value = "IOC")]
        ImmediateOrCancel,

        /// <summary>
        /// Fill Or Kill orders are rejected if the entire size cannot be matched.
        /// </summary>
        [EnumMember(Value = "FOK")]
        FillOrKill

    }

    /// <summary>
    /// Self trade protection options
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<StpMode>))]
    public enum StpMode
    {

        /// <summary>
        /// Decrease and Cancel
        /// </summary>
        [EnumMember(Value = "DC")]
        DecreaseAndCancel,

        /// <summary>
        /// Cancel oldest
        /// </summary>
        [EnumMember(Value = "CO")]
        CancelOldest,

        /// <summary>
        /// Cancel newest
        /// </summary>
        [EnumMember(Value = "CN")]
        CancelNewest,

        /// <summary>
        /// Cancel both
        /// </summary>
        [EnumMember(Value = "CB")]
        CancelBoth

    }

    /// <summary>
    /// Stop order type
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<StopType>))]
    public enum StopType
    {
        /// <summary>
        /// Stop entry
        /// </summary>
        [EnumMember(Value = "entry")]
        Entry,

        /// <summary>
        /// Stop loss
        /// </summary>
        [EnumMember(Value = "loss")]
        Loss
    }

    /// <summary>
    /// Order status
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<OrderStatus>))]
    public enum OrderStatus
    {
        /// <summary>
        /// Done
        /// </summary>
        [EnumMember(Value = "done")]
        Done,

        /// <summary>
        /// Active
        /// </summary>
        [EnumMember(Value = "active")]
        Active
    }

    /// <summary>
    /// Order types
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<OrderType>))]
    public enum OrderType
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
        Limit,

        /// <summary>
        /// Market stop order
        /// </summary>
        [EnumMember(Value = "market_stop")]
        MarketStop,

        /// <summary>
        /// Limit stop order
        /// </summary>
        [EnumMember(Value = "limit_stop")]
        LimitStop

    }
}
