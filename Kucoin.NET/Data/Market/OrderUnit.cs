using System;

using Newtonsoft.Json;

using Kucoin.NET.Observable;
using Kucoin.NET.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Schema;

namespace Kucoin.NET.Data.Market
{

    /// <summary>
    /// Level 2 Ask or Bid
    /// </summary>

    [JsonConverter(typeof(OrderUnitConverter))]
    public class OrderUnit : ICloneable, ISequencedOrderUnit
    {
        protected decimal price;
        protected decimal size;
        protected long seq;

        /// <summary>
        /// Price * Size
        /// </summary>
        public virtual decimal Total => price * size;

        /// <summary>
        /// The sequence number of this current price.
        /// </summary>
        public virtual long Sequence
        {
            get => seq;
            set
            {
                if (seq != value)
                {
                    seq = value;
                }
            }
        }

        /// <summary>
        /// The price in quote currency of the ask or bid.
        /// </summary>
        public virtual decimal Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = value;
                }
            }
        }

        /// <summary>
        /// The size of the ask or bid.
        /// </summary>
        public virtual decimal Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
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

        public override int GetHashCode() => (price.ToString() + size.ToString() + seq.ToString()).GetHashCode();

        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Create a new order unit from this object.
        /// </summary>
        /// <returns></returns>
        public virtual T Clone<T>() where T: IOrderUnit, new()
        {
            var ret = new T()
            {
                Price = price,
                Size = size
            };

            if (ret is ISequencedOrderUnit seq)
            {
                seq.Sequence = this.seq;
            }

            return ret;
        }

        public override string ToString()
        {
            return $"{Price} ({Size})" + ((Sequence != 0) ? $": {Sequence}" : "");

        }


        internal OrderUnit(string[] data)
        {
            if (data[0].Contains("E"))
            {
                price = (decimal)double.Parse(data[0]);
            }
            else
            {
                price = decimal.Parse(data[0]);
            }

            size = decimal.Parse(data[1]);
            if (data.Length > 2) seq = long.Parse(data[2]);
        }

        public OrderUnit()
        {
        }

    }


    /// <summary>
    /// Level 2 Ask or Bid
    /// </summary>

    [JsonConverter(typeof(OrderUnitConverter))]
    public class ObservableOrderUnit : OrderUnit, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public override decimal Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public override decimal Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public override long Sequence
        {
            get => seq;
            set
            {
                if (seq != value)
                {
                    seq = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableOrderUnit() : base()
        {
        }

        internal ObservableOrderUnit(string[] data) : base(data)
        {
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        

    }

}
