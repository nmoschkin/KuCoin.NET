using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Websockets
{

    


    /// <summary>
    /// Feed <see cref="IObserver{T}"/> observation provider.
    /// </summary>
    /// <typeparam name="T">The type of information the feed provides.</typeparam>
    public class FeedObject<T> : IDisposable where T: class
    {
        protected KucoinBaseWebsocketFeed<T> feed;
        protected IObserver<T> observer;
        protected bool disposed = false;
        
        public bool Disposed => disposed;

        public IObserver<T> Observer => observer;

        public KucoinBaseWebsocketFeed<T> Feed => feed;

        internal FeedObject(KucoinBaseWebsocketFeed<T> feed, IObserver<T> observer)
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

            feed?.RemoveObservation(this);

            disposed = true;
        }

        ~FeedObject()
        {
            Dispose(false);
        }
    }
}
