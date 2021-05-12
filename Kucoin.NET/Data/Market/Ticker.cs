using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Helpers;
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

        [JsonProperty("sequence")]
        public long Sequence { get; set; }


        [JsonProperty("bestAsk")]
        public decimal BestAsk { get; set; }

        [JsonProperty("size")]
        public decimal Size { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("bestBidSize")]
        public decimal BestBidSize { get; set; }


        [JsonProperty("bestBid")]
        public decimal BestBid { get; set; }


        [JsonProperty("bestAskSize")]
        public decimal BestAskPrice { get; set; }
        

        [JsonProperty("time")]
        public long Time { get; set; }


        [JsonIgnore]
        public DateTime Timestamp
        {
            get
            {
                return EpochTime.MillisecondsToDate(Time);
            }
        }

    }
}
