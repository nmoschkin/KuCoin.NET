using Kucoin.NET.Data;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Websockets
{
    /// <summary>
    /// Contract market snapshot data
    /// </summary>
    public class ContractMarketSnapshot : DataObject, ISymbol, IStreamableObject
    {
        [JsonIgnore()]
        public string Symbol { get; set; }

        /// <summary>
        /// 24h Volume
        /// </summary>
        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        public override string ToString()
        {
            return $"{Symbol}: {LastPrice} (Vol {Volume}) ({PriceChangePercent}% Change) {Timestamp:G}";
        }

        /// <summary>
        /// 24h Turnover
        /// </summary>
        [JsonProperty("turnover")]
        public decimal Turnover { get; set; }


        /// <summary>
        /// Last price
        /// </summary>
        [JsonProperty("lastPrice")]
        public decimal LastPrice { get; set; }


        /// <summary>
        /// 24h Change
        /// </summary>
        [JsonProperty("priceChgPct")]
        public decimal PriceChangePercent { get; set; }


        /// <summary>
        /// Snapshot time (nanosecond)
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }



    }


}
