using KuCoin.NET.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    /// <summary>
    /// A websocket feed.
    /// </summary>
    public interface IWebsocketFeed
    {

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of the active feeds.
        /// </summary>
        /// <returns></returns>
        IEnumerable GetActiveFeeds();

        /// <summary>
        /// Release an object.
        /// </summary>
        /// <param name="obj">Object to release.</param>
        void Release(IWebsocketListener obj);

        /// <summary>
        /// Gets a value indicating the connected status.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Disconnect the feed.
        /// </summary>
        void Disconnect();


        /// <summary>
        /// Establish a connection to the remote server for the feed.
        /// </summary>
        /// <returns>True if the connection was successfully established.</returns>
        Task<bool> Connect();

    }
    
    /// <summary>
    /// A websocket feed that streams an object of type <typeparamref name="TObject"/> to a listener of type <typeparamref name="TListener"/>.
    /// </summary>
    /// <typeparam name="TObject">A <see cref="IStreamableObject"/>.</typeparam>
    /// <typeparam name="TListener">A <see cref="IWebsocketListener"/>.</typeparam>
    public interface IWebsocketFeed<TObject, TListener> : IWebsocketFeed
        where TObject: IStreamableObject
        where TListener: IWebsocketListener<TObject>
    {
        new IEnumerable<TListener> GetActiveFeeds();
    }

    /// <summary>
    /// A websocket feed that streams an object of type <typeparamref name="T"/> to a listener of type <see cref="FeedObject{T}"/>.
    /// </summary>
    /// <typeparam name="T">A <see cref="IStreamableObject"/>.</typeparam>
    public interface IWebsocketFeed<T> : IWebsocketFeed<T, FeedObject<T>>
        where T: class, IStreamableObject
    {
    }


}
