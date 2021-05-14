using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Kucoin.NET.Data.Interfaces;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Candlestick structure.
    /// </summary>
    public class Candle : IWriteableTypedCandle
    {
        public Candle(IList<string> values, KlineType type)
        {

            Deserialize(values);
            Type = type;
        }

        public virtual void Deserialize(object source)
        {
            var values = source as IList<string>;

            Timestamp = EpochTime.SecondsToDate(long.Parse(values[0]));

            OpenPrice = decimal.Parse(values[1]);
            ClosePrice = decimal.Parse(values[2]);

            HighPrice = decimal.Parse(values[3]);
            LowPrice = decimal.Parse(values[4]);

            Amount = decimal.Parse(values[5]);
            Volume = decimal.Parse(values[6]);
        }

        public Candle()
        {
        }

        public static bool IsTimeInCandle(ITypedCandle candle, DateTime time)
        {
            var dv = candle.Timestamp.AddSeconds(candle.Type.Length);

            if (time > dv || time < candle.Timestamp) return false;
            else return true;
        }

        public virtual DateTime Timestamp { get; set; }

        public virtual decimal OpenPrice { get; set; }

        public virtual decimal ClosePrice { get; set; }

        public virtual decimal HighPrice { get; set; }

        public virtual decimal LowPrice { get; set; }

        public virtual decimal Amount { get; set; }

        public virtual decimal Volume { get; set; }

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
