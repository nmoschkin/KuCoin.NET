using Kucoin.NET.Data.Market;
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

namespace Kucoin.NET.Data.Websockets
{

    public class SnapshotItem : ISymbol
    {
        [JsonProperty("trading")]
        public decimal? Trading { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("buy")]
        public decimal? Buy { get; set; }

        [JsonProperty("sell")]
        public decimal? Sell { get; set; }

        [JsonProperty("sort")]
        public int Sort { get; set; }

        [JsonProperty("volValue")]
        public decimal? VolumeValue { get; set; }

        [JsonProperty("baseCurrency")]
        public string BaseCurrency { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("quoteCurrency")]
        public string QuoteCurrency { get; set; }

        [JsonProperty("symbolCode")]
        public string SymbolCode { get; set; }

        [JsonProperty("datetime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }

        [JsonProperty("high")]
        public decimal? High { get; set; }

        [JsonProperty("vol")]
        public decimal? Volume { get; set; }

        [JsonProperty("low")]
        public decimal? Low { get; set; }

        [JsonProperty("changePrice")]
        public decimal? ChangePrice { get; set; }

        [JsonProperty("changeRate")]
        public decimal? ChangeRate { get; set; }

        [JsonProperty("lastTradedPrice")]
        public decimal? LastTradedPrice { get; set; }

        [JsonProperty("board")]
        public int Board { get; set; }

        [JsonProperty("mark")]
        public int Mark { get; set; }


    }
}
