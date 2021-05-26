using Kucoin.NET.Data.Interfaces;
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
    public class AtomicOrderBook : ObservableBase, IOrderBook<AtomicOrderUnit>, IOrderUnitList
    {
        private DateTime time;
        private long seq;

        private ObservableAtomicOrderUnits<AtomicOrderUnit> asks = new ObservableAtomicOrderUnits<AtomicOrderUnit>();

        private ObservableAtomicOrderUnits<AtomicOrderUnit> bids = new ObservableAtomicOrderUnits<AtomicOrderUnit>(true);

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

        IList<IOrderUnit> IOrderUnitList.Asks => (IList<IOrderUnit>)bids;

        IList<IOrderUnit> IOrderUnitList.Bids => (IList<IOrderUnit>)bids;


        [JsonProperty("asks")]
        public ObservableAtomicOrderUnits<AtomicOrderUnit> Asks
        {
            get => asks;
            set
            {
                SetProperty(ref asks, value);
            }
        }

        [JsonProperty("bids")]
        public ObservableAtomicOrderUnits<AtomicOrderUnit> Bids
        {
            get => bids;
            set
            {
                SetProperty(ref bids, value);
            }
        }

        KeyedCollection<decimal, AtomicOrderUnit> IKeyedOrderUnitList<AtomicOrderUnit>.Asks => asks;

        KeyedCollection<decimal, AtomicOrderUnit> IKeyedOrderUnitList<AtomicOrderUnit>.Bids => asks;
    }
}
