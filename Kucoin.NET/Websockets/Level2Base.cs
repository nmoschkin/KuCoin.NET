using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Observations;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets
{
    /// <summary>
    /// Calibrated Level 2 Market Feed base class.
    /// </summary>
    public abstract class Level2Base<TBook, TUnit, TUpdate, TObservation> :
        KucoinBaseWebsocketFeed
        where TBook : IOrderBook<TUnit>, new()
        where TUnit : IOrderUnit, new()
        where TUpdate : new()
        where TObservation : Level2ObservationBase<TBook, TUnit, TUpdate>
    {

        internal readonly Dictionary<string, TObservation> activeFeeds = new Dictionary<string, TObservation>();

        protected object lockObj = new object();
        protected DateTime cycle = DateTime.MinValue;

        protected int updateInterval = 500;

        public override bool IsPublic => false;

        /// <summary>
        /// Event that gets fired when the feed for a symbol has been calibrated and is ready to be used.
        /// </summary>
        public virtual event EventHandler<SymbolCalibratedEventArgs<TBook, TUnit, TUpdate>> SymbolCalibrated;

        /// <summary>
        /// Create a new Level 2 feed with the specified credentials.
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <remarks>
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public Level2Base(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false)
            : base(key, secret, passphrase, isSandbox)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        /// <summary>
        /// Create a new Level 2 feed with the specified credentials.
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        /// <remarks>
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public Level2Base(ICredentialsProvider credProvider) : base(credProvider)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        /// <summary>
        /// Gets or sets a length of time (in milliseconds) that indicates how often the orderbook is pushed to the UI thread.
        /// </summary>
        /// <remarks>
        /// The default value is 500 milliseconds.
        /// </remarks>
        public virtual int UpdateInterval
        {
            get => updateInterval;
            set
            {
                SetProperty(ref updateInterval, value);
            }
        }

        protected abstract TObservation CreateNewObserver(string symbol, int pieces = 50);

        /// <summary>
        /// Adds a Level 2 subscription for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to subscribe.</param>
        /// <param name="pieces">Market depth, or 0 for full depth.</param>
        /// <returns></returns>
        public async Task<TObservation> AddSymbol(string symbol, int pieces)
        {
            var p = await AddSymbols(new string[] { symbol }, pieces);
            return p[symbol];
        }

        /// <summary>
        /// Adds a Level 2 subscription for the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to subscribe.</param>
        /// <param name="pieces">Market depth, or 0 for full depth.</param>
        /// <returns></returns>
        public abstract Task<Dictionary<string, TObservation>> AddSymbols(IEnumerable<string> symbols, int pieces = 200);

        /// <summary>
        /// Remove a Level 2 subscription for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to remove.</param>
        /// <returns></returns>
        internal virtual async Task RemoveSymbol(string symbol)
        {
            await RemoveSymbols(new string[] { symbol });
        }

        /// <summary>
        /// Remove a Level 2 subscription for the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to remove.</param>
        /// <returns></returns>
        public abstract Task RemoveSymbols(IEnumerable<string> symbols);


        /// <summary>
        /// Get the Level 2 Data Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <param name="pieces">The number of pieces.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Settings the number of pieces to 0 returns the full market depth. 
        /// Use 0 to calibrate a full level 2 feed.
        /// </remarks>
        public abstract Task<TBook> GetPartList(string symbol, int pieces = 20);

        /// <summary>
        /// Get the full Level 2 Data Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Returns the full market depth. 
        /// Use this to calibrate a full level 2 feed.
        /// </remarks>
        public Task<TBook> GetAggregatedOrder(string symbol) => GetPartList(symbol, 0);

        /// <summary>
        /// Initialize the order book with a call to <see cref="GetAggregatedOrder(string)"/>.
        /// </summary>
        /// <param name="symbol">The symbol to initialize.</param>
        /// <remarks>
        /// This method is typically called after the feed has been buffered.
        /// </remarks>
        protected abstract Task InitializeOrderBook(string symbol);

    }


}
