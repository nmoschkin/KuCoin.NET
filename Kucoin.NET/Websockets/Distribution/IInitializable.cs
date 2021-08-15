using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Websockets.Distribution
{
    public interface IInitializable<TKey, TValue>
    {
        /// <summary>
        /// Sets the data provider for the initial data.
        /// </summary>
        /// <param name="dataProvider">An object that provides initial data.</param>
        void SetInitialDataProvider(IInitialDataProvider<TKey, TValue> dataProvider);

        IInitialDataProvider<TKey, TValue> DataProvider { get; }

        /// <summary>
        /// Returns true if the data provider is available.
        /// </summary>
        bool IsDataProviderAvailable { get; }

        /// <summary>
        /// True if the object is calibrated.
        /// </summary>
        bool IsCalibrated { get; }
        
        /// <summary>
        /// True if the object is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the number of times the object has been reset.
        /// </summary>
        int ResetCount { get; }

        /// <summary>
        /// Timeout, in milliseconds, to wait for a reset to complete before trying again.
        /// </summary>
        int ResetTimeout { get; set; }

        /// <summary>
        /// Maximum number of times to retry from a timeout before aborting the attempt, completely.
        /// </summary>
        /// <remarks>
        /// Exceeding this value may trigger an exception.
        /// </remarks>
        int MaxTimeoutRetries { get; set; }

        /// <summary>
        /// Calibrate the data.
        /// </summary>
        void Calibrate();

        /// <summary>
        /// Initialize the data.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Reset the data.
        /// </summary>
        void Reset();

    }
}
