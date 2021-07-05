using System;

using Newtonsoft.Json;

using Kucoin.NET.Observable;
using Kucoin.NET.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Schema;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Data.Market
{

    /// <summary>
    /// Level 2 Ask or Bid
    /// </summary>

    [JsonConverter(typeof(AtomicOrderUnitConverter))]
    public class AtomicOrderUnit : ICloneable, IAtomicOrderUnit
    {
        protected decimal price;
        protected decimal size;
        protected DateTime time;
        protected string orderId;




        /// <summary>
        /// Price * Size
        /// </summary>
        public virtual decimal Total => price * size;


        public virtual string OrderId
        {
            get => orderId;
            set
            {
                if (orderId != value)
                {
                    orderId = value;
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

        public virtual DateTime Timestamp
        {
            get => time;
            set
            {
                if (time != value)
                {
                    time = value;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is AtomicOrderUnit other)
            {
                return other.price == price && other.size == size && other.time == time && other.orderId == orderId;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => (price.ToString() + size.ToString() + time.ToString() + (orderId ?? "")).GetHashCode();

        internal AtomicOrderUnit(object[] data)
        {
            orderId = (string)data[0];
            price = decimal.Parse((string)data[1]);
            size = decimal.Parse((string)data[2]);
            time = EpochTime.NanosecondsToDate((long)data[3]);
        }

        public AtomicOrderUnit()
        {
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Create a new order unit from this object.
        /// </summary>
        /// <returns></returns>
        public virtual T Clone<T>() where T : IAtomicOrderUnit, new()
        {
            var ret = new T()
            {
                Price = price,
                Size = size,
                OrderId = orderId,
                Timestamp = time
            };

            return ret;
        }

        public override string ToString()
        {
            return $"{OrderId} - {Timestamp}: {Price} ({Size})";
        }

    }


    /// <summary>
    /// Level 2 Ask or Bid
    /// </summary>

    [JsonConverter(typeof(AtomicOrderUnitConverter))]
    public class ObservableAtomicOrderUnit : AtomicOrderUnit, INotifyPropertyChanged
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

        public override string OrderId
        {
            get => orderId;
            set
            {
                if (orderId != value)
                {
                    orderId = value;
                    OnPropertyChanged();
                }
            }
        }

        public override DateTime Timestamp
        {
            get => time;
            set
            {
                if (time != value)
                {
                    time = value;
                    OnPropertyChanged();
                }
            }
        }

        internal ObservableAtomicOrderUnit(object[] data) : base(data)
        {
        }

        public ObservableAtomicOrderUnit() : base()
        {
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

    [JsonConverter(typeof(AtomicOrderUnitConverter))]
    public struct AtomicOrderStruct : IAtomicOrderUnit
    {
        internal string orderId;
        internal DateTime timestamp;
        internal decimal price;
        internal decimal size;

        [JsonProperty("orderId")]
        public string OrderId
        {
            get => orderId;
            set => orderId = value;
        }

        [JsonProperty("time")]
        public DateTime Timestamp
        {
            get => timestamp;
            set => timestamp = value;
        }

        [JsonProperty("price")]
        public decimal Price
        {
            get => price;
            set => price = value;
        }

        [JsonProperty("size")]
        public decimal Size
        {
            get => size;
            set => size = value;
        }

        internal AtomicOrderStruct(object[] data)
        {
            orderId = (string)data[0];
            price = decimal.Parse((string)data[1]);
            size = decimal.Parse((string)data[2]);
            timestamp = EpochTime.NanosecondsToDate((long)data[3]);
        }

        public AtomicOrderStruct Clone()
        {
            return (AtomicOrderStruct)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
    }

}
