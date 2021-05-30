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
    /// Standard Level 2 feed implementation with observables and UI data binding support.
    /// </summary>
    public class Level2 : Level2Base<OrderBook<OrderUnit>, OrderUnit, Level2Update, Level2Observation>
    {
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

        public override event EventHandler<SymbolCalibratedEventArgs<OrderBook<OrderUnit>, OrderUnit, Level2Update>> SymbolCalibrated;

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

        public override async Task<Dictionary<string, Level2Observation>> AddSymbols(IEnumerable<string> symbols, int pieces = 200)
        {
            if (disposed) throw new ObjectDisposedException(nameof(Level2));
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, Level2Observation>();

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

        public override async Task<OrderBook<OrderUnit>> GetPartList(string symbol, int pieces = 20)
        {
            var curl = pieces > 0 ? string.Format("/api/v1/market/orderbook/level2_{0}", pieces) : "/api/v2/market/orderbook/level2";
            var param = new Dictionary<string, object>();

            param.Add("symbol", (string)symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            var result = jobj.ToObject<OrderBook<OrderUnit>>();

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

        public override async Task RemoveSymbols(IEnumerable<string> symbols)
        {
            if (disposed) throw new ObjectDisposedException(nameof(Level2));
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

        protected override Level2Observation CreateNewObserver(string symbol, int pieces = 50)
        {
            return new Level2Observation(this, symbol, pieces);
        }

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

                        if (activeFeeds.TryGetValue(symbol, out Level2Observation af))
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
                                        SymbolCalibrated?.Invoke(this, new SymbolCalibratedEventArgs<OrderBook<OrderUnit>, OrderUnit, Level2Update>(af));
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

        protected override async Task InitializeOrderBook(string symbol)
        {
            if (!activeFeeds.ContainsKey(symbol)) return;

            var af = activeFeeds[symbol];

            var data = await GetAggregatedOrder(af.Symbol);

            af.FullDepthOrderBook = data;
            af.Initialized = true;
        }
    }


}
