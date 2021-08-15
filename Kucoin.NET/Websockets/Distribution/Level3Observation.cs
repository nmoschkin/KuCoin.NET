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
using System.Diagnostics.Contracts;
using System.Threading;

namespace Kucoin.NET.Websockets.Observations
{
    public class Level3Observation : MarketObservation<KeyedAtomicOrderBook<AtomicOrderStruct>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>, Level3Update>, IFeedDiagnostics, IMarketVolume
    {
        new private Level3 parent;

        private long matchTime = DateTime.UtcNow.Ticks;

        private long transactSec = 0;

        private long matchSec = 0;

        private IInitialDataProvider<string, KeyedAtomicOrderBook<AtomicOrderStruct>> dataProvider;

        private DateTime? lastFailureTime = null;

        private KeyedAtomicOrderBook<AtomicOrderStruct> fullDepth;

        private ObservableAtomicOrderBook<ObservableAtomicOrderUnit> orderBook;

        private List<Level3Update> initBuffer = new List<Level3Update>();

        private bool disabled;

        private bool updVol;

        private bool diagEnable;

        private bool failure;

        private int interval = 100;

        private int marketDepth = 50;

        private bool calibrated;

        private bool initialized;

        private int resets = 0;

        private int maxResets = 3;

        private int resetTimeout = 30000;

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
            get => fullDepth;
            protected set
            {
                if (SetProperty(ref fullDepth, value))
                {
                    OnPropertyChanged(nameof(InternalData));
                }
            }
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
                if (SetProperty(ref fullDepth, value))
                {
                    OnPropertyChanged(nameof(FullDepthOrderBook));
                }
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

        public override bool Failure
        {
            get => failure;
            protected set
            {
                if (SetProperty(ref failure, value))
                {
                    if (value)
                    {
                        state = FeedState.Failed;
                        lastFailureTime = DateTime.UtcNow;
                    }

                    OnPropertyChanged(nameof(State));
                    OnPropertyChanged(nameof(TimeUntilNextRetry));
                    OnPropertyChanged(nameof(LastFailureTime));
                }
            }
        }

        /// <summary>
        /// The time of the last failure, or null if the feed is running normally.
        /// </summary>
        public DateTime? LastFailureTime
        {
            get => lastFailureTime;
            protected set
            {
                if (SetProperty(ref lastFailureTime, value))
                {
                    OnPropertyChanged(nameof(TimeUntilNextRetry));
                }
            }
        }

        /// <summary>
        /// The number of milliseconds remaining until the next retry, or null if the feed is running normally.
        /// </summary>
        public double? TimeUntilNextRetry
        {
            get
            {
                if (lastFailureTime is DateTime t)
                {
                    return (resetTimeout - (DateTime.UtcNow - t).TotalMilliseconds);
                }
                else
                {
                    return null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override async Task Calibrate()
        {
            await Task.Run(() =>
            {
                lock (lockObj)
                {
                    if (failure || fullDepth == null) return;
                    calibrating = true;

                    lock (initBuffer)
                    {
                        foreach (var q in initBuffer)
                        {
                            if (q.Sequence > fullDepth.Sequence) ProcessObject(q);
                        }

                        initBuffer.Clear();
                    }

                    calibrating = false;
                    State = FeedState.Running;

                    IsCalibrated = true;
                    ResetCount++;
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override async Task Initialize()
        {
            lock(lockObj)
            {
                IsInitialized = false;
                IsCalibrated = false;

                initializing = true;
                calibrating = false;

                State = FeedState.Initializing;
            }

            var fd = await DataProvider.ProvideInitialData(key);

            lock (lockObj)
            {
                initializing = false;
                IsInitialized = fd != null;

                if (!initialized)
                {
                    lastFailureTime = DateTime.Now;
                    Failure = true;
                }

                FullDepthOrderBook = fd;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override async Task Reset()
        {
            await Task.Run(() =>
            {
                lock (lockObj)
                {
                    initialized = calibrated = failure = false;
                    lastFailureTime = null;

                    _ = Task.Run(() =>
                    {
                        OnPropertyChanged(nameof(IsInitialized));
                        OnPropertyChanged(nameof(IsCalibrated));
                        OnPropertyChanged(nameof(Failure));
                        OnPropertyChanged(nameof(TimeUntilNextRetry));
                        OnPropertyChanged(nameof(LastFailureTime));
                    });

                    FullDepthOrderBook = null;

                    initBuffer.AddRange(processBuffer);
                    initBuffer = new List<Level3Update>(buffer);

                    buffer = new List<Level3Update>();
                    processBuffer = new Level3Update[0];

                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool DoWork()
        {
            int i, c;

            lock (lockObj)
            {
                c = buffer.Count;
                if (c == 0) return false;

                if (processBuffer == null)
                {
                    processBuffer = new Level3Update[c * 2];
                }
                if (processBuffer.Length < c)
                {
                    Array.Resize(ref processBuffer, c * 2);
                }

                buffer.CopyTo(processBuffer, 0);
                buffer.Clear();

                for (i = 0; i < c; i++)
                {
                    if (!ProcessObject(processBuffer[i]))
                    {
                        return i > 0;
                    }
                }

                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ProcessObject(Level3Update obj)
        {
            if (disposedValue) return false;

            lock (lockObj)
            {
                if (failure)
                {
                    if (lastFailureTime != null && lastFailureTime is DateTime t)
                    {
                        if ((DateTime.UtcNow - t).TotalMilliseconds > resetTimeout)
                        {
                            LastFailureTime = null;
                            _ = Reset();
                            return false;
                        }
                    }
                    else
                    {
                        OnPropertyChanged(nameof(TimeUntilNextRetry));
                    }

                    return false;
                }

                if (!calibrated && !calibrating)
                {
                    lock (initBuffer)
                    {
                        initBuffer.Add(obj);

                        if (initializing) return true;
                        initializing = true;
                    }

                    Initialize().ContinueWith(async (t) =>
                    {
                        if (failure) return;

                        //Task.Delay(100).ContinueWith(async (t2) =>
                        //{
                            if (!failure)
                            {
                                await Calibrate();
                            }
                        //});
                    
                    });

                    return true;
                }
                else if (fullDepth == null)
                {
                    if (!failure) Failure = true;
                    return false;
                }
                else if (obj.Sequence <= fullDepth.Sequence)
                {
                    return false;
                }
                else if (calibrated && obj.Sequence - fullDepth.Sequence > 1)
                {
                    _ = Reset();
                    return false;
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

                return true;
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
        protected bool SequencePieces(string subj, Level3Update change, KeyedBook<AtomicOrderStruct> pieces, KeyedBook<AtomicOrderStruct> otherPieces)
        {
            switch (subj)
            {
                case "done":

                    pieces.Remove(change.OrderId);
                    return true;

                case "open":

                    if (change.Price == null || change.Price == 0 || change.Size == null || change.Size == 0) return true;

                    var u = new AtomicOrderStruct
                    {
                        Price = change.Price ?? 0,
                        Size = change.Size ?? 0,
                        Timestamp = change.Timestamp ?? DateTime.Now,
                        OrderId = change.OrderId
                    };

                    if (pieces.ContainsKey(u.OrderId))
                    {
                        return false;
                        //pieces.Remove(u.OrderId);
                    }

                    pieces.Add(u);

                    return true;

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
                        //Reset();
                        return false;

                        //var u2 = new AtomicOrderStruct
                        //{
                        //    Price = change.Price ?? 0,
                        //    Size = change.Size ?? 0,
                        //    Timestamp = change.Timestamp ?? DateTime.Now,
                        //    OrderId = change.OrderId
                        //};

                        //pieces.Add(u2);
                    }

                    return true;

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

                    return true;

            }

            return false;
        }

    }
}
