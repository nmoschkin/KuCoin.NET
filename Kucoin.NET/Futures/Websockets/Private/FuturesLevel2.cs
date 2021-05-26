using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets;
using Kucoin.NET.Websockets.Observations;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets.Private
{
    //public class FuturesLevel2 : FuturesBaseWebsocketFeed
    //{


    //    internal readonly Dictionary<string, Level2Observation> activeFeeds = new Dictionary<string, Level2Observation>();

    //    protected int updateInterval = 500;

    //    public override bool IsPublic => false;

    //    /// <summary>
    //    /// Event that gets fired when the feed for a symbol has been calibrated and is ready to be used.
    //    /// </summary>
    //    public EventHandler<SymbolCalibratedEventArgs> SymbolCalibrated;

    //    /// <summary>
    //    /// Create a new Level 2 feed with the specified credentials.
    //    /// </summary>
    //    /// <param name="key">API Key</param>
    //    /// <param name="secret">API Secret</param>
    //    /// <param name="passphrase">API Passphrase</param>
    //    /// <param name="isSandbox">Is Sandbox Mode</param>
    //    /// <remarks>
    //    /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
    //    /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
    //    /// </remarks>
    //    public FuturesLevel2(
    //        string key,
    //        string secret,
    //        string passphrase,
    //        bool isSandbox = false)
    //        : base(key, secret, passphrase, isSandbox)
    //    {
    //        if (!Dispatcher.Initialized)
    //        {
    //            throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
    //        }
    //    }

    //    /// <summary>
    //    /// Create a new Level 2 feed with the specified credentials.
    //    /// </summary>
    //    /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
    //    /// <remarks>
    //    /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
    //    /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
    //    /// </remarks>
    //    public FuturesLevel2(ICredentialsProvider credProvider) : base(credProvider)
    //    {
    //        if (!Dispatcher.Initialized)
    //        {
    //            throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets a length of time (in milliseconds) that indicates how often the orderbook is pushed to the UI thread.
    //    /// </summary>
    //    /// <remarks>
    //    /// The default value is 500 milliseconds.
    //    /// </remarks>
    //    public int UpdateInterval
    //    {
    //        get => updateInterval;
    //        set
    //        {
    //            SetProperty(ref updateInterval, value);
    //        }
    //    }

    //    /// <summary>
    //    /// Adds a Level 2 subscription for the specified symbol.
    //    /// </summary>
    //    /// <param name="symbol">The symbol to subscribe.</param>
    //    /// <param name="pieces">Market depth, or 0 for full depth.</param>
    //    /// <returns></returns>
    //    public async Task<ILevel2OrderBookProvider> AddSymbol(string symbol, int pieces)
    //    {
    //        var p = await AddSymbols(new string[] { symbol }, pieces);
    //        return p[symbol];
    //    }

    //    /// <summary>
    //    /// Adds a Level 2 subscription for the specified symbols.
    //    /// </summary>
    //    /// <param name="symbols">The symbols to subscribe.</param>
    //    /// <param name="pieces">Market depth, or 0 for full depth.</param>
    //    /// <returns></returns>
    //    public async Task<Dictionary<string, ILevel2OrderBookProvider>> AddSymbols(IEnumerable<string> symbols, int pieces = 200)
    //    {
    //        if (disposed) throw new ObjectDisposedException(nameof(FuturesLevel2));
    //        if (!Connected)
    //        {
    //            await Connect();
    //        }

    //        var sb = new StringBuilder();
    //        var lnew = new Dictionary<string, ILevel2OrderBookProvider>();

    //        foreach (var sym in symbols)
    //        {
    //            if (activeFeeds.ContainsKey(sym))
    //            {
    //                lnew.Add(sym, activeFeeds[sym]);
    //                continue;
    //            }

    //            if (sb.Length > 0) sb.Append(',');
    //            sb.Append(sym);

    //            var obs = new Level2Observation(this, sym, pieces);
    //            activeFeeds.Add(sym, obs);

    //            lnew.Add(sym, obs);
    //        }

    //        var topic = $"/market/level2:{sb}";

    //        var e = new FeedMessage()
    //        {
    //            Type = "subscribe",
    //            Id = connectId.ToString("d"),
    //            Topic = topic,
    //            Response = true,
    //            PrivateChannel = false
    //        };

    //        await Send(e);

    //        return lnew;
    //    }

    //    /// <summary>
    //    /// Remove a Level 2 subscription for the specified symbol.
    //    /// </summary>
    //    /// <param name="symbol">The symbol to remove.</param>
    //    /// <returns></returns>
    //    internal virtual async Task RemoveSymbol(string symbol)
    //    {
    //        await RemoveSymbols(new string[] { symbol });
    //    }

    //    /// <summary>
    //    /// Remove a Level 2 subscription for the specified symbols.
    //    /// </summary>
    //    /// <param name="symbols">The symbols to remove.</param>
    //    /// <returns></returns>
    //    internal virtual async Task RemoveSymbols(IEnumerable<string> symbols)
    //    {
    //        if (disposed) throw new ObjectDisposedException(nameof(FuturesLevel2));
    //        if (!Connected) return;

    //        var sb = new StringBuilder();

    //        foreach (var sym in symbols)
    //        {
    //            if (activeFeeds.ContainsKey(sym))
    //            {
    //                if (!activeFeeds[sym].disposed)
    //                {
    //                    activeFeeds[sym].Dispose();
    //                }

    //                activeFeeds.Remove(sym);
    //            }

    //            if (sb.Length > 0) sb.Append(',');
    //            sb.Append(sym);
    //        }

    //        var topic = $"/market/level2:{sb}";

    //        var e = new FeedMessage()
    //        {
    //            Type = "unsubscribe",
    //            Id = connectId.ToString("d"),
    //            Topic = topic,
    //            Response = true,
    //            PrivateChannel = false
    //        };

    //        await Send(e);
    //    }


    //    /// <summary>
    //    /// Get the Level 2 Data Book for the specified trading symbol.
    //    /// </summary>
    //    /// <param name="symbol">The trading symbol.</param>
    //    /// <param name="pieces">The number of pieces.</param>
    //    /// <returns>The part book snapshot.</returns>
    //    /// <remarks>
    //    /// Settings the number of pieces to 0 returns the full market depth. 
    //    /// Use 0 to calibrate a full level 2 feed.
    //    /// </remarks>
    //    public async Task<OrderBook> GetPartList(string symbol, int pieces = 20)
    //    {
    //        var curl = pieces > 0 ? string.Format("/api/v1/market/orderbook/level2_{0}", pieces) : "/api/v2/market/orderbook/level2";
    //        var param = new Dictionary<string, object>();

    //        param.Add("symbol", (string)symbol);

    //        var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
    //        var result = jobj.ToObject<OrderBook>();

    //        foreach (var ask in result.Asks)
    //        {
    //            ask.Sequence = result.Sequence;
    //        }

    //        foreach (var bid in result.Bids)
    //        {
    //            bid.Sequence = result.Sequence;
    //        }

    //        return result;

    //    }

    //    /// <summary>
    //    /// Get the full Level 2 Data Book for the specified trading symbol.
    //    /// </summary>
    //    /// <param name="symbol">The trading symbol.</param>
    //    /// <returns>The part book snapshot.</returns>
    //    /// <remarks>
    //    /// Returns the full market depth. 
    //    /// Use this to calibrate a full level 2 feed.
    //    /// </remarks>
    //    public Task<OrderBook> GetAggregatedOrder(string symbol) => GetPartList(symbol, 0);

    //    /// <summary>
    //    /// Initialize the order book with a call to <see cref="GetAggregatedOrder(string)"/>.
    //    /// </summary>
    //    /// <param name="symbol">The symbol to initialize.</param>
    //    /// <remarks>
    //    /// This method is typically called after the feed has been buffered.
    //    /// </remarks>
    //    protected async Task InitializeOrderBook(string symbol)
    //    {
    //        if (!activeFeeds.ContainsKey(symbol)) return;

    //        var af = activeFeeds[symbol];

    //        var data = await GetAggregatedOrder(af.Symbol);

    //        af.FullDepthOrderBook = data;
    //        af.Initialized = true;
    //    }

    //    object lockObj = new object();
    //    DateTime cycle = DateTime.MinValue;

    //    protected override async Task HandleMessage(FeedMessage msg)
    //    {
    //        if (msg.Type == "message")
    //        {
    //            if (msg.Subject == "trade.l2update")
    //            {
    //                if (cycle == DateTime.MinValue) cycle = DateTime.Now;

    //                var i = msg.Topic.IndexOf(":");

    //                if (i != -1)
    //                {
    //                    var symbol = msg.Topic.Substring(i + 1);

    //                    if (activeFeeds.TryGetValue(symbol, out Level2Observation af))
    //                    {
    //                        var update = msg.Data.ToObject<Level2Update>();

    //                        if (!af.Calibrated)
    //                        {
    //                            af.OnNext(update);

    //                            if ((DateTime.Now - cycle).TotalMilliseconds >= updateInterval)
    //                            {
    //                                await InitializeOrderBook(af.Symbol);

    //                                lock (lockObj)
    //                                {
    //                                    af.Calibrate();
    //                                    cycle = DateTime.Now;
    //                                }
    //                                _ = Task.Run(() =>
    //                                {
    //                                    SymbolCalibrated?.Invoke(this, new SymbolCalibratedEventArgs(af));
    //                                });
    //                            }
    //                        }
    //                        else
    //                        {
    //                            lock (lockObj)
    //                            {
    //                                af.OnNext(update);

    //                                if ((DateTime.Now - cycle).TotalMilliseconds >= updateInterval)
    //                                {
    //                                    af.RequestPush();
    //                                    cycle = DateTime.Now;
    //                                }
    //                            }
    //                        }

    //                    }
    //                }
    //            }
    //        }
    //    }

    //}
}
