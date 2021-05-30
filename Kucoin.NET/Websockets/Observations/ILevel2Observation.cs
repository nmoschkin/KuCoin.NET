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

    /// <summary>
    /// Standard level 2 order book provider interface.
    /// </summary>
    public interface ILevel2OrderBookProvider : ILevel2OrderBookProvider<OrderBook<OrderUnit>, OrderUnit, Level2Update>
    {
    }

    /// <summary>
    /// Level 2 order book provider interface.
    /// </summary>
    /// <typeparam name="TBook">The type of order book (must implement <see cref="IOrderBook{T}"/> of <see cref="TUnit"/>.)</typeparam>
    /// <typeparam name="TUnit">The type of the order unit (must implement <see cref="IOrderUnit"/>.)</typeparam>
    /// <typeparam name="TUpdate">The type of the update object pushed with <see cref="IObserver{T}.OnNext(T)"/>.</typeparam>
    public interface ILevel2OrderBookProvider<TBook, TUnit, TUpdate> :
        INotifyPropertyChanged,
        IDisposable,
        ISymbol,
        IObserver<TUpdate>
        where TBook : IOrderBook<TUnit>
        where TUnit : IOrderUnit
        where TUpdate : new()
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
