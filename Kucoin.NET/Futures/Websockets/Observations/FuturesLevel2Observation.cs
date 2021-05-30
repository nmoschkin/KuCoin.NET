using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Futures.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets;
using Kucoin.NET.Websockets.Observations;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Websockets.Observations
{
    /// <summary>
    /// KuCoin Futures Level 2 Observation standard implementation.
    /// </summary>
    public class FuturesLevel2Observation : Level2ObservationBase<FuturesOrderBook, OrderUnit, FuturesLevel2Update>
    {
        protected bool calibrated;
        protected bool initialized;

        protected List<FuturesLevel2Update> orderBuffer = new List<FuturesLevel2Update>();
        internal FuturesLevel2Observation(KucoinBaseWebsocketFeed parent, string symbol, int pieces = 50) : base(parent, symbol, pieces)
        {
        }

        public override void RequestPush()
        {
            lock (lockObj)
            {
                pushRequested = true;
            }
        }

        public override bool ObservablePieces => true;

        /// <summary>
        /// Gets a value indicating that this order book is initialized with the full-depth (preflight) order book.
        /// </summary>
        public override bool Initialized
        {
            get => !disposed ? initialized : throw new ObjectDisposedException(nameof(FuturesLevel2Observation));
            internal set
            {
                if (disposed) throw new ObjectDisposedException(nameof(FuturesLevel2Observation));
                SetProperty(ref initialized, value);
            }
        }

        /// <summary>
        /// Gets a value indicating that this observation has been calibrated.
        /// </summary>
        public override bool Calibrated
        {
            get => !disposed ? calibrated : throw new ObjectDisposedException(nameof(FuturesLevel2Observation));
            protected set
            {
                if (disposed) throw new ObjectDisposedException(nameof(FuturesLevel2Observation));
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
        /// <param name="change">The change to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        protected void SequencePieces(FuturesLevel2Update change, SortedKeyedOrderUnitBase<OrderUnit> pieces)
        {
            decimal cp = change.Change.Price;

            if (change.Change.Size == 0.0M)
            {
                pieces.Remove(cp);
            }
            else
            {
                if (pieces.Contains(cp))
                {
                    var piece = pieces[cp];

                    piece.Size = change.Change.Size;

                    if (piece is ISequencedOrderUnit seqpiece && change is ISequencedOrderUnit seqchange)
                        seqpiece.Sequence = seqchange.Sequence;
                }
                else
                {
                    var ou = new OrderUnit();

                    ou.Price = change.Change.Price;
                    ou.Size = change.Change.Size;
                    ou.Sequence = change.Sequence;

                    pieces.Add(ou);
                }
            }
        }

        /// <summary>
        /// De-initialize and clear all data and begin a new calibration.
        /// 
        /// </summary>
        public override void Reset()
        {
            orderBuffer = new List<FuturesLevel2Update>();

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
        /// <see cref="IObserver{T}"/> implementation for <see cref="FuturesLevel2Update"/> data.
        /// </summary>
        /// <param name="value"></param>
        public override void OnNext(FuturesLevel2Update value)
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

                    if (value.Change.Side == Side.Buy)
                    {

                        SequencePieces(value, fullDepth.Bids);

                    }
                    else
                    {
                        SequencePieces(value, fullDepth.Asks);

                    }

                    fullDepth.Sequence = value.Sequence;
                }
                catch (Exception ex)
                {
                    string e = ex.Message;
                }
            }
        }

        /// <summary>
        /// Copy the changes from a preflight source to a live destination.
        /// </summary>
        /// <param name="src">Source data.</param>
        /// <param name="dest">Destination collection.</param>
        /// <param name="pieces">The number of pieces to copy.</param>
        /// <param name="clone">True to clone the observable objects so that their changes do not show up in the live feed.</param>
        protected void CopyTo(IList<OrderUnit> src, IList<OrderUnit> dest, int pieces)
        {
            int i, c = pieces < src.Count ? pieces : src.Count;
            int x = dest.Count;

            if (x != c)
            {
                x = 0;
                dest.Clear();

                if (ObservablePieces)
                {
                    foreach (var piece in src)
                    {
                        dest.Add(piece);
                        if (++x == c) break;
                    }
                }
                else
                {
                    foreach (var piece in src)
                    {
                        dest.Add(piece.Clone());
                        if (++x == c) break;
                    }
                }
            }
            else
            {
                if (ObservablePieces)
                {
                    for (i = 0; i < c; i++)
                    {
                        dest[i] = src[i];
                    }
                }
                else
                {
                    for (i = 0; i < c; i++)
                    {
                        dest[i] = src[i].Clone();
                    }
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
                    var ob = new FuturesOrderBook();
                    OrderBook = ob;
                }

                if (Dispatcher.Initialized)
                {
                    Dispatcher.InvokeOnMainThread((o) =>
                    {
                        orderBook.Sequence = fullDepth.Sequence;
                        orderBook.Timestamp = DateTime.Now;

                        CopyBook();
                    });
                }
                else
                {
                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = DateTime.Now;

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
                    ((Level2Base<FuturesOrderBook, OrderUnit, FuturesLevel2Update, FuturesLevel2Observation>)connectedFeed).RemoveSymbol(symbol).Wait();
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

        ~FuturesLevel2Observation()
        {
            Dispose(false);
        }

        #endregion

    }
}
