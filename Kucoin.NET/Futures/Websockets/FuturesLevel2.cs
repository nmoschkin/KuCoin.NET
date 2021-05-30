using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Futures.Data.Market;
using Kucoin.NET.Futures.Websockets.Observations;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets
{
    public class FuturesLevel2 : Level2Base<FuturesOrderBook, OrderUnit, FuturesChangeFeedItem, FuturesLevel2Observation>
    {
        public FuturesLevel2(ICredentialsProvider credProvider) : base(credProvider, true)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        public FuturesLevel2(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox, true)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        public override event EventHandler<SymbolCalibratedEventArgs<FuturesOrderBook, OrderUnit, FuturesChangeFeedItem>> SymbolCalibrated;


        public override async Task<Dictionary<string, FuturesLevel2Observation>> AddSymbols(IEnumerable<string> symbols, int pieces = 200)
        {
            if (disposed) throw new ObjectDisposedException(nameof(FuturesLevel2));
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, FuturesLevel2Observation>();

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

            var topic = $"/contractMarket/level2:{sb}";

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

        public override async Task<FuturesOrderBook> GetPartList(string symbol, int pieces = 20)
        {
            var curl = string.Format("/api/v1/level2/snapshot");
            var param = new Dictionary<string, object>();

            param.Add("symbol", (string)symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            var result = jobj.ToObject<FuturesOrderBook>();

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
            if (disposed) throw new ObjectDisposedException(nameof(FuturesLevel2));
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

        protected override FuturesLevel2Observation CreateNewObserver(string symbol, int pieces = 50)
        {
            return new FuturesLevel2Observation(this, symbol, pieces);
        }

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Subject == "level2")
                {
                    if (cycle == DateTime.MinValue) cycle = DateTime.Now;

                    var i = msg.Topic.IndexOf(":");

                    if (i != -1)
                    {
                        var symbol = msg.Topic.Substring(i + 1);

                        if (activeFeeds.TryGetValue(symbol, out FuturesLevel2Observation af))
                        {
                            var update = msg.Data.ToObject<FuturesChangeFeedItem>();

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
                                        SymbolCalibrated?.Invoke(this, new SymbolCalibratedEventArgs<FuturesOrderBook, OrderUnit, FuturesChangeFeedItem>(af));
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
