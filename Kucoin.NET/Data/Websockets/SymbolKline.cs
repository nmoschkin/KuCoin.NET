using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Websockets
{

    /// <summary>
    /// Represents a combination trading pair (symbol) and <see cref="Kucoin.NET.Data.Market.KlineType"/>.
    /// </summary>
    public class SymbolKline
    {
        KlineType type;

        string symbol;

        /// <summary>
        /// Gets or sets the <see cref="Market.KlineType"/>.
        /// </summary>
        public KlineType KlineType
        {
            get => type;
            set => type = value;
        }

        /// <summary>
        /// Gets or sets the trading pair (symbol).
        /// </summary>
        public string Symbol
        {
            get => symbol;
            set => symbol = value;
        }

        /// <summary>
        /// Instantiate a new <see cref="SymbolKline"/> with the specified parameters.
        /// </summary>
        /// <param name="symbol">The trading pair (symbol)</param>
        /// <param name="type">The <see cref="Market.KlineType"/>.</param>
        public SymbolKline(string symbol, KlineType type)
        {
            this.type = type;
            this.symbol = symbol;
        }

        /// <summary>
        /// Instantiate a new <see cref="SymbolKline"/> with the specified parameters.
        /// </summary>
        public SymbolKline()
        {

        }

        /// <summary>
        /// Returns the serialized symbol and kline-type combination.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{symbol}_{type}";
        }

        /// <summary>
        /// Parse a string into a new <see cref="SymbolKline"/> instance.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>A new <see cref="SymbolKline"/> instance.</returns>
        public static SymbolKline Parse(string s)
        {
            var parts = s.Split('_');

            var output = new SymbolKline();

            output.Symbol = parts[0];
            output.KlineType = KlineType.Parse(parts[1]);

            return output;
        }

    }
}
