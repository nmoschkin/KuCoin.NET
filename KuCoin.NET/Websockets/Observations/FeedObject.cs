using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Websockets.Distribution;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace KuCoin.NET.Websockets
{

    


    /// <summary>
    /// Feed <see cref="IObserver{T}"/> observation provider.
    /// </summary>
    /// <typeparam name="T">The type of information the feed provides.</typeparam>
    public class FeedObject<T> : IWebsocketListener<T>, IDisposable where T: class, IStreamableObject
    {
        protected IWebsocketFeed<T> feed;
        protected IObserver<T> observer;
        protected bool disposed = false;
                
        public bool Disposed => disposed;

        public IObserver<T> Observer => observer;

        public IWebsocketFeed<T> Feed => feed;

        public IWebsocketFeed Parent { get; }

        public FeedObject(IWebsocketFeed<T> feed, IObserver<T> observer)
        {
            this.feed = feed;
            this.observer = observer;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return; // throw new ObjectDisposedException(GetType().FullName);

            feed?.Release(this);

            disposed = true;
        }

        ~FeedObject()
        {
            Dispose(false);
        }
    }
}
