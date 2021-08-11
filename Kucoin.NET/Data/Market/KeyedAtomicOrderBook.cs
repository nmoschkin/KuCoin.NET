using Kucoin.NET.Data.Order;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Observable;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Represents Level 3 Market Data
    /// </summary>
    public class KeyedAtomicOrderBook<T> : 
        IKeyedAtomicOrderBook<KeyedBook<T>, T>
        where T: IAtomicOrderUnit
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

        IList<T> IOrderUnitList<T>.Asks => Asks;

        IList<T> IOrderUnitList<T>.Bids => Bids;
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
    }

}
