using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Observable;
using Kucoin.NET.Rest;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Symbol ticker.
    /// </summary>
    public class Ticker : ISymbol 
    {
        /// <summary>
        /// Trading symbol
        /// </summary>
        [JsonIgnore]
        public string Symbol
        {
            get;
            internal set;
        }

        void ISymbol.SetSymbol(string symbol)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// Sequence Number
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        /// <summary>
        /// Best Ask  (selling)
        /// </summary>
        [JsonProperty("bestAsk")]
        public decimal BestAsk { get; set; }

        /// <summary>
        /// Filled Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }

        /// <summary>
        /// Filled Size
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Best Bid Size (buying)
        /// </summary>
        [JsonProperty("bestBidSize")]
        public decimal BestBidSize { get; set; }

        /// <summary>
        /// Best Bid (buying)
        /// </summary>
        [JsonProperty("bestBid")]
        public decimal BestBid { get; set; }

        /// <summary>
        /// Best Ask Size (selling)
        /// </summary>
        [JsonProperty("bestAskSize")]
        public decimal BestAskSize { get; set; }

        /// <summary>
        /// Time Stamp
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public virtual DateTime Timestamp { get; set; }

    }
}
