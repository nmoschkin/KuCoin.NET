using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Observations
{

    public class Level3Observation : CustomLevel3Observation<ObservableAtomicOrderBook<ObservableAtomicOrderUnit>, ObservableAtomicOrderUnit>, ILevel3OrderBookProvider
    {
        public Level3Observation(KucoinBaseWebsocketFeed parent, string symbol, int pieces = 50) : base(parent, symbol, pieces)
        {
        }

    }


    /// <summary>
    /// Custom level 2 observation for custom Level 2 feed implementations.
    /// </summary>
    /// <typeparam name="TBookOut">The type of your custom order book.</typeparam>
    /// <typeparam name="TUnitOut">The type of your custom order pieces.</typeparam>
    public class CustomLevel3Observation<TBookOut, TUnitOut> : Level3ObservationBase<TBookOut, TUnitOut, KeyedAtomicOrderBook<AtomicOrderStruct>, AtomicOrderStruct, Level3Update>
        where TBookOut : IAtomicOrderBook<TUnitOut>, new()
        where TUnitOut : IAtomicOrderUnit, new()
    {

        protected bool calibrated;
        protected bool initialized;

        protected List<Level3Update> orderBuffer = new List<Level3Update>();

        public override event OnNextHandler<Level3Update> NextObject;

        public CustomLevel3Observation(KucoinBaseWebsocketFeed parent, string symbol, int pieces = 50) : base(parent, symbol, pieces)
        {
        }

        public override void RequestPush()
        {
            lock (lockObj)
            {
                pushRequested = true;
            }
        }

        /// <summary>
        /// Gets a value indicating that this order book is initialized with the full-depth (preflight) order book.
        /// </summary>
        public override bool Initialized
        {
            get => !disposed ? initialized : throw new ObjectDisposedException(nameof(Level2Observation));
            internal set
            {
                if (disposed) throw new ObjectDisposedException(nameof(Level2Observation));
                SetProperty(ref initialized, value);
            }
        }

        /// <summary>
        /// Gets a value indicating that this observation has been calibrated.
        /// </summary>
        public override bool Calibrated
        {
            get => !disposed ? calibrated : throw new ObjectDisposedException(nameof(Level2Observation));
            protected set
            {
                if (disposed) throw new ObjectDisposedException(nameof(Level2Observation));
                SetProperty(ref calibrated, value);
            }
        }

        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="changes">The changes to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SequencePieces(string subj, Level3Update change, KeyedBook<AtomicOrderStruct> pieces, KeyedBook<AtomicOrderStruct> otherPieces)
        {
            switch (subj)
            {
                case "done":

                    pieces.Remove(change.OrderId);
                    return;

                case "open":

                    if (change.Price == null || change.Price == 0 || change.Size == null || change.Size == 0) return;

                    var u = new AtomicOrderStruct
                    {
                        Price = change.Price ?? 0,
                        Size = change.Size ?? 0,
                        Timestamp = change.Timestamp ?? DateTime.Now,
                        OrderId = change.OrderId
                    };

                    pieces.Add(u);

                    return;

                case "change":

                    if (pieces.TryGetValue(change.OrderId, out AtomicOrderStruct piece))
                    {
                        pieces.Remove(piece.OrderId);

                        piece.Size = change.Size ?? 0;
                        piece.Timestamp = change.Timestamp ?? DateTime.Now;
                        piece.Price = change.Price ?? 0;

                        pieces.Add(piece);
                    }
                    else
                    {
                        var u2 = new AtomicOrderStruct
                        {
                            Price = change.Price ?? 0,
                            Size = change.Size ?? 0,
                            Timestamp = change.Timestamp ?? DateTime.Now,
                            OrderId = change.OrderId
                        };

                        pieces.Add(u2);
                    }

                    return;

                case "match":

                    if (change.Price is decimal p && change.Size is decimal csize
                        && otherPieces.TryGetValue(change.MakerOrderId, out AtomicOrderStruct o))
                    {
                        o.Size -= csize;

                        otherPieces.Remove(o.OrderId);
                        otherPieces.Add(o);

                        // A match is a real component of volume.
                        // we can keep our own tally of the market volume per k-line.
                        if (updVol) Level3Volume += (csize * p);
                    }

                    return;

            }
        }

        /// <summary>
        /// De-initialize and clear all data and begin a new calibration.
        /// 
        /// </summary>
        public override void Reset()
        {
            lock (lockObj)
            {
                orderBuffer = new List<Level3Update>();

                Initialized = false;
                Calibrated = false;
            }

        }

        bool bf = false;
        bool af = false;

        /// <summary>
        /// Calibrate the order book from cached data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Calibrate()
        {
            lock (lockObj)
            {
                calibrated = true;

                bf = true;
                foreach (var q in orderBuffer)
                {
                    if (q.Sequence > fullDepth.Sequence) DoNext(q);
                }
                bf = false;
                orderBuffer.Clear();
            }

            OnPropertyChanged(nameof(Calibrated));
        }

        public override void OnNext(Level3Update value)
        {
            lock (lockObj)
            {
                orderBuffer.Add(value);
            }
        }

        #region Diagnostics

        long grandTotal = 0;
        long matchTotal = 0;

        long secTotalCount = 0;
        long secMatchCount = 0;
        
        long matchesPerSec = 0;
        long totalPerSec = 0;

        bool diagEnable;

        DateTime lastMeasureTime = DateTime.UtcNow;

        public long MatchTotal => matchTotal;
        public long GrandTotal => grandTotal;

        /// <summary>
        /// Enable collecting diagnostic information such as total transactions, matches, efficiency.
        /// </summary>
        public bool DiagnosticsEnabled
        {
            get => diagEnable;
            set
            {
                if (SetProperty(ref diagEnable, value))
                {
                    grandTotal = 0;
                    matchTotal = 0;

                    secTotalCount = 0;
                    secMatchCount = 0;

                    matchesPerSec = 0;
                    totalPerSec = 0;

                }
            }
        }

        /// <summary>
        /// Maximum number of times to passively acquire a lock before actively acquiring it.
        /// </summary>
        public int MaxIdles
        {
            get => maxIdles;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("Must be greater than 0");
                SetProperty(ref maxIdles, value);
            }
        }

        /// <summary>
        /// Gets the number of times the doer has tried to get a lock but failed.
        /// </summary>
        public int IdleCount
        {
            get => idleCount;
        }

        /// <summary>
        /// Total matches per second (instant)
        /// </summary>
        public long MatchesPerSecond => matchesPerSec;

        /// <summary>
        /// Total transactions per second (instant)
        /// </summary>
        public long TransactionsPerSecond => totalPerSec;

        #endregion Diagnostics

        int maxIdles = 10;
        int idleCount = 0;

        private Level3Update[] updates = new Level3Update[1024];

        public override bool DoWork()
        {
            if (!calibrated)
            {
                Monitor.Enter(lockObj);
            }
            else
            {
                if (!Monitor.TryEnter(lockObj))
                {
                    if (idleCount >= maxIdles) return false;

                    idleCount++;

                    if (idleCount < maxIdles)
                    {
                        return false;
                    }
                    else
                    {
                        Monitor.Enter(lockObj);
                        idleCount = 0;
                    }
                }
                else
                {
                    idleCount = 0;
                }

            }


            var c = orderBuffer.Count;

            if (c == 0)
            {
                Monitor.Exit(lockObj);
                return false;
            }

            if (updates.Length < c)
            {
                Array.Resize(ref updates, c * 2);
            }

            orderBuffer.CopyTo(updates);
            orderBuffer.Clear();

            for (int i = 0; i < c; i++)
            {
                DoNext(updates[i]);
                
                if (diagEnable)
                {
                    grandTotal++;
                    secTotalCount++;

                    if (updates[i].Subject == "match")
                    {
                        matchTotal++;
                        secMatchCount++;
                    }
                }
            }

            if (diagEnable)
            {
                if ((DateTime.UtcNow - lastMeasureTime).TotalSeconds >= 1)
                {
                    lastMeasureTime = DateTime.UtcNow;

                    matchesPerSec = secMatchCount;
                    totalPerSec = secTotalCount;

                    secMatchCount = secTotalCount = 0;
                }


            }

            Monitor.Exit(lockObj);
            return true;

        }

        /// <summary>
        /// <see cref="IObserver{T}"/> implementation for <see cref="Level3Update"/> data.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DoNext(Level3Update value)
        {
            if (disposed) return;

            var level3 = this.connectedFeed as Level3;

            if (!calibrated && !bf)
            {
                _ = Task.Run(() => level3.State = FeedState.Initializing);

                lock (lockObj)
                {
                    orderBuffer.Add(value);
                }

                if (af) return;

                af = true;
                level3.InitializeOrderBook(this.symbol).ContinueWith((t) =>
                {
                    if (FullDepthOrderBook == null)
                    {
                        af = false;
                        return;
                    }

                    Calibrate();
                    if (level3.UpdateInterval != 0) RequestPush();

                    _ = Task.Run(() =>
                    {
                        level3.State = FeedState.Running;
                    });

                    af = false;
                });

                return;
            }
            else if (value.Sequence <= fullDepth.Sequence)
            {
                return;
            }
            else if (fullDepth == null)
            {
                return;
            }

            lock (lockObj)
            {
                if (!bf && value.Sequence - fullDepth.Sequence > 1)
                {
                    Reset();
                    return;
                }

                if (value.Side == null)
                {
                    if (value.Subject == "done")
                    {
                        if (fullDepth.Asks.ContainsKey(value.OrderId))
                        {
                            fullDepth.Asks.Remove(value.OrderId);
                        }

                        if (fullDepth.Bids.ContainsKey(value.OrderId))
                        {
                            fullDepth.Bids.Remove(value.OrderId);
                        }
                    }
                }
                else if (value.Side == Side.Sell)
                {
                    SequencePieces(value.Subject, value, fullDepth.Asks, fullDepth.Bids);
                }
                else if (value.Side == Side.Buy)
                {
                    SequencePieces(value.Subject, value, fullDepth.Bids, fullDepth.Asks);
                }

                fullDepth.Sequence = value.Sequence;
                fullDepth.Timestamp = value.Timestamp ?? DateTime.Now;

            }

            //}
            //catch
            //{

            //}

            //NextObject?.Invoke(value);
        }

        /// <summary>
        /// Copy the changes from a preflight source to a live destination.
        /// </summary>
        /// <param name="src">Source data.</param>
        /// <param name="dest">Destination collection.</param>
        /// <param name="pieces">The number of pieces to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CopyTo(IList<AtomicOrderStruct> src, IList<TUnitOut> dest, int pieces)
        {
            int i, c = pieces < src.Count ? pieces : src.Count;
            int x = dest.Count;

            if (x != c)
            {
                x = 0;
                dest.Clear();

                foreach (var piece in src)
                {
                    var u = new TUnitOut()
                    {
                        Price = piece.Price,
                        Size = piece.Size,
                        OrderId = piece.OrderId,
                        Timestamp = piece.Timestamp
                    };

                    dest.Add(u);
                    if (++x == c) break;
                }
            }
            else
            {
                for (i = 0; i < c; i++)
                {
                    dest[i].Price = src[i].Price;
                    dest[i].Size = src[i].Size;
                    dest[i].Timestamp = src[i].Timestamp;
                    dest[i].OrderId = src[i].OrderId;
                }
            }
        }

        /// <summary>
        /// Push the preflight book to the live feed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnPushLive(bool auto)
        {
            if (!auto && autoPush) return;

            lock (lockObj)
            {
                if (orderBook == null)
                {
                    var ob = new TBookOut();
                    OrderBook = ob;
                }

                if (Dispatcher.Initialized)
                {
                    Dispatcher.InvokeOnMainThread((o) =>
                    {
                        if (orderBook == null) return;

                        orderBook.Sequence = fullDepth.Sequence;
                        orderBook.Timestamp = fullDepth.Timestamp;

                        CopyBook();
                        OnPropertyChanged(nameof(FullDepthOrderBook));
                    });
                }
                else
                {
                    if (orderBook == null) return;

                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = fullDepth.Timestamp;

                    CopyBook();
                    OnPropertyChanged(nameof(FullDepthOrderBook));
                }

            }

            GC.Collect(0);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CopyBook()
        {
            CopyTo(fullDepth.Asks, orderBook.Asks, pieces);
            CopyTo(fullDepth.Bids, orderBook.Bids, pieces);
        }

        #region IDisposable pattern

        protected override void Dispose(bool disposing)
        {
            if (disposed) return;

            disposed = true;

            ParallelService.UnregisterService(this);

            cts?.Cancel();
            PushThread = null;

            if (disposing)
            {
                if (connectedFeed != null)
                {
                    _ = ((Level3)connectedFeed).RemoveSymbol(symbol);
                }
            }

            connectedFeed = null;
            fullDepth = default;
            orderBook = default;
            pieces = 0;
            symbol = null;
            calibrated = false;
            initialized = false;
        }

        public override void OnCompleted()
        {
        }

        public override void OnError(Exception error)
        {
            throw error;
        }

        ~CustomLevel3Observation()
        {
            Dispose(false);
        }

        #endregion

    }

}
