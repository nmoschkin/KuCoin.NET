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
        int Pieces { get; set; }

        /// <summary>
        /// The raw preflight (full depth) order book.
        /// </summary>
        OrderBook FullDepthOrderBook { get; }

        /// <summary>
        /// Specifies whether or not the order size is updated for each price in the live book
        /// or only in the preflight book.
        /// </summary>
        bool LiveOrderSizeUpdates { get; set; }

        /// <summary>
        /// Reset and de-initialize the feed to trigger recalibration.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Level 2 Observation Cache
    /// </summary>
    public sealed class Level2Observation : ObservableBase, ILevel2OrderBookProvider
    {
        private Thread orgThread;
        private OrderBook preflightBook;
        private OrderBook orderBook;
        private string sym;
        private int pieces;
        private bool liveOrderSizeUpdates = true;
        private bool cal;
        private bool init;
        private Level2 parent;

        private List<Level2Update> calCache = new List<Level2Update>();

        public Level2 Parent => !disposed ? parent : throw new ObjectDisposedException(nameof(Level2Observation));

        private List<Level2Update> Cache => calCache;

        public bool LiveOrderSizeUpdates
        {
            get => liveOrderSizeUpdates;
            set
            {
                SetProperty(ref liveOrderSizeUpdates, value);
            }
        }

        internal Level2Observation(Level2 parent, string symbol, int pieces = 20)
        {
            orgThread = Thread.CurrentThread;
            sym = symbol;

            this.parent = parent;
            this.pieces = pieces;
        }

        public string Symbol => !disposed ? sym : throw new ObjectDisposedException(nameof(Level2Observation));

        void ISymbol.SetSymbol(string symbol)
        {
            SetProperty(ref sym, symbol);
        }

        /// <summary>
        /// The number of pieces to serve to the live order book feed.
        /// </summary>
        public int Pieces
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
            get => !disposed ? init : throw new ObjectDisposedException(nameof(Level2Observation));
            internal set
            {
                if (disposed) throw new ObjectDisposedException(nameof(Level2Observation));
                SetProperty(ref init, value);
            }
        }

        /// <summary>
        /// Gets a value indicating that this observation has been calibrated.
        /// </summary>
        public bool Calibrated
        {
            get => !disposed ? cal : throw new ObjectDisposedException(nameof(Level2Observation));
            private set
            {
                if (disposed) throw new ObjectDisposedException(nameof(Level2Observation));
                SetProperty(ref cal, value);
            }
        }

        /// <summary>
        /// Remove the piece from the order book.
        /// </summary>
        /// <param name="pieces"></param>
        /// <param name="price"></param>
        private void RemovePiece(ObservableCollection<OrderUnit> pieces, decimal price)
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
        /// Trim pieces older than the given sequence from the order book.
        /// </summary>
        /// <param name="pieces"></param>
        /// <param name="sequence"></param>
        private void TrimPieces(ObservableCollection<OrderBook> pieces, long sequence)
        {
            int i, c = pieces.Count;
            for (i = c - 1; i >= 0; i--)
            {
                if (pieces[i].Sequence < sequence)
                {
                    pieces.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="changes">The changes to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        private void SequencePieces(List<OrderUnit> changes, ObservableCollection<OrderUnit> pieces)
        {
            bool f;

            foreach (var change in changes)
            {
                f = false;

                if (change.Size == 0.0M)
                {
                    RemovePiece(pieces, change.Price);
                }                
                else
                {
                    foreach (var piece in pieces)
                    {
                        if (change.Price == piece.Price)
                        {
                            f = true;

                            piece.Size = change.Size;
                            piece.Sequence = change.Sequence;

                            break;
                        }
                    }

                    if (!f)
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
            calCache = new List<Level2Update>();

            Initialized = false;
            Calibrated = false;
        }

        /// <summary>
        /// Calibrate the order book from cached data.
        /// </summary>
        public void Calibrate()
        {
            cal = true;

            foreach (var q in calCache)
            {
                if (q.SequenceStart > preflightBook.Sequence) OnNext(q);
            }

            calCache.Clear();

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
                    if (!cal)
                    {
                        calCache.Add(value);
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
        /// Sort the given pieces by price.
        /// </summary>
        /// <param name="pieces">The pieces to sort.</param>
        /// <param name="desc">Sort descending (for bids.)</param>
        private void SortByPrice(List<OrderUnit> pieces, bool desc)
        {
            if (desc)
            {
                pieces.Sort((a, b) => a.Price < b.Price ? 1 : a.Price > b.Price ? -1 : 0);
            }
            else
            {
                pieces.Sort((a, b) => a.Price < b.Price ? -1 : a.Price > b.Price ? 1 : 0);
            }
        }

        /// <summary>
        /// Copy the changes from a preflight source to a live destination.
        /// </summary>
        /// <param name="src">Source data.</param>
        /// <param name="dest">Destination collection.</param>
        /// <param name="pieces">The number of pieces to copy.</param>
        /// <param name="clone">True to clone the observable objects so that their changes do not show up in the live feed.</param>
        private void CopyTo(List<OrderUnit> src, ObservableCollection<OrderUnit> dest, int pieces, bool clone = false)
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
        internal void PushPreflight()
        {
            Dispatcher.InvokeOnMainThread((o) =>
            {
                if (preflightBook == null) return;

                if (OrderBook == null)
                {
                    OrderBook = new OrderBook();
                }

                orderBook.Sequence = preflightBook.Sequence;
                orderBook.Time = EpochTime.DateToNanoseconds(DateTime.Now);

                var lbid = new List<OrderUnit>(preflightBook.Bids);
                var lask = new List<OrderUnit>(preflightBook.Asks);

                SortByPrice(lask, false);
                CopyTo(lask, orderBook.Asks, pieces, !liveOrderSizeUpdates);

                SortByPrice(lbid, true);
                CopyTo(lbid, orderBook.Bids, pieces, !liveOrderSizeUpdates);

            });
        }

        public override string ToString() => $"{sym} : {pieces}";


        #region IDisposable pattern

        /// <summary>
        /// Terminate the feed for this symbol from the server, nullify all resources and remove this object from the parent feed's list of listeners.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        public bool disposed;

        private void Dispose(bool disposing)
        {
            if (disposed) return;

            disposed = true;

            if (disposing)
            {
                if (parent != null)
                {
                    parent.RemoveSymbol(sym).Wait();
                }
            }

            lock (preflightBook)
            {
                parent = null;
                preflightBook = null;
                orderBook = null;
                pieces = 0;
                sym = null;
                cal = false;
                init = false;
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
