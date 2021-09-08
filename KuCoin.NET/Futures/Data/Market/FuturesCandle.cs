using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Market
{
    public class FuturesCandle : CandleBase, IKlineCandle<FuturesKlineType>
    {
        public FuturesKlineType Type { get; set; }

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

            return $"({Type.ToString(true)}) {Timestamp}: Open: {OpenPrice}, Close: {ClosePrice}, Direction: {dir}";
        }

    }
}
