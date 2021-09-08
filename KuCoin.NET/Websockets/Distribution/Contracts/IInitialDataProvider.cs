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
}
