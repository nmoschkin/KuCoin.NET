using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Distribution.Services;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Base class for all market-specific websocket distribution feeds that use the <see cref="ParallelService"/> (e.g. Level 2 and Level 3.)
    /// </summary>
    /// <typeparam name="TDistributable">The type of object that contains the distributed information.</typeparam>
    /// <typeparam name="TValue">The type of object that is being streamed.</typeparam>
    /// <typeparam name="TInitial">The type of the initial data set.</typeparam>
    public abstract class MarketFeed<TDistributable, TValue, TInitial, TObservable> :
        DistributionFeed<TDistributable, TValue, TInitial>, IMarketFeed<TDistributable, TValue, TInitial, TObservable>
        where TDistributable : MarketObservation<TInitial, TObservable, TValue>
        where TValue : ISymbol
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
     
        /// <summary>
        /// Add the specified symbol to the feed.
        /// </summary>
        /// <param name="symbol">The trading symbol to add.</param>
        /// <returns></returns>
        public virtual async Task<TDistributable> AddSymbol(string symbol) => await SubscribeOne(symbol);

        /// <summary>
        /// Add the specified trading symbols to the feed.
        /// </summary>
        /// <param name="symbols">The symbols to add.</param>
        /// <returns></returns>
        public virtual async Task<IDictionary<string, TDistributable>> AddSymbols(IEnumerable<string> symbols) => await SubscribeMany(symbols);

        /// <summary>
        /// Remove the specified trading symbol from the feed.
        /// </summary>
        /// <param name="symbol">The symbol to remove.</param>
        /// <returns></returns>
        public virtual async Task RemoveSymbol(string symbol) => await UnsubscribeOne(symbol);

        /// <summary>
        /// Remove the specified trading symbols from the feed.
        /// </summary>
        /// <param name="symbols">The symbols to remove.</param>
        /// <returns></returns>
        public virtual async Task RemoveSymbols(IEnumerable<string> symbols) => await UnsubscribeMany(symbols);



    }
}
