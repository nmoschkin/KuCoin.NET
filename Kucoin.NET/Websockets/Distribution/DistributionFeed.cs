using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Observable;
using Kucoin.NET.Websockets.Distribution.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Base class for all websocket distribution feeds that use the <see cref="ParallelService"/>.
    /// </summary>
    /// <typeparam name="TDistributable">The type of object that contains the distributed information.</typeparam>
    /// <typeparam name="TValue">The type of object that is being streamed.</typeparam>
    /// <typeparam name="TInitial">The type of the initial data set.</typeparam>
    public abstract class DistributionFeed<TDistributable, TValue, TInitial> : 
        KucoinBaseWebsocketFeed, 
        IInitialDataProvider<string, TInitial>, 
        IAsyncUnsubscribableSubscriptionProvider<string, TDistributable>, 
        IDistributor<TDistributable, TValue> 
        where TDistributable : DistributableObject<string, TValue> 
        where TValue : ISymbol
    {

        protected Dictionary<string, TDistributable> activeFeeds = new Dictionary<string, TDistributable>();
        protected FeedState state;

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

       
        public virtual IReadOnlyDictionary<string, TDistributable> ActiveFeeds { get => activeFeeds; }
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
            var currentFeeds = new Dictionary<string, TDistributable>(activeFeeds);
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
        public virtual IReadOnlyDictionary<string, TDistributable> GetActiveFeeds() => activeFeeds;

        public virtual void Release(IDistributable obj) => Release((IDistributable<string, TValue>)obj);
        public virtual async Task UnsubscribeOne(string key)
        {
            await UnsubscribeMany(new string[] { key });
        }
        
        public abstract Task UnsubscribeMany(IEnumerable<string> keys);
        public virtual async Task<TDistributable> SubscribeOne(string key)
        {
            var x = await SubscribeMany(new string[] { key });
            return x?.Values?.FirstOrDefault();
        }
        public abstract Task<IDictionary<string, TDistributable>> SubscribeMany(IEnumerable<string> keys);
        public abstract Task<TInitial> ProvideInitialData(string key);
        public abstract void Release(IDistributable<string, TValue> obj);

        async Task IAsyncUnsubscribableSubscriptionProvider.UnsubscribeOne(object key)
        {
            await UnsubscribeOne((string)key);
        }

        async Task IAsyncUnsubscribableSubscriptionProvider.UnsubscribeMany(System.Collections.IEnumerable keys)
        {
            await UnsubscribeMany((IEnumerable<string>)keys);
        }

        async Task<IDisposable> IAsyncSubscriptionProvider.SubscribeOne(object key)
        {
            return await SubscribeOne((string)key);
        }

        async Task<System.Collections.IEnumerable> IAsyncSubscriptionProvider.SubscribeMany(System.Collections.IEnumerable keys)
        {
            return await SubscribeMany((IEnumerable<string>)keys);
        }
    }

}
