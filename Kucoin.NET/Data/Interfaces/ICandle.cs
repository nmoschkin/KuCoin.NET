
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// K-Line Candlestick Interface
    /// </summary>
    public interface IBasicCandle
    {
        /// <summary>
        /// Open time stamp of the candle stick.
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Open price in quote currency
        /// </summary>
        decimal OpenPrice { get; }

        /// <summary>
        /// Close price in quote currency
        /// </summary>
        decimal ClosePrice { get; }

        /// <summary>
        /// Highest price in quote currency
        /// </summary>
        decimal HighPrice { get; }

        /// <summary>
        /// Lowest price in quote currency
        /// </summary>
        decimal LowPrice { get; }

        /// <summary>
        /// Trading volume
        /// </summary>
        decimal Volume { get; }

    }

    /// <summary>
    /// K-Line Candlestick Interface
    /// </summary>
    public interface ICandle : IBasicCandle 
    { 
        /// <summary>
        /// Amount
        /// </summary>
        decimal Amount { get; }
    }

    /// <summary>
    /// Writable K-Line Candlestick Interface
    /// </summary>
    public interface IWritableBasicCandle : IBasicCandle
    {
        /// <summary>
        /// Open time stamp of the candle stick.
        /// </summary>
        new DateTime Timestamp { get; set; }

        /// <summary>
        /// Open price in quote currency
        /// </summary>
        new decimal OpenPrice { get; set; }

        /// <summary>
        /// Close price in quote currency
        /// </summary>
        new decimal ClosePrice { get; set; }

        /// <summary>
        /// Highest price in quote currency
        /// </summary>
        new decimal HighPrice { get; set; }

        /// <summary>
        /// Lowest price in quote currency
        /// </summary>
        new decimal LowPrice { get; set; }

        /// <summary>
        /// Trading volume
        /// </summary>
        new decimal Volume { get; set; }
    }

    /// <summary>
    /// Writable K-Line Candlestick Interface
    /// </summary>
    public interface IWritableCandle : IWritableBasicCandle, ICandle 
    { 
        /// <summary>
        /// Amount
        /// </summary>
        new decimal Amount { get; set; }
    }

    /// <summary>
    /// K-Line Candlestick with K-Line Type Interface
    /// </summary>
    public interface ITypedBasicCandle<T> : IBasicCandle where T : IKlineType
    {
        /// <summary>
        /// The type (length) of the K-Line
        /// </summary>
        T Type { get; }

    }

    /// <summary>
    /// Writable K-Line Candlestick with K-Line Type Interface
    /// </summary>
    public interface IWritableTypedBasicCandle<T> : IWritableBasicCandle, ITypedBasicCandle<T> where T : IKlineType
    {
        /// <summary>
        /// The type (length) of the K-Line
        /// </summary>
        new T Type { get; set; }

    }

    /// <summary>
    /// K-Line Candlestick with K-Line Type Interface
    /// </summary>
    public interface ITypedCandle<T> : ICandle where T: IKlineType
    {
        /// <summary>
        /// The type (length) of the K-Line
        /// </summary>
        T Type { get; }

    }

    /// <summary>
    /// Writable K-Line Candlestick with K-Line Type Interface
    /// </summary>
    public interface IWritableTypedCandle<T> : IWritableCandle, ITypedCandle<T> where T: IKlineType
    {
        /// <summary>
        /// The type (length) of the K-Line
        /// </summary>
        new T Type { get; set; }

    }


}
