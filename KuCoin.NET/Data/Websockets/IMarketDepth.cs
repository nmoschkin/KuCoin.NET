using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.Websockets
{
    /// <summary>
    /// An object that has a static market depth.
    /// </summary>
    public interface IReadOnlyMarketDepth
    {

        /// <summary>
        /// Gets the market depth.
        /// </summary>
        int MarketDepth { get; }

    }

    /// <summary>
    /// An object that has a mutable market depth.
    /// </summary>
    public interface IMarketDepth : IReadOnlyMarketDepth
    {
        /// <summary>
        /// Gets or sets the market depth (the maximum number of items in the market list).
        /// </summary>
        new int MarketDepth { get; set; }
    }

}
