using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Observable;
using Kucoin.NET.Websockets.Distribution;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{
    public enum DistributionLayer 
    {
        Link,
        MessagePump
    }

    public abstract class KucoinDistributionFeed<TDistributable, TValue> : KucoinBaseWebsocketFeed, IAsyncUnsubscribableSubscriptionProvider<string, TDistributable, TValue>, IDistributor<TDistributable, TValue> where TDistributable : DistributableObject<string, TValue> where TValue : ISymbol
    {
        protected Dictionary<string, TDistributable> activeFeeds = new Dictionary<string, TDistributable>();   

        public virtual DistributionLayer DistributionLayer { get; protected set; } = DistributionLayer.MessagePump;

        public KucoinDistributionFeed(ICredentialsProvider credentialsProvider, DistributionLayer distributionLayer = DistributionLayer.MessagePump) : base(credentialsProvider)
        {
            DistributionLayer = distributionLayer;
        }

        public KucoinDistributionFeed(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false, DistributionLayer distributionLayer = DistributionLayer.MessagePump) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
            DistributionLayer = distributionLayer;
        }

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

        public virtual IReadOnlyDictionary<string, TDistributable> GetActiveFeeds() => activeFeeds;

        public abstract void Release(IDistributable obj);
        public abstract Task<TDistributable> SubscribeOne(string key);
        public abstract Task<IEnumerable<TDistributable>> SubscribeMany(IEnumerable<string> keys);
        public abstract Task UnsubscribeOne(string key);
        public abstract Task UnsubscribeMany(IEnumerable<string> keys);
    }

}
