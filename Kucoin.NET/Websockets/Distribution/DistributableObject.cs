using Kucoin.NET.Data.Market;
using Kucoin.NET.Observable;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Websockets.Distribution
{
    /// <summary>
    /// An observation that does regular work on the input feed data in a background thread.
    /// </summary>
    /// <typeparam name="TValue">The type of data being handled.</typeparam>
    public abstract class DistributableObject<TKey, TValue> : ObservableBase, IDistributable<TKey, TValue>
    {
        public virtual event EventHandler DistributionCompleted;

        protected bool disposedValue;

        protected object lockObj = new object();

        protected IDistributor parent;

        protected TKey key;

        public object LockObject => lockObj;

        protected List<TValue> buffer;

        public IDistributor Parent => parent;

        public TKey Key => key; 

        protected FeedState state;
        
        /// <summary>
        /// Gets the current run state of the feed.
        /// </summary>
        public virtual FeedState State
        {
            get => state;
            protected set
            {
                SetProperty(ref state, value);
            }
        }
             
        /// <summary>
        /// Create a new active observation.
        /// </summary>
        /// <param name="parent">The parent distributor</param>
        /// <param name="key"></param>
        public DistributableObject(IDistributor parent, TKey key)
        {
            this.parent = parent;
            this.key = key;
            buffer = new List<TValue>();
            ParallelService.RegisterService(this);
            State = FeedState.Subscribed;
        }

        bool IDistributable.DoWork()
        {
            return DoWork();
        }

        /// <summary>
        /// Do work.
        /// </summary>
        /// <returns>True if work was done.</returns>
        protected abstract bool DoWork();

        /// <summary>
        /// Process the object with internal data handling.
        /// </summary>
        /// <param name="obj"></param>
        public abstract bool ProcessObject(TValue obj);

        public virtual void OnCompleted()
        {
            DistributionCompleted?.Invoke(this, new EventArgs());   
        }

        public virtual void OnError(Exception error)
        {
            throw error;
        }

        public virtual void OnNext(TValue value)
        {
            lock (lockObj)
            {
                lock (buffer)
                {
                    buffer.Add(value);
                }
            }
        }

        #region Disposable Pattern

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                parent?.Release(this);
                parent = null;

                ParallelService.UnregisterService(this);

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~DistributableObject()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Pattern

    }
}
