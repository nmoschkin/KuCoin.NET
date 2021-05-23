using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Websockets.Private
{

    /// <summary>
    /// Level 2 5 best asks/bids.
    /// </summary>
    public class Level2Depth5 : SymbolTopicFeedBase<StaticMarketDepthUpdate>
    {               
        public override bool IsPublic => false;

        protected override string Subject => "level2";

        protected override string Topic => "/spotMarket/level2Depth5";

        protected Dictionary<string, ObservableStaticMarketDepthUpdate> symbolObservables = new Dictionary<string, ObservableStaticMarketDepthUpdate>();

        internal Level2Depth5() : base(null)
        {
        }

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

        public new async Task<ObservableStaticMarketDepthUpdate> AddSymbol(string symbol)
        {
            var res = await AddSymbols(new string[] { symbol });
            return res[symbol];
        }

        public new async Task<Dictionary<string, ObservableStaticMarketDepthUpdate>> AddSymbols(IEnumerable<string> symbols)
        {
            await base.AddSymbols(symbols);
            var dict = new Dictionary<string, ObservableStaticMarketDepthUpdate>();

            foreach (var sym in symbols)
            {
                var update = new ObservableStaticMarketDepthUpdate();
                dict.Add(sym, update);

                if (!symbolObservables.ContainsKey(sym))
                {
                    symbolObservables.Add(sym, update);
                }
            }

            return dict;
        }

        public override async Task RemoveSymbols(IEnumerable<string> symbols)
        {
            await base.RemoveSymbols(symbols);

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
                            
                            Dispatcher.InvokeOnMainThread((o) => 
                            {
                                update.UpdateObservable();
                            });
                        }

                        if (observers.Count > 0)
                        {
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


        }

        public Level2Depth5(
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

        public Level2Depth5(ICredentialsProvider credProvider, bool isSandbox = false) : base(credProvider, isSandbox)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public Level2Depth5(IEnumerable<string> symbols, 
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

            Connect().ContinueWith(async (t) =>
            {
                await AddSymbols(symbols);
            });
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbol">The symbol to watch.</param>
        public Level2Depth5(string symbol,
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

            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
            });
        }


        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public Level2Depth5(IEnumerable<string> symbols,
          ICredentialsProvider credProvider,
          bool isSandbox = false)
            : base(credProvider, isSandbox)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }

            Connect().ContinueWith(async (t) =>
            {
                await AddSymbols(symbols);
            });
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbol">The symbol to watch.</param>
        public Level2Depth5(string symbol,
          ICredentialsProvider credProvider,
          bool isSandbox = false)
            : base(credProvider, isSandbox)

        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }

            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
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
        protected override string Topic => "/spotMarket/level2Depth50";

        /// <summary>
        /// The depth of this level 2 feed.
        /// </summary>
        public override Level2Depth Depth => Level2Depth.Depth50;

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public Level2Depth50(IEnumerable<string> symbols,
          ICredentialsProvider credProvider,
          bool isSandbox = false)
            : base(credProvider, isSandbox)
        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbols(symbols);
            });
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbol">The symbol to watch.</param>
        public Level2Depth50(string symbol,
          ICredentialsProvider credProvider,
          bool isSandbox = false)
            : base(credProvider, isSandbox)

        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
            });
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbol">The symbol to watch.</param>
        public Level2Depth50(ICredentialsProvider credProvider,
            bool isSandbox = false)
            : base(credProvider, isSandbox)

        {
        }


    }
}
