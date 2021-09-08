using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Futures.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Websockets;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Futures.Websockets
{

    /// <summary>
    /// Level 2 5 best asks/bids.
    /// </summary>
    public class FuturesLevel2Depth5 : SymbolTopicFeedBase<FuturesStaticMarketDepthUpdate>
    {
        public override bool IsPublic => true;

        public override string Subject => "level2";

        public override string Topic => "/contractMarket/level2Depth5";

        protected Dictionary<string, ObservableFuturesStaticMarketDepthUpdate> symbolObservables = new Dictionary<string, ObservableFuturesStaticMarketDepthUpdate>();

        /// <summary>
        /// Retrieves the observable market data for the given symbol, or null if not found.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>An observable object.</returns>
        public ObservableFuturesStaticMarketDepthUpdate this[string symbol]
        {
            get
            {
                if (symbolObservables.TryGetValue(symbol, out ObservableFuturesStaticMarketDepthUpdate update))
                {
                    return update;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Subscribe to the specified symbol.
        /// </summary>
        /// <param name="symbol">The trading pair (symbol) to subscribe.</param>
        /// <returns>A new <see cref="Observations.ILevel2OrderBookProvider"/> implementation instance.</returns>
        public new async Task<ObservableFuturesStaticMarketDepthUpdate> AddSymbol(string symbol)
        {
            var res = await AddSymbols(new string[] { symbol });
            return res[symbol];
        }

        /// <summary>
        /// Subscribe to the specified symbols.
        /// </summary>
        /// <param name="symbols">The trading pairs (symbols) to subscribe.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> containing a list of <see cref="Observations.ILevel2OrderBookProvider"/> implementation instances keyed by symbol.
        /// </returns>
        public new async Task<Dictionary<string, ObservableFuturesStaticMarketDepthUpdate>> AddSymbols(IEnumerable<string> symbols)
        {
            await base.SubscribeMany(symbols);
            var dict = new Dictionary<string, ObservableFuturesStaticMarketDepthUpdate>();

            foreach (var sym in symbols)
            {
                var update = new ObservableFuturesStaticMarketDepthUpdate();
                dict.Add(sym, update);

                if (!symbolObservables.ContainsKey(sym))
                {
                    symbolObservables.Add(sym, update);
                }
            }

            return dict;
        }

        /// <summary>
        /// Unsubscribe from the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to unsubscribe.</param>
        /// <returns></returns>
        public override async Task UnsubscribeMany(IEnumerable<string> symbols)
        {
            await base.UnsubscribeMany(symbols);

            foreach (var sym in symbols)
            {
                if (symbolObservables.ContainsKey(sym))
                {
                    symbolObservables.Remove(sym);
                }
            }

        }

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Subject == subject)
                {
                    var i = msg.Topic.IndexOf(":");
                    string sym;

                    if (i != -1)
                    {
                        sym = msg.Topic.Substring(i + 1);

                        if (symbolObservables.TryGetValue(sym, out ObservableFuturesStaticMarketDepthUpdate update))
                        {
                            JsonConvert.PopulateObject(msg.Data.ToString(), update);

                            if (Dispatcher.Initialized)
                            {
                                Dispatcher.InvokeOnMainThread((o) =>
                                {
                                    update.UpdateObservable();
                                });
                            }
                            else
                            {
                                update.UpdateObservable();
                            }
                        }

                        var data = msg.Data.ToObject<FuturesStaticMarketDepthUpdate>();

                        if (i != -1)
                        {
                            data.Symbol = sym;
                        }

                        await PushNext(data);
                    }

                }
            }


        }


        /// <summary>
        /// Create a new Level 2 5-best asks/bids feed with the specified credentials.
        /// </summary>
        /// <remarks>
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public FuturesLevel2Depth5() : base(null, null, null, futures: true)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call KuCoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        /// <summary>
        /// Create a new Level 2 5-best asks/bids feed with the specified credentials.
        /// </summary>
        /// <param name="symbols">The initial symbols to subscribe to.</param>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <remarks>
        /// This constructor will automatically attempt to connect to the remote server and subscribe to the specified symbol feeds.
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public FuturesLevel2Depth5(IEnumerable<string> symbols,
          string key,
          string secret,
          string passphrase,
          bool isSandbox = false)
            : base(key, secret, passphrase, isSandbox, futures: true)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call KuCoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }

            Connect().ContinueWith(async (t) =>
            {
                await AddSymbols(symbols);
            });
        }

        /// <summary>
        /// Create a new Level 2 5-best asks/bids feed with the specified credentials.
        /// </summary>
        /// <param name="symbol">The initial symbol to subscribe to.</param>
        /// <remarks>
        /// This constructor will automatically attempt to connect to the remote server and subscribe to the specified symbol feed.
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public FuturesLevel2Depth5(string symbol
            )
            : base(null, null, null, futures: true)

        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call KuCoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }

            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
            });
        }


        /// <summary>
        /// Create a new Level 2 5-best asks/bids feed with the specified credentials.
        /// </summary>
        /// <param name="symbols">The initial symbols to subscribe to.</param>
        /// <remarks>
        /// This constructor will automatically attempt to connect to the remote server and subscribe to the specified symbol feeds.
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public FuturesLevel2Depth5(IEnumerable<string> symbols
          )
            : base(null, null, null, futures: true)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call KuCoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }

            Connect().ContinueWith(async (t) =>
            {
                await AddSymbols(symbols);
            });
        }

        /// <summary>
        /// The depth of this level 2 feed.
        /// </summary>
        public virtual Level2Depth Depth => Level2Depth.Depth5;

    }


    /// <summary>
    /// Level 2 50 best asks/bids
    /// </summary>
    public class FuturesLevel2Depth50 : FuturesLevel2Depth5
    {
        public override string Topic => "/contractMarket/level2Depth50";

        /// <summary>
        /// The depth of this level 2 feed.
        /// </summary>
        public override Level2Depth Depth => Level2Depth.Depth50;


        /// <summary>
        /// Create a new Level 2 50-best asks/bids feed with the specified credentials.
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <remarks>
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public FuturesLevel2Depth50() : base()
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call KuCoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        /// <summary>
        /// Create a new Level 2 50-best asks/bids feed with the specified credentials.
        /// </summary>
        /// <param name="symbols">The initial symbols to subscribe to.</param>
        /// <remarks>
        /// This constructor will automatically attempt to connect to the remote server and subscribe to the specified symbol feeds.
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public FuturesLevel2Depth50(IEnumerable<string> symbols) : base()
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call KuCoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }

            Connect().ContinueWith(async (t) =>
            {
                await AddSymbols(symbols);
            });
        }

        /// <summary>
        /// Create a new Level 2 50-best asks/bids feed with the specified credentials.
        /// </summary>
        /// <param name="symbol">The initial symbol to subscribe to.</param>
        /// <remarks>
        /// This constructor will automatically attempt to connect to the remote server and subscribe to the specified symbol feed.
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public FuturesLevel2Depth50(string symbol) : base()

        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call KuCoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }

            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
            });
        }




    }
}
