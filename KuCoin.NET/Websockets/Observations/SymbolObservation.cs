using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Websockets
{
    /// <summary>
    /// Provides symbol-level observation filtering based on trading symbols.
    /// </summary>
    /// <typeparam name="T">The type of the object to observe.</typeparam>
    /// <remarks>
    /// The active symbols can be changed at any time and will be instantly reflected on the next <see cref="IObserver{T}.OnNext(T)"/> call.
    /// </remarks>
    public class SymbolObservation<T> : FeedObject<T> where T: class, IStreamableObject
    {
        private List<string> activeSymbols;

        /// <summary>
        /// The list of symbols that are active for this observer.
        /// </summary>
        /// <remarks>
        /// If this list is empty, the observer is active for all subscribed symbols.
        /// </remarks>
        public List<string> ActiveSymbols
        {
            get => activeSymbols;
            set
            {
                activeSymbols = value;
            }
        }
        
        /// <summary>
        /// Create a new granular observer provider.
        /// </summary>
        /// <param name="symbols">The symbols that are active for the specified observer.</param>
        /// <param name="feed">The origin feed.</param>
        /// <param name="observer">The observer.</param>
        public SymbolObservation(IEnumerable<string> symbols, KucoinBaseWebsocketFeed<T> feed, IObserver<T> observer) : base(feed, observer)
        {
            activeSymbols = new List<string>(symbols);
        }

        /// <summary>
        /// Create a new undifferentiated granular observer provider.
        /// </summary>
        /// <param name="feed">The origin feed.</param>
        /// <param name="observer">The observer.</param>
        public SymbolObservation(KucoinBaseWebsocketFeed<T> feed, IObserver<T> observer) : base(feed, observer)
        {
            activeSymbols = new List<string>();
        }
    }
}
