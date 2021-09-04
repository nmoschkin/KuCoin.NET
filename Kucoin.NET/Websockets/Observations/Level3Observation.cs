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
using System.Linq;

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

        private KlineType klineType = KlineType.Min1;

        private DateTime klineTime = KlineType.Min1.GetCurrentKlineStartTime();

        private Candle candle = new Candle() { Type = KlineType.Min1, Timestamp = KlineType.Min1.GetCurrentKlineStartTime() };

        private KlineType sortingKlineType = KlineType.Min1;

        private DateTime sortingKlineTime = KlineType.Min1.GetCurrentKlineStartTime();

        private Candle sortingCandle = new Candle() { Type = KlineType.Min1, Timestamp = KlineType.Min1.GetCurrentKlineStartTime() };

        private List<Candle> lastCandles = new List<Candle>();

        private bool disabled = true;

        private bool updVol;

        private bool diagEnable;

        private bool failure;

        private int interval = 100;

        private int marketDepth = 50;

        private bool initialized;

        private int resets = 0;

        private int maxResets = 3;

        private int resetTimeout = 30000;

        private decimal marketVolume;

        private decimal sortingVolume;
        
        private bool initializing;

        public override event EventHandler Initialized;

        public Level3Observation(Level3 parent, string symbol) : base(parent, symbol)
        {
            this.parent = parent;
            this.dataProvider = parent;
            this.IsObservationDisabled = true;
        }

        public Candle Candle
        {
            get => candle;
            set
            {
                SetProperty(ref candle, value);
            }
        }


        public Candle SortingCandle
        {
            get => sortingCandle;
            set
            {
                SetProperty(ref sortingCandle, value);
            }
        }

        public List<Candle> LastCandles
        {
            get => lastCandles;
            set
            {
                SetProperty(ref lastCandles, value);
            }
        }

        public Candle LastCandle
        {
            get
            {
                if (lastCandles == null) return null;
                else return lastCandles.LastOrDefault();
            }
        }

        public KlineType KlineType
        {
            get => klineType;
            set
            {
                if (SetProperty(ref klineType, value))
                {
                    KlineTime = klineType.GetCurrentKlineStartTime();
                    Candle = new Candle() { Type = klineType, Timestamp = sortingKlineTime };
                }
            }
        }

        public KlineType SortingKlineType
        {
            get => sortingKlineType;
            set
            {
                if (SetProperty(ref sortingKlineType, value))
                {
                    sortingKlineTime = klineType.GetCurrentKlineStartTime();
                    SortingCandle = new Candle() { Type = klineType, Timestamp = sortingKlineTime };
                }
            }
        }


        public DateTime SortingKlineTime
        {
            get => sortingKlineTime;
            set
            {
                SetProperty(ref sortingKlineTime, value);
            }
        }

        public DateTime KlineTime
        {
            get => klineTime;
            set
            {
                SetProperty(ref klineTime, value);  
            }
        }

        public override bool IsObservationDisabled
        {
            get => disabled;
            set
            {
                SetProperty(ref disabled, value);    
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
            get => orderBook;
            protected set
            {
                if (SetProperty(ref orderBook, value))
                {
                    OnPropertyChanged(nameof(ObservableData));
                }
            }
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
                if (SetProperty(ref orderBook, value))
                {
                    OnPropertyChanged(nameof(OrderBook));
                }
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

        public decimal SortingVolume
        {
            get => vt?.Volume ?? (sortingVolume == 0M ? marketVolume : sortingVolume);
            set
            {
                SetProperty(ref sortingVolume, value);  
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
                if (fullDepth == null) return;
                if (this.marketDepth <= 0) return;

                var asks = fullDepth.Asks as IList<AtomicOrderStruct>;
                var bids = fullDepth.Asks as IList<AtomicOrderStruct>;

                if (orderBook == null)
                {
                    OrderBook = new ObservableAtomicOrderBook<ObservableAtomicOrderUnit>();
                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = fullDepth.Timestamp;
                }

                int marketDepth = this.marketDepth;

                if (asks.Count < marketDepth) marketDepth = asks.Count;

                if (orderBook.Asks.Count != marketDepth)
                {
                    orderBook.Asks.Clear();
                    for (int i = 0; i < marketDepth; i++)
                    {
                        orderBook.Asks.Add(asks[i].Clone<ObservableAtomicOrderUnit>());
                    }
                }
                else
                {
                    for (int i = 0; i < marketDepth; i++)
                    {
                        if (orderBook.Asks[i].Price == asks[i].Price)
                        {
                            orderBook.Asks[i].Price = asks[i].Price;
                            orderBook.Asks[i].Size = asks[i].Size;
                        }
                        else
                        {
                            orderBook.Asks[i] = asks[i].Clone<ObservableAtomicOrderUnit>();
                        }
                    }
                }

                if (bids.Count < marketDepth) marketDepth = bids.Count;

                if (orderBook.Bids.Count != marketDepth)
                {
                    orderBook.Bids.Clear();
                    for (int i = 0; i < marketDepth; i++)
                    {
                        orderBook.Bids.Add(bids[i].Clone<ObservableAtomicOrderUnit>());
                    }
                }
                else
                {
                    for (int i = 0; i < marketDepth; i++)
                    {
                        if (orderBook.Bids[i].Price == bids[i].Price)
                        {
                            orderBook.Bids[i].Price = bids[i].Price;
                            orderBook.Bids[i].Size = bids[i].Size;
                        }
                        else
                        {
                            orderBook.Bids[i] = bids[i].Clone<ObservableAtomicOrderUnit>();
                        }
                    }
                }

            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override async Task<bool> Initialize()
        {
            lock(lockObj)
            {
                IsInitialized = false;
                initializing = true;

                State = FeedState.Initializing;
            }

            KeyedAtomicOrderBook<AtomicOrderStruct> fd = null;
            int maxtries = 3;

            for (int tries = 0; tries < maxtries; tries++)
            {
                await Task.Delay(100);
                fd = await DataProvider.ProvideInitialData(key);
                if (fd != null) break;
            }

            lock (lockObj)
            {
                if (fd == null)
                {
                    State = FeedState.Failed;
                    LastFailureTime = DateTime.Now;
                    Failure = true;
                    OnPropertyChanged(nameof(TimeUntilNextRetry));
                }

                FullDepthOrderBook = fd;

                initializing = false;
                IsInitialized = fd != null;

                return IsInitialized;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override async Task Reset()
        {
            await Task.Run(() =>
            {
                lock (lockObj)
                {
                    if (initializing) return;

                    initialized = failure = false;
                    LastFailureTime = null;

                    _ = Task.Run(() =>
                    {
                        OnPropertyChanged(nameof(IsInitialized));
                        OnPropertyChanged(nameof(Failure));
                        OnPropertyChanged(nameof(TimeUntilNextRetry));
                        OnPropertyChanged(nameof(LastFailureTime));
                    });

                    FullDepthOrderBook = null;
                    buffer.Clear();
                }
            });
        }
        
        CancellationTokenSource cts;
        DateTime? startFetch;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void DoWork()
        {
            lock (lockObj)
            {
                if (!initialized || failure)
                {
                    if (failure)
                    {
                        buffer.Clear();

                        if (lastFailureTime is DateTime t && (DateTime.UtcNow - t).TotalMilliseconds >= resetTimeout)
                        {
                            initializing = false;
                            LastFailureTime = null;
                            Failure = false;
                            State = FeedState.Initializing;

                            _ = Reset();
                        }
                        else
                        {
                            OnPropertyChanged(nameof(TimeUntilNextRetry));
                        }

                        return;
                    }

                    if (!initializing)
                    {
                        initializing = true;
                        startFetch = DateTime.Now;
                        cts = new CancellationTokenSource();

                        Initialize().ContinueWith((t) =>
                        {
                            if (t.Result)
                            {
                                State = FeedState.Running;
                            }

                            cts = null;
                            startFetch = null;
                        }, cts.Token);
                    }
                    else if (startFetch != null && (DateTime.Now - (DateTime)startFetch).TotalMilliseconds >= (resetTimeout * 2))
                    {
                        cts?.Cancel();
                        startFetch = null;
                        cts = null;
                        State = FeedState.Failed;
                        LastFailureTime = DateTime.Now;
                        Failure = true;
                        OnPropertyChanged(nameof(TimeUntilNextRetry));

                        return;
                    }

                    return;
                }

                foreach (var obj in buffer)
                {
                    ProcessObject(obj);
                }

                buffer.Clear();
                return;
            }
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ProcessObject(Level3Update obj)
        {
            if (disposedValue) return false;

            lock (lockObj)
            {
                if (fullDepth == null)
                {
                    if (!failure) Failure = true;
                    return false;
                }
                else if (obj.Sequence <= fullDepth.Sequence)
                {
                    return false;
                }
                else if (obj.Sequence - fullDepth.Sequence > 1)
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

                    if (obj.Subject[0] == 'm')
                    {
                        MatchTotal++;
                        matchSec++;
                    }
                }
            
                if (obj.Side == null)
                {
                    if (obj.Subject[0] == 'd')
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

                if (updVol)
                {
                    decimal price = (decimal)fullDepth.Bids[0].Price;

                    if (!Candle.IsTimeInCandle(candle, fullDepth.Timestamp))
                    {
                        candle.ClosePrice = price;
                        LastCandles.Add(candle);

                        Candle = new Candle();
                        Candle.Type = KlineType;
                        MarketVolume = 0;
                        Candle.Volume = 0;

                        Candle.OpenPrice = Candle.ClosePrice = Candle.HighPrice = Candle.LowPrice = price;
                        KlineTime = Candle.Timestamp = klineType.GetCurrentKlineStartTime();
                    }
                    else
                    {
                        Candle.ClosePrice = price;

                        if (price > Candle.HighPrice)
                        {
                            Candle.HighPrice = price;
                        }
                        else if (price < Candle.LowPrice)
                        {
                            Candle.LowPrice = price;    
                        }
                    }

                    if (!Candle.IsTimeInCandle(sortingCandle, fullDepth.Timestamp))
                    {
                        sortingCandle.ClosePrice = price;
                        SortingCandle = new Candle();
                        SortingCandle.Type = SortingKlineType;

                        SortingVolume = 0;
                        SortingCandle.Volume = 0;

                        SortingCandle.OpenPrice = SortingCandle.ClosePrice = SortingCandle.HighPrice = SortingCandle.LowPrice = price;
                        SortingKlineTime = SortingCandle.Timestamp = sortingKlineType.GetCurrentKlineStartTime();
                    }
                    else
                    {
                        SortingCandle.ClosePrice = price;

                        if (price > SortingCandle.HighPrice)
                        {
                            SortingCandle.HighPrice = price;
                        }
                        else if (price < Candle.LowPrice)
                        {
                            SortingCandle.LowPrice = price;
                        }
                    }

                }

                return true;
            }
        }

        VolumeTool vt = new VolumeTool();

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
            switch (subj[0])
            {
                case 'd':

                    pieces.Remove(change.OrderId);
                    return true;

                case 'o':

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
                    }

                    pieces.Add(u);

                    return true;

                case 'c':

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
                        return false;
                    }

                    return true;

                case 'm':

                    if (change.Price is decimal p && change.Size is decimal csize
                        && otherPieces.TryGetValue(change.MakerOrderId, out AtomicOrderStruct o))
                    {
                        o.Size -= csize;

                        otherPieces.Remove(o.OrderId);
                        otherPieces.Add(o);

                        // A match is a real component of volume.
                        // we can keep our own tally of the market volume per k-line.
                        if (updVol)
                        {
                            MarketVolume += (csize * p);
                            SortingVolume += (csize * p);

                            vt.Add(csize * p, change.Timestamp ?? DateTime.Now);

                            Candle.Volume = marketVolume;
                            SortingCandle.Volume = sortingVolume;
                        }
                    }

                    return true;

            }

            return false;
        }

    }

    public class VolumeTool
    {
        private Candle candle = new Candle() { Type = KlineType.Min1, Timestamp = KlineType.Min1.GetCurrentKlineStartTime() };

        private readonly List<decimal> volumes = new List<decimal>(new decimal[5] { 0, 0, 0, 0, 0 });

        private int cidx = 0;

        private bool ra = true;

        /// <summary>
        /// Gets the current sum or average volume.
        /// </summary>
        public decimal Volume
        {
            get
            {
                if (volumes == null) return 0;

                lock (volumes)
                {
                    if (ra)
                    {
                        if (cidx == 0) return volumes[0];
                        return volumes.Where((a) => a != 0M)?.Average() ?? 0;
                    }
                    else
                    {
                        return volumes.Sum();
                    }
                }
            }
        }

        /// <summary>
        /// How many minutes to look back for the sum/average calculation.
        /// </summary>
        public int MinutesRolling
        {
            get
            {
                if (volumes == null) return 0;

                lock (volumes)
                {
                    return volumes?.Count ?? 0;
                }
            }
            set
            {
                if (volumes == null) return;

                lock (volumes)
                {
                    volumes.Clear();
                    volumes.AddRange(new decimal[value]);
                    cidx = 0;
                }
            }
        }

        /// <summary>
        /// Average (as opposed to sum)
        /// </summary>
        public bool RollingAverage
        {
            get => ra;
            set
            {
                ra = value;
            }
        }

        public VolumeTool()
        {
        }
        
        public void Add(decimal v, DateTime t)
        {
            if (volumes == null) return;

            lock (volumes)
            {
                if (!Candle.IsTimeInCandle(candle, t))
                {
                    if (++cidx == 5)
                    {
                        volumes.RemoveAt(0);
                        volumes.Add(0);
                        cidx = 4;
                    }

                    candle.Timestamp = KlineType.Min1.GetCurrentKlineStartTime();
                }

                volumes[cidx] += v;
            }
        }
    }


}
