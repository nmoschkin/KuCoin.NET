using System;

using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;

using System.ComponentModel;

namespace Kucoin.NET.Websockets.Observations
{

    /// <summary>
    /// Standard level 2 order book provider interface.
    /// </summary>
    public interface ILevel3OrderBookProvider : ILevel3OrderBookProvider<ObservableAtomicOrderBook<ObservableAtomicOrderUnit>, ObservableAtomicOrderUnit, KeyedAtomicOrderBook<AtomicOrderStruct>, AtomicOrderStruct, Level3Update>
    {
    }

    /// <summary>
    /// Level 2 order book provider interface.
    /// </summary>
    /// <typeparam name="TBookOut">The type of order book (must implement <see cref="IOrderBook{T}"/> of <see cref="TUnitOut"/>.)</typeparam>
    /// <typeparam name="TUnitOut">The type of the order unit (must implement <see cref="IOrderUnit"/>.)</typeparam>
    /// <typeparam name="TUpdate">The type of the update object pushed with <see cref="IObserver{T}.OnNext(T)"/>.</typeparam>
    public interface ILevel3OrderBookProvider<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate> :
        INotifyPropertyChanged,
        IDisposable,
        ISymbol,
        IObserver<TUpdate>
        where TBookOut : IAtomicOrderBook<TUnitOut>
        where TUnitOut : IAtomicOrderUnit
        where TBookIn : KeyedAtomicOrderBook<TUnitIn>
        where TUnitIn : IAtomicOrderUnit, new()
        where TUpdate : new()
    {
        
        
        /// <summary>
        /// Event that is fired when the Observable order book is updated
        /// </summary>
        event EventHandler<Level3OrderBookUpdatedEventArgs<TBookOut, TUnitOut>> OrderBookUpdated;

        /// <summary>
        /// Event that is fired for every new object received
        /// </summary>
        event OnNextHandler<TUpdate> NextObject;

        /// <summary>
        /// The sorted and sliced order book (user-facing data).
        /// </summary>
        TBookOut OrderBook { get; }

        /// <summary>
        /// The number of pieces to push to the order book from the preflight book.
        /// </summary>
        int Pieces { get; }

        /// <summary>
        /// The raw preflight (full depth) order book.
        /// </summary>
        TBookIn FullDepthOrderBook { get; }

        /// <summary>
        /// Gets the K-Line type to apply to volume refresh cycles.
        /// </summary>
        KlineType VolumeTime { get; set; }

        /// <summary>
        /// Gets the volume of the current K-line as calculated by matches.
        /// </summary>
        decimal Level3Volume { get; }

        /// <summary>
        /// Reset and reinitialize the feed to trigger recalibration.
        /// </summary>
        void Reset();

        /// <summary>
        /// True if the object is disposed and no longer usable.
        /// </summary>
        bool Disposed { get; }

        void RequestPush();
    }

}
