using Kucoin.NET.Data.Interfaces;
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
    /// Represents Level 3 Market Data
    /// </summary>
    public class AtomicOrderBook : ObservableBase, IAtomicOrderBook
    {
        private DateTime time;
        private long seq;

        private ObservableAtomicOrderUnits asks = new ObservableAtomicOrderUnits();

        private ObservableAtomicOrderUnits bids = new ObservableAtomicOrderUnits(true);

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
        IList<IAtomicOrderUnit> IAtomicOrderUnitList.Asks => (IList<IAtomicOrderUnit>)asks;

        [JsonProperty("bids")]
        IList<IAtomicOrderUnit> IAtomicOrderUnitList.Bids => (IList<IAtomicOrderUnit>)bids;

        IList<IOrderUnit> IOrderUnitList.Asks => (IList<IOrderUnit>)bids;

        IList<IOrderUnit> IOrderUnitList.Bids => (IList<IOrderUnit>)bids;


        public ObservableAtomicOrderUnits Asks
        {
            get => asks;
            set
            {
                SetProperty(ref asks, value);
            }
        }

        public ObservableAtomicOrderUnits Bids
        {
            get => bids;
            set
            {
                SetProperty(ref bids, value);
            }
        }



    }
}
