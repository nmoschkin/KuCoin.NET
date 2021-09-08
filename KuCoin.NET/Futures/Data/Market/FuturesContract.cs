using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Json;
using KuCoin.NET.Observable;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Market
{
    /// <summary>
    /// Contains data about an open futures contract
    /// </summary>
    public class FuturesContract : DataObject, ISymbolicated, IDataObject
    {

        /// <summary>
        /// Base currency
        /// </summary>
        [JsonProperty("baseCurrency")]
        public string BaseCurrency { get; set; }


        /// <summary>
        /// Fair price marking method
        /// </summary>
        [JsonProperty("fairMethod")]
        public string FairMethod { get; set; }


        /// <summary>
        /// Ticker symbol of the based currency
        /// </summary>
        [JsonProperty("fundingBaseSymbol")]
        public string FundingBaseSymbol { get; set; }


        /// <summary>
        /// Ticker symbol of the quote currency
        /// </summary>
        [JsonProperty("fundingQuoteSymbol")]
        public string FundingQuoteSymbol { get; set; }


        /// <summary>
        /// Funding rate symbol
        /// </summary>
        [JsonProperty("fundingRateSymbol")]
        public string FundingRateSymbol { get; set; }


        /// <summary>
        /// Index symbol
        /// </summary>
        [JsonProperty("indexSymbol")]
        public string IndexSymbol { get; set; }


        /// <summary>
        /// Initial margin requirement
        /// </summary>
        [JsonProperty("initialMargin")]
        public decimal InitialMargin { get; set; }


        /// <summary>
        /// Enabled ADL or not
        /// </summary>
        [JsonProperty("isDeleverage")]
        public bool IsDeleverage { get; set; }


        /// <summary>
        /// Reverse contract or not
        /// </summary>
        [JsonProperty("isInverse")]
        public bool IsInverse { get; set; }


        /// <summary>
        /// Whether quanto or not
        /// </summary>
        [JsonProperty("isQuanto")]
        public bool IsQuanto { get; set; }


        /// <summary>
        /// Minimum lot size
        /// </summary>
        [JsonProperty("lotSize")]
        public decimal LotSize { get; set; }


        /// <summary>
        /// Maintenance margin requirement
        /// </summary>
        [JsonProperty("maintainMargin")]
        public decimal MaintenanceMargin { get; set; }


        /// <summary>
        /// Maker fees
        /// </summary>
        [JsonProperty("makerFeeRate")]
        public decimal MakerFeeRate { get; set; }


        /// <summary>
        /// Fixed maker fees
        /// </summary>
        [JsonProperty("makerFixFee")]
        public decimal MakerFixFee { get; set; }


        /// <summary>
        /// Marking method
        /// </summary>
        [JsonProperty("markMethod")]
        public string MarkMethod { get; set; }


        /// <summary>
        /// Maximum order quantity
        /// </summary>
        [JsonProperty("maxOrderQty")]
        public decimal MaxOrderQuantity { get; set; }


        /// <summary>
        /// Maximum order price
        /// </summary>
        [JsonProperty("maxPrice")]
        public decimal MaxPrice { get; set; }


        /// <summary>
        /// Maximum risk limit (unit: XBT)
        /// </summary>
        [JsonProperty("maxRiskLimit")]
        public decimal MaxRiskLimit { get; set; }


        /// <summary>
        /// Minimum risk limit (unit: XBT)
        /// </summary>
        [JsonProperty("minRiskLimit")]
        public decimal MinRiskLimit { get; set; }


        /// <summary>
        /// Contract multiplier
        /// </summary>
        [JsonProperty("multiplier")]
        public decimal Multiplier { get; set; }


        /// <summary>
        /// Quote currency
        /// </summary>
        [JsonProperty("quoteCurrency")]
        public string QuoteCurrency { get; set; }


        /// <summary>
        /// Risk limit increment value (unit: XBT)
        /// </summary>
        [JsonProperty("riskStep")]
        public decimal RiskStep { get; set; }


        /// <summary>
        /// Contract group
        /// </summary>
        [JsonProperty("rootSymbol")]
        public string RootSymbol { get; set; }


        /// <summary>
        /// Contract status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }


        /// <summary>
        /// Ticker symbol of the contract
        /// </summary>
        [JsonProperty("symbol")]
        [KeyProperty]
        public string Symbol { get; set; }


        /// <summary>
        /// Taker fees
        /// </summary>
        [JsonProperty("takerFeeRate")]
        public decimal TakerFeeRate { get; set; }


        /// <summary>
        /// Fixed taker fees
        /// </summary>
        [JsonProperty("takerFixFee")]
        public decimal TakerFixFee { get; set; }


        /// <summary>
        /// Minimum price changes
        /// </summary>
        [JsonProperty("tickSize")]
        public decimal TickSize { get; set; }


        /// <summary>
        /// Type of the contract
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }


        /// <summary>
        /// Maximum leverage
        /// </summary>
        [JsonProperty("maxLeverage")]
        public decimal MaxLeverage { get; set; }


        /// <summary>
        /// Volume of 24 hours
        /// </summary>
        [JsonProperty("volumeOf24h")]
        public decimal VolumeOf24h { get; set; }


        /// <summary>
        /// Turnover of 24 hours
        /// </summary>
        [JsonProperty("turnoverOf24h")]
        public decimal TurnoverOf24h { get; set; }


        /// <summary>
        /// Open interest
        /// </summary>
        [JsonProperty("openInterest")]
        public decimal OpenInterest { get; set; }


        /// <summary>
        /// 24H Low
        /// </summary>
        [JsonProperty("lowPrice")]
        public decimal LowPrice { get; set; }


        /// <summary>
        /// 24H High
        /// </summary>
        [JsonProperty("highPrice")]
        public decimal HighPrice { get; set; }


        /// <summary>
        /// 24H Change%
        /// </summary>
        [JsonProperty("priceChgPct")]
        public decimal PriceChangePercent { get; set; }


        /// <summary>
        /// 24H Change
        /// </summary>
        [JsonProperty("priceChg")]
        public decimal PriceChange { get; set; }

        public override string ToString() => $"{Symbol} : {BaseCurrency}-{QuoteCurrency}";


        public static explicit operator string(FuturesContract val) => val?.Symbol;



    }


}
