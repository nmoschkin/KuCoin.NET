using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Observable;
using Kucoin.NET.Websockets.Distribution;

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
    /// Strategies for distributing data.
    /// </summary>
    [DefaultValue(MessagePump)]
    public enum DistributionStrategy 
    {
        /// <summary>
        /// Deserialization and distribution occurs on the data link thread.
        /// </summary>
        Link,

        /// <summary>
        /// Deserialization and distribution occurs on the message pump thread (default.)
        /// </summary>
        MessagePump
    }

    /// <summary>
    /// Base class for all websocket distribution feeds that use the <see cref="ParallelService"/>.
    /// </summary>
    /// <typeparam name="TDistributable">The type of object that contains the distributed information.</typeparam>
    /// <typeparam name="TValue">The type of object that is being streamed.</typeparam>
    /// <typeparam name="TInitial">The type of the initial data set.</typeparam>
    public abstract class DistributionFeed<TDistributable, TValue, TInitial> : 
        KucoinBaseWebsocketFeed, 
        IInitialDataProvider<string, TInitial>, 
        IAsyncUnsubscribableSubscriptionProvider<string, TDistributable, TValue>, 
        IDistributor<TDistributable, TValue> 
        where TDistributable : DistributableObject<string, TValue> 
        where TValue : ISymbol
    {

        protected Dictionary<string, TDistributable> activeFeeds = new Dictionary<string, TDistributable>();

        /// <summary>
        /// Instantiate a new distribution feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        /// <param name="distributionStrategy">Data distribution strategy.</param>
        public DistributionFeed(ICredentialsProvider credentialsProvider, DistributionStrategy distributionStrategy = DistributionStrategy.MessagePump) : base(credentialsProvider)
        {
            DistributionStrategy = distributionStrategy;
        }

        /// <summary>
        /// Instantiate a new distribution feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        /// <param name="futures">True if KuCoin Futures.</param>
        /// <param name="distributionStrategy">Data distribution strategy.</param>
        public DistributionFeed(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false, DistributionStrategy distributionStrategy = DistributionStrategy.MessagePump) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
            DistributionStrategy = distributionStrategy;
        }


        /// <summary>
        /// Gets the subscription topic.
        /// </summary>
        public abstract string Topic { get; }

        
        /// <summary>
        /// Gets the relative URL used to acquire the initial data.
        /// </summary>
        public abstract string InitialDataUrl { get; }

        /// <summary>
        /// Gets the distribution strategy for deserializing and distributing data.
        /// </summary>
        public virtual DistributionStrategy DistributionStrategy { get; protected set; } = DistributionStrategy.MessagePump;

       
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
    }

}
