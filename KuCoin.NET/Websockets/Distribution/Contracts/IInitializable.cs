using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    /// <summary>
    /// Interface for objects that do initialization work.
    /// </summary>
    public interface IInitializable
    {

        event EventHandler Initialized;

        /// <summary>
        /// Returns true if the data provider is available.
        /// </summary>
        bool IsDataProviderAvailable { get; }
        
        /// <summary>
        /// True if the object is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// True if initialization resulted in an error.
        /// </summary>
        bool Failure { get; }

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
        /// Initialize the data.
        /// </summary>
        /// <returns>True if successful.</returns>
        Task<bool> Initialize();

        /// <summary>
        /// Reset the data.
        /// </summary>
        Task Reset();

    }

    /// <summary>
    /// Interface for objects that do initialization work.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public interface IInitializable<TKey, TValue> : IInitializable, IInternalData<TValue>
    {
                
        /// <summary>
        /// Sets the data provider for the initial data.
        /// </summary>
        /// <param name="dataProvider">An object that provides initial data.</param>
        void SetInitialDataProvider(IInitialDataProvider<TKey, TValue> dataProvider);

        /// <summary>
        /// Gets the attached data provider.
        /// </summary>
        IInitialDataProvider<TKey, TValue> DataProvider { get; }


    }

}
