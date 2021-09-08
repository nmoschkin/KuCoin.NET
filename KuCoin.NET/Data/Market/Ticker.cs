using KuCoin.NET.Helpers;
using KuCoin.NET.Json;
using KuCoin.NET.Observable;
using KuCoin.NET.Rest;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// Symbol ticker.
    /// </summary>
    public class Ticker : DataObject, ISymbol, IStreamableObject
    {
        /// <summary>
        /// Trading symbol
        /// </summary>
        [JsonIgnore]
        public virtual string Symbol
        {
            get;
            internal set;
        }

        string ISymbol.Symbol { get => Symbol; set => Symbol = value; }

        /// <summary>
        /// Sequence Number
        /// </summary>
        [JsonProperty("sequence")]
        public virtual long Sequence { get; set; }

        /// <summary>
        /// Best Ask  (selling)
        /// </summary>
        [JsonProperty("bestAsk")]
        public virtual decimal BestAsk { get; set; }

        /// <summary>
        /// Filled Size
        /// </summary>
        [JsonProperty("size")]
        public virtual decimal Size { get; set; }

        /// <summary>
        /// Filled Size
        /// </summary>
        [JsonProperty("price")]
        public virtual decimal Price { get; set; }

        /// <summary>
        /// Best Bid Size (buying)
        /// </summary>
        [JsonProperty("bestBidSize")]
        public virtual decimal BestBidSize { get; set; }

        /// <summary>
        /// Best Bid (buying)
        /// </summary>
        [JsonProperty("bestBid")]
        public virtual decimal BestBid { get; set; }

        /// <summary>
        /// Best Ask Size (selling)
        /// </summary>
        [JsonProperty("bestAskSize")]
        public virtual decimal BestAskSize { get; set; }

        /// <summary>
        /// Time Stamp
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public virtual DateTime Timestamp { get; set; }

    }
}
