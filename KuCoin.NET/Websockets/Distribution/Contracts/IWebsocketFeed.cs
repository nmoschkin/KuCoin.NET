using KuCoin.NET.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
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

        bool Connected { get; }

        void Disconnect();

        Task<bool> Connect();

    }

    
    public interface IWebsocketFeed<T, U> : IWebsocketFeed
        where T: IStreamableObject
        where U: IWebsocketListener<T>
    {
        new IEnumerable<U> GetActiveFeeds();
    }

    public interface IWebsocketFeed<T> : IWebsocketFeed<T, FeedObject<T>>
        where T: class, IStreamableObject
    {
    }


}
