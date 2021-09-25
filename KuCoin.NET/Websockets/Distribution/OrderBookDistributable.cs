using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    public abstract class OrderBookDistributable<TBookIn, TBookOut, TDistributable, TParent> 
        : MarketPresenter<TBookIn, TBookOut, TDistributable>, IFeedDiagnostics, IMarketVolume
        where TParent: IDistributor, IInitialDataProvider<string, TBookIn>
        where TBookIn: class, new()
        where TBookOut: class, new()
        where TDistributable: ISymbol, IStreamableObject
    {
        protected new TParent parent;
        
        protected long matchTime = DateTime.UtcNow.Ticks;

        protected long transactSec = 0;

        protected long matchSec = 0;

        protected IInitialDataProvider<string, TBookIn> dataProvider;

        protected DateTime? lastFailureTime = null;

        protected TBookIn fullDepth;

        protected TBookOut orderBook;

        protected IKlineType klineType = KuCoin.NET.Data.Market.KlineType.Min15;

        protected DateTime klineTime = KuCoin.NET.Data.Market.KlineType.Min15.GetCurrentKlineStartTime();

        protected Candle candle = new Candle() { Type = KuCoin.NET.Data.Market.KlineType.Min15, Timestamp = KuCoin.NET.Data.Market.KlineType.Min15.GetCurrentKlineStartTime() };

        protected List<Candle> lastCandles = new List<Candle>();

        protected bool disabled = true;

        protected bool updVol;

        protected bool diagEnable;

        protected int interval = 100;

        protected int marketDepth = 50;

        public OrderBookDistributable(TParent parent, string symbol, bool observationDisabledByDefault, bool direct) : base(parent, symbol, direct)
        {
            this.parent = parent;
            base.parent = parent;

            dataProvider = parent;
            IsPresentationDisabled = observationDisabledByDefault;
        }

        public virtual Candle Candle
        {
            get => candle;
            set
            {
                SetProperty(ref candle, value);
            }
        }

        public virtual List<Candle> LastCandles
        {
            get => lastCandles;
            protected set
            {
                SetProperty(ref lastCandles, value);
            }
        }

        public virtual Candle LastCandle
        {
            get
            {
                if (lastCandles == null) return null;
                else return lastCandles.LastOrDefault();
            }
        }

        public virtual IKlineType KlineType
        {
            get => klineType;
            set
            {
                if (SetProperty(ref klineType, value))
                {
                    var oldVol = Candle.Volume;
                    DateTime oldEnd = Candle.Timestamp;

                    KlineTime = klineType.GetCurrentKlineStartTime();

                    Candle = new Candle() { Type = (KlineType)klineType, Timestamp = klineTime };

                    if (Candle.IsTimeInCandle(Candle, oldEnd))
                    {
                        Candle.Volume = oldVol;
                    }

                    OnPropertyChanged(nameof(MarketVolume));
                }
            }
        }

        public virtual DateTime KlineTime
        {
            get => klineTime;
            protected set
            {
                SetProperty(ref klineTime, value);
            }
        }

        public override bool IsPresentationDisabled
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
        public virtual TBookIn FullDepthOrderBook
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
        public virtual TBookOut OrderBook
        {
            get => orderBook;
            protected set
            {
                if (SetProperty(ref orderBook, value))
                {
                    OnPropertyChanged(nameof(PresentedData));
                }
            }
        }

        public override TBookIn InternalData
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

        public override TBookOut PresentedData
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

        public virtual decimal MarketVolume
        {
            get => candle?.Volume ?? 0M;
            protected set
            {
                if (candle != null) candle.Volume = value;
            }
        }

        public virtual bool IsVolumeEnabled
        {
            get => updVol;
            set
            {
                SetProperty(ref updVol, value);
            }
        }

        public virtual bool DiagnosticsEnabled
        {
            get => diagEnable;
            set
            {
                SetProperty(ref diagEnable, value);
            }
        }

        public virtual long GrandTotal { get; protected set; }

        public virtual long MatchesPerSecond { get; protected set; }

        public virtual long MatchTotal { get; protected set; }

        public virtual long TransactionsPerSecond { get; protected set; }

        int cpass = 0;
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

                        if (lastFailureTime == null || (lastFailureTime is DateTime t && (DateTime.UtcNow - t).TotalMilliseconds >= resetTimeout))
                        {
                            initializing = false;
                            Reset().ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        else
                        {
                            OnPropertyChanged(nameof(TimeUntilNextRetry));
                            return;
                        }
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
                    else if ((startFetch != null && (DateTime.Now - (DateTime)startFetch).TotalMilliseconds >= (resetTimeout * 2)))
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

                int c = buffer.Count;
                if (c == 0) return;

                for(int i = 0; i < c; i++)
                {
                    ProcessObject(buffer[i]);
                }

                buffer.Clear();
                return;
            }
        }


        public override void SetInitialDataProvider(IInitialDataProvider<string, TBookIn> dataProvider)
        {
            DataProvider = dataProvider;
        }

    }

}
