using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Observations;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Futures.Websockets;
using Kucoin.NET.Data.Market;
using System.Net;

namespace Kucoin.NET.Websockets.Public
{

    public enum FeedState
    {
        Disconnected,
        Connected,
        Subscribed,
        Unsubscribed,
        Initializing,
        Running
    }

    /// <summary>
    /// Calibrated Level 2 Market Feed base class and core logic implementation.
    /// </summary>
    /// <remarks>
    /// Generally, you should not use this base class to construct your own Level 2 handlers.
    /// Use either <see cref="Level2StandardBase{TBook, TUnit}"/> or <see cref="Level2FuturesBase{TBook, TUnit}"/>, instead.
    /// </remarks>
    public abstract class Level2Base<TBook, TUnit, TUpdate, TObservation> :
        KucoinBaseWebsocketFeed
        where TBook : IOrderBook<TUnit>, new()
        where TUnit : IOrderUnit, new()
        where TUpdate : new()
        where TObservation : Level2ObservationBase<TBook, TUnit, TUpdate>
    {

        internal readonly Dictionary<string, TObservation> activeFeeds = new Dictionary<string, TObservation>();

        protected object lockObj = new object();
        protected long cycle = 0;

        protected int defaultPieces = 50;
        protected int updateInterval = 500;

        protected FeedState state = FeedState.Disconnected;

        /// <summary>
        /// Get the feed subscription subject
        /// </summary>
        public abstract string Subject { get; }

        /// <summary>
        /// Get the feed subscription topic
        /// </summary>
        public abstract string Topic { get; }

        /// <summary>
        /// Get the aggregated order book retrieval end point.
        /// </summary>
        public abstract string AggregateEndpoint { get; }

        public override bool IsPublic => true;

        /// <summary>
        /// Event that gets fired when the feed for a symbol has been calibrated and is ready to be used.
        /// </summary>
        public virtual event EventHandler<SymbolCalibratedEventArgs<TBook, TUnit, TUpdate>> SymbolCalibrated;

        /// <summary>
        /// Create a new Level 2 feed.
        /// </summary>
        /// <param name="futures">True if the feed is for KuCoin Futures.</param>
        public Level2Base(
            bool futures = false)
            : base(null, null, null, futures: futures)
        {
            recvBufferSize = 65535;
        }

        public Level2Base(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        public Level2Base(string key, string secret, string passphrase, bool isSandbox=false,bool futures=false) : base(key,secret,passphrase, isSandbox: isSandbox, futures: futures)
        {
        }

        /// <summary>
        /// Gets or sets a length of time (in milliseconds) that indicates how often the order book is pushed to the UI thread.
        /// </summary>
        /// <remarks>
        /// The default value is 500 milliseconds.
        /// Set to 0 to disable automatic updates.
        /// </remarks>
        public virtual int UpdateInterval
        {
            get => updateInterval;
            set
            {
                // the minimum value is 5 milliseconds
                if (value < 5) value = 5;

                SetProperty(ref updateInterval, value);
            }
        }

        /// <summary>
        /// Gets or sets the default number of pieces for new observations.
        /// </summary>
        /// <remarks>
        /// The default value is 50 pieces.
        /// 
        /// To always include the full market depth, set this value to 0.
        /// 
        /// The value of this property only affects newly created observations.  Changing this value does not change the number of pieces
        /// in the live order books of observations that have already been created.  You can use the <see cref="ILevel2OrderBookProvider{TBook, TUnit, TUpdate}.Pieces"/> property
        /// on individual observations to change their default number of pieces.
        /// </remarks>
        public virtual int DefaultPieces
        {
            get => defaultPieces;
            set
            {
                SetProperty(ref defaultPieces, value);
            }
        }

        public FeedState State
        {
            get => state;
            set
            {
                SetProperty(ref state, value);
            }
        }

        protected override void OnConnected()
        {
            _ = Ping();
            State = FeedState.Connected;
            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            State = FeedState.Disconnected;
            base.OnDisconnected();
        }

        /// <summary>
        /// Create a new instance of a class derived from <see cref="Level2ObservationBase{TBook, TUnit, TUpdate}"/>.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>A new observation.</returns>
        protected abstract TObservation CreateNewObservation(string symbol);

        /// <summary>
        /// Adds a Level 2 subscription for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to subscribe.</param>
        /// <returns></returns>
        public async Task<TObservation> AddSymbol(string symbol)
        {
            var p = await AddSymbols(new string[] { symbol });
            return p[symbol];
        }

        /// <summary>
        /// Adds a Level 2 subscription for the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to subscribe.</param>
        /// <returns></returns>
        public virtual async Task<Dictionary<string, TObservation>> AddSymbols(IEnumerable<string> symbols)
        {
            if (disposed) throw new ObjectDisposedException(nameof(Level2Base<TBook, TUnit, TUpdate, TObservation>));
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, TObservation>();

            foreach (var sym in symbols)
            {
                if (activeFeeds.ContainsKey(sym))
                {
                    if (!lnew.ContainsKey(sym))
                    {
                        lnew.Add(sym, activeFeeds[sym]);
                    }
                    continue;
                }

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);

                var obs = CreateNewObservation(sym);
                activeFeeds.Add(sym, obs);

                if (!lnew.ContainsKey(sym))
                {
                    lnew.Add(sym, activeFeeds[sym]);
                }
            }

            var topic = $"{Topic}:{sb}";

            var e = new FeedMessage()
            {
                Type = "subscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true,
                PrivateChannel = false
            };

            await Send(e);

            State = FeedState.Subscribed;
            return lnew;
        }

        /// <summary>
        /// Remove a Level 2 subscription for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to remove.</param>
        /// <returns></returns>
        internal virtual async Task RemoveSymbol(string symbol)
        {
            await RemoveSymbols(new string[] { symbol });
        }

        /// <summary>
        /// Remove a Level 2 subscription for the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to remove.</param>
        /// <returns></returns>
        internal virtual async Task RemoveSymbols(IEnumerable<string> symbols)
        {
            if (disposed) throw new ObjectDisposedException(nameof(Level2Base<TBook, TUnit, TUpdate, TObservation>));
            if (!Connected) return;

            var sb = new StringBuilder();

            foreach (var sym in symbols)
            {
                if (activeFeeds.ContainsKey(sym))
                {
                    if (!activeFeeds[sym].Disposed)
                    {
                        activeFeeds[sym].Dispose();
                    }

                    activeFeeds.Remove(sym);
                }

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);
            }

            var topic = $"{Topic}:{sb}";

            var e = new FeedMessage()
            {
                Type = "unsubscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true,
                PrivateChannel = false
            };

            await Send(e);

            if (activeFeeds.Count == 0)
            {
                cycle = 0;
                State = FeedState.Unsubscribed;
            }
        }


        /// <summary>
        /// Get the full Level 2 Data Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Returns the full market depth. 
        /// Use this to calibrate a full level 2 feed.
        /// </remarks>
        public virtual async Task<KeyedOrderBook<OrderUnit>> GetAggregatedOrder(string symbol)
        {
            var curl = AggregateEndpoint;
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            var result = jobj.ToObject<KeyedOrderBook<OrderUnit>>();

            foreach (var ask in result.Asks)
            {
                if (ask is ISequencedOrderUnit seq)
                    seq.Sequence = result.Sequence;
            }

            foreach (var bid in result.Bids)
            {
                if (bid is ISequencedOrderUnit seq)
                    seq.Sequence = result.Sequence;
            }

            return result;
        }

        /// <summary>
        /// Initialize the order book with a call to <see cref="GetAggregatedOrder(string)"/>.
        /// </summary>
        /// <param name="symbol">The symbol to initialize.</param>
        /// <remarks>
        /// This method is typically called after the feed has been buffered.
        /// </remarks>
        protected virtual async Task InitializeOrderBook(string symbol)
        {
            if (!activeFeeds.ContainsKey(symbol)) return;

            var af = activeFeeds[symbol];

            var data = await GetAggregatedOrder(af.Symbol);

            af.FullDepthOrderBook = data;
            af.Initialized = true;
        }

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Subject == Subject)
                {
                    if (cycle == 0) cycle = DateTime.UtcNow.Ticks;

                    var i = msg.Topic.IndexOf(":");

                    if (i != -1)
                    {
                        var symbol = msg.Topic.Substring(i + 1);

                        if (activeFeeds.TryGetValue(symbol, out TObservation af))
                        {
                            var update = msg.Data.ToObject<TUpdate>();

                            if (!af.Calibrated)
                            {
                                _ = Task.Run(() => State = FeedState.Initializing);

                                af.OnNext(update);

                                if ((DateTime.UtcNow.Ticks - cycle) >= ((updateInterval == 0 ? 50 : updateInterval) * 10_000))
                                {
                                    await InitializeOrderBook(af.Symbol);

                                    lock (lockObj)
                                    {
                                        af.Calibrate();
                                        af.RequestPush();

                                        cycle = DateTime.UtcNow.Ticks;
                                    }

                                    _ = Task.Run(() =>
                                    {
                                        SymbolCalibrated?.Invoke(this, new SymbolCalibratedEventArgs<TBook, TUnit, TUpdate>(af));
                                        State = FeedState.Running;
                                    });

                                }
                            }
                            else
                            {
                                lock (lockObj)
                                {
                                    af.OnNext(update);

                                    if (updateInterval == 0) return;

                                    if ((DateTime.UtcNow.Ticks - cycle) >= (updateInterval * 10_000))
                                    {
                                        af.RequestPush();
                                        cycle = DateTime.UtcNow.Ticks;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
