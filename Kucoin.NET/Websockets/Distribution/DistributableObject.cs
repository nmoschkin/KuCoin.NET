using Kucoin.NET.Data.Market;
using Kucoin.NET.Observable;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Websockets.Distributable
{
    /// <summary>
    /// An observation that does regular work on the input feed data in a background thread.
    /// </summary>
    /// <typeparam name="TValue">The type of data being handled.</typeparam>
    public abstract class DistributableObject<TKey, TValue> : ObservableBase, IDisposable, IDistributable<TKey, TValue>
    {
        public virtual event EventHandler DistributionCompleted;

        protected bool disposedValue;

        protected string symbol;

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
        /// <param name="parent"></param>
        /// <param name="symbol"></param>
        public DistributableObject(IDistributor parent, string symbol)
        {
            this.parent = parent;
            this.symbol = symbol;

            ParallelService.RegisterService(this);
            State = FeedState.Subscribed;
        }

        protected TValue[] processValues = new TValue[0];

        bool IDistributable.DoWork()
        {
            int i, c;
            lock (lockObj)
            {
                lock (buffer)
                {
                    c = buffer.Count;
                    if (c == 0) return false;
                    if (c > processValues.Length)
                    {
                        Array.Resize(ref processValues, c * 2);
                    }

                    lock (processValues)
                    {
                        buffer.CopyTo(processValues, c);
                    }

                    buffer.Clear();
                }

                lock (processValues)
                {
                    for (i = 0; i < c; i++)
                    {
                        ProcessObject(processValues[i]);
                    }
                }
            }

            return true;
        }

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
            lock (buffer)
            {
                buffer.Add(value);
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
                    parent?.Release(this);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FeedObservation()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable Pattern

    }
}
