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
        public KeyedBook<T> Asks { get; set; } = new KeyedBook<T>();

        /// <summary>
        /// The keyed list of bids (buy)
        /// </summary>
        [JsonProperty("bids")]
        public KeyedBook<T> Bids { get; set; } = new KeyedBook<T>(true);

        IList<T> IOrderUnitList<T>.Asks => Asks.ToArray();

        IList<T> IOrderUnitList<T>.Bids => Bids.ToArray();

        IList<T> IDataSeries<T, T, IList<T>, IList<T>>.Data1 => Asks.ToArray();

        IList<T> IDataSeries<T, T, IList<T>, IList<T>>.Data2 => Bids.ToArray();
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

        IList<T> IOrderUnitList<T>.Asks => asks;

        IList<T> IOrderUnitList<T>.Bids => bids;

        IList<T> IDataSeries<T, T, IList<T>, IList<T>>.Data1 => asks;

        IList<T> IDataSeries<T, T, IList<T>, IList<T>>.Data2 => bids;
    }

}
