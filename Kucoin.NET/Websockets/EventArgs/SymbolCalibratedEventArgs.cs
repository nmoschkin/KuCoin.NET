﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Data.Interfaces;

namespace Kucoin.NET.Websockets
{
    /// <summary>
    /// Provides data for a SymbolCalibrated event.
    /// </summary>
    public class SymbolCalibratedEventArgs<TBook, TUnit> : EventArgs 
        where TBook: IOrderBook<TUnit>
        where TUnit: IOrderUnit
    {
        /// <summary>
        /// Gets the symbol of the calibrated feed.
        /// </summary>
        public string Symbol { get; private set; }

        /// <summary>
        /// Gets the market depth of the calibrated feed.
        /// </summary>
        public int MarketDepth { get; private set; }


        public ILevel2OrderBookProvider<TBook, TUnit> Provider { get; private set; }

        public SymbolCalibratedEventArgs(ILevel2OrderBookProvider<TBook, TUnit> provider)

        {
            Provider = provider;

            MarketDepth = provider.FullDepthOrderBook.Asks.Count;
            Symbol = provider.Symbol;

        }

    }
}
