using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Websockets.Distribution.Contracts;
using KuCoin.NET.Websockets.Public;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{



    /// <summary>
    /// An object that provides distributed objects with data.
    /// </summary>
    public interface IDistributor : IWebsocketFeed, IFeedState, ILogProvider
    {
        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of the active feeds.
        /// </summary>
        /// <returns></returns>
        new IEnumerable<IDistributable> GetActiveFeeds();

        /// <summary>
        /// Release an object from the distributor.
        /// </summary>
        /// <param name="obj">Object to release.</param>
        void Release(IDistributable obj);


        Task<bool> Reconnect();

    }

    /// <summary>
    /// An object that provides distributed objects with data.
    /// </summary>
    public interface IDistributor<TDistribution, TValue> : IDistributor<string, TDistribution, TValue> where TDistribution : IDistributable<string, TValue> where TValue : IStreamableObject
    {
        void Release(IDistributable<string, TValue> obj);
    }

    /// <summary>
    /// An object that provides distributed objects with data.
    /// </summary>
    public interface IDistributor<TKey, TDistribution, TValue> : IDistributor, IWebsocketFeed<TValue, TDistribution> where TDistribution: IDistributable<TKey, TValue> where TValue: IStreamableObject
    {
        /// <summary>
        /// Gets a dictionary of active feeds, keyed on a value of type <typeparamref name="TKey"/>.
        /// </summary>
        IReadOnlyDictionary<TKey, TDistribution> ActiveFeeds { get; }

    }
}
