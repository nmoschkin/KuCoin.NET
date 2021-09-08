using KuCoin.NET.Data.Market;
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

namespace KuCoin.NET.Data.Websockets
{
    /// <summary>
    /// Represents market or symbol snapshot data
    /// </summary>
    public class SnapshotItem : DataObject, ISymbol, IStreamableObject
    {
        /// <summary>
        /// True if the symbol is trading
        /// </summary>
        [JsonProperty("trading")]
        public bool Trading { get; set; }

        /// <summary>
        /// The trading symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// The buy amount
        /// </summary>
        [JsonProperty("buy")]
        public decimal? Buy { get; set; }

        /// <summary>
        /// The sell amount
        /// </summary>
        [JsonProperty("sell")]
        public decimal? Sell { get; set; }

        /// <summary>
        /// The sort
        /// </summary>
        [JsonProperty("sort")]
        public int Sort { get; set; }

        /// <summary>
        /// Volume value
        /// </summary>
        [JsonProperty("volValue")]
        public decimal? VolumeValue { get; set; }

        /// <summary>
        /// Base currency
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseCurrency { get; set; }

        /// <summary>
        /// The trading market
        /// </summary>
        [JsonProperty("market")]
        public string Market { get; set; }

        /// <summary>
        /// Quote currency
        /// </summary>
        [JsonProperty("quoteCurrency")]
        public string QuoteCurrency { get; set; }

        /// <summary>
        /// Symbol code
        /// </summary>
        [JsonProperty("symbolCode")]
        public string SymbolCode { get; set; }

        /// <summary>
        /// Time stamp
        /// </summary>
        [JsonProperty("datetime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Highest price
        /// </summary>
        [JsonProperty("high")]
        public decimal? High { get; set; }

        /// <summary>
        /// Volume
        /// </summary>
        [JsonProperty("vol")]
        public decimal? Volume { get; set; }

        /// <summary>
        /// Lowest price
        /// </summary>
        [JsonProperty("low")]
        public decimal? Low { get; set; }

        /// <summary>
        /// Change price
        /// </summary>
        [JsonProperty("changePrice")]
        public decimal? ChangePrice { get; set; }

        /// <summary>
        /// Change rate
        /// </summary>
        [JsonProperty("changeRate")]
        public decimal? ChangeRate { get; set; }

        /// <summary>
        /// Last traded price
        /// </summary>
        [JsonProperty("lastTradedPrice")]
        public decimal? LastTradedPrice { get; set; }

        /// <summary>
        /// Board
        /// </summary>
        [JsonProperty("board")]
        public int Board { get; set; }

        /// <summary>
        /// Mark
        /// </summary>
        [JsonProperty("mark")]
        public int Mark { get; set; }


    }
}
