using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Candlestick structure.
    /// </summary>
    public struct Kline
    {

        public Kline(IList<string> values, KlineType type)
        {
            Timestamp = EpochTime.SecondsToDate(long.Parse(values[0]));

            OpenPrice = decimal.Parse(values[1]);
            ClosePrice = decimal.Parse(values[2]);

            HighPrice = decimal.Parse(values[3]);
            LowPrice = decimal.Parse(values[4]);

            Amount = decimal.Parse(values[5]);
            Volume = decimal.Parse(values[6]);

            Type = type;
        }

        public bool IsTimeInKline(DateTime time)
        {
            var dv = Timestamp.AddSeconds(Type.Length);

            if (time > dv || time < Timestamp) return false;
            else return true;
        }

        public DateTime Timestamp;

        public decimal OpenPrice;

        public decimal ClosePrice;

        public decimal HighPrice;

        public decimal LowPrice;

        public decimal Amount;

        public decimal Volume;

        public KlineType Type;

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
