using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Helpers;
using KuCoin.NET.Observable;
using KuCoin.NET.Websockets.Distribution.Contracts;
using KuCoin.NET.Websockets.Distribution.Services;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Base class for all websocket distribution feeds that use the <see cref="ParallelService"/>.
    /// </summary>
    /// <typeparam name="TDistributable">The type of object that contains the distributed information.</typeparam>
    /// <typeparam name="TValue">The type of object that is being streamed.</typeparam>
    /// <typeparam name="TInitial">The type of the initial data set.</typeparam>
    public abstract class DistributionFeed<TDistributable, TValue, TInitial> : DistributionFeed<TDistributable, string, TValue, TInitial>
        where TDistributable : DistributableObject<string, TValue>
        where TValue : ISymbol, IStreamableObject
    {

        /// <summary>
        /// Instantiate a new distribution feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public DistributionFeed(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
        }

        /// <summary>
        /// Instantiate a new distribution feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        /// <param name="futures">True if KuCoin Futures.</param>
        public DistributionFeed(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
        }
    }

    /// <summary>
    /// Base class for all websocket distribution feeds that use the <see cref="ParallelService"/>.
    /// </summary>
    /// <typeparam name="TDistributable">The type of object that contains the distributed information.</typeparam>
    /// <typeparam name="TValue">The type of object that is being streamed.</typeparam>
    /// <typeparam name="TInitial">The type of the initial data set.</typeparam>
    public abstract class DistributionFeed<TDistributable, TKey, TValue, TInitial> : 
        KucoinBaseWebsocketFeed, 
        IInitialDataProvider<TKey, TInitial>, 
        IAsyncUnsubscribableSubscriptionProvider<TKey, TDistributable>, 
        IDistributor<TKey, TDistributable, TValue>,
        IMutableLogProvider
        where TDistributable : DistributableObject<TKey, TValue> 
        where TValue : IStreamableObject
    {

        protected SortedDictionary<TKey, TDistributable> activeFeeds = new SortedDictionary<TKey, TDistributable>();
        protected FeedState state;
        protected SimpleLog log = KuCoinSystem.Logger;

        public virtual SimpleLog Logger
        {
            get => log;
            set
            {
                SetProperty(ref log, value);
            }
        }

        public virtual FeedState State
        {
            get => state;
            protected set
            {
                SetProperty(ref state, value);
            }
        }

        public virtual void RefreshState()
        {
            FeedState newState = FeedState.Disconnected;
            FeedState? lastState = null;

            foreach (var feed in activeFeeds.Values)
            {
                if (lastState is FeedState fs)
                {
                    if (feed.State != fs)
                    {
                        newState = FeedState.Multiple;
                        break;
                    }
                }

                newState = feed.State;
                lastState = feed.State;
            }

            State = newState;
        }

        /// <summary>
        /// Instantiate a new distribution feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public DistributionFeed(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
        }

        /// <summary>
        /// Instantiate a new distribution feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        /// <param name="futures">True if KuCoin Futures.</param>
        public DistributionFeed(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
        }

        /// <summary>
        /// Gets the relative URL used to acquire the initial data.
        /// </summary>
        public abstract string InitialDataUrl { get; }

       
        public virtual IReadOnlyDictionary<TKey, TDistributable> ActiveFeeds { get => activeFeeds; }
        IEnumerable<IDistributable> IDistributor.GetActiveFeeds()
        {
            var l = new List<IDistributable>(); 
            foreach (var feed in ActiveFeeds)
            {
                l.Add(feed.Value);
            }

            return l;
        }

        /// <summary>
        /// Attempt to-establish the connection.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> Reconnect()
        {
            var currentFeeds = new Dictionary<TKey, TDistributable>(activeFeeds);
            Disconnect();

            activeFeeds.Clear();

            await Connect();
            await SubscribeMany(currentFeeds.Keys);

            return (socket?.State == System.Net.WebSockets.WebSocketState.Open && activeFeeds.Count == currentFeeds.Count);
        }

        /// <summary>
        /// Gets all active subscriptions as dictionary of trading symbol keys and <see cref="TDistributable"/> values.
        /// </summary>
        /// <returns></returns>
        public virtual IReadOnlyDictionary<TKey, TDistributable> GetActiveFeeds() => activeFeeds;

        public virtual void Release(IDistributable obj) => Release((IDistributable<TKey, TValue>)obj);
        public virtual async Task UnsubscribeOne(TKey key)
        {
            await UnsubscribeMany(new TKey[] { key });
        }
        
        public abstract Task UnsubscribeMany(IEnumerable<TKey> keys);
        public virtual async Task<TDistributable> SubscribeOne(TKey key)
        {
            var x = await SubscribeMany(new TKey[] { key });
            return x?.Values?.FirstOrDefault();
        }
        public abstract Task<IDictionary<TKey, TDistributable>> SubscribeMany(IEnumerable<TKey> keys);
        public abstract Task<TInitial> ProvideInitialData(TKey key);

        public void Release(IDistributable<TKey, TValue> obj) => Release((IWebsocketListener)obj);

        async Task IAsyncUnsubscribableSubscriptionProvider.UnsubscribeOne(object key)
        {
            await UnsubscribeOne((TKey)key);
        }

        async Task IAsyncUnsubscribableSubscriptionProvider.UnsubscribeMany(System.Collections.IEnumerable keys)
        {
            await UnsubscribeMany((IEnumerable<TKey>)keys);
        }

        async Task<IDisposable> IAsyncSubscriptionProvider.SubscribeOne(object key)
        {
            return await SubscribeOne((TKey)key);
        }

        async Task<System.Collections.IEnumerable> IAsyncSubscriptionProvider.SubscribeMany(System.Collections.IEnumerable keys)
        {
            return await SubscribeMany((IEnumerable<TKey>)keys);
        }

        IEnumerable IWebsocketFeed.GetActiveFeeds()
        {
            return GetActiveFeeds();
        }

        public abstract void Release(IWebsocketListener obj);

        IEnumerable<TDistributable> IWebsocketFeed<TValue, TDistributable>.GetActiveFeeds()
        {
            return activeFeeds.Values;
        }
    }


    public abstract class CallbackEnabledDistributionFeed<TDistributable, TKey, TValue, TInitial> 
        : DistributionFeed<TDistributable, TKey, TValue, TInitial>, IInitialDataProviderCallback<TKey, TValue>
        where TDistributable : DistributableObject<TKey, TValue>
        where TValue : IStreamableObject

    {
        /// <summary>
        /// Instantiate a new distribution feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public CallbackEnabledDistributionFeed(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
        }

        /// <summary>
        /// Instantiate a new distribution feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        /// <param name="futures">True if KuCoin Futures.</param>
        public CallbackEnabledDistributionFeed(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
        }

        public abstract void BeginProvideInitialData(TKey key, Action<TValue> callback);
        
    }

}
