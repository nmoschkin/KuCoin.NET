using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using System.ComponentModel;
using System.Text;

namespace Kucoin.NET.Websockets.Distribution
{
    /// <summary>
    /// Contains an observable version of internal information.
    /// </summary>
    public interface IObservableCopy : INotifyPropertyChanged
    {
        /// <summary>
        /// Copy the internal contents of the object to an observed location.
        /// </summary>
        void CopyToObservable();

        /// <summary>
        /// Prefer that the copy method be executed on the main thread.
        /// </summary>
        bool PreferDispatcher { get; }

        /// <summary>
        /// The interval between pushes, in milliseconds.
        /// </summary>
        /// <remarks>
        /// This should be a value divisible by 10.
        /// </remarks>
        int Interval { get; set; }

    }

    /// <summary>
    /// Contains an observable version of internal information.
    /// </summary>
    public interface IObservableCopy<TInternal, TObservable> : IObservableCopy 
    {
        /// <summary>
        /// The internal copy of the data we observing.
        /// </summary>
        /// <remarks>
        /// This is data that can change rapidly and is likely not observable.
        /// </remarks>
        TInternal InternalData { get; }

        /// <summary>
        /// The public-facing copy of the data we are observing.
        /// </summary>
        /// <remarks>
        /// This data is likely to implement <see cref="INotifyPropertyChanged"/> or <see cref="INotifyCollectionChanged"/>.
        /// </remarks>
        TObservable ObservableData { get; }

    }

}
