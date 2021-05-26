using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Market
{


    public class FuturesChangeFeedItem
    {
        /// <summary>
        /// Sequence number which is used to judge the continuity of pushed messages
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        /// <summary>
        /// Price, side, quantity
        /// </summary>
        [JsonProperty("change")]
        [JsonConverter(typeof(FuturesChangeConverter))]
        public FuturesChange Change { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }

    }

    public class FuturesChange
    {
        public decimal Price { get; set; }

        public decimal Size { get; set; }

        public Side Side { get; set; }

        public FuturesChange()
        {

        }

        public FuturesChange(string chg)
        {
            var p = chg.Split(',');

            Price = decimal.Parse(p[0]);
            Side = p[1] == "buy" ? Side.Buy : Side.Sell;
            Size = decimal.Parse(p[2]);
        }

        public override string ToString()
        {
            return $"{Price},{Side.ToString().ToLower()},{Size}";
        }

    }





}
