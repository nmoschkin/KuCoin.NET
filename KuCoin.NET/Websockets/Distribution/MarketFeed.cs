using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Helpers;
using KuCoin.NET.Websockets.Distribution.Services;
using KuCoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Base class for all market-specific websocket distribution feeds that use the <see cref="ParallelService"/> (e.g. Level 2 and Level 3.)
    /// </summary>
    /// <typeparam name="TDistributable">The type of object that contains the distributed information.</typeparam>
    /// <typeparam name="TValue">The type of object that is being streamed.</typeparam>
    /// <typeparam name="TInitial">The type of the initial data set.</typeparam>
    public abstract class MarketFeed<TDistributable, TValue, TInitial, TObservable> :
        DistributionFeed<TDistributable, TValue, TInitial>, IMarketFeed<TDistributable, TValue, TInitial, TObservable>
        where TDistributable : MarketPresenter<TInitial, TObservable, TValue>
        where TValue : ISymbol, IStreamableObject
    {

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public MarketFeed(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
        }

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        /// <param name="futures">True if KuCoin Futures.</param>
        public MarketFeed(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
        }
     
    }
}
