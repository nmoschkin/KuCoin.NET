using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Websockets.Private;
using Kucoin.NET.Observable;

using System.Text;
using System.ComponentModel;
using System.Threading;
using Kucoin.NET.Helpers;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Kucoin.NET.Websockets.Observations
{

    /// <summary>
    /// Level 2 Observation base class.
    /// </summary>
    /// <typeparam name="TBook">The type of the order book to maintain.</typeparam>
    /// <typeparam name="TUnit">The type of order units contained in the order book.</typeparam>
    /// <typeparam name="TUpdate">The type of data to expect from the observable source.</typeparam>
    /// <remarks>
    /// Observations link <see cref="IObservable{T}"/> to <see cref="IObserver{T}"/>.
    /// </remarks>
    public abstract class Level3ObservationBase<TBook, TUnit, TUpdate> :
        ObservableBase,
        ILevel3OrderBookProvider<TBook, TUnit, TUpdate>
        where TBook : IAtomicOrderBook<TUnit>, new()
        where TUnit : IAtomicOrderUnit, new()
        where TUpdate : new()
    {
        public virtual event EventHandler<Level3OrderBookUpdatedEventArgs<TBook, TUnit>> OrderBookUpdated;

        protected TBook fullDepth;
        protected TBook orderBook;
        protected string symbol;
        protected int pieces;
        protected KucoinBaseWebsocketFeed connectedFeed;
        protected Task PushThread;
        protected CancellationTokenSource cts;
        protected object lockObj = new object();
        protected bool pushRequested = false;

        /// <summary>
        /// Level 2 observation base class.
        /// </summary>
        /// <param name="parent">The owner feed.</param>
        /// <param name="symbol">The trading symbol observed in this observation.</param>
        /// <param name="pieces">The maximum number of pieces in the live book.</param>
        /// <remarks>
        /// Observations link <see cref="IObservable{T}"/> to <see cref="IObserver{T}"/>.
        /// </remarks>
        public Level3ObservationBase(KucoinBaseWebsocketFeed parent, string symbol, int pieces = 50)
        {
            this.symbol = symbol;

            this.connectedFeed = parent;
            this.pieces = pieces;

            orderBook = new TBook();
            cts = new CancellationTokenSource();

            PushThread = Task.Factory.StartNew(
                async () =>
                {
                    bool pr = false;

                    while (!cts.IsCancellationRequested)
                    {
                        lock (lockObj)
                        {
                            if (pushRequested)
                            {
                                pushRequested = false;
                                pr = true;
                            }
                        }

                        if (pr)
                        {
                            pr = false;

                            PushLive();
                            OrderBookUpdated?.Invoke(this, new Level3OrderBookUpdatedEventArgs<TBook, TUnit>(this.symbol, orderBook));
                        }

                        // we always want to give up time-slices on a thread like this.
                        // a 5 millisecond delay provides an even data flow.
                        await Task.Delay(5);
                    }
                },
                TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach
            );

        }

        /// <summary>
        /// Gets the parent feed.
        /// </summary>
        public virtual KucoinBaseWebsocketFeed ConnectedFeed => !disposed ? connectedFeed : throw new ObjectDisposedException(nameof(Level2Observation));


        /// <summary>
        /// Gets the trading symbol that is being observed.
        /// </summary>        
        public string Symbol => !disposed ? symbol : throw new ObjectDisposedException(nameof(Level2Observation));


        void ISymbol.SetSymbol(string symbol)
        {
            base.SetProperty(ref this.symbol, symbol);
        }

        /// <summary>
        /// The number of pieces to serve to the live order book feed.
        /// </summary>
        public virtual int Pieces
        {
            get => !disposed ? pieces : throw new ObjectDisposedException(nameof(Level2Observation));
            set
            {
                SetProperty(ref pieces, value);
            }
        }

        /// <summary>
        /// Gets the full-depth order book.
        /// </summary>
        public TBook FullDepthOrderBook
        {
            get => fullDepth;
            internal set
            {
                SetProperty(ref fullDepth, value);
            }
        }

        /// <summary>
        /// Gets the order book truncated to <see cref="Pieces"/> asks and bids.
        /// </summary>
        public TBook OrderBook
        {
            get => orderBook;
            internal set
            {
                SetProperty(ref orderBook, value);
            }
        }

        /// <summary>
        /// Gets a value indicating that this order book is initialized with the full-depth (preflight) order book.
        /// </summary>
        public abstract bool Initialized { get; internal set; }

        /// <summary>
        /// Gets a value indicating that this observation has been calibrated.
        /// </summary>
        public abstract bool Calibrated { get; protected set; }

        /// <summary>
        /// Issue a request for the full-depth order book to be pushed to the live feed.
        /// </summary>
        public abstract void RequestPush();

        /// <summary>
        /// Push the full-depth order book to the live feed.
        /// </summary>
        protected abstract void PushLive();

        /// <summary>
        /// Reset and recalibrate the order book.
        /// 
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Calibrate the order book from cached data.
        /// </summary>
        public abstract void Calibrate();

        public abstract void OnNext(TUpdate value);

        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public override string ToString() => $"{symbol} : {pieces}";


        #region IDisposable pattern

        /// <summary>
        /// Terminate the feed for this symbol from the server, nullify all resources and remove this object from the parent feed's list of listeners.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// A value indicating if the object is disposed.
        /// </summary>
        protected bool disposed;

        /// <summary>
        /// Gets a value indicating if the object is disposed.
        /// </summary>
        public bool Disposed => disposed;

        /// <summary>
        /// Performs all the necessary steps to dispose the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            disposed = true;

            cts?.Cancel();
            PushThread.Dispose();
            PushThread = null;
        }

        ~Level3ObservationBase()
        {
            Dispose(false);
        }

        #endregion

    }
}
