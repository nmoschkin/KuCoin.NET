using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Observable;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Market
{

    /// <summary>
    /// Represents a Level 3 order
    /// </summary>
    [JsonConverter(typeof(AtomicOrderUnitConverter))]
    public class AtomicOrderUnit : 
        ObservableBase, 
        IAtomicOrderUnit, 
        ICloneable
    {
        private decimal price;
        private decimal size;
        private DateTime time;
        private string orderId;

        int hc = 0;

        internal decimal SortFactor => price;

        /// <summary>
        /// Price * Size
        /// </summary>
        public decimal Total => price * size;

        /// <summary>
        /// The Order Id of this order.
        /// </summary>
        public string OrderId
        {
            get => orderId;
            set
            {
                SetProperty(ref orderId, value);
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
        /// The size in base currency of the ask or bid.
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

        /// <summary>
        /// The time stamp of the order.
        /// </summary>
        public DateTime Timestamp
        {
            get => time;
            set => time = value;
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

        private void CalcHash()
        {
            hc = (price.ToString() + size.ToString() + (orderId ?? "") + time.ToString()).GetHashCode();
        }
        public override int GetHashCode() => hc;
                
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>
        /// Create a copy of this object.
        /// </summary>
        /// <returns></returns>
        public AtomicOrderUnit Clone()
        {
            return new AtomicOrderUnit()
            {
                price = price,
                size = size,
                time = time,
                orderId = orderId
            };
        }

        public override string ToString()
        {
            return $"{orderId} : {time} : {Price} ({Size})";

        }


        /// <summary>
        /// Creates a new, empty atomic order unit.
        /// </summary>
        public AtomicOrderUnit()
        {

        }

        /// <summary>
        /// Create a new atomic order unit based on raw JSON-sourced input.
        /// </summary>
        /// <param name="values">The JSON sourced input.</param>
        /// <remarks>
        /// For internal use, only.
        /// </remarks>
        internal AtomicOrderUnit(object[] values)
        {
            OrderId = (string)values[0];
            Price = decimal.Parse((string)values[1]);
            Size = decimal.Parse((string)values[2]);
            Timestamp = EpochTime.NanosecondsToDate((long)values[3]);
        }

    }
}
