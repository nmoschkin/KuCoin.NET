using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Data.Market;

namespace Kucoin.NET.Websockets
{
    /// <summary>
    /// Provides data for a SymbolCalibrated event.
    /// </summary>
    public class SymbolCalibratedEventArgs<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate> : EventArgs 
        where TBookOut: IOrderBook<TUnitOut>
        where TUnitOut: IOrderUnit
        where TBookIn: KeyedOrderBook<TUnitIn>
        where TUnitIn: IOrderUnit 
        where TUpdate: new()
    {
        /// <summary>
        /// Gets the symbol of the calibrated feed.
        /// </summary>
        public string Symbol { get; private set; }

        /// <summary>
        /// Gets the market depth of the calibrated feed.
        /// </summary>
        public int MarketDepth { get; private set; }


        public ILevel2OrderBookProvider<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate> Provider { get; private set; }

        public SymbolCalibratedEventArgs(ILevel2OrderBookProvider<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate> provider)

        {
            Provider = provider;

            MarketDepth = provider.FullDepthOrderBook.Asks.Count;
            Symbol = provider.Symbol;

        }

    }

    /// <summary>
    /// Provides data for a SymbolCalibrated event.
    /// </summary>
    public class Level3SymbolCalibratedEventArgs<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate> : EventArgs
        where TBookOut : IAtomicOrderBook<TUnitOut>
        where TUnitOut : IAtomicOrderUnit
        where TBookIn : KeyedAtomicOrderBook<TUnitIn>
        where TUnitIn : IAtomicOrderUnit
        where TUpdate : new()
    {
        /// <summary>
        /// Gets the symbol of the calibrated feed.
        /// </summary>
        public string Symbol { get; private set; }

        /// <summary>
        /// Gets the market depth of the calibrated feed.
        /// </summary>
        public int MarketDepth { get; private set; }


        public ILevel3OrderBookProvider<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate> Provider { get; private set; }

        public Level3SymbolCalibratedEventArgs(ILevel3OrderBookProvider<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate> provider)

        {
            Provider = provider;

            MarketDepth = provider.FullDepthOrderBook.Asks.Count;
            Symbol = provider.Symbol;

        }

    }

}
