using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Websockets
{
    /// <summary>
    /// An object that has a market depth.
    /// </summary>
    public interface IReadOnlyMarketDepth
    {

        /// <summary>
        /// Gets the market depth.
        /// </summary>
        int MarketDepth { get; }

    }

    /// <summary>
    /// An object that has a market depth that can change.
    /// </summary>
    public interface IMarketDepth : IReadOnlyMarketDepth
    {
        /// <summary>
        /// Gets or sets the market depth.
        /// </summary>
        new int MarketDepth { get; set; }
    }

}
