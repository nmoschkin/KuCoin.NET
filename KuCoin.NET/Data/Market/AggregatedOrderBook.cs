using KuCoin.NET.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using KuCoin.NET.Rest;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.Market
{

    /// <summary>
    /// Provides the standard, observable order book implementation.
    /// </summary>
    /// <typeparam name="T">The type of the order unit.</typeparam>
    public class AggregatedOrderBook<T> : IKeyedOrderBook<OrderUnitKeyedCollection<T>, T> where T: IOrderUnit, new()
    {
        /// <summary>
        /// The sequence number of the order
        /// </summary>
        [JsonProperty("sequence")]
        [KeyProperty]
        public long Sequence { get; set; }

        /// <summary>
        /// Asks (sell)
        /// </summary>
        [JsonProperty("asks")]
        public OrderUnitKeyedCollection<T> Asks { get; set; } = new OrderUnitKeyedCollection<T>();

        /// <summary>
        /// Bids (buy)
        /// </summary>
        [JsonProperty("bids")]
        public OrderUnitKeyedCollection<T> Bids { get; set; } = new OrderUnitKeyedCollection<T>(true);

        /// <summary>
        /// The time stamp of the order
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public virtual DateTime Timestamp { get; set; }

        ICollection<T> IOrderUnitCollection<T>.Asks => Asks;

        ICollection<T> IOrderUnitCollection<T>.Bids => Bids;

        ICollection<T> IDataSeries<T, T, ICollection<T>, ICollection<T>>.Data1 => Asks;

        ICollection<T> IDataSeries<T, T, ICollection<T>, ICollection<T>>.Data2 => Bids;
    }

    /// <summary>
    /// Provides the standard, observable order book implementation.
    /// </summary>
    /// <typeparam name="T">The type of the order unit.</typeparam>
    public class ObservableOrderBook<T> : 
        ObservableBase, 
        IOrderBook<T> 
        where T : IOrderUnit
    {
        protected long seq;
        protected DateTime time;
        protected ObservableCollection<T> asks = new ObservableCollection<T>();
        protected ObservableCollection<T> bids = new ObservableCollection<T>();

        /// <summary>
        /// The sequence number of the order
        /// </summary>
        [JsonProperty("sequence")]
        [KeyProperty]
        public long Sequence
        {
            get => seq;
            set
            {
                SetProperty(ref seq, value);
            }
        }

        /// <summary>
        /// Asks (sell)
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
        /// Bids (buy)
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

        /// <summary>
        /// The time stamp of the order
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public virtual DateTime Timestamp
        {
            get => time;
            set
            {
                SetProperty(ref time, value);
            }
        }

        ICollection<T> IOrderUnitCollection<T>.Asks => Asks;

        ICollection<T> IOrderUnitCollection<T>.Bids => Bids;

        ICollection<T> IDataSeries<T, T, ICollection<T>, ICollection<T>>.Data1 => Asks;

        ICollection<T> IDataSeries<T, T, ICollection<T>, ICollection<T>>.Data2 => Bids;
    }

}
