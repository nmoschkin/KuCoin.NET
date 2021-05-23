using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Websockets
{

    /// <summary>
    /// Feed <see cref="IObserver{T}"/> observation provider.
    /// </summary>
    /// <typeparam name="T">The type of information the feed provides.</typeparam>
    public class FeedObservation<T> : IDisposable where T: class
    {
        protected KucoinBaseWebsocketFeed<T> feed;
        protected IObserver<T> observer;
        protected bool disposed = false;
        
        public bool Disposed => disposed;

        public IObserver<T> Observer => observer;

        public KucoinBaseWebsocketFeed<T> Feed => feed;

        internal FeedObservation(KucoinBaseWebsocketFeed<T> feed, IObserver<T> observer)
        {
            this.feed = feed;
            this.observer = observer;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposed) return; // throw new ObjectDisposedException(nameof(FeedObservation<T>));

            feed.RemoveObservation(this);

            disposed = true;
        }

        ~FeedObservation()
        {
            Dispose(false);
        }
    }
}
