using KuCoin.NET.Data;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Trade
{
    /// <summary>
    /// Position Details
    /// </summary>
    public class PositionDetails : DataObject
    {
        /// <summary>
        /// Position ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }


        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Auto deposit margin or not
        /// </summary>
        [JsonProperty("autoDeposit")]
        public bool AutoDeposit { get; set; }


        /// <summary>
        /// Maintenance margin requirement
        /// </summary>
        [JsonProperty("maintMarginReq")]
        public decimal MaintMarginReq { get; set; }


        /// <summary>
        /// Risk limit
        /// </summary>
        [JsonProperty("riskLimit")]
        public decimal RiskLimit { get; set; }


        /// <summary>
        /// Leverage o the order
        /// </summary>
        [JsonProperty("realLeverage")]
        public decimal RealLeverage { get; set; }


        /// <summary>
        /// Cross mode or not
        /// </summary>
        [JsonProperty("crossMode")]
        public bool CrossMode { get; set; }


        /// <summary>
        /// ADL ranking percentile
        /// </summary>
        [JsonProperty("delevPercentage")]
        public decimal DelevPercentage { get; set; }


        /// <summary>
        /// Open time
        /// </summary>
        [JsonProperty("openingTimestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime OpeningTimestamp { get; set; }


        /// <summary>
        /// Current timestamp
        /// </summary>
        [JsonProperty("currentTimestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CurrentTimestamp { get; set; }


        /// <summary>
        /// Current postion quantity
        /// </summary>
        [JsonProperty("currentQty")]
        public decimal CurrentQuantity { get; set; }


        /// <summary>
        /// Current postion value
        /// </summary>
        [JsonProperty("currentCost")]
        public decimal CurrentCost { get; set; }


        /// <summary>
        /// Current commission
        /// </summary>
        [JsonProperty("currentComm")]
        public decimal CurrentCommission { get; set; }


        /// <summary>
        /// Unrealised value
        /// </summary>
        [JsonProperty("unrealisedCost")]
        public decimal UnrealisedCost { get; set; }


        /// <summary>
        /// Accumulated realised gross profit value
        /// </summary>
        [JsonProperty("realisedGrossCost")]
        public decimal RealisedGrossCost { get; set; }


        /// <summary>
        /// Current realised position value
        /// </summary>
        [JsonProperty("realisedCost")]
        public decimal RealisedCost { get; set; }


        /// <summary>
        /// Opened position or not
        /// </summary>
        [JsonProperty("isOpen")]
        public bool IsOpen { get; set; }


        /// <summary>
        /// Mark price
        /// </summary>
        [JsonProperty("markPrice")]
        public decimal MarkPrice { get; set; }


        /// <summary>
        /// Mark value
        /// </summary>
        [JsonProperty("markValue")]
        public decimal MarkValue { get; set; }


        /// <summary>
        /// Position value
        /// </summary>
        [JsonProperty("posCost")]
        public decimal PositionCost { get; set; }


        /// <summary>
        /// Added margin
        /// </summary>
        [JsonProperty("posCross")]
        public decimal PositionCross { get; set; }


        /// <summary>
        /// Leverage margin
        /// </summary>
        [JsonProperty("posInit")]
        public decimal PositionInit { get; set; }


        /// <summary>
        /// Bankruptcy cost
        /// </summary>
        [JsonProperty("posComm")]
        public decimal PositionCommission { get; set; }


        /// <summary>
        /// Funding fees paid out
        /// </summary>
        [JsonProperty("posLoss")]
        public decimal PositionLoss { get; set; }


        /// <summary>
        /// Position margin
        /// </summary>
        [JsonProperty("posMargin")]
        public decimal PositionMargin { get; set; }


        /// <summary>
        /// Maintenance margin
        /// </summary>
        [JsonProperty("posMaint")]
        public decimal PositionMaint { get; set; }


        /// <summary>
        /// Position margin
        /// </summary>
        [JsonProperty("maintMargin")]
        public decimal MaintMargin { get; set; }


        /// <summary>
        /// Accumulated realised gross profit value
        /// </summary>
        [JsonProperty("realisedGrossPnl")]
        public decimal RealisedGrossPnl { get; set; }


        /// <summary>
        /// Realised profit and loss
        /// </summary>
        [JsonProperty("realisedPnl")]
        public decimal RealisedPnl { get; set; }


        /// <summary>
        /// Unrealised profit and loss
        /// </summary>
        [JsonProperty("unrealisedPnl")]
        public decimal UnrealisedPnl { get; set; }


        /// <summary>
        /// Profit-loss ratio of the position
        /// </summary>
        [JsonProperty("unrealisedPnlPcnt")]
        public decimal UnrealisedPnlPcnt { get; set; }


        /// <summary>
        /// Rate of return on investment
        /// </summary>
        [JsonProperty("unrealisedRoePcnt")]
        public decimal UnrealisedRoePcnt { get; set; }


        /// <summary>
        /// Average entry price
        /// </summary>
        [JsonProperty("avgEntryPrice")]
        public decimal AvgEntryPrice { get; set; }


        /// <summary>
        /// Liquidation price
        /// </summary>
        [JsonProperty("liquidationPrice")]
        public decimal LiquidationPrice { get; set; }


        /// <summary>
        /// Bankruptcy price
        /// </summary>
        [JsonProperty("bankruptPrice")]
        public decimal BankruptPrice { get; set; }


        /// <summary>
        /// Currency used to clear and settle the trades
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleCurrency { get; set; }

    }


}
