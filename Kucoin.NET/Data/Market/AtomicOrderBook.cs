using Kucoin.NET.Data.Interfaces;
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
    public class AtomicOrderBook<T> : ObservableBase, IOrderBook<T>, IOrderUnitList<T> where T: IAtomicOrderUnit
    {
        private DateTime time;
        private long seq;

        private ObservableAtomicOrderUnits<T> asks = new ObservableAtomicOrderUnits<T>();

        private ObservableAtomicOrderUnits<T> bids = new ObservableAtomicOrderUnits<T>(true);

        [JsonProperty("sequence")]
        public long Sequence
        {
            get => seq;
            set
            {
                SetProperty(ref seq, value);
            }
        }

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


        [JsonProperty("asks")]
        public ObservableAtomicOrderUnits<T> Asks
        {
            get => asks;
            set
            {
                SetProperty(ref asks, value);
            }
        }

        [JsonProperty("bids")]
        public ObservableAtomicOrderUnits<T> Bids
        {
            get => bids;
            set
            {
                SetProperty(ref bids, value);
            }
        }

        SortedKeyedOrderUnitBase<T> IKeyedOrderUnitList<T>.Asks => asks;

        IList<T> IOrderUnitList<T>.Asks => asks;

        SortedKeyedOrderUnitBase<T> IKeyedOrderUnitList<T>.Bids => bids;

        IList<T> IOrderUnitList<T>.Bids => bids;
    }
}
