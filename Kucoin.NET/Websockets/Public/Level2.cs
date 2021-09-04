using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Websockets.Distribution;
using Kucoin.NET.Websockets.Observations;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Public
{
    public interface ILevel2 : IMarketFeed<Level2Observation, Level2Update, KeyedOrderBook<OrderUnitStruct>, ObservableOrderBook<ObservableOrderUnit>>
    {

    }

    public class Level2 : MarketFeed<Level2Observation, Level2Update, KeyedOrderBook<OrderUnitStruct>, ObservableOrderBook<ObservableOrderUnit>>, ILevel2
    {
        object lockObj = new object();

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public Level2(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
            if (credentialsProvider.GetFutures()) throw new NotSupportedException("Cannot use Futures API credentials on a spot market feed.");

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
        public Level2(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: false)
        {
            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;

        }
        public override string InitialDataUrl => "/api/v3/market/orderbook/level2";

        public string Subject => "trade.l2update";

        public override string Topic => "/market/level2";

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

        public override void Release(IDistributable<string, Level2Update> obj) => Release((Level2Observation)obj);

        public void Release(Level2Observation obj)
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

        public override async Task<IDictionary<string, Level2Observation>> SubscribeMany(IEnumerable<string> keys)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, Level2Observation>();

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

                    var obs = new Level2Observation(this, sym);
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
                new StringToDecimalConverter()
            }
        };

        protected override void RouteJsonPacket(string json, FeedMessage e = null)
        {
            var msg = JsonConvert.DeserializeObject<FeedMessage<Level2Update>>(json, settings);
            var symbol = msg.Data.Symbol;

            if (msg.TunnelId == tunnelId && msg.Type == "message")
            {
                if (string.IsNullOrEmpty(symbol)) return;
                activeFeeds[symbol].OnNext(msg.Data);
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
