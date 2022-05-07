
using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Order;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;
using KuCoin.NET.Websockets.Observations;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Abstract base class for all live order book market feeds, including Level 2 and Level 3 for both the spot and futures markets.
    /// </summary>
    /// <typeparam name="TDistributable">The type of the order book distributable object.</typeparam>
    /// <typeparam name="TValue">The type of object that is being streamed.</typeparam>
    /// <typeparam name="TBookIn">The base order book type.</typeparam>
    /// <typeparam name="TBookOut">The observerable order book type.</typeparam>
    /// <typeparam name="TParent">The parent of the feed (the inheriter of <see cref="OrderBookFeed{TDistributable, TValue, TBookIn, TBookOut, TParent}"/>.)</typeparam>
    public abstract class OrderBookFeed<TDistributable, TValue, TBookIn, TBookOut, TParent> 
        : MarketFeed<TDistributable, TValue, TBookIn, TBookOut>, 
        IInitialDataProviderCallback<string, TBookIn>
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

        public void BeginProvideInitialData(string key, Action<TBookIn> callback)
        {
            BeginProvideInitialData(key, callback, 0);
        }

        /// <summary>
        /// Begin the request for initial data, and call the <paramref name="callback"/> function after the request has returned.
        /// </summary>
        /// <param name="key">They key for the data to acquire.</param>
        /// <param name="callback">The callback function to execute after the network request returns.</param>
        /// <param name="tryCount">The number of failed connection attempts that have been made, so far.</param>
        /// <remarks>
        /// The default behavior of this function is to attempt a network connection three times. If the network connection fails to produce a good result after three attempts, <paramref name="callback"/> will be called with a null parameter.
        /// <br /><br />
        /// This method is called directly by <see cref="BeginProvideInitialData(string, Action{TBookIn})"/>.
        /// <br /><br />
        /// Derived methods that override the default <paramref name="tryCount"/> handling behavior will need to remember to increment <paramref name="tryCount"/> by 1 for every successive call.
        /// </remarks>
        protected virtual void BeginProvideInitialData(string key, Action<TBookIn> callback, int tryCount)
        {
            var curl = InitialDataUrl;
            var param = new Dictionary<string, object>
            {
                { "symbol", key }
            };

            Thread.Sleep(250);

            BeginMakeRequest((jobj) =>
            {
                if (jobj == null)
                {
                    if (tryCount >= 3)
                    {
                        callback(null);
                        return;
                    }

                    BeginProvideInitialData(key, callback, tryCount + 1);
                    return;
                }
                var result = jobj.ToObject<TBookIn>();
                callback(result);

            }, HttpMethod.Get, curl, auth: !IsPublic, reqParams: param);

        }


        public override async Task<TBookIn> ProvideInitialData(string key)
        {
            Exception err = null;
            
            TBookIn result = null;

            var curl = InitialDataUrl;
            int tries, maxtries = 3;

            var param = new Dictionary<string, object>
            {
                { "symbol", key }
            };

            for (tries = 0; tries < maxtries; tries++)
            {
                try
                {
                    var jobj = await MakeRequest(HttpMethod.Get, curl, auth: !IsPublic, reqParams: param);
                    result = jobj.ToObject<TBookIn>();
                    break;
                }
                catch (Exception ex)
                {
                    err = ex;
                }
            }

            if (err != null)
            {
                Logger.Log(err.Message);
                throw err;
            }

            return result;
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
