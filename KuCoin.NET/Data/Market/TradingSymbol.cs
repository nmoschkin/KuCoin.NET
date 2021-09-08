using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KuCoin.NET.Json;
using KuCoin.NET.Observable;

using Newtonsoft.Json;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// Trading symbol
    /// </summary>
    public class TradingSymbol : DataObject, ISymbolicated, IDataObject
    {
        /// <summary>
        /// Gets the trading pair as an array of strings [base, quote].
        /// </summary>
        [JsonIgnore]
        public string[] TradingPair
        {
            get
            {
                return Symbol.Split('-');
            }
        }

        /// <summary>
        /// The trading symbol.
        /// </summary>
        [JsonProperty("symbol")]
        [KeyProperty()]
        public string Symbol { get; set; }

        /// <summary>
        /// The name of the trading symbol.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The base currency.
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseCurrency { get; set; }

        /// <summary>
        /// The quote currency.
        /// </summary>
        [JsonProperty("quoteCurrency")]
        public string QuoteCurrency { get; set; }

        /// <summary>
        /// Base currency minimum size.
        /// </summary>
        [JsonProperty("baseMinSize")]
        public decimal BaseMinSize { get; set; }

        /// <summary>
        /// Quote currency minimum size.
        /// </summary>
        [JsonProperty("quoteMinSize")]
        public decimal QuoteMinSize { get; set; }

        /// <summary>
        /// Base currency maximum size.
        /// </summary>
        [JsonProperty("baseMaxSize")]
        public decimal BaseMaxSize { get; set; }

        /// <summary>
        /// Quote currency maximum size.
        /// </summary>
        [JsonProperty("quoteMaxSize")]
        public decimal QuoteMaxSize { get; set; }

        /// <summary>
        /// Base currency increment step.
        /// </summary>
        [JsonProperty("baseIncrement")]
        public decimal BaseIncrement { get; set; }

        /// <summary>
        /// Quote currency increment step.
        /// </summary>
        [JsonProperty("quoteIncrement")]
        public decimal QuoteIncrement { get; set; }

        /// <summary>
        /// Price increment step.
        /// </summary>
        [JsonProperty("priceIncrement")]
        public decimal PriceIncrement { get; set; }

        /// <summary>
        /// The currency in which fees are charged.
        /// </summary>
        [JsonProperty("feeCurrency")]
        public string FeeCurrency { get; set; }

        /// <summary>
        /// Trading is enabled for this pair on KuCoin.
        /// </summary>
        [JsonProperty("enableTrading")]
        public bool EnableTrading { get; set; }

        /// <summary>
        /// Margin trading is enabled for this pair on KuCoin.
        /// </summary>
        [JsonProperty("isMarginEnabled")]
        public bool IsMarginEnabled { get; set; }

        /// <summary>
        /// The price limit rate.
        /// </summary>
        [JsonProperty("priceLimitRate")]
        public decimal PriceLimitRate { get; set; }

        /// <summary>
        /// Returns the trading pair (symbol)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Symbol;
        }

        public static explicit operator string(TradingSymbol s)
        {
            return s?.Symbol;
        }

        public static explicit operator TradingSymbol(string s)
        {
            if (s == null) return default;

            if (KuCoin.NET.Rest.Market.Instance.Symbols != null && KuCoin.NET.Rest.Market.Instance.Symbols.Count != 0 && KuCoin.NET.Rest.Market.Instance.Symbols.TryGetValue(s, out TradingSymbol sym)) 
            {
                return sym;
            }

            var parts = s.Split('-');
            var bc = "USDT";
            var sc = s;

            if (parts != null && parts.Length == 2)
            {
                bc = parts[1];
                sc = parts[0];
            }

            return new TradingSymbol()
            {
                Symbol = s,
                Name = s,
                BaseCurrency = sc,
                QuoteCurrency = bc,
                FeeCurrency = bc
            };
        }

    }
}
