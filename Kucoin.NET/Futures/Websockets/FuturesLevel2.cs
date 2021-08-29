using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Futures.Data.Market;
using Kucoin.NET.Futures.Websockets.Observations;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Websockets.Distribution;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets
{
    public interface IFuturesLevel2 : IMarketFeed<FuturesLevel2Observation, FuturesLevel2Update, KeyedOrderBook<OrderUnitStruct>, ObservableOrderBook<ObservableOrderUnit>>
    {

    }

    public class FuturesLevel2 : MarketFeed<FuturesLevel2Observation, FuturesLevel2Update, KeyedOrderBook<OrderUnitStruct>, ObservableOrderBook<ObservableOrderUnit>>, IFuturesLevel2
    {
        object lockObj = new object();

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        /// <param name="distributionStrategy">Data distribution strategy.</param>
        public FuturesLevel2(ICredentialsProvider credentialsProvider, DistributionStrategy distributionStrategy = DistributionStrategy.MessagePump) : base(credentialsProvider, distributionStrategy)
        {
            if (distributionStrategy == DistributionStrategy.Link)
            {
                base.wantMsgPumpThread = false;
            }

            if (!credentialsProvider.GetFutures()) throw new NotSupportedException("Cannot use spot market API credentials on a futures feed.");

            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;
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
        public FuturesLevel2(string key, string secret, string passphrase, bool isSandbox = false, DistributionStrategy distributionStrategy = DistributionStrategy.MessagePump) : base(key, secret, passphrase, isSandbox: isSandbox, futures: true, distributionStrategy)
        {
            if (distributionStrategy == DistributionStrategy.Link)
            {
                base.wantMsgPumpThread = false;
            }

            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;

        }
        public string Subject => "level2";

        public override string InitialDataUrl => "/api/v1/level2/snapshot";

        public override string Topic => "/contractMarket/level2";

        public override bool IsPublic => false;

        public override async Task<KeyedOrderBook<OrderUnitStruct>> ProvideInitialData(string key)
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
                    var result = jobj.ToObject<KeyedOrderBook<OrderUnitStruct>>();

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


        public override DistributionStrategy DistributionStrategy
        {
            get => strategy;
            set
            {
                if (SetProperty(ref strategy, value))
                {
                    wantMsgPumpThread = (strategy == DistributionStrategy.MessagePump);
                    if (wantMsgPumpThread)
                    {
                        EnableMessagePumpThread();
                    }
                    else
                    {
                        DisableMessagePumpThread();
                    }
                }
            }
        }
        public override void Release(IDistributable<string, FuturesLevel2Update> obj) => Release((FuturesLevel2Observation)obj);

        public void Release(FuturesLevel2Observation obj)
        {
            if (activeFeeds.ContainsValue(obj))
            {
                try
                {
                    UnsubscribeOne(obj.Key).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                {

                }
            }
        }

        public override async Task<IDictionary<string, FuturesLevel2Observation>> SubscribeMany(IEnumerable<string> keys)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, FuturesLevel2Observation>();

            lock (lockObj)
            {
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

                    var obs = new FuturesLevel2Observation(this, sym);
                    activeFeeds.Add(sym, obs);

                    if (!lnew.ContainsKey(sym))
                    {
                        lnew.Add(sym, activeFeeds[sym]);
                    }
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

            lock (lockObj)
            {
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
            if (wantMsgPumpThread)
            {
                base.AddPacket(json);
            }
            else
            {
                RouteJsonPacket(json);
            }
        }

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new JsonConverter[]
            {
                new StringToDecimalConverter()
            }
        };

        protected override void RouteJsonPacket(string json, FeedMessage e = null)
        {
            var msg = JsonConvert.DeserializeObject<FeedMessage<FuturesLevel2Update>>(json, settings);


            if (msg.TunnelId == tunnelId && msg.Type == "message")
            {

                var i = msg.Topic.IndexOf(":");

                if (i != -1)
                {
                    var symbol = msg.Topic.Substring(i + 1);
                    if (string.IsNullOrEmpty(symbol)) return;

                    FuturesLevel2Observation af;

                    lock (lockObj)
                    {
                        af = activeFeeds[symbol];
                    }

                    var update = msg.Data;
                    af.OnNext(update);
                }
            }
            else
            {
                base.RouteJsonPacket(json, e);
            }
        }

        protected override Task HandleMessage(FeedMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
