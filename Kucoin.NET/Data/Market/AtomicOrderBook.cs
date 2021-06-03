using Kucoin.NET.Data.Order;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Observable;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Represents Level 3 Market Data
    /// </summary>
    public class AtomicOrderBook<T> : 
        ObservableBase, 
        IAtomicOrderBook<T>
        where T: IAtomicOrderUnit
    {
        private DateTime time;
        private long seq;

        private ObservableLevel3OrderUnits<T> asks = new ObservableLevel3OrderUnits<T>(false);

        private ObservableLevel3OrderUnits<T> bids = new ObservableLevel3OrderUnits<T>(true);

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
        public ObservableLevel3OrderUnits<T> Asks
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
        public ObservableLevel3OrderUnits<T> Bids
        {
            get => bids;
            set
            {
                SetProperty(ref bids, value);
            }
        }

        Level3KeyedCollectionBase<T> IAtomicOrderBook<T>.Asks => asks;
        Level3KeyedCollectionBase<T> IAtomicOrderBook<T>.Bids => bids;


    }
}
