using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Kucoin.NET.Websockets.Distribution;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Observations
{
    public class Level3Observation : MarketObservation<KeyedAtomicOrderBook<AtomicOrderStruct>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>, Level3Update>, IFeedDiagnostics, IMarketVolume
    {
        new private Level3 parent;

        long matchTime = DateTime.UtcNow.Ticks;

        long transactSec = 0;

        long matchSec = 0;

        IInitialDataProvider<string, KeyedAtomicOrderBook<AtomicOrderStruct>> dataProvider;

        private KeyedAtomicOrderBook<AtomicOrderStruct> fullDepth;

        private ObservableAtomicOrderBook<ObservableAtomicOrderUnit> orderBook;

        private bool disabled;

        private bool updVol;

        private bool diagEnable;

        private int interval = 100;

        private int marketDepth = 50;

        private bool calibrated;

        private bool initialized;

        private int resets = 0;

        private int maxResets = 3;

        private int resetTimeout = 10000;

        private decimal marketVolume;

        private bool initializing;
        private bool calibrating;

        public Level3Observation(Level3 parent, string symbol) : base(parent, symbol)
        {
            this.parent = parent;
            this.dataProvider = parent;
            this.IsObservationDisabled = true;
        }

        public override bool IsObservationDisabled
        {
            get => disabled;
            set
            {
                SetProperty(ref disabled, true);    
            }
        }

        /// <summary>
        /// Gets the full depth order book.
        /// </summary>
        public KeyedAtomicOrderBook<AtomicOrderStruct> FullDepthOrderBook
        {
            get => InternalData;
            protected set => InternalData = value;    
        }

        /// <summary>
        /// Get the best-ask/bid observable order book.
        /// </summary>
        public ObservableAtomicOrderBook<ObservableAtomicOrderUnit> OrderBook
        {
            get => ObservableData;
            protected set =>  ObservableData = value;
        }

        public override KeyedAtomicOrderBook<AtomicOrderStruct> InternalData
        {
            get => fullDepth;
            protected set
            {
                SetProperty(ref fullDepth, value);
            }
        }

        public override ObservableAtomicOrderBook<ObservableAtomicOrderUnit> ObservableData
        {
            get => orderBook;   
            protected set
            {
                SetProperty(ref orderBook, value);      
            }
        }

        public override bool PreferDispatcher => true;

        public override int Interval
        {
            get => interval;
            set
            {
                SetProperty(ref interval, value);
            }
        }

        public override int MarketDepth
        {
            get => marketDepth;
            set
            {
                SetProperty(ref marketDepth, value);
            }
        }

        public decimal MarketVolume
        {
            get => marketVolume;
            set
            {
                SetProperty(ref marketVolume, value);
            }
        }

        public bool IsVolumeEnabled
        {
            get => updVol;
            set
            {
                SetProperty(ref updVol, value);
            }
        }

        public bool DiagnosticsEnabled
        {
            get => diagEnable;
            set
            {
                SetProperty(ref diagEnable, value);
            }
        }

        public long GrandTotal { get; protected set; }

        public long MatchesPerSecond { get; protected set; }

        public long MatchTotal {  get; protected set; }

        public long TransactionsPerSecond { get; protected set; }

        public int QueueLength => buffer?.Count ?? 0;

        public override IInitialDataProvider<string, KeyedAtomicOrderBook<AtomicOrderStruct>> DataProvider
        {
            get => dataProvider;
            protected set
            {
                SetProperty(ref dataProvider, value);
            }
        }

        public override bool IsDataProviderAvailable { get; protected set; } = true;

        /// <summary>
        /// Gets a value indicating that this order book is initialized with the full-depth (preflight) order book.
        /// </summary>
        public override bool IsInitialized
        {
            get => !disposedValue ? initialized : throw new ObjectDisposedException(GetType().FullName);
            protected set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                if (SetProperty(ref initialized, value))
                {
                    //if (value) State = FeedState.Running;
                    //else State = FeedState.Initializing;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating that this observation has been calibrated.
        /// </summary>
        public override bool IsCalibrated
        {
            get => !disposedValue ? calibrated : throw new ObjectDisposedException(GetType().FullName);
            protected set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                if (SetProperty(ref calibrated, value))
                {
                    //if (value) State = FeedState.Running;
                    //else State = FeedState.Initializing;
                }
            }
        }
        public override int ResetCount 
        {
            get => resets;
            protected set
            {
                SetProperty(ref resets, value);
            }
        }

        public override int ResetTimeout
        {
            get => resetTimeout;
            set
            {
                SetProperty(ref resetTimeout, value);
            }
        }
        public override int MaxTimeoutRetries
        {
            get => maxResets;
            set
            {
                SetProperty(ref maxResets, value);  
            }
        }

        public override void Calibrate()
        {
            lock (buffer)
            {
                calibrated = true;
                calibrating = true;

                foreach (var q in buffer)
                {
                    if (q.Sequence > fullDepth.Sequence) ProcessObject(q);
                }

                calibrating = false;
                buffer.Clear();
            }
        }

        protected override bool DoWork()
        {
            if (initializing) return false;
            return base.DoWork();
        }

        public override void CopyToObservable()
        {
            lock (lockObj)
            {
                if (orderBook == null)
                {
                    orderBook = new ObservableAtomicOrderBook<ObservableAtomicOrderUnit>();
                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = fullDepth.Timestamp;
                }

                int marketDepth = this.marketDepth;

                if (fullDepth.Asks.Count < marketDepth) marketDepth = fullDepth.Asks.Count;

                if (orderBook.Asks.Count != marketDepth)
                {
                    orderBook.Asks.Clear();
                    for (int i = 0; i < marketDepth; i++)
                    {
                        orderBook.Asks.Add(fullDepth.Asks[i].Clone<ObservableAtomicOrderUnit>());
                    }
                }
                else
                {
                    for (int i = 0; i < marketDepth; i++)
                    {
                        if (orderBook.Asks[i].OrderId == fullDepth.Asks[i].OrderId)
                        {
                            orderBook.Asks[i].Price = fullDepth.Asks[i].Price;
                            orderBook.Asks[i].Size = fullDepth.Asks[i].Size;
                            orderBook.Asks[i].Timestamp = fullDepth.Asks[i].Timestamp;
                        }
                        else
                        {
                            orderBook.Asks[i] = fullDepth.Asks[i].Clone<ObservableAtomicOrderUnit>();
                        }
                    }
                }

                if (fullDepth.Bids.Count < marketDepth) marketDepth = fullDepth.Bids.Count;

                if (orderBook.Bids.Count != marketDepth)
                {
                    orderBook.Bids.Clear();
                    for (int i = 0; i < marketDepth; i++)
                    {
                        orderBook.Bids.Add(fullDepth.Bids[i].Clone<ObservableAtomicOrderUnit>());
                    }
                }
                else
                {
                    for (int i = 0; i < marketDepth; i++)
                    {
                        if (orderBook.Bids[i].OrderId == fullDepth.Bids[i].OrderId)
                        {
                            orderBook.Bids[i].Price = fullDepth.Bids[i].Price;
                            orderBook.Bids[i].Size = fullDepth.Bids[i].Size;
                            orderBook.Bids[i].Timestamp = fullDepth.Bids[i].Timestamp;
                        }
                        else
                        {
                            orderBook.Bids[i] = fullDepth.Bids[i].Clone<ObservableAtomicOrderUnit>();
                        }
                    }
                }

            }

        }

        public override void Initialize()
        {
            lock (lockObj)
            {
                IsInitialized = false;
                IsCalibrated = false;
                State = FeedState.Initializing;

                FullDepthOrderBook = DataProvider.ProvideInitialData(key).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            IsInitialized = true;
        }

        public override void ProcessObject(Level3Update obj)
        {

            if (disposedValue) return;

            if (!calibrated && !calibrating)
            {
                if (initializing) return;
                initializing = true;
                State = FeedState.Initializing;

                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();

                Initialize();
                Calibrate();

                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();

                //DataProvider.ProvideInitialData(key).ContinueWith((t) =>
                //{
                //    lock (buffer)
                //    {
                //        FullDepthOrderBook = t.Result;
                //        IsInitialized = true;

                //        Calibrate();
                //        calibrating = false;
                //    }

                //});

                calibrating = false;
                initializing = false;

                IsInitialized = true;
                IsCalibrated = true;

                State = FeedState.Running;

                return;
            }
            else if (!calibrating && obj.Sequence <= fullDepth.Sequence)
            {
                return;
            }
            else if (fullDepth == null)
            {
                return;
            }

            if (diagEnable)
            {
                var tt = DateTime.UtcNow.Ticks;
                if (tt - matchTime >= 10_000_000)
                {
                    matchTime = tt;

                    TransactionsPerSecond = transactSec;
                    MatchesPerSecond = matchSec;
                    transactSec = 0;
                    matchSec = 0;
                }

                GrandTotal++;
                transactSec++;

                if (obj.Subject == "match")
                {
                    MatchTotal++;
                    matchSec++;
                }
            }

            if (!calibrating && obj.Sequence - fullDepth.Sequence > 1)
            {
                if (obj.Sequence <= fullDepth.Sequence) return;

                Reset();
                return;
            }
            
            if (calibrating && obj.Sequence - fullDepth.Sequence == 1)
            {
                IsInitialized = true;
                IsCalibrated = true;
                calibrating = false;
                State = FeedState.Running;
            }

            if (obj.Side == null)
            {
                if (obj.Subject == "done")
                {
                    if (fullDepth.Asks.ContainsKey(obj.OrderId))
                    {
                        fullDepth.Asks.Remove(obj.OrderId);
                    }

                    if (fullDepth.Bids.ContainsKey(obj.OrderId))
                    {
                        fullDepth.Bids.Remove(obj.OrderId);
                    }
                }
            }
            else if (obj.Side == Side.Sell)
            {
                SequencePieces(obj.Subject, obj, fullDepth.Asks, fullDepth.Bids);
            }
            else if (obj.Side == Side.Buy)
            {
                SequencePieces(obj.Subject, obj, fullDepth.Bids, fullDepth.Asks);
            }

            fullDepth.Sequence = obj.Sequence;
            fullDepth.Timestamp = obj.Timestamp ?? DateTime.Now;

        }

        public override void Reset()
        {
            lock (lockObj)
            {
                buffer = new List<Level3Update>();

                IsInitialized = false;
                IsCalibrated = false;
            }
        }

        public override void SetInitialDataProvider(IInitialDataProvider<string, KeyedAtomicOrderBook<AtomicOrderStruct>> dataProvider)
        {
            DataProvider = dataProvider;
        }
                
        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="changes">The changes to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SequencePieces(string subj, Level3Update change, KeyedBook<AtomicOrderStruct> pieces, KeyedBook<AtomicOrderStruct> otherPieces)
        {
            switch (subj)
            {
                case "done":

                    pieces.Remove(change.OrderId);
                    return;

                case "open":

                    if (change.Price == null || change.Price == 0 || change.Size == null || change.Size == 0) return;

                    var u = new AtomicOrderStruct
                    {
                        Price = change.Price ?? 0,
                        Size = change.Size ?? 0,
                        Timestamp = change.Timestamp ?? DateTime.Now,
                        OrderId = change.OrderId
                    };

                    if (pieces.ContainsKey(u.OrderId))
                    {
                        Reset();
                        return;
                    }

                    pieces.Add(u);

                    return;

                case "change":

                    if (pieces.TryGetValue(change.OrderId, out AtomicOrderStruct piece))
                    {
                        pieces.Remove(piece.OrderId);

                        piece.Size = change.Size ?? 0;
                        piece.Timestamp = change.Timestamp ?? DateTime.Now;
                        piece.Price = change.Price ?? 0;

                        pieces.Add(piece);
                    }
                    else
                    {
                        Reset();
                        return;

                        //var u2 = new AtomicOrderStruct
                        //{
                        //    Price = change.Price ?? 0,
                        //    Size = change.Size ?? 0,
                        //    Timestamp = change.Timestamp ?? DateTime.Now,
                        //    OrderId = change.OrderId
                        //};

                        //pieces.Add(u2);
                    }

                    return;

                case "match":

                    if (change.Price is decimal p && change.Size is decimal csize
                        && otherPieces.TryGetValue(change.MakerOrderId, out AtomicOrderStruct o))
                    {
                        o.Size -= csize;

                        otherPieces.Remove(o.OrderId);
                        otherPieces.Add(o);

                        // A match is a real component of volume.
                        // we can keep our own tally of the market volume per k-line.
                        if (updVol) marketVolume += (csize * p);
                    }

                    return;

            }
        }

    }
}
