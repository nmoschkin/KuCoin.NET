
using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoinApp
{
    public class KlineCandle : FancyCandles.ICandle
    {
        private Kline kline;
        
        public Kline Source
        {
            get => kline;
            set => kline = value;
        }
        
        public DateTime t => kline.Timestamp;
        public double O => (double)kline.OpenPrice;
        public double H => (double)kline.HighPrice;
        public double L => (double)kline.LowPrice;
        public double C => (double)kline.ClosePrice;
        public double V => (double)kline.Volume;



        public KlineCandle(Kline kline)
        {
            this.kline = kline;
        }

        public static implicit operator KlineCandle(Kline k) => new KlineCandle(k);
        public static implicit operator Kline(KlineCandle k) => k.kline;
    }
}
