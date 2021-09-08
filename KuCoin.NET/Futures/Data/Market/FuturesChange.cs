using KuCoin.NET.Data;
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
    /// Represents a change packet pushed from the Futures Level 2 feed.
    /// </summary>
    public struct FuturesLevel2Update : ISymbol, IStreamableObject
    {
        [JsonIgnore]
        public string Symbol { get; set; }

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


        public Dictionary<string, object> ToDict()
        {
            return DataObject.ToDict(this);
        }

    }

    /// <summary>
    /// Represents a change piece of data on the Futures Level 2 feed.
    /// </summary>
    public struct FuturesChange : IFuturesOrderUpdate
    {
        /// <summary>
        /// The change price in quote currency
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The size of units
        /// </summary>
        public decimal Size { get; set; }

        /// <summary>
        /// The side of the change (buy or sell)
        /// </summary>
        public Side Side { get; set; }

        /// <summary>
        /// Initialize the class directly from the feed data.
        /// </summary>
        /// <param name="chg"></param>
        public FuturesChange(string chg)
        {
            var p = chg.Split(',');

            Price = decimal.Parse(p[0]);
            Side = p[1] == "buy" ? Side.Buy : Side.Sell;
            Size = decimal.Parse(p[2]);
        }

        /// <summary>
        /// Print the class as feed data.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Price},{Side.ToString().ToLower()},{Size}";
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        public FuturesChange Clone() => (FuturesChange)MemberwiseClone();


        public Dictionary<string, object> ToDict()
        {
            return DataObject.ToDict(this);
        }
    }





}
