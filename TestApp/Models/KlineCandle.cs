using Kucoin.NET.Data.Market;
using Kucoin.NET.Observable;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoinApp
{
    public class KlineCandle : ObservableBase, FancyCandles.ICandle, IWritableTypedCandle, ICloneable
    {
        private DateTime ts;
        private decimal o;
        private decimal h;
        private decimal l;
        private decimal c;
        private decimal v;
        private decimal a;
        private KlineType kt;

        public DateTime Timestamp
        {
            get => ts;
            set
            {
                SetProperty(ref ts, value);
            }
        }

        public decimal OpenPrice
        {
            get => o;
            set
            {
                SetProperty(ref o, value);
            }
        }

        public decimal ClosePrice
        {
            get => c;
            set
            {
                SetProperty(ref c, value);
            }
        }

        public decimal HighPrice
        {
            get => h;
            set
            {
                SetProperty(ref h, value);
            }
        }

        public decimal LowPrice
        {
            get => l;
            set
            {
                SetProperty(ref l, value);
            }
        }

        public decimal Volume
        {
            get => v;
            set
            {
                SetProperty(ref v, value);
            }
        }


        public decimal Amount
        {
            get => a;
            set
            {
                SetProperty(ref a, value);
            }
        }

        public KlineType Type
        {
            get => kt;
            set
            {
                SetProperty(ref kt, value);
            }
        }

        DateTime FancyCandles.ICandle.t => Timestamp;
        double FancyCandles.ICandle.O => (double)OpenPrice;
        double FancyCandles.ICandle.H => (double)HighPrice;
        double FancyCandles.ICandle.L => (double)LowPrice;
        double FancyCandles.ICandle.C => (double)ClosePrice;
        double FancyCandles.ICandle.V => (double)Volume;

        public KlineCandle()
        {
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        public KlineCandle Clone()
        {
            return (KlineCandle)MemberwiseClone();
        }


        public KlineCandle(Candle kline)
        {
            Timestamp = kline.Timestamp;
            OpenPrice = kline.OpenPrice;
            HighPrice = kline.HighPrice;
            LowPrice = kline.LowPrice;
            ClosePrice = kline.ClosePrice;
            Volume = kline.Volume;
            Amount = kline.Amount;
            Type = kline.Type;
        }

    }
}
