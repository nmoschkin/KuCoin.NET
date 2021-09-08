using KuCoin.NET.Data.Market;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Market
{
    /// <summary>
    /// Futures symbol ticker
    /// </summary>
    public class FuturesTicker : Ticker
    {

        [JsonProperty("symbol")]
        public override string Symbol { get => base.Symbol; internal set => base.Symbol = value; }

        [JsonProperty("bestAskPrice")]
        public override decimal BestAsk { get => base.BestAsk; set => base.BestAsk = value; }

        [JsonProperty("bestBidPrice")]
        public override decimal BestBid { get => base.BestBid; set => base.BestBid = value; }

        [JsonProperty("bestAskSize")]
        public override decimal BestAskSize { get => base.BestAskSize; set => base.BestAskSize = value; }

        [JsonProperty("bestBidSize")]
        public override decimal BestBidSize { get => base.BestBidSize; set => base.BestBidSize = value; }

        [JsonIgnore]
        public override decimal Size
        {
            get => base.BestAskSize == 0 ? base.BestBidSize : base.BestAskSize;
            set => base.BestAskSize = value;
        }

        [JsonIgnore]
        public override decimal Price
        {
            get => base.BestAsk == 0 ? base.BestBid : base.BestAsk;
            set => base.BestAsk = value;
        }

        /// <summary>
        /// Time Stamp
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public override DateTime Timestamp { get; set; }

    }
}
