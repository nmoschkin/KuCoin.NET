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
using Kucoin.NET.Data.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Observations
{
    /// <summary>
    /// Standard Spot Market Level 2 observation and order book provider.
    /// </summary>
    public class Level2Observation : Level2ObservationBase<OrderBook<OrderUnit>, OrderUnit, Level2Update>, ILevel2OrderBookProvider
    {
    
        protected bool calibrated;
        protected bool initialized;

        protected List<Level2Update> orderBuffer = new List<Level2Update>();
        internal Level2Observation(KucoinBaseWebsocketFeed parent, string symbol, int pieces = 50) : base(parent, symbol, pieces)
        {
        }

        public override void RequestPush()
        {
            lock(lockObj)
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
        protected void SequencePieces(IList<OrderUnit> changes, KeyedCollection<decimal, OrderUnit> pieces)
        {
            foreach (var change in changes)
            {
                decimal cp = change.Price;

                if (change.Size == 0.0M)
                {
                    pieces.Remove(cp);
                }                
                else
                {
                    if (pieces.Contains(cp))
                    {
                        var piece = pieces[cp];

                        piece.Size = change.Size;
                        if (piece is ISequencedOrderUnit seqpiece && change is ISequencedOrderUnit seqchange)
                            seqpiece.Sequence = seqchange.Sequence;
                    }
                    else
                    {
                        pieces.Add(change);
                    }
                }
            }
        }

        /// <summary>
        /// De-initialize and clear all data and begin a new calibration.
        /// 
        /// </summary>
        public override void Reset()
        {
            orderBuffer = new List<Level2Update>();

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
                if (q.SequenceStart > fullDepth.Sequence) OnNext(q);
            }

            orderBuffer.Clear();

            OnPropertyChanged(nameof(Calibrated));
        }

        /// <summary>
        /// <see cref="IObserver{T}"/> implementation for <see cref="Level2Update"/> data.
        /// </summary>
        /// <param name="value"></param>
        public override void OnNext(Level2Update value)
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

                    SequencePieces(value.Changes.Asks, fullDepth.Asks);
                    SequencePieces(value.Changes.Bids, fullDepth.Bids);

                    fullDepth.Sequence = value.SequenceEnd;
                    fullDepth.Timestamp = DateTime.Now;
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
            lock(lockObj)
            {
                if (orderBook == null)
                {
                    var ob = new OrderBook<OrderUnit>();
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
                    ((Level2)connectedFeed).RemoveSymbol(symbol).Wait();
                }
            }

            lock(lockObj)
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

        ~Level2Observation()
        {
            Dispose(false);
        }

        #endregion

    }


}
