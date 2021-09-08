
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// K-Line Candlestick Interface
    /// </summary>
    public interface IReadOnlyCandle : IDataObject
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
    public interface IReadOnlyFullCandle : IReadOnlyCandle 
    { 
        /// <summary>
        /// Amount
        /// </summary>
        decimal Amount { get; }
    }

    /// <summary>
    /// Writable K-Line Candlestick Interface
    /// </summary>
    public interface ICandle : IReadOnlyCandle, IStreamableObject
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
    public interface IFullCandle : ICandle, IReadOnlyFullCandle 
    { 
        /// <summary>
        /// Amount
        /// </summary>
        new decimal Amount { get; set; }
    }

    /// <summary>
    /// K-Line Candlestick with K-Line Type Interface
    /// </summary>
    public interface IReadOnlyKlineCandle<T> : IReadOnlyCandle where T : IKlineType
    {
        /// <summary>
        /// The type (length) of the K-Line
        /// </summary>
        T Type { get; }

    }

    /// <summary>
    /// Writable K-Line Candlestick with K-Line Type Interface
    /// </summary>
    public interface IKlineCandle<T> : ICandle, IReadOnlyKlineCandle<T> where T : IKlineType
    {
        /// <summary>
        /// The type (length) of the K-Line
        /// </summary>
        new T Type { get; set; }

    }

    /// <summary>
    /// K-Line Candlestick with K-Line Type Interface
    /// </summary>
    public interface IReadOnlyFullKlineCandle<T> : IReadOnlyFullCandle where T: IKlineType
    {
        /// <summary>
        /// The type (length) of the K-Line
        /// </summary>
        T Type { get; }

    }

    /// <summary>
    /// Writable K-Line Candlestick with K-Line Type Interface
    /// </summary>
    public interface IFullKlineCandle<T> : IFullCandle, IReadOnlyFullKlineCandle<T> where T: IKlineType
    {
        /// <summary>
        /// The type (length) of the K-Line
        /// </summary>
        new T Type { get; set; }

    }


}
