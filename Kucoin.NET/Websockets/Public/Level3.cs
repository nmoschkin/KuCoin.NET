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
using Kucoin.NET.Json;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;

namespace Kucoin.NET.Websockets.Public
{
    public interface ILevel3 : IMarketFeed<Level3Observation, Level3Update, KeyedAtomicOrderBook<AtomicOrderStruct>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>>
    {

    }

    public class Level3 : MarketFeed<Level3Observation, Level3Update, KeyedAtomicOrderBook<AtomicOrderStruct>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>>, ILevel3
    {
        object lockObj = new object();
        int topiclen;

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        /// <param name="distributionStrategy">Data distribution strategy.</param>
        public Level3(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
            if (credentialsProvider.GetFutures()) throw new NotSupportedException("Cannot use Futures API credentials on a spot market feed.");

            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;
            topiclen = Topic.Length;
        }

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        /// <param name="distributionStrategy">Data distribution strategy.</param>
        public Level3(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: false)
        {
            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;
            topiclen = Topic.Length;
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
                    UnsubscribeOne(obj.Key).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                {

                }
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

                    var obs = new Level3Observation(this, sym);
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

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new JsonConverter[]
            {
                new Level3UpdateConverter()
            }
        };

        protected FeedMessage<Level3Update> msg = new FeedMessage<Level3Update>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void RouteJsonPacket(string json, FeedMessage e = null)
        {
            JsonConvert.PopulateObject(json, msg, settings);

            if (msg.Type[0] == 'm' && msg.TunnelId == tunnelId)
            {
                var update = msg.Data;
                update.Subject = msg.Subject;

                activeFeeds[update.Symbol].OnNext(update);
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
