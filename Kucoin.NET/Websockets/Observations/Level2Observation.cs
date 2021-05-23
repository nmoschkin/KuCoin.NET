﻿using System;
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

namespace Kucoin.NET.Websockets.Observations
{
    public interface ILevel2OrderBookProvider : INotifyPropertyChanged, IDisposable, ISymbol, IObserver<Level2Update>
    {
        /// <summary>
        /// The sorted and sliced order book (user-facing data).
        /// </summary>
        OrderBook OrderBook { get; }

        /// <summary>
        /// The number of pieces to push to the order book from the preflight book.
        /// </summary>
        int Pieces { get; }

        /// <summary>
        /// The raw preflight (full depth) order book.
        /// </summary>
        OrderBook FullDepthOrderBook { get; }

        /// <summary>
        /// Specifies whether or not the order size is updated for each price in the live book
        /// or only in the preflight book.
        /// </summary>
        bool UseObservableOrders { get; set; }

        /// <summary>
        /// Reset and de-initialize the feed to trigger recalibration.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Level 2 Observation Cache
    /// </summary>
    public class Level2Observation : ObservableBase, ILevel2OrderBookProvider
    {
        protected OrderBook preflightBook;
        protected OrderBook orderBook;
        protected string symbol;
        protected int pieces;
        protected bool useObservableOrders = true;
        protected bool calibrated; 
        protected bool initialized; 
        protected Level2 connectedFeed;

        protected List<Level2Update> orderBuffer = new List<Level2Update>();

        internal Level2Observation(Level2 parent, string symbol, int pieces = 50)
        {
            this.symbol = symbol;

            this.connectedFeed = parent;
            this.pieces = pieces;

            orderBook = new OrderBook();
        }


        public Level2 ConnectedFeed => !disposed ? connectedFeed : throw new ObjectDisposedException(nameof(Level2Observation));

        public virtual IList<Level2Update> OrderBuffer => orderBuffer;

        public string Symbol => !disposed ? symbol : throw new ObjectDisposedException(nameof(Level2Observation));

        public virtual bool UseObservableOrders
        {
            get => useObservableOrders;
            set
            {
                SetProperty(ref useObservableOrders, value);
            }
        }

        void ISymbol.SetSymbol(string symbol)
        {
            base.SetProperty(ref this.symbol, symbol);
        }

        /// <summary>
        /// The number of pieces to serve to the live order book feed.
        /// </summary>
        public int Pieces
        {
            get => !disposed ? pieces : throw new ObjectDisposedException(nameof(Level2Observation));
            internal set
            {
                SetProperty(ref pieces, value);
            }
        }

        /// <summary>
        /// Gets the full-depth order book.
        /// </summary>
        public OrderBook FullDepthOrderBook
        {
            get => preflightBook;
            internal set
            {
                SetProperty(ref preflightBook, value);
            }
        }

        /// <summary>
        /// Gets the order book truncated to <see cref="Pieces"/> asks and bids.
        /// </summary>
        public OrderBook OrderBook
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
        public bool Initialized
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
        public bool Calibrated
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
        protected void RemovePiece(IList<OrderUnit> pieces, decimal price)
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
        protected void SequencePieces(IList<OrderUnit> changes, ObservableOrderUnits pieces)
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
                        piece.Sequence = change.Sequence;
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
        public void Reset()
        {
            orderBuffer = new List<Level2Update>();

            Initialized = false;
            Calibrated = false;
        }

        /// <summary>
        /// Calibrate the order book from cached data.
        /// </summary>
        public void Calibrate()
        {
            calibrated = true;

            foreach (var q in orderBuffer)
            {
                if (q.SequenceStart > preflightBook.Sequence) OnNext(q);
            }

            orderBuffer.Clear();

            OnPropertyChanged(nameof(Calibrated));
        }

        /// <summary>
        /// <see cref="IObserver{T}"/> implementation for <see cref="Level2Update"/> data.
        /// </summary>
        /// <param name="value"></param>
        public void OnNext(Level2Update value)
        {
            if (disposed || preflightBook == null) return;

            lock (preflightBook)
            {
                try
                {
                    if (!calibrated)
                    {
                        orderBuffer.Add(value);
                        return;
                    }

                    SequencePieces(value.Changes.Asks, preflightBook.Asks);
                    SequencePieces(value.Changes.Bids, preflightBook.Bids);

                    preflightBook.Sequence = value.SequenceEnd;
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
        protected void CopyTo(IList<OrderUnit> src, IList<OrderUnit> dest, int pieces, bool clone = false)
        {
            int i, c = pieces < src.Count ? pieces : src.Count;
            int x = dest.Count;

            if (x != c)
            {
                x = 0;
                dest.Clear();

                if (!clone)
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
                if (!clone)
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
        public void PushPreflight()
        {
            if (orderBook == null)
            {
                var ob = new OrderBook();
                OrderBook = ob;
            }

            if (Dispatcher.Initialized)
            {
                Dispatcher.InvokeOnMainThread((o) =>
                {
                    orderBook.Sequence = preflightBook.Sequence;
                    orderBook.Time = EpochTime.DateToNanoseconds(DateTime.Now);

                    CopyBook();
                });
            }
            else
            {
                orderBook.Sequence = preflightBook.Sequence;
                orderBook.Time = EpochTime.DateToNanoseconds(DateTime.Now);

                CopyBook();
            }
        }

        protected void CopyBook()
        {
            CopyTo(preflightBook.Asks, orderBook.Asks, pieces, !useObservableOrders);
            CopyTo(preflightBook.Bids, orderBook.Bids, pieces, !useObservableOrders);
        }

        public override string ToString() => $"{symbol} : {pieces}";


        #region IDisposable pattern

        /// <summary>
        /// Terminate the feed for this symbol from the server, nullify all resources and remove this object from the parent feed's list of listeners.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        public bool disposed;

        protected void Dispose(bool disposing)
        {
            if (disposed) return;

            disposed = true;

            if (disposing)
            {
                if (connectedFeed != null)
                {
                    connectedFeed.RemoveSymbol(symbol).Wait();
                }
            }

            lock (preflightBook)
            {
                connectedFeed = null;
                preflightBook = null;
                orderBook = null;
                pieces = 0;
                symbol = null;
                calibrated = false;
                initialized = false;
            }
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
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
