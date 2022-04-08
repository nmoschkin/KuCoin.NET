using System;

using Newtonsoft.Json;

using KuCoin.NET.Observable;
using KuCoin.NET.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Schema;
using KuCoin.NET.Helpers;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace KuCoin.NET.Data.Market
{

    /// <summary>
    /// Atomic (Level 3) Order Unit
    /// </summary>

    [JsonConverter(typeof(AtomicOrderUnitConverter))]
    public class AtomicOrderUnit : DataObject, ICloneable, IAtomicOrderUnit
    {
        protected decimal price;
        protected decimal size;
        protected DateTime time;
        protected string orderId;

        public long Sequence { get; set; }

        /// <summary>
        /// Price * Size
        /// </summary>
        public virtual decimal Total => price * size;

        /// <summary>
        /// The Order Id
        /// </summary>
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

        /// <summary>
        /// The timestamp of this order.
        /// </summary>
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

        public AtomicOrderUnit(object[] data)
        {
            orderId = (string)data[0];
            price = decimal.Parse((string)data[1]);
            size = decimal.Parse((string)data[2]);
            time = EpochTime.NanosecondsToDate((long)data[3]);
        }

        /// <summary>
        /// Create a new atomic order unit.
        /// </summary>
        public AtomicOrderUnit()
        {
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Create a new atomic order unit implementation from the values in this object.
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

            if (ret is AtomicOrderUnit other)
            {
                other.Sequence = Sequence;
            }

            return ret;
        }

        public override string ToString()
        {
            if (Sequence != 0)
            {
                return $"{OrderId} - {Timestamp}: {Price} ({Size})  :  {Sequence}";
            }
            else
            {
                return $"{OrderId} - {Timestamp}: {Price} ({Size})";
            }
        }

    }


    /// <summary>
    /// Observable Atomic (Level 3) Order Unit
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

        /// <summary>
        /// Initialize the object with data.
        /// </summary>
        /// <param name="data">Data.</param>
        public ObservableAtomicOrderUnit(object[] data) : base(data)
        {
        }

        /// <summary>
        /// Create a new observable atomic order unit.
        /// </summary>
        public ObservableAtomicOrderUnit() : base()
        {
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

    /// <summary>
    /// Atomic (Level 3) Order Unit (struct implementation)
    /// </summary>
    [JsonConverter(typeof(AtomicOrderUnitConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public struct AtomicOrderStruct : IAtomicOrderUnit
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        internal string orderId;
        
        internal decimal price;
        
        internal decimal size;
        
        internal DateTime timestamp;

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

        /// <summary>
        /// Initialize the structure from data.
        /// </summary>
        /// <param name="data">The data.</param>
        public AtomicOrderStruct(object[] data)
        {
            orderId = (string)data[0];
            price = decimal.Parse((string)data[1]);
            size = decimal.Parse((string)data[2]);
            timestamp = EpochTime.NanosecondsToDate((long)data[3]);
        }
        public object[] GetObjects()
        {
            return new object[] { orderId, price.ToString(), size.ToString(), EpochTime.DateToNanoseconds(timestamp) };
        }
        /// <summary>
        /// Make an exact copy of this struct.
        /// </summary>
        /// <returns></returns>
        public AtomicOrderStruct Clone()
        {
            return (AtomicOrderStruct)MemberwiseClone();
        }

        /// <summary>
        /// Create a new atomic order unit implementation from the values in this object.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="IAtomicOrderUnit"/> and can be created.</typeparam>
        /// <returns></returns>
        public T Clone<T>() where T: IAtomicOrderUnit, new()
        {
            T ret = new T()
            {
                Price = Price,
                Size = Size,
                OrderId = OrderId,
                Timestamp = Timestamp
            };

            return ret;
        }

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                { "price", Price },
                { "size", Size },
                { "orderId", OrderId },
                { "ts", Timestamp },
            };
        }
        
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
    }

}
