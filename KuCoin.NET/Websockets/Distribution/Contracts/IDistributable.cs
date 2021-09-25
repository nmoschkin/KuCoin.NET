using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;

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
    /// An object that can do distributed work (typically in parallel with other such objects.)
    /// </summary>
    public interface IDistributable : IWebsocketListener, INotifyPropertyChanged // : IComparable<IDistributable>
    {
        /// <summary>
        /// Do work.
        /// </summary>
        void DoWork();

        /// <summary>
        /// The current number of buffered, waiting objects.
        /// </summary>
        int QueueLength { get; }

        /// <summary>
        /// Object that can be locked to support thread interlocking.
        /// </summary>
        object LockObject { get; }

    }

    /// <summary>
    /// An object that can do distributed work when an object of type <typeparamref name="TValue"/> is received (typically in parallel with other such objects.)
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The subscription object type.</typeparam>
    /// <remarks>
    /// A value of type <typeparamref name="TKey"/> is used to uniquely identify a distributable object from other objects that handle identical workloads.
    /// </remarks>
    public interface IDistributable<TKey, TValue> : IDistributable, IObserver<TValue>, IWebsocketListener<TValue> where TValue: IStreamableObject
    {
        /// <summary>
        /// Get the object key.
        /// </summary>
        TKey Key { get; }

    }

    /// <summary>
    /// An object that can do distributed work when an object of type <typeparamref name="T"/> is received (typically in parallel with other such objects.)
    /// </summary>
    /// <typeparam name="T">The subscription object type.</typeparam>
    /// <remarks>
    /// A string key is used to uniquely identify a distributable object from other objects that handle identical workloads.
    /// </remarks>
    public interface IDistributable<T> : IDistributable<string, T> where T: IStreamableObject
    {
    }

}
