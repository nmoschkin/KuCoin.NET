using KuCoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using KuCoin.NET.Helpers;

namespace KuCoin.NET.Websockets.Public
{

    /// <summary>
    /// Level 2 5 best asks/bids.
    /// </summary>
    public class Level2Depth5 : SymbolTopicFeedBase<StaticMarketDepthUpdate>
    {
        public override bool IsPublic => true;

        public override string Subject => "level2";

        public override string Topic => "/spotMarket/level2Depth5";

        protected Dictionary<string, ObservableStaticMarketDepthUpdate> symbolObservables = new Dictionary<string, ObservableStaticMarketDepthUpdate>();

        /// <summary>
        /// Retrieves the observable market data for the given symbol, or null if not found.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>An observable object.</returns>
        public ObservableStaticMarketDepthUpdate this[string symbol]
        {
            get
            {
                if (symbolObservables.TryGetValue(symbol, out ObservableStaticMarketDepthUpdate update))
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
        public async Task<ObservableStaticMarketDepthUpdate> AddSymbol(string symbol)
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
        public async Task<Dictionary<string, ObservableStaticMarketDepthUpdate>> AddSymbols(IEnumerable<string> symbols)
        {
            await base.SubscribeMany(symbols);
            var dict = new Dictionary<string, ObservableStaticMarketDepthUpdate>();

            foreach (var sym in symbols)
            {
                if (!symbolObservables.TryGetValue(sym, out ObservableStaticMarketDepthUpdate upd))
                {
                    var update = new ObservableStaticMarketDepthUpdate();
                    update.parent = this;

                    dict.Add(sym, update);
                    symbolObservables.Add(sym, update);
                }
                else
                {
                    dict.Add(sym, upd);
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

                        if (symbolObservables.TryGetValue(sym, out ObservableStaticMarketDepthUpdate update))
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

                        var data = msg.Data.ToObject<StaticMarketDepthUpdate>();

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
        public Level2Depth5() : base(null, null, null)
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
        public Level2Depth5(IEnumerable<string> symbols,
          string key,
          string secret,
          string passphrase,
          bool isSandbox = false)
            : base(key, secret, passphrase, isSandbox)
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
        public Level2Depth5(string symbol
            )
            : base(null, null, null)

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
        public Level2Depth5(IEnumerable<string> symbols
          )
            : base(null, null, null)
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
    public class Level2Depth50 : Level2Depth5
    {
        public override string Topic => "/spotMarket/level2Depth50";

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
        public Level2Depth50() : base()
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
        public Level2Depth50(IEnumerable<string> symbols) : base()
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
        public Level2Depth50(string symbol) : base()

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
