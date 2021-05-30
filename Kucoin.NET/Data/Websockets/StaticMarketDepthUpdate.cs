using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kucoin.NET.Data.Websockets
{

    /// <summary>
    /// The depth of the market for the feed.
    /// </summary>
    public enum Level2Depth
    {
        /// <summary>
        /// 5 Best Asks/Bids
        /// </summary>
        Depth5 = 5,

        /// <summary>
        /// 50 Best Asks/Bids
        /// </summary>
        Depth50 = 50
    }

    /// <summary>
    /// Level 2 Static Market Depth Feed (5/50) Changes
    /// </summary>
    public class StaticMarketDepthUpdate : ISymbol, IOrderUnitList<IOrderUnit>
    {
        public virtual string Symbol { get; set; }

        /// <summary>
        /// Asks (from sellers)
        /// </summary>
        [JsonProperty("asks")]
        public OrderUnit[] Asks { get; set; }

        /// <summary>
        /// Bids (from buyers)
        /// </summary>
        [JsonProperty("bids")]
        public OrderUnit[] Bids { get; set; }

        IList<IOrderUnit> IOrderUnitList<IOrderUnit>.Asks => Asks;

        IList<IOrderUnit> IOrderUnitList<IOrderUnit>.Bids => Bids;


        /// <summary>
        /// Gets the market depth of this update.
        /// </summary>
        public virtual Level2Depth Depth => Asks?.Length == 50 ? Level2Depth.Depth50 : Level2Depth.Depth5;

        void ISymbol.SetSymbol(string symbol)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// Time Stamp
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public virtual DateTime Timestamp { get; set; }

    }


    /// <summary>
    /// Level 2 Static Market Depth Feed (5/50) Changes
    /// </summary>
    public class ObservableStaticMarketDepthUpdate : StaticMarketDepthUpdate, IOrderUnitList<IOrderUnit>, INotifyPropertyChanged, ISymbol
    {
        private string symbol = null;
        private DateTime time;

        private ObservableCollection<OrderUnit> observableAsks = null;
        private ObservableCollection<OrderUnit> observableBids = null;
        private OrderUnit[] asks = null;
        private OrderUnit[] bids = null;

        public event PropertyChangedEventHandler PropertyChanged;

        IList<IOrderUnit> IOrderUnitList<IOrderUnit>.Asks => asks;

        IList<IOrderUnit> IOrderUnitList<IOrderUnit>.Bids => bids;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            if (backingStore == null && value == null)
            {
                return false;
            }
            else if (backingStore == null || value == null)
            {
                backingStore = value;
                OnPropertyChanged(propertyName);

                return true;
            }
            else
            {
                if (backingStore.Equals(value)) return false;

                backingStore = value;
                OnPropertyChanged(propertyName);

                return true;
            }
        }

        public override string Symbol
        {
            get => symbol;
            set
            {
                SetProperty(ref symbol, value);
            }
        }

        /// <summary>
        /// Asks (from sellers)
        /// </summary>
        [JsonProperty("asks")]
        internal OrderUnit[] InternalAsks
        {
            get => asks;
            set
            {
                SetProperty(ref asks, value);
            }
        }

        /// <summary>
        /// Bids (from buyers)
        /// </summary>
        [JsonProperty("bids")]
        internal OrderUnit[] InternalBids
        {
            get => bids;
            set
            {
                SetProperty(ref bids, value);
            }
        }
        public override Level2Depth Depth => asks?.Length == 50 ? Level2Depth.Depth50 : Level2Depth.Depth5;

        internal void UpdateObservable()
        {
            if (observableAsks == null || observableBids == null)
            {
                Asks = new ObservableCollection<OrderUnit>(asks);
                Bids = new ObservableCollection<OrderUnit>(bids);

                return;
            }

            int c = Math.Max(observableAsks.Count, asks.Length);
            int i;

            for (i = 0; i < c; i++)
            {
                if (i > asks.Length - 1)
                {
                    observableAsks.RemoveAt(asks.Length);
                    continue;
                }

                if (i > observableAsks.Count - 1)
                {
                    observableAsks.Add(asks[i]);
                }
                else
                {

                    if (!observableAsks[i].Equals(asks[i]))
                    {
                        observableAsks[i].Price = asks[i].Price;
                        observableAsks[i].Size = asks[i].Size;
                        observableAsks[i].Sequence = asks[i].Sequence;
                    }

                }
            }

            c = Math.Max(observableBids.Count, bids.Length);

            for (i = 0; i < c; i++)
            {
                if (i > bids.Length - 1)
                {
                    observableBids.RemoveAt(bids.Length);
                    continue;
                }

                if (i > observableBids.Count - 1)
                {
                    observableBids.Add(bids[i]);
                }
                else
                {
                    if (!observableBids[i].Equals(bids[i]))
                    {
                        observableBids[i].Price = bids[i].Price;
                        observableBids[i].Size = bids[i].Size;
                        observableBids[i].Sequence = bids[i].Sequence;
                    }
                }
            }

        }

        [JsonIgnore]
        public new ObservableCollection<OrderUnit> Asks
        {
            get => observableAsks;
            set
            {
                SetProperty(ref observableAsks, value);
            }
        }

        [JsonIgnore]
        public new ObservableCollection<OrderUnit> Bids
        {
            get => observableBids;
            set
            {
                SetProperty(ref observableBids, value);
            }
        }

        [JsonProperty("timestamp")]
        public override DateTime Timestamp
        {
            get => time;
            set
            {
                SetProperty(ref time, value);
            }
        }
    }
}
