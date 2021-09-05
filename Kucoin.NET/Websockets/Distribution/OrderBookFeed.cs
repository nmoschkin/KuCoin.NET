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
    public abstract class OrderBookFeed<TObject, TUpdate, TBookIn, TBookOut, TParent> : MarketFeed<TObject, TUpdate, TBookIn, TBookOut>
        where TObject: OrderBookDistributable<TBookIn, TBookOut, TUpdate, TParent>
        where TUpdate: ISymbol
        where TBookIn: class, new()
        where TBookOut: class, new()
        where TParent: OrderBookFeed<TObject, TUpdate, TBookIn, TBookOut, TParent>
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
        protected abstract TObject CreateFeed(string sym);

        public override async Task<IDictionary<string, TObject>> SubscribeMany(IEnumerable<string> keys)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, TObject>();

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

        protected FeedMessage<TUpdate> msg = new FeedMessage<TUpdate>();

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //protected override abstract void RouteJsonPacket(string json, FeedMessage e = null);

        protected override Task HandleMessage(FeedMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
