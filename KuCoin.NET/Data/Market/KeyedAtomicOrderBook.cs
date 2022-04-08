using KuCoin.NET.Helpers;
using KuCoin.NET.Json;
using KuCoin.NET.Observable;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// Represents Level 3 Market Data
    /// </summary>
    public class KeyedAtomicOrderBook<T> : 
        IKeyedAtomicOrderBook<KeyedBook<T>, T>
        where T: IAtomicOrderUnit, new()
    {

        /// <summary>
        /// The current sequence number of the order book.
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        /// <summary>
        /// The current time stamp of the order book.
        /// </summary>
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        [JsonProperty("time")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The keyed list of asks (sell)
        /// </summary>
        [JsonProperty("asks")]
        public virtual KeyedBook<T> Asks { get; set; } = new KeyedBook<T>(false);

        /// <summary>
        /// The keyed list of bids (buy)
        /// </summary>
        [JsonProperty("bids")]
        public virtual KeyedBook<T> Bids { get; set; } = new KeyedBook<T>(true);

        ICollection<T> IOrderUnitCollection<T>.Asks => Asks;

        ICollection<T> IOrderUnitCollection<T>.Bids => Bids;

        ICollection<T> IDataSeries<T, T, ICollection<T>, ICollection<T>>.Data1 => Asks;
        ICollection<T> IDataSeries<T, T, ICollection<T>, ICollection<T>>.Data2 => Bids;
    }

    /// <summary>
    /// Represents Level 3 Market Data
    /// </summary>
    public class ObservableAtomicOrderBook<T> :
        ObservableBase,
        IAtomicOrderBook<T>
        where T : IAtomicOrderUnit
    {

        protected long seq;
        protected DateTime time;
        protected ObservableCollection<T> asks = new ObservableCollection<T>();
        protected ObservableCollection<T> bids = new ObservableCollection<T>();

        /// <summary>
        /// The current sequence number of the order book.
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence
        {
            get => seq;
            set
            {
                SetProperty(ref seq, value);
            }
        }

        /// <summary>
        /// The current time stamp of the order book.
        /// </summary>
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        [JsonProperty("time")]
        public DateTime Timestamp
        {
            get => time;
            set
            {
                SetProperty(ref time, value);
            }
        }

        /// <summary>
        /// The keyed list of asks (sell)
        /// </summary>
        [JsonProperty("asks")]
        public ObservableCollection<T> Asks
        {
            get => asks;
            set
            {
                SetProperty(ref asks, value);
            }
        }

        /// <summary>
        /// The keyed list of bids (buy)
        /// </summary>
        [JsonProperty("bids")]
        public ObservableCollection<T> Bids
        {
            get => bids;
            set
            {
                SetProperty(ref bids, value);
            }
        }

        ICollection<T> IOrderUnitCollection<T>.Asks => asks;

        ICollection<T> IOrderUnitCollection<T>.Bids => bids;

        ICollection<T> IDataSeries<T, T, ICollection<T>, ICollection<T>>.Data1 => asks;

        ICollection<T> IDataSeries<T, T, ICollection<T>, ICollection<T>>.Data2 => bids;
    }

}
