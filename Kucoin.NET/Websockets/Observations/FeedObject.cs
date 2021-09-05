using Kucoin.NET.Data;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Websockets.Distribution;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Kucoin.NET.Websockets
{

    


    /// <summary>
    /// Feed <see cref="IObserver{T}"/> observation provider.
    /// </summary>
    /// <typeparam name="T">The type of information the feed provides.</typeparam>
    public class FeedObject<T> : IWebsocketListener<T>, IDisposable where T: class, IStreamableObject
    {
        protected KucoinBaseWebsocketFeed<T> feed;
        protected IObserver<T> observer;
        protected bool disposed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Disposed => disposed;

        public IObserver<T> Observer => observer;

        public KucoinBaseWebsocketFeed<T> Feed => feed;

        public IWebsocketFeed Parent { get; }

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
