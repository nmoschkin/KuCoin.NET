using Kucoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Interfaces;
using System.Net.Http;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Observable;
using System.Linq;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Websockets.Private
{
    /// <summary>
    /// Calibrated Level 2 Market Feed
    /// </summary>
    public class Level2<TBook, TUnit> : 
        KucoinBaseWebsocketFeed
        where TBook : IOrderBook<TUnit>, new()
        where TUnit : IOrderUnit, new()
    {

        internal readonly Dictionary<string, Level2Observation<TBook, TUnit>> activeFeeds = new Dictionary<string, Level2Observation<TBook, TUnit>>();

        protected int updateInterval = 500;

        public override bool IsPublic => false;

        /// <summary>
        /// Event that gets fired when the feed for a symbol has been calibrated and is ready to be used.
        /// </summary>
        public EventHandler<SymbolCalibratedEventArgs<TBook, TUnit, Level2Update>> SymbolCalibrated;

        /// <summary>
        /// Create a new Level 2 feed with the specified credentials.
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <remarks>
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public Level2(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false)
            : base(key, secret, passphrase, isSandbox)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        /// <summary>
        /// Create a new Level 2 feed with the specified credentials.
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        /// <remarks>
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public Level2(ICredentialsProvider credProvider) : base(credProvider)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        /// <summary>
        /// Gets or sets a length of time (in milliseconds) that indicates how often the orderbook is pushed to the UI thread.
        /// </summary>
        /// <remarks>
        /// The default value is 500 milliseconds.
        /// </remarks>
        public int UpdateInterval
        {
            get => updateInterval;
            set
            {
                SetProperty(ref updateInterval, value);
            }
        }

        protected virtual Level2Observation<TBook, TUnit> CreateNewObserver(string symbol, int pieces = 50)
        {
            return new Level2Observation<TBook, TUnit>(this, symbol, pieces);
        }

        /// <summary>
        /// Adds a Level 2 subscription for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to subscribe.</param>
        /// <param name="pieces">Market depth, or 0 for full depth.</param>
        /// <returns></returns>
        public async Task<ILevel2OrderBookProvider<TBook, TUnit, Level2Update>> AddSymbol(string symbol, int pieces)
        {
            var p = await AddSymbols(new string[] { symbol }, pieces);
            return p[symbol];
        }

        /// <summary>
        /// Adds a Level 2 subscription for the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to subscribe.</param>
        /// <param name="pieces">Market depth, or 0 for full depth.</param>
        /// <returns></returns>
        public async Task<Dictionary<string, ILevel2OrderBookProvider<TBook, TUnit, Level2Update>>> AddSymbols(IEnumerable<string> symbols, int pieces = 200)
        {
            if (disposed) throw new ObjectDisposedException(nameof(Level2<TBook, TUnit>));
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, ILevel2OrderBookProvider<TBook, TUnit, Level2Update>>();

            foreach (var sym in symbols)
            {
                if (activeFeeds.ContainsKey(sym))
                {
                    lnew.Add(sym, activeFeeds[sym]);
                    continue;
                }

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);

                var obs = CreateNewObserver(sym, pieces);
                activeFeeds.Add(sym, obs);

                lnew.Add(sym, obs);
            }

            var topic = $"/market/level2:{sb}";

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
            if (disposed) throw new ObjectDisposedException(nameof(Level2<TBook, TUnit>));
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

            var topic = $"/market/level2:{sb}";

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
        /// Get the Level 2 Data Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <param name="pieces">The number of pieces.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Settings the number of pieces to 0 returns the full market depth. 
        /// Use 0 to calibrate a full level 2 feed.
        /// </remarks>
        public async Task<TBook> GetPartList(string symbol, int pieces = 20)
        {
            var curl = pieces > 0 ? string.Format("/api/v1/market/orderbook/level2_{0}", pieces) : "/api/v2/market/orderbook/level2";
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
        /// Get the full Level 2 Data Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Returns the full market depth. 
        /// Use this to calibrate a full level 2 feed.
        /// </remarks>
        public Task<TBook> GetAggregatedOrder(string symbol) => GetPartList(symbol, 0);

        /// <summary>
        /// Initialize the order book with a call to <see cref="GetAggregatedOrder(string)"/>.
        /// </summary>
        /// <param name="symbol">The symbol to initialize.</param>
        /// <remarks>
        /// This method is typically called after the feed has been buffered.
        /// </remarks>
        protected async Task InitializeOrderBook(string symbol)
        {
            if (!activeFeeds.ContainsKey(symbol)) return;

            var af = activeFeeds[symbol];

            var data = await GetAggregatedOrder(af.Symbol);

            af.FullDepthOrderBook = data;
            af.Initialized = true;
        }

        object lockObj = new object();
        DateTime cycle = DateTime.MinValue;

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Subject == "trade.l2update")
                {
                    if (cycle == DateTime.MinValue) cycle = DateTime.Now;

                    var i = msg.Topic.IndexOf(":");

                    if (i != -1)
                    {
                        var symbol = msg.Topic.Substring(i + 1);

                        if (activeFeeds.TryGetValue(symbol, out Level2Observation<TBook, TUnit> af))
                        {
                            var update = msg.Data.ToObject<Level2Update>();

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
                                        SymbolCalibrated?.Invoke(this, new SymbolCalibratedEventArgs<TBook, TUnit, Level2Update>(af));
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

    /// <summary>
    /// Create a new KuCoin Level 2 websocket feed using the standard observable objects.
    /// </summary>
    public class Level2 : Level2<OrderBook<OrderUnit>, OrderUnit>
    {
        public Level2(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        public Level2(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox)
        {
        }

        protected override Level2Observation<OrderBook<OrderUnit>, OrderUnit> CreateNewObserver(string symbol, int pieces = 50)
        {
            return new Level2Observation(this, symbol, pieces);
        }

        public new async Task<Dictionary<string, ILevel2OrderBookProvider>> AddSymbols(IEnumerable<string> symbols, int pieces = 50)
        {
            var res = await base.AddSymbols(symbols, pieces);
            var dict = new Dictionary<string, ILevel2OrderBookProvider>();

            foreach (var kv in res)
            {

                dict.Add(kv.Key, (ILevel2OrderBookProvider)kv.Value);
            }

            return dict;
        }

        public new async Task<ILevel2OrderBookProvider> AddSymbol(string symbol, int pieces = 50)
        {
            return (ILevel2OrderBookProvider)await base.AddSymbol(symbol, pieces);
        }



    }


}
