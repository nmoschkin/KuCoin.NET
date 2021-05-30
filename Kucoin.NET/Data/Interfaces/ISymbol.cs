
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Declare this interface to get a ticker symbol associated with an implementation.
    /// </summary>
    public interface IReadOnlySymbol
    {
        /// <summary>
        /// Gets the ticker symbol associated with this object.
        /// </summary>
        string Symbol { get; }
    }

    /// <summary>
    /// Declare this interface to modify a ticker symbol associated with an implementation.
    /// </summary>
    public interface ISymbol : IReadOnlySymbol
    {
        /// <summary>
        /// Sets the ticker symbol associated with this object.
        /// </summary>
        /// <param name="symbol"></param>
        void SetSymbol(string symbol);
    }



}
