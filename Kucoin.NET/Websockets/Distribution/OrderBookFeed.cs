using Kucoin.NET.Data;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Websockets.Observations;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Abstract base class for all live order book market feeds, including Level 2 and Level 3 for both the spot and futures markets.
    /// </summary>
    /// <typeparam name="TDistributable">The type of the order book distributable object.</typeparam>
    /// <typeparam name="TValue">The type of object that is being streamed.</typeparam>
    /// <typeparam name="TBookIn">The base order book type.</typeparam>
    /// <typeparam name="TBookOut">The observerable order book type.</typeparam>
    /// <typeparam name="TParent">The parent of the feed (the inheriter of <see cref="OrderBookFeed{TDistributable, TValue, TBookIn, TBookOut, TParent}"/>.)</typeparam>
    public abstract class OrderBookFeed<TDistributable, TValue, TBookIn, TBookOut, TParent> : MarketFeed<TDistributable, TValue, TBookIn, TBookOut>
        where TDistributable: OrderBookDistributable<TBookIn, TBookOut, TValue, TParent>
        where TValue: ISymbol, IStreamableObject
        where TBookIn: class, new()
        where TBookOut: class, new()
        where TParent: OrderBookFeed<TDistributable, TValue, TBookIn, TBookOut, TParent>
    {
        protected object lockObj = new object();

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public OrderBookFeed(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
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
        /// <param name="futures">Is Futures feed.</param>
        public OrderBookFeed(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;
        }

        /// <summary>
        /// Create a new feed for the given symbol.
        /// </summary>
        /// <param name="sym">The symbol to create the feed for.</param>
        /// <returns></returns>
        protected abstract TDistributable CreateFeed(string sym);

        public override async Task<TBookIn> ProvideInitialData(string key)
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
                    var result = jobj.ToObject<TBookIn>();

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


        public override async Task<IDictionary<string, TDistributable>> SubscribeMany(IEnumerable<string> keys)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, TDistributable>();

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

                    var obs = CreateFeed(sym);
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

        protected JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>()
            {
                new StringToDecimalConverter()
            }
        };

        protected FeedMessage<TValue> msg = new FeedMessage<TValue>();

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //protected override abstract void RouteJsonPacket(string json, FeedMessage e = null);

        protected override Task HandleMessage(FeedMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
