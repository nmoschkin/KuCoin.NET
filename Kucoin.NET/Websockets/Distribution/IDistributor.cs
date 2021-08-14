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

namespace Kucoin.NET.Websockets.Distributable
{
    /// <summary>
    /// An object that provides distributed objects with data.
    /// </summary>
    public interface IDistributor
    {
        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of the active feeds.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDistributable> GetActiveFeeds();

        void Release(IDistributable obj);
    }

    /// <summary>
    /// An object that provides distributed objects with data.
    /// </summary>
    public interface IDistributor<T> : IDistributor<string, T> where T : IDistributable<string, T>
    {
    }

    /// <summary>
    /// An object that provides distributed objects with data.
    /// </summary>
    public interface IDistributor<TKey, TValue> : IDistributor where TValue: IDistributable<TKey, TValue>
    {
        /// <summary>
        /// Gets a dictionary of active feeds, keyed on a value of type <typeparamref name="TKey"/>.
        /// </summary>
        IReadOnlyDictionary<TKey, TValue> ActiveFeeds { get; }

    }
}
