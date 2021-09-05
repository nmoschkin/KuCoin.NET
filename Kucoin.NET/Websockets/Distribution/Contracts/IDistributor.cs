using Kucoin.NET.Data.Market;
using Kucoin.NET.Websockets.Public;

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

namespace Kucoin.NET.Websockets.Distribution
{
    /// <summary>
    /// An object that provides distributed objects with data.
    /// </summary>
    public interface IDistributor : IFeedState
    {
        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of the active feeds.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDistributable> GetActiveFeeds();

        /// <summary>
        /// Release an object from the distributor.
        /// </summary>
        /// <param name="obj">Object to release.</param>
        void Release(IDistributable obj);

        bool Connected { get; }

        void Disconnect();

        Task<bool> Connect();

        Task<bool> Reconnect();

    }

    /// <summary>
    /// An object that provides distributed objects with data.
    /// </summary>
    public interface IDistributor<TDistribution, TValue> : IDistributor<string, TDistribution, TValue> where TDistribution : IDistributable<string, TValue>
    {
        void Release(IDistributable<string, TValue> obj);
    }

    /// <summary>
    /// An object that provides distributed objects with data.
    /// </summary>
    public interface IDistributor<TKey, TDistribution, TValue> : IDistributor where TDistribution: IDistributable<TKey, TValue>
    {
        /// <summary>
        /// Gets a dictionary of active feeds, keyed on a value of type <typeparamref name="TKey"/>.
        /// </summary>
        IReadOnlyDictionary<TKey, TDistribution> ActiveFeeds { get; }

    }
}
