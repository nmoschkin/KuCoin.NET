using System;

using Newtonsoft.Json;

using Kucoin.NET.Observable;
using Kucoin.NET.Json;

namespace Kucoin.NET.Data.Market
{

    /// <summary>
    /// Level 2 Ask or Bid
    /// </summary>

    [JsonConverter(typeof(OrderUnitConverter))]
    public class OrderUnit : ObservableBase, ICloneable, ISequencedOrderUnit
    {
        private decimal price;
        private decimal size;
        private long seq;

        int hc = 0;

        internal decimal SortFactor => price;

        /// <summary>
        /// Price * Size
        /// </summary>
        public decimal Total => price * size;

        /// <summary>
        /// The sequence number of this current price.
        /// </summary>
        public long Sequence
        {
            get => seq;
            set
            {
                if (SetProperty(ref seq, value)) CalcHash();
            }
        }

        /// <summary>
        /// The price in quote currency of the ask or bid.
        /// </summary>
        public decimal Price
        {
            get => price;
            set
            {
                if (SetProperty(ref price, value))
                {
                    CalcHash();
                    OnPropertyChanged(nameof(SortFactor));
                }
            }
        }

        /// <summary>
        /// The size of the ask or bid.
        /// </summary>
        public decimal Size
        {
            get => size;
            set
            {
                if (SetProperty(ref size, value))
                {
                    CalcHash();
                    OnPropertyChanged(nameof(SortFactor));
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is OrderUnit other)
            {
                return other.price == price && other.size == size && other.seq == seq;
            }
            else
            {
                return false;
            }
        }

        private void CalcHash()
        {
            hc = (price.ToString() + size.ToString() + seq.ToString()).GetHashCode();
        }
        public override int GetHashCode() => hc;

        internal OrderUnit(string[] data)
        {
            price = decimal.Parse(data[0]);
            size = decimal.Parse(data[1]);
            if (data.Length > 2) seq = long.Parse(data[2]);
        }

        public OrderUnit()
        {
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>
        /// Create a copy of this object.
        /// </summary>
        /// <returns></returns>
        public OrderUnit Clone()
        {
            return new OrderUnit()
            {
                price = price,
                size = size,
                seq = seq
            };
        }

        public override string ToString()
        {
            return $"{Price} ({Size})" + ((Sequence != 0) ? $": {Sequence}" : "");

        }

    }

}
