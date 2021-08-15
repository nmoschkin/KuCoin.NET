using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Public;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Websockets.Distribution;
using Kucoin.NET.Websockets.Observations;
using System.Threading;

namespace Kucoin.NET.Websockets.Public
{
    public interface ILevel3 : IMarketFeed<Level3Observation, Level3Update, KeyedAtomicOrderBook<AtomicOrderStruct>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>>
    {

    }

    public class Level3 : MarketFeed<Level3Observation, Level3Update, KeyedAtomicOrderBook<AtomicOrderStruct>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>>, ILevel3
    {

        bool wmp;

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        /// <param name="distributionStrategy">Data distribution strategy.</param>
        public Level3(ICredentialsProvider credentialsProvider, DistributionStrategy distributionStrategy = DistributionStrategy.MessagePump) : base(credentialsProvider, distributionStrategy)
        {
            if (distributionStrategy == DistributionStrategy.Link)
            {
                base.wantMsgPumpThread = false;
            }

            wmp = base.wantMsgPumpThread;

            //recvBufferSize = 262144;
            //minQueueBuffer = 10000;
            //chunkSize = 512;

        }

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        /// <param name="futures">True if KuCoin Futures.</param>
        /// <param name="distributionStrategy">Data distribution strategy.</param>
        public Level3(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false, DistributionStrategy distributionStrategy = DistributionStrategy.MessagePump) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures, distributionStrategy)
        {
            if (distributionStrategy == DistributionStrategy.Link)
            {
                base.wantMsgPumpThread = false;
            }

            wmp = base.wantMsgPumpThread;

            //recvBufferSize = 262144;
            //minQueueBuffer = 10000;
            //chunkSize = 512;

        }

        public override string Topic => "/spotMarket/level3";

        public override string InitialDataUrl => "/api/v3/market/orderbook/level3";

        public override bool IsPublic => false;

        public override async Task<KeyedAtomicOrderBook<AtomicOrderStruct>> ProvideInitialData(string key)
        {
            Exception err = null;

            var cts = new CancellationTokenSource();

            var ft = Task.Run(async () =>
            {
                var curl = InitialDataUrl;
                var param = new Dictionary<string, object>();

                param.Add("symbol", key);

                try
                {
                    var jobj = await MakeRequest(HttpMethod.Get, curl, auth: !IsPublic, reqParams: param);
                    var result = jobj.ToObject<KeyedAtomicOrderBook<AtomicOrderStruct>>();

                    GC.Collect(2);
                    return result;
                }
                catch (Exception ex)
                {
                    err = ex;
                    return null;
                }

            }, cts.Token);

            DateTime start = DateTime.UtcNow;
            
            while ((DateTime.UtcNow - start).TotalSeconds < 60)
            {
                await Task.Delay(10);

                if (ft.IsCompleted)
                {
                    return ft.Result;
                }
            }

            cts.Cancel();

            if (err != null) throw err;

            return null;
        }

        public override void Release(IDistributable<string, Level3Update> obj) => Release((Level3Observation)obj);

        public void Release(Level3Observation obj)
        {
            if (activeFeeds.ContainsValue(obj))
            {
                try
                {
                    obj.OnCompleted();
                    obj.Dispose();
                }
                catch
                {

                }

                activeFeeds.Remove(obj.Symbol);
            }
        }

        public override async Task<IDictionary<string, Level3Observation>> SubscribeMany(IEnumerable<string> keys)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, Level3Observation>();

            foreach (var sym in keys)
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

                var obs = new Level3Observation(this, sym);
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

        public override async Task UnsubscribeMany(IEnumerable<string> keys)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected) return;

            var sb = new StringBuilder();

            foreach (var sym in keys)
            {
                if (activeFeeds.ContainsKey(sym))
                {
                    try
                    {
                        activeFeeds[sym].Dispose();
                    }
                    catch { }

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
                State = FeedState.Unsubscribed;
            }
        }

        protected override void AddPacket(string json)
        {
            if (wmp)
            {
                base.AddPacket(json);
            }
            else
            {
                RouteJsonPacket(json);
            }
        }

        protected override void RouteJsonPacket(string json, FeedMessage e = null)
        {
            var msg = JsonConvert.DeserializeObject<FeedMessage<Level3Update>>(json);

            if (msg.TunnelId == tunnelId && msg.Type == "message")
            {
                var i = msg.Topic.IndexOf(":");

                if (i != -1)
                {
                    var symbol = msg.Topic.Substring(i + 1);
                    if (string.IsNullOrEmpty(symbol)) return;

                    var af = activeFeeds[symbol];

                    var update = msg.Data;
                    update.Subject = msg.Subject;

                    af.OnNext(update);
                }
            }
            else
            {
                base.RouteJsonPacket(json, e);
            }
        }

        public async Task<Level3Observation> AddSymbol(string symbol) => await SubscribeOne(symbol);

        public async Task<IDictionary<string, Level3Observation>> AddSymbols(IEnumerable<string> symbols) => await SubscribeMany(symbols);

        public async Task RemoveSymbol(string symbol) => await UnsubscribeOne(symbol);

        public async Task RemoveSymbols(IEnumerable<string> symbols) => await UnsubscribeMany(symbols);

        protected override Task HandleMessage(FeedMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
