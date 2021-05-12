using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Kucoin.NET.Websockets.Observations;

namespace Kucoin.NET.Websockets
{
    /// <summary>
    /// Provides data for a SymbolCalibrated event.
    /// </summary>
    public class SymbolCalibratedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the symbol of the calibrated feed.
        /// </summary>
        public string Symbol { get; private set; }

        /// <summary>
        /// Gets the market depth of the calibrated feed.
        /// </summary>
        public int MarketDepth { get; private set; }


        public ILevel2OrderBookProvider Provider { get; private set; }

        public SymbolCalibratedEventArgs(ILevel2OrderBookProvider provider)

        {
            Provider = provider;

            MarketDepth = provider.FullDepthOrderBook.Asks.Count;
            Symbol = provider.Symbol;

        }

    }
}
