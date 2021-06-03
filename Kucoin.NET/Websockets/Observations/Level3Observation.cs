using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kucoin.NET.Websockets.Observations
{

    public class Level3Observation : CustomLevel3Observation<AtomicOrderBook<AtomicOrderUnit>, AtomicOrderUnit>, ILevel3OrderBookProvider
    {
        public Level3Observation(KucoinBaseWebsocketFeed parent, string symbol, int pieces = 50) : base(parent, symbol, pieces)
        {
        }

    }


    /// <summary>
    /// Custom level 2 observation for custom Level 2 feed implementations.
    /// </summary>
    /// <typeparam name="TBook">The type of your custom order book.</typeparam>
    /// <typeparam name="TUnit">The type of your custom order pieces.</typeparam>
    public class CustomLevel3Observation<TBook, TUnit> : Level3ObservationBase<TBook, TUnit, Level3Update>
        where TBook : IAtomicOrderBook<TUnit>, new()
        where TUnit : IAtomicOrderUnit, new()
    {

        protected bool calibrated;
        protected bool initialized;

        protected List<Level3Update> orderBuffer = new List<Level3Update>();

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
        /// Remove the piece from the order book.
        /// </summary>
        /// <param name="pieces"></param>
        /// <param name="price"></param>
        protected void RemovePiece(IList<IOrderUnit> pieces, decimal price)
        {
            int i, c = pieces.Count;

            for (i = 0; i < c; i++)
            {
                if (pieces[i].Price == price)
                {
                    pieces.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="changes">The changes to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        protected void SequencePieces(Level3Update change, KeyedCollection<string, TUnit> pieces, KeyedCollection<string, TUnit> otherPieces)
        {

            if (change.Subject == "done")
            {
                pieces.Remove(change.OrderId);

            }
            else if (change.Subject == "open")
            {
                if (change.Price == null || change.Price == 0 || change.Size == null || change.Size == 0) return;

                var u = new TUnit();

                u.Price = (decimal)change.Price;
                u.Size = (decimal)change.Size;
                u.Timestamp = (DateTime)change.Timestamp;
                u.OrderId = change.OrderId;

                pieces.Add(u);
            }
            else if (change.Subject == "change")
            {

                if (pieces.Contains(change.OrderId))
                {
                    pieces[change.OrderId].Price = (decimal)change.Price;
                    pieces[change.OrderId].Size = (decimal)change.Size;
                    pieces[change.OrderId].Timestamp = (DateTime)change.Timestamp;

                }
            }
            else if (change.Subject == "match")
            {
                if (otherPieces.Contains(change.MakerOrderId))
                {
                    var o = otherPieces[change.MakerOrderId];
                    o.Size -= (decimal)change.Size;

                    //if (o.Size == 0)
                    //{
                    //    otherPieces.Remove(o.OrderId);
                    //}
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

        /// <summary>
        /// Calibrate the order book from cached data.
        /// </summary>
        public override void Calibrate()
        {
            calibrated = true;

            foreach (var q in orderBuffer)
            {
                if (q.Sequence > fullDepth.Sequence) OnNext(q);
            }

            orderBuffer.Clear();

            OnPropertyChanged(nameof(Calibrated));
        }

        /// <summary>
        /// <see cref="IObserver{T}"/> implementation for <see cref="Level3Update"/> data.
        /// </summary>
        /// <param name="value"></param>
        public override void OnNext(Level3Update value)
        {
            if (disposed || fullDepth == null) return;

            lock (lockObj)
            {
                try
                {
                    if (!calibrated)
                    {
                        orderBuffer.Add(value);
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
                    fullDepth.Timestamp = DateTime.Now;
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Copy the changes from a preflight source to a live destination.
        /// </summary>
        /// <param name="src">Source data.</param>
        /// <param name="dest">Destination collection.</param>
        /// <param name="pieces">The number of pieces to copy.</param>
        protected void CopyTo(IList<TUnit> src, IList<TUnit> dest, int pieces)
        {
            int i, c = pieces < src.Count ? pieces : src.Count;
            int x = dest.Count;

            if (x != c)
            {
                x = 0;
                dest.Clear();

                foreach (var piece in src)
                {
                    dest.Add(piece);
                    if (++x == c) break;
                }
            }
            else
            {
                for (i = 0; i < c; i++)
                {
                    dest[i] = src[i];
                }
            }

        }

        /// <summary>
        /// Push the preflight book to the live feed.
        /// </summary>
        protected override void PushLive()
        {
            lock (lockObj)
            {
                if (orderBook == null)
                {
                    var ob = new TBook();
                    OrderBook = ob;
                }

                if (Dispatcher.Initialized)
                {
                    Dispatcher.InvokeOnMainThread((o) =>
                    {
                        orderBook.Sequence = fullDepth.Sequence;
                        orderBook.Timestamp = fullDepth.Timestamp;

                        CopyBook();
                    });
                }
                else
                {
                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = fullDepth.Timestamp;

                    CopyBook();
                }
            }
        }

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
            PushThread.Dispose();
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
