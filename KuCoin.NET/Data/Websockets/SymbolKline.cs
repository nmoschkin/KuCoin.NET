using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KuCoin.NET.Data.Websockets
{

    /// <summary>
    /// Represents a combination trading pair (symbol) and <see cref="KuCoinSystem.NET.Data.Market.KlineType"/>.
    /// </summary>
    public struct SymbolKline : ISymbol, IEquatable<SymbolKline>, IComparable<SymbolKline>
    {
        /// <summary>
        /// Represents an empty SymbolKline structure.
        /// </summary>
        public static readonly SymbolKline Empty = new SymbolKline(null, KlineType.Invalid);

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

#if DOTNETSTD
        public override bool Equals(object obj)
#else
        public override bool Equals([NotNullWhen(true)] object obj)
#endif
        {
            if (obj is SymbolKline other)
            {
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(SymbolKline other)
        {
            return other.symbol == symbol && other.KlineType == KlineType;
        }

        public override int GetHashCode()
        {
            return (symbol?.GetHashCode() ?? 0) ^ (KlineType.GetHashCode());
        }

        public int CompareTo(SymbolKline other)
        {
            int i = KlineType.TimeSpan.CompareTo(other.KlineType.TimeSpan);
            if (i != 0) return i;
            return string.Compare(symbol, other.symbol);
        }

        public static bool operator ==(SymbolKline val1, SymbolKline val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(SymbolKline val1, SymbolKline val2)
        {
            return !val1.Equals(val2);
        }

        public static bool operator >(SymbolKline val1, SymbolKline val2)
        {
            return val1.CompareTo(val2) > 0;
        }
        public static bool operator <(SymbolKline val1, SymbolKline val2)
        {
            return val1.CompareTo(val2) < 0;
        }

        public static bool operator >=(SymbolKline val1, SymbolKline val2)
        {
            return val1.CompareTo(val2) >= 0;
        }
        public static bool operator <=(SymbolKline val1, SymbolKline val2)
        {
            return val1.CompareTo(val2) <= 0;
        }

        public static explicit operator SymbolKline(string val)
        {
            return Parse(val);
        }

        public static implicit operator string(SymbolKline val)
        {
            return val.ToString();
        }

        public static explicit operator SymbolKline((string, string) val)
        {
            return new SymbolKline(val.Item1, KlineType.Parse(val.Item2));
        }

        public static implicit operator (string, string)(SymbolKline val)
        {
            return (val.Symbol, val.KlineType.ToString());
        }

        public static implicit operator (string, KlineType)(SymbolKline val)
        {
            return (val.Symbol, val.KlineType);
        }

        public static implicit operator SymbolKline((string, KlineType) val)
        {
            return new SymbolKline(val.Item1, val.Item2);
        }

        

    }
}
