using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Futures.Data.Market;
using Kucoin.NET.Websockets.Distribution;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets.Observations
{
    public class FuturesLevel2Observation : MarketObservation<KeyedOrderBook<OrderUnitStruct>, ObservableOrderBook<ObservableOrderUnit>, FuturesLevel2Update>, IFeedDiagnostics, IMarketVolume
    {
        new private FuturesLevel2 parent;

        private long matchTime = DateTime.UtcNow.Ticks;

        private long transactSec = 0;

        private long matchSec = 0;

        private IInitialDataProvider<string, KeyedOrderBook<OrderUnitStruct>> dataProvider;

        private DateTime? lastFailureTime = null;

        private KeyedOrderBook<OrderUnitStruct> fullDepth;

        private ObservableOrderBook<ObservableOrderUnit> orderBook;

        private KlineType klineType = KlineType.Min1;

        private DateTime klineTime = KlineType.Min1.GetCurrentKlineStartTime();

        private Candle candle = new Candle() { Type = KlineType.Min1, Timestamp = DateTime.Now.AddSeconds(-DateTime.Now.Second) };

        private KlineType sortingKlineType = KlineType.Min15;

        private DateTime sortingKlineTime = KlineType.Min15.GetCurrentKlineStartTime();

        private Candle sortingCandle = new Candle() { Type = KlineType.Min15, Timestamp = DateTime.Now.AddSeconds(-DateTime.Now.Second) };

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

        public FuturesLevel2Observation(FuturesLevel2 parent, string symbol) : base(parent, symbol)
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
        public KeyedOrderBook<OrderUnitStruct> FullDepthOrderBook
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
        public ObservableOrderBook<ObservableOrderUnit> OrderBook
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

        public override KeyedOrderBook<OrderUnitStruct> InternalData
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

        public override ObservableOrderBook<ObservableOrderUnit> ObservableData
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
            get => sortingVolume == 0M ? marketVolume : sortingVolume;
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

        public long MatchTotal { get; protected set; }

        public long TransactionsPerSecond { get; protected set; }

        public int QueueLength => buffer?.Count ?? 0;

        public override IInitialDataProvider<string, KeyedOrderBook<OrderUnitStruct>> DataProvider
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
                SetProperty(ref initialized, value);
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

                var asks = fullDepth.Asks as IList<OrderUnitStruct>;
                var bids = fullDepth.Asks as IList<OrderUnitStruct>;

                if (orderBook == null)
                {
                    OrderBook = new ObservableOrderBook<ObservableOrderUnit>();
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
                        orderBook.Asks.Add(asks[i].Clone<ObservableOrderUnit>());
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
                            orderBook.Asks[i] = asks[i].Clone<ObservableOrderUnit>();
                        }
                    }
                }

                if (bids.Count < marketDepth) marketDepth = bids.Count;

                if (orderBook.Bids.Count != marketDepth)
                {
                    orderBook.Bids.Clear();
                    for (int i = 0; i < marketDepth; i++)
                    {
                        orderBook.Bids.Add(bids[i].Clone<ObservableOrderUnit>());
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
                            orderBook.Bids[i] = bids[i].Clone<ObservableOrderUnit>();
                        }
                    }
                }

            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override async Task<bool> Initialize()
        {
            lock (lockObj)
            {
                IsInitialized = false;
                initializing = true;

                State = FeedState.Initializing;
            }

            KeyedOrderBook<OrderUnitStruct> fd = null;
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
        protected override bool DoWork()
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

                        return false;
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

                        return false;
                    }

                    return true;
                }

                int i, c = buffer.Count;
                if (c == 0) return false;

                if (c >= 20)
                {
                    for (i = 0; i < 20; i++)
                    {
                        ProcessObject(buffer[i]);
                    }

                    buffer.RemoveRange(0, 20);
                }
                else
                {
                    foreach (var obj in buffer)
                    {
                        ProcessObject(obj);
                    }

                    buffer.Clear();
                }

                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ProcessObject(FuturesLevel2Update obj)
        {
            if (disposedValue || fullDepth == null) return false;

            lock (lockObj)
            {
                try
                {

                    if (obj.Change.Side == Side.Buy)
                    {

                        SequencePieces(obj, fullDepth.Bids);

                    }
                    else
                    {
                        SequencePieces(obj, fullDepth.Asks);

                    }

                    fullDepth.Sequence = obj.Sequence;
                }
                catch (Exception ex)
                {
                    string e = ex.Message;
                }
            }

            if (updVol)
            {
                decimal price = (decimal)fullDepth.Bids[0].Price;

                if (!Candle.IsTimeInCandle(candle, fullDepth.Timestamp.ToUniversalTime()))
                {
                    candle.ClosePrice = price;
                    LastCandles.Add(candle);

                    Candle = new Candle();

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

                if (!Candle.IsTimeInCandle(sortingCandle, fullDepth.Timestamp.ToUniversalTime()))
                {
                    sortingCandle.ClosePrice = price;
                    SortingCandle = new Candle();

                    MarketVolume = 0;
                    SortingCandle.Volume = 0;

                    SortingCandle.OpenPrice = Candle.ClosePrice = Candle.HighPrice = Candle.LowPrice = price;
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

        public override void SetInitialDataProvider(IInitialDataProvider<string, KeyedOrderBook<OrderUnitStruct>> dataProvider)
        {
            DataProvider = dataProvider;
        }


        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="change">The change to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        private void SequencePieces(FuturesLevel2Update change, Level2KeyedCollection<OrderUnitStruct> pieces)
        {
            decimal cp = change.Change.Price;

            if (change.Change.Size == 0.0M)
            {
                if (updVol)
                {
                    marketVolume += pieces[cp].Price * pieces[cp].Size;
                    sortingVolume += pieces[cp].Price * pieces[cp].Size;
                }
                pieces.Remove(cp);
            }
            else
            {
                if (pieces.Contains(cp))
                {
                    var piece = pieces[cp];

                    piece.Size = change.Change.Size;

                    if (piece is ISequencedOrderUnit seqpiece)
                        seqpiece.Sequence = change.Sequence;
                }
                else
                {
                    var ou = new OrderUnitStruct();

                    ou.Price = change.Change.Price;
                    ou.Size = change.Change.Size;

                    if (ou is ISequencedOrderUnit seqpiece)
                        seqpiece.Sequence = change.Sequence;

                    pieces.Add(ou);
                }
            }
        }
    }

}
