
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
        /// Gets or sets the ticker symbol associated with this object.
        /// </summary>
        /// <param name="symbol"></param>
        new string Symbol { get; set; }
    }

    /// <summary>
    /// Describes an interface that contains symbol, base, and quote currency information.
    /// </summary>
    public interface IReadOnlySymbolicated : IReadOnlySymbol
    {
        /// <summary>
        /// Gets the base currency
        /// </summary>
        string BaseCurrency { get; }

        /// <summary>
        /// Gets the quote currency
        /// </summary>
        string QuoteCurrency { get; }
    }


    /// <summary>
    /// Describes an interface that contains symbol, base, and quote currency information.
    /// </summary>
    public interface ISymbolicated : IReadOnlySymbolicated, ISymbol
    {
        /// <summary>
        /// Gets or sets the base currency
        /// </summary>
        new string BaseCurrency { get; set; }

        /// <summary>
        /// Gets or sets the quote currency
        /// </summary>
        new string QuoteCurrency { get; set; }
    }



}
