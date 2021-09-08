using KuCoin.NET.Rest;
using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.Market
{

    /// <summary>
    /// K-Line Candlestick basic implementation.
    /// </summary>
    public abstract class CandleBase : DataObject, ICandle
    {
        public DateTime Timestamp { get; set; }

        public decimal OpenPrice { get; set; }

        public decimal ClosePrice { get; set; }

        public decimal HighPrice { get; set; }

        public decimal LowPrice { get; set; }

        public decimal Volume { get; set; }

    }


    /// <summary>
    /// K-Line Candlestick standard implementation.
    /// </summary>
    public class Candle : CandleBase, IFullKlineCandle<KlineType>
    {
        /// <summary>
        /// Initialize a new candle stick object from a list of string values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <remarks>
        /// Format is timestamp,open,close,high,low,amount,volume.
        /// Decimal values are represented as quoted strings.
        /// </remarks>
        public Candle(IList<string> values)
        {
            ReadData(values);
        }

        /// <summary>
        /// Initialize a new candle stick object with the specified K-Line type from a list of string values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="type">The K-Line type (the length of time represented by the candle stick)</param>
        /// <remarks>
        /// Format is timestamp,open,close,high,low,amount,volume.
        /// Decimal values are represented as quoted strings.
        /// </remarks>
        public Candle(IList<string> values, KlineType type)
        {
            ReadData(values);
            Type = type;
        }

        /// <summary>
        /// Read data from a list of strings.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <remarks>
        /// Format is timestamp,open,close,high,low,amount,volume.
        /// Decimal values are represented as quoted strings.
        /// </remarks>
        public virtual void ReadData(IList<string> values)
        {           
            Timestamp = EpochTime.SecondsToDate(long.Parse(values[0]));

            OpenPrice = decimal.Parse(values[1]);
            ClosePrice = decimal.Parse(values[2]);

            HighPrice = decimal.Parse(values[3]);
            LowPrice = decimal.Parse(values[4]);

            Amount = decimal.Parse(values[5]);
            Volume = decimal.Parse(values[6]);
        }

        /// <summary>
        /// Create a new, empty candle stick.
        /// </summary>
        public Candle()
        {
        }

        /// <summary>
        /// Test whether the specified time falls within the time range of the specified <see cref="IReadOnlyFullKlineCandle{T}"/>.
        /// </summary>
        /// <param name="candle">The candlestick to test.</param>
        /// <param name="time">The time to test.</param>
        /// <returns>True if the time falls within the candlestick.</returns>
        public static bool IsTimeInCandle<T>(IReadOnlyFullKlineCandle<T> candle, DateTime time) where T: IKlineType
        {
            DateTime dv;

            if (time.Kind != DateTimeKind.Utc) time = time.ToUniversalTime();

            dv = candle.Timestamp.ToUniversalTime() + candle.Type.TimeSpan;

            if (time >= dv || time < candle.Timestamp.ToUniversalTime()) return false;
            else return true;
        }

        public virtual decimal Amount { get; set; }

        public virtual KlineType Type { get; set; }

        public override string ToString()
        {

            string dir;

            if (OpenPrice > ClosePrice)
            {
                dir = "Down";
            }
            else
            {
                dir = "Up";
            }

            return $"({Type}) {Timestamp}: Open: {OpenPrice}, Close: {ClosePrice}, Direction: {dir}";
        }

    }
}
