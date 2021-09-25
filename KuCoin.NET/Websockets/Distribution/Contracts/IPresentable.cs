using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using System.ComponentModel;
using System.Text;

namespace KuCoin.NET.Websockets.Distribution
{
    /// <summary>
    /// An object that presents data.
    /// </summary>
    public interface IPresentable : INotifyPropertyChanged
    {
        /// <summary>
        /// Present the data.
        /// </summary>
        void PresentData();

        /// <summary>
        /// Prefer that the copy method be executed on the main thread.
        /// </summary>
        [DefaultValue(true)]
        bool PreferDispatcher { get; }

        /// <summary>
        /// The interval between pushes, in milliseconds.
        /// </summary>
        /// <remarks>
        /// This should be a value divisible by 10.
        /// </remarks>
        [DefaultValue(10)]
        int Interval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the presentation observation is disabled, and pushes do not occur.
        /// </summary>
        bool IsPresentationDisabled { get; set; }
    }

    /// <summary>
    /// Maintains data for presentation.
    /// </summary>
    public interface IPresentable<TPresented> : IPresentable
    {

        /// <summary>
        /// The presentation version of the data.
        /// </summary>
        /// <remarks>
        /// This data is likely to implement <see cref="INotifyPropertyChanged"/> or <see cref="INotifyCollectionChanged"/>.
        /// </remarks>
        TPresented PresentedData { get; }

    }

    /// <summary>
    /// Maintains a version of the data for presentation.
    /// </summary>
    public interface IPresentable<TInternal, TPresented> : IPresentable<TPresented>, IInternalData<TInternal>
    {


    }

}
