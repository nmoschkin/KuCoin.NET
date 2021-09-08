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
    /// All Symbols Ticker list item.
    /// </summary>
    public class AllSymbolsTickerItem : DataObject, IStreamableObject
    {

        /// <summary>
        /// The timestamp of the current observation.
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The ticker symbol.
        /// </summary>
        [JsonProperty("symbol")]
        [KeyProperty]
        public string Symbol { get; set; }

        /// <summary>
        /// Name of trading pairs, it would change after renaming
        /// </summary>
        [JsonProperty("symbolName")]
        public string SymbolName { get; set; }

        /// <summary>
        /// Best Bid Price
        /// </summary>
        [JsonProperty("buy")]
        public decimal? Buy { get; set; }

        /// <summary>
        /// Best Ask Price
        /// </summary>
        [JsonProperty("sell")]
        public decimal? Sell { get; set; }

        /// <summary>
        /// 24h change rate
        /// </summary>
        [JsonProperty("changeRate")]
        public decimal? ChangeRate { get; set; }

        /// <summary>
        /// 24h change price
        /// </summary>
        [JsonProperty("changePrice")]
        public decimal? ChangePrice { get; set; }

        /// <summary>
        /// 24h highest price
        /// </summary>
        [JsonProperty("high")]
        public decimal? High { get; set; }

        /// <summary>
        /// 24h lowest price
        /// </summary>
        [JsonProperty("low")]
        public decimal? Low { get; set; }

        /// <summary>
        /// 24h volume，the aggregated trading volume in BTC
        /// </summary>
        [JsonProperty("vol")]
        public decimal? Volume { get; set; }

        /// <summary>
        /// 24h total, the trading volume in quote currency of last 24 hours
        /// </summary>
        [JsonProperty("volValue")]
        public decimal? VolumeValue { get; set; }

        /// <summary>
        /// last price
        /// </summary>
        [JsonProperty("last")]
        public decimal? LastPrice { get; set; }

        /// <summary>
        /// 24h average transaction price yesterday
        /// </summary>
        [JsonProperty("averagePrice")]
        public decimal? AveragePrice { get; set; }

        /// <summary>
        /// Basic Taker Fee
        /// </summary>
        [JsonProperty("takerFeeRate")]
        public decimal? TakerFeeRate { get; set; }

        /// <summary>
        /// Basic Maker Fee
        /// </summary>
        [JsonProperty("makerFeeRate")]
        public decimal? MakerFeeRate { get; set; }

        /// <summary>
        /// Taker Fee Coefficient
        /// </summary>
        [JsonProperty("takerCoefficient")]
        public decimal? TakerCoefficient { get; set; }

        /// <summary>
        /// Taker Fee Coefficient
        /// </summary>
        [JsonProperty("makerCoefficient")]
        public decimal? MakerCoefficient { get; set; }
    }
}
