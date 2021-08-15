using System;
using System.Collections.Generic;
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
        TInternal InternalData { get; }

        TObservable ObservableData { get; }

    }

}
