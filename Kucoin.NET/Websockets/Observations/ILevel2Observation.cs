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
using System.Runtime.InteropServices;

namespace Kucoin.NET.Websockets.Observations
{

    public class OrderBookUpdatedEventArgs<TBook, TUnit> : EventArgs
        where TBook : IOrderBook<TUnit>
        where TUnit : IOrderUnit
    {

        public string Symbol { get; private set; }

        public TBook OrderBook { get; private set; }

        public OrderBookUpdatedEventArgs(string symbol, TBook book)
        {
            Symbol = symbol;
            OrderBook = book;
        }

    }

    public interface ILevel2OrderBookProvider : ILevel2OrderBookProvider<OrderBook<OrderUnit>, OrderUnit, Level2Update>
    {
    }

    public interface ILevel2OrderBookProvider<TBook, TUnit, TUpdate> :
        INotifyPropertyChanged,
        IDisposable,
        ISymbol,
        IObserver<TUpdate>
        where TBook : IOrderBook<TUnit>
        where TUnit : IOrderUnit
    {
        event EventHandler<OrderBookUpdatedEventArgs<TBook, TUnit>> OrderBookUpdated;

        /// <summary>
        /// The sorted and sliced order book (user-facing data).
        /// </summary>
        TBook OrderBook { get; }

        /// <summary>
        /// The number of pieces to push to the order book from the preflight book.
        /// </summary>
        int Pieces { get; }

        /// <summary>
        /// The raw preflight (full depth) order book.
        /// </summary>
        TBook FullDepthOrderBook { get; }


        /// <summary>
        /// Reset and reinitialize the feed to trigger recalibration.
        /// </summary>
        void Reset();

        /// <summary>
        /// True if the object is disposed and no longer usable.
        /// </summary>
        bool Disposed { get; }
    }

}
