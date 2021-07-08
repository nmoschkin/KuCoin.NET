using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
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
        protected void SequencePieces(Level3Update change, Level3KeyedCollection<AtomicOrderStruct> pieces, Level3KeyedCollection<AtomicOrderStruct> otherPieces)
        {

            if (change.Subject == "done")
            {
                pieces.Remove(change.OrderId);

            }
            else if (change.Subject == "open")
            {
                if (change.Price == null || change.Price == 0 || change.Size == null || change.Size == 0) return;

                var u = new AtomicOrderStruct
                {
                    Price = (decimal)change.Price,
                    Size = (decimal)change.Size,
                    Timestamp = (DateTime)change.Timestamp,
                    OrderId = change.OrderId
                };

                pieces.Add(u);
            }
            else if (change.Subject == "change")
            {
                if (pieces.Contains(change.OrderId))
                {
                    var piece = pieces[change.OrderId];
                
                    pieces.Remove(piece.OrderId);

                    piece.Size = (decimal)change.Size;
                    piece.Timestamp = (DateTime)change.Timestamp;
                    piece.Price = (decimal)change.Price;

                    pieces.Add(piece);
                }
            }
            else if (change.Subject == "match")
            {
                if (otherPieces.Contains(change.MakerOrderId))
                {
                    var o = otherPieces[change.MakerOrderId];
                    var p = (decimal)change.Price;
                    decimal csize = (decimal)change.Size;

                    o.Size -= csize;

                    if (o.Size == 0)
                    {
                        otherPieces.Remove(change.MakerOrderId);
                    }

                    // A match is a real component of volume.
                    // we can keep our own tally of the market volume per k-line.
                    if (updVol) Level3Volume += csize * p;
                }
            }
        }

        /// <summary>
        /// De-initialize and clear all data and begin a new calibration.
        /// 
        /// </summary>
        public override void Reset()
        {
            orderBuffer = new List<Level3Update>();

            Initialized = false;
            Calibrated = false;
        }
        bool bf = false;

        /// <summary>
        /// Calibrate the order book from cached data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Calibrate()
        {
            calibrated = true;

            bf = true;
            foreach (var q in orderBuffer)
            {
                if (q.Sequence > fullDepth.Sequence) OnNext(q);
            }
            bf = false;
            orderBuffer.Clear();

            OnPropertyChanged(nameof(Calibrated));
        }

        /// <summary>
        /// <see cref="IObserver{T}"/> implementation for <see cref="Level3Update"/> data.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnNext(Level3Update value)
        {
            if (disposed) return;

            lock (lockObj)
            {
                try
                {
                    if (!calibrated)
                    {
                        orderBuffer.Add(value);
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

                    if (!bf && value.Sequence - fullDepth.Sequence > 1)
                    {
                        Reset();
                        return;
                    }

                    if (value.Side == Side.Sell)
                    {
                        SequencePieces(value, fullDepth.Asks, fullDepth.Bids);
                    }
                    else if (value.Side == Side.Buy)
                    {
                        SequencePieces(value, fullDepth.Bids, fullDepth.Asks);
                    }
                    else
                    {
                        if (value.Subject == "done")
                        {
                            if (fullDepth.Asks.Contains(value.OrderId))
                            {
                                fullDepth.Asks.Remove(value.OrderId);
                            }

                            if (fullDepth.Bids.Contains(value.OrderId))
                            {
                                fullDepth.Bids.Remove(value.OrderId);
                            }
                        }
                    }

                    fullDepth.Sequence = value.Sequence;
                    fullDepth.Timestamp = value.Timestamp ?? DateTime.Now;
                }
                catch
                {

                }
            }

            NextObject?.Invoke(value);
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
                        orderBook.Sequence = fullDepth.Sequence;
                        orderBook.Timestamp = fullDepth.Timestamp;

                        CopyBook();
                        OnPropertyChanged(nameof(FullDepthOrderBook));
                    });
                }
                else
                {
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

            cts?.Cancel();
            PushThread = null;

            if (disposing)
            {
                if (connectedFeed != null)
                {
                    ((Level3)connectedFeed).RemoveSymbol(symbol).Wait();
                }
            }

            lock (lockObj)
            {
                connectedFeed = null;
                fullDepth = default;
                orderBook = default;
                pieces = 0;
                symbol = null;
                calibrated = false;
                initialized = false;
            }
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
