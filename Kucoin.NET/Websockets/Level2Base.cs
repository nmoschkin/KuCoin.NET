using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Observations;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Websockets.Private;
using Kucoin.NET.Futures.Websockets;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Helpers;

namespace Kucoin.NET.Websockets
{
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
        protected DateTime cycle = DateTime.MinValue;

        protected int defaultPieces = 50;
        protected int updateInterval = 500;

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
        /// Create a new Level 2 feed with the specified credentials.
        /// </summary>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <remarks>
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public Level2Base(
            bool futures = false)
            : base(null, null, null, futures: futures)
        {
            if (!Dispatcher.Initialized && !Dispatcher.Initialize())
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
            this.cred = null;
        }
      

        /// <summary>
        /// Gets or sets a length of time (in milliseconds) that indicates how often the orderbook is pushed to the UI thread.
        /// </summary>
        /// <remarks>
        /// The default value is 500 milliseconds.
        /// </remarks>
        public virtual int UpdateInterval
        {
            get => updateInterval;
            set
            {
                SetProperty(ref updateInterval, value);
            }
        }

        /// <summary>
        /// Gets or sets the default number of pieces for new observations.
        /// </summary>
        /// <remarks>
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
                    lnew.Add(sym, activeFeeds[sym]);
                    continue;
                }

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);

                var obs = CreateNewObservation(sym);
                activeFeeds.Add(sym, obs);

                lnew.Add(sym, obs);
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
        public virtual async Task<TBook> GetAggregatedOrder(string symbol)
        {
            var curl = AggregateEndpoint;
            var param = new Dictionary<string, object>();

            param.Add("symbol", (string)symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            var result = jobj.ToObject<TBook>();

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
                    if (cycle == DateTime.MinValue) cycle = DateTime.Now;

                    var i = msg.Topic.IndexOf(":");

                    if (i != -1)
                    {
                        var symbol = msg.Topic.Substring(i + 1);

                        if (activeFeeds.TryGetValue(symbol, out TObservation af))
                        {
                            var update = msg.Data.ToObject<TUpdate>();

                            if (!af.Calibrated)
                            {
                                af.OnNext(update);

                                if ((DateTime.Now - cycle).TotalMilliseconds >= updateInterval)
                                {
                                    await InitializeOrderBook(af.Symbol);

                                    lock (lockObj)
                                    {
                                        af.Calibrate();
                                        cycle = DateTime.Now;
                                    }
                                    _ = Task.Run(() =>
                                    {
                                        SymbolCalibrated?.Invoke(this, new SymbolCalibratedEventArgs<TBook, TUnit, TUpdate>(af));
                                    });
                                }
                            }
                            else
                            {
                                lock (lockObj)
                                {
                                    af.OnNext(update);

                                    if ((DateTime.Now - cycle).TotalMilliseconds >= updateInterval)
                                    {
                                        af.RequestPush();
                                        cycle = DateTime.Now;
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
