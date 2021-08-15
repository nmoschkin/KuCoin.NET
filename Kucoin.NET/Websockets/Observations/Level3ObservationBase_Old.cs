//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;

//using Kucoin.NET.Data.Market;
//using Kucoin.NET.Data.Websockets;
//using Kucoin.NET.Websockets.Private;
//using Kucoin.NET.Observable;

//using System.Text;
//using System.ComponentModel;
//using System.Threading;
//using Kucoin.NET.Helpers;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Runtime.InteropServices;
//using Kucoin.NET.Websockets.Distribution;

//namespace Kucoin.NET.Websockets.Observations.Old
//{

//    /// <summary>
//    /// Level 3 Observation base class.
//    /// </summary>
//    /// <typeparam name="TBookOut">The type of the order book to maintain.</typeparam>
//    /// <typeparam name="TUnitOut">The type of order units contained in the order book.</typeparam>
//    /// <typeparam name="TUpdate">The type of data to expect from the observable source.</typeparam>
//    /// <remarks>
//    /// Observations link <see cref="IObservable{T}"/> to <see cref="IObserver{T}"/>.
//    /// </remarks>
//    public abstract class Level3ObservationBase<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate> :
//        ObservableBase,
//        ILevel3OrderBookProvider<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate>, 
//        IDistributable
//        where TBookOut : IAtomicOrderBook<TUnitOut>, new()
//        where TUnitOut : IAtomicOrderUnit, new()
//        where TBookIn : KeyedAtomicOrderBook<TUnitIn>, new()
//        where TUnitIn : IAtomicOrderUnit, new()
//        where TUpdate : new()
//    {
//        public virtual event EventHandler<Level3OrderBookUpdatedEventArgs<TBookOut, TUnitOut>> OrderBookUpdated;
//        public virtual event OnNextHandler<TUpdate> NextObject;
//        IDistributor IDistributable.Parent => throw new NotImplementedException();
//        protected TBookIn fullDepth;
//        protected TBookOut orderBook;
//        protected string symbol;
//        protected int pieces;
//        protected KucoinBaseWebsocketFeed connectedFeed;
//        protected Thread PushThread;
//        protected CancellationTokenSource cts;
//        internal object lockObj = new object();
//        internal bool locked = false;
//        protected bool pushRequested = false;

//        protected KlineType klineType = KlineType.Min1;
//        protected decimal l3vol;
//        protected DateTime ckline;

//        protected bool autoPush = true;
//        protected bool updVol = false;
        
//        /// <summary>
//        /// Level 2 observation base class.
//        /// </summary>
//        /// <param name="parent">The owner feed.</param>
//        /// <param name="symbol">The trading symbol observed in this observation.</param>
//        /// <param name="pieces">The maximum number of pieces in the live book.</param>
//        /// <remarks>
//        /// Observations link <see cref="IObservable{T}"/> to <see cref="IObserver{T}"/>.
//        /// </remarks>
//        public Level3ObservationBase(KucoinBaseWebsocketFeed parent, string symbol, int pieces = 50)
//        {
//            this.symbol = symbol;

//            this.connectedFeed = parent;
//            this.pieces = pieces;

//            orderBook = new TBookOut();
//            cts = new CancellationTokenSource();

//            PushThread = new Thread(
//                async () =>
//                {
//                    bool pr = false;

//                    ckline = klineType.GetCurrentKlineStartTime();
//                    DateTime test = ckline.AddSeconds(klineType.Length);

//                    while (!cts.IsCancellationRequested)
//                    {
//                        if (autoPush)
//                        {
//                            lock (lockObj)
//                            {
//                                if (pushRequested)
//                                {
//                                    pushRequested = false;
//                                    pr = true;
//                                }
//                            }

//                            if (pr)
//                            {
//                                pr = false;

//                                OnPushLive(true);
//                                OrderBookUpdated?.Invoke(this, new Level3OrderBookUpdatedEventArgs<TBookOut, TUnitOut>(this.symbol, orderBook));
//                            }
//                        }

//                        // we always want to give up time-slices on a thread like this.
//                        // a 5 millisecond delay provides an even data flow.
//                        await Task.Delay(5);

//                        if (updVol)
//                        {
//                            if (DateTime.UtcNow >= test)
//                            {
//                                ckline = klineType.GetCurrentKlineStartTime();
//                                test = ckline.AddSeconds(klineType.Length);

//                                Level3Volume = 0.0M;
//                            }
//                        }

//                    }
//                }
//            );

//            PushThread.IsBackground = true;
//            PushThread.Start();

//            ParallelService.RegisterService(this);
//        }

//        /// <summary>
//        /// Gets the parent feed.
//        /// </summary>
//        public virtual KucoinBaseWebsocketFeed ConnectedFeed => !disposed ? connectedFeed : throw new ObjectDisposedException(GetType().FullName);


//        /// <summary>
//        /// Gets the trading symbol that is being observed.
//        /// </summary>        
//        public string Symbol => !disposed ? symbol : throw new ObjectDisposedException(GetType().FullName);

//        string ISymbol.Symbol
//        {
//            get => symbol;
//            set
//            {
//                SetProperty(ref symbol, value);
//            }
//        }

//        /// <summary>
//        /// The number of pieces to serve to the live order book feed.
//        /// </summary>
//        public virtual int Pieces
//        {
//            get => !disposed ? pieces : throw new ObjectDisposedException(GetType().FullName);
//            set
//            {
//                SetProperty(ref pieces, value);
//            }
//        }

//        /// <summary>
//        /// Gets the full-depth order book.
//        /// </summary>
//        public TBookIn FullDepthOrderBook
//        {
//            get => fullDepth;
//            internal set
//            {
//                SetProperty(ref fullDepth, value);
//            }
//        }

//        /// <summary>
//        /// Gets the order book truncated to <see cref="Pieces"/> asks and bids.
//        /// </summary>
//        public TBookOut OrderBook
//        {
//            get => orderBook;
//            internal set
//            {
//                SetProperty(ref orderBook, value);
//            }
//        }

//        /// <summary>
//        /// Specify whether or not to calculate the instantaneous volume.
//        /// </summary>
//        public bool UpdateVolume
//        {
//            get => updVol;
//            set
//            {
//                SetProperty(ref updVol, value);
//            }
//        }

//        /// <summary>
//        /// The volume K-Line length.
//        /// </summary>
//        public KlineType VolumeTime
//        {
//            get => klineType;
//            set
//            {
//                SetProperty(ref klineType, value);
//            }
//        }

//        /// <summary>
//        /// The current volume.
//        /// </summary>
//        public decimal Level3Volume
//        {
//            get => l3vol;
//            set
//            {
//                lock (lockObj)
//                {
//                    SetProperty(ref l3vol, value);
//                }
//            }
//        }

//        public bool AutoPushEnabled
//        {
//            get => autoPush;
//            set
//            {
//                SetProperty(ref autoPush, value);
//            }
//        }

//        /// <summary>
//        /// Gets a value indicating that this order book is initialized with the full-depth (preflight) order book.
//        /// </summary>
//        public abstract bool Initialized { get; internal set; }

//        /// <summary>
//        /// Gets a value indicating that this observation has been calibrated.
//        /// </summary>
//        public abstract bool Calibrated { get; protected set; }

//        /// <summary>
//        /// Issue a request for the full-depth order book to be pushed to the live feed.
//        /// </summary>
//        public abstract void RequestPush();

//        /// <summary>
//        /// Push the full-depth order book to the live feed.
//        /// </summary>
//        public virtual void PushLive()
//        {
//            OnPushLive(false);
//            OrderBookUpdated?.Invoke(this, new Level3OrderBookUpdatedEventArgs<TBookOut, TUnitOut>(this.symbol, orderBook));
//        }

//        /// <summary>
//        /// Push the preflight book to the live feed.
//        /// </summary>
//        protected abstract void OnPushLive(bool auto);

//        /// <summary>
//        /// Reset and recalibrate the order book.
//        /// 
//        /// </summary>
//        public abstract void Reset();

//        /// <summary>
//        /// Calibrate the order book from cached data.
//        /// </summary>
//        public abstract void Calibrate();

//        public abstract void OnNext(TUpdate value);

//        public abstract void OnCompleted();

//        public abstract void OnError(Exception error);

//        public override string ToString() => $"{symbol} : {pieces}";


//        #region IDisposable pattern

//        /// <summary>
//        /// Terminate the feed for this symbol from the server, nullify all resources and remove this object from the parent feed's list of listeners.
//        /// </summary>
//        public void Dispose()
//        {
//            Dispose(true);
//        }

//        /// <summary>
//        /// A value indicating if the object is disposed.
//        /// </summary>
//        protected bool disposed;

//        /// <summary>
//        /// Gets a value indicating if the object is disposed.
//        /// </summary>
//        public bool Disposed => disposed;

//        public object LockObject => lockObj;

//        /// <summary>
//        /// Performs all the necessary steps to dispose the object.
//        /// </summary>
//        /// <param name="disposing"></param>
//        protected virtual void Dispose(bool disposing)
//        {
//            if (disposed) return;

//            disposed = true;
//            ParallelService.UnregisterService(this);

//            cts?.Cancel();
//            PushThread = null;
//        }

//        public abstract bool DoWork();

//        ~Level3ObservationBase()
//        {
//            Dispose(false);
//        }

//        #endregion

//    }
//}
