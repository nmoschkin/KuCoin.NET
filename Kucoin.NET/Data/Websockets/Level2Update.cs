using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Observable;
using Kucoin.NET.Websockets.Private;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.Websockets
{
    /// <summary>
    /// Level 2 New Sequence Changes
    /// </summary>
    public class Changes
    {
        /// <summary>
        /// Asks (from sellers)
        /// </summary>
        [JsonProperty("asks")]
        public List<OrderUnit> Asks { get; set; }

        /// <summary>
        /// Bids (from buyers)
        /// </summary>
        [JsonProperty("bids")]
        public List<OrderUnit> Bids { get; set; }
    }

    /// <summary>
    /// Level 2 Static Market Depth Feed (5/50) Changes
    /// </summary>
    public class StaticMarketDepthUpdate : ISymbol
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

        /// <summary>
        /// Gets the market depth of this update.
        /// </summary>
        public virtual Level2Depth Depth => Asks?.Length == 50 ? Level2Depth.Depth50 : Level2Depth.Depth5;

        void ISymbol.SetSymbol(string symbol)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// InternalTimestamp
        /// </summary>
        [JsonProperty("timestamp")]
        internal virtual long InternalTimestamp { get; set; }

        [JsonIgnore]
        public virtual DateTime Timestamp => EpochTime.MillisecondsToDate(InternalTimestamp);

    }


    /// <summary>
    /// Level 2 Static Market Depth Feed (5/50) Changes
    /// </summary>
    public class ObservableStaticMarketDepthUpdate : StaticMarketDepthUpdate, INotifyPropertyChanged, ISymbol
    {
        private string symbol = null;
        private long time = 0;

        private ObservableCollection<OrderUnit> observableAsks = null;
        private ObservableCollection<OrderUnit> observableBids = null;
        private OrderUnit[] asks = null;
        private OrderUnit[] bids = null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName]string propertyName = null)
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

            int c = (int)Depth, i;

            for (i = 0; i < c; i++)
            {
                if (!observableAsks[i].Equals(asks[i]))
                {
                    observableAsks[i].Price = asks[i].Price;
                    observableAsks[i].Size = asks[i].Size;
                    observableAsks[i].Sequence = asks[i].Sequence;
                }
                if (!observableBids[i].Equals(bids[i]))
                {
                    observableBids[i].Price = bids[i].Price;
                    observableBids[i].Size = bids[i].Size;
                    observableBids[i].Sequence = bids[i].Sequence;
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
        internal override long InternalTimestamp
        {
            get => time;
            set
            {
                if (SetProperty(ref time, value))
                {
                    OnPropertyChanged(nameof(Timestamp));
                }
            }
        }

        [JsonIgnore]
        public override DateTime Timestamp => EpochTime.MillisecondsToDate(time);

    }

    /// <summary>
    /// Level 2 Full Market Depth Data Update
    /// </summary>
    public class Level2Update : ISymbol
    {
        void ISymbol.SetSymbol(string symbol)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// Update sequence start
        /// </summary>
        [JsonProperty("sequenceStart")]
        public long SequenceStart { get; set; }

        /// <summary>
        /// Update sequence end
        /// </summary>
        [JsonProperty("sequenceEnd")]
        public long SequenceEnd { get; set; }

        /// <summary>
        /// The symbol for this update
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// The changes for this sequence
        /// </summary>
        [JsonProperty("changes")]
        public Changes Changes { get; set; }

    }
}
