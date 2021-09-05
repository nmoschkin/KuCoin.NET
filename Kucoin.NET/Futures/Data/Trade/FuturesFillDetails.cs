using Kucoin.NET.Data;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Trade
{
 
    /// <summary>
    /// Futures Fill Details
    /// </summary>
    public class FuturesFillDetails : IDataObject
    {


        /// <summary>
        /// Ticker symbol of the contract
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Trade ID
        /// </summary>
        [JsonProperty("tradeId")]
        public string TradeId { get; set; }


        /// <summary>
        /// Order ID
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }


        /// <summary>
        /// Transaction side
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }


        /// <summary>
        /// Liquidity- taker or maker
        /// </summary>
        [JsonProperty("liquidity")]
        public LiquidityType Liquidity { get; set; }


        /// <summary>
        /// Filled price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }


        /// <summary>
        /// Filled amount
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }


        /// <summary>
        /// Order value
        /// </summary>
        [JsonProperty("value")]
        public decimal Value { get; set; }


        /// <summary>
        /// Floating fees
        /// </summary>
        [JsonProperty("feeRate")]
        public decimal FeeRate { get; set; }


        /// <summary>
        /// Fixed fees
        /// </summary>
        [JsonProperty("fixFee")]
        public decimal FixFee { get; set; }


        /// <summary>
        /// Charging currency
        /// </summary>
        [JsonProperty("feeCurrency")]
        public string FeeCurrency { get; set; }


        /// <summary>
        /// A mark to the stop order type
        /// </summary>
        [JsonProperty("stop")]
        public string Stop { get; set; }


        /// <summary>
        /// Transaction fee
        /// </summary>
        [JsonProperty("fee")]
        public decimal Fee { get; set; }


        /// <summary>
        /// Order type
        /// </summary>
        [JsonProperty("orderType")]
        public OrderType OrderType { get; set; }


        /// <summary>
        /// Trade type (trade, liquidation, ADL or settlement)
        /// </summary>
        [JsonProperty("tradeType")]
        public FuturesTradeType TradeType { get; set; }


        /// <summary>
        /// Time the order created
        /// </summary>
        [JsonProperty("createdAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CreatedAt { get; set; }


        /// <summary>
        /// Settlement currency
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleCurrency { get; set; }


        /// <summary>
        /// Trade time in nanosecond
        /// </summary>
        [JsonProperty("tradeTime")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime TradeTime { get; set; }

    }

}
