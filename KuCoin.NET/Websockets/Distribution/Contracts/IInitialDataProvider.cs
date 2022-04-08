using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    
    /// <summary>
    /// Provides initial data to a websocket feed observation.
    /// </summary>
    /// <typeparam name="TKey">The type of key that identifies the data.</typeparam>
    /// <typeparam name="TData">The type of data.</typeparam>
    public interface IInitialDataProvider<TKey, TData>
    {
        /// <summary>
        /// Provide initial data.
        /// </summary>
        /// <param name="key">The key for the data to acquire.</param>
        /// <returns>The data.</returns>
        Task<TData> ProvideInitialData(TKey key);

    }

    /// <summary>
    /// Provides initial data to a websocket feed observation with the results then passed to a callback function.
    /// </summary>
    /// <typeparam name="TKey">The type of key that identifies the data.</typeparam>
    /// <typeparam name="TData">The type of data.</typeparam>
    public interface IInitialDataProviderCallback<TKey, TData> 
    {
        /// <summary>
        /// Begin the request for initial data, and call the <paramref name="callback"/> function after the request has returned.
        /// </summary>
        /// <param name="key">They key for the data to acquire.</param>
        /// <param name="callback">The callback function to execute after the network request returns.</param>
        /// <remarks>
        /// If the network call fails, <paramref name="callback"/> will be called with a null parameter.
        /// </remarks>
        void BeginProvideInitialData(TKey key, Action<TData> callback);
    }

}
