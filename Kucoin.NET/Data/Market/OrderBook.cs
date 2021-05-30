using Kucoin.NET.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;
using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Json;
using Kucoin.NET.Data.Order;

namespace Kucoin.NET.Data.Market
{

    public class OrderBook<T> : ObservableBase, IOrderBook<T> where T: IOrderUnit 
    {
        private ObservableOrderUnits<T> asks = new ObservableOrderUnits<T>();

        private ObservableOrderUnits<T> bids = new ObservableOrderUnits<T>(true);

        private long sequence;

        [JsonProperty("sequence")]
        [KeyProperty]
        public long Sequence
        {
            get => sequence;
            set
            {
                SetProperty(ref sequence, value);
            }
        }

        SortedKeyedOrderUnitBase<T> IKeyedOrderUnitList<T>.Asks => asks;

        SortedKeyedOrderUnitBase<T> IKeyedOrderUnitList<T>.Bids => bids;

        [JsonProperty("asks")]
        public ObservableOrderUnits<T> Asks
        {
            get => asks;
            set
            {
                SetProperty(ref asks, value);
            }
        }

        [JsonProperty("bids")]
        public ObservableOrderUnits<T> Bids
        {
            get => bids;
            set
            {
                SetProperty(ref bids, value);
            }
        }

        private DateTime time;

        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public virtual DateTime Timestamp 
        {
            get => time;
            set
            {
                SetProperty(ref time, value);
            }
        }

    }


}
