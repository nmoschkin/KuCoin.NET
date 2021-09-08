using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Websockets
{
    /// <summary>
    /// Position Information
    /// </summary>
    public class PositionData : DataObject, ISymbol, IStreamableObject
    {
        [JsonIgnore]
        public string Symbol { get; set; }

        /// <summary>
        /// Accumulated realised profit and loss
        /// </summary>
        [JsonProperty("realisedGrossPnl")]
        public decimal RealisedGrossPnl { get; set; }


        /// <summary>
        /// Cross mode or not
        /// </summary>
        [JsonProperty("crossMode")]
        public bool CrossMode { get; set; }


        /// <summary>
        /// Liquidation price
        /// </summary>
        [JsonProperty("liquidationPrice")]
        public decimal LiquidationPrice { get; set; }


        /// <summary>
        /// Manually added margin amount
        /// </summary>
        [JsonProperty("posLoss")]
        public decimal PosLoss { get; set; }


        /// <summary>
        /// Average entry price
        /// </summary>
        [JsonProperty("avgEntryPrice")]
        public decimal AvgEntryPrice { get; set; }


        /// <summary>
        /// Unrealised profit and loss
        /// </summary>
        [JsonProperty("unrealisedPnl")]
        public decimal UnrealisedPnl { get; set; }


        /// <summary>
        /// Mark price
        /// </summary>
        [JsonProperty("markPrice")]
        public decimal MarkPrice { get; set; }


        /// <summary>
        /// Position margin
        /// </summary>
        [JsonProperty("posMargin")]
        public decimal PosMargin { get; set; }


        /// <summary>
        /// Risk limit
        /// </summary>
        [JsonProperty("riskLimit")]
        public decimal RiskLimit { get; set; }


        /// <summary>
        /// Unrealised value
        /// </summary>
        [JsonProperty("unrealisedCost")]
        public decimal UnrealisedCost { get; set; }


        /// <summary>
        /// Bankruptcy cost
        /// </summary>
        [JsonProperty("posComm")]
        public decimal PosComm { get; set; }


        /// <summary>
        /// Maintenance margin
        /// </summary>
        [JsonProperty("posMaint")]
        public decimal PosMaint { get; set; }


        /// <summary>
        /// Position value
        /// </summary>
        [JsonProperty("posCost")]
        public decimal PosCost { get; set; }


        /// <summary>
        /// Maintenance margin rate
        /// </summary>
        [JsonProperty("maintMarginReq")]
        public decimal MaintMarginReq { get; set; }


        /// <summary>
        /// Bankruptcy price
        /// </summary>
        [JsonProperty("bankruptPrice")]
        public decimal BankruptPrice { get; set; }


        /// <summary>
        /// Currently accumulated realised position value
        /// </summary>
        [JsonProperty("realisedCost")]
        public decimal RealisedCost { get; set; }


        /// <summary>
        /// Mark value
        /// </summary>
        [JsonProperty("markValue")]
        public decimal MarkValue { get; set; }


        /// <summary>
        /// Position margin
        /// </summary>
        [JsonProperty("posInit")]
        public decimal PosInit { get; set; }


        /// <summary>
        /// Realised profit and losts
        /// </summary>
        [JsonProperty("realisedPnl")]
        public decimal RealisedPnl { get; set; }


        /// <summary>
        /// Position margin
        /// </summary>
        [JsonProperty("maintMargin")]
        public decimal MaintMargin { get; set; }


        /// <summary>
        /// Leverage of the order
        /// </summary>
        [JsonProperty("realLeverage")]
        public decimal RealLeverage { get; set; }


        /// <summary>
        /// Current position value
        /// </summary>
        [JsonProperty("currentCost")]
        public decimal CurrentCost { get; set; }


        /// <summary>
        /// Open time
        /// </summary>
        [JsonProperty("openingTimestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime OpeningTimestamp { get; set; }


        /// <summary>
        /// Current position
        /// </summary>
        [JsonProperty("currentQty")]
        public decimal CurrentQty { get; set; }


        /// <summary>
        /// ADL ranking percentile
        /// </summary>
        [JsonProperty("delevPercentage")]
        public decimal DelevPercentage { get; set; }


        /// <summary>
        /// Current commission
        /// </summary>
        [JsonProperty("currentComm")]
        public decimal CurrentComm { get; set; }


        /// <summary>
        /// Accumulated reliased gross profit value
        /// </summary>
        [JsonProperty("realisedGrossCost")]
        public decimal RealisedGrossCost { get; set; }


        /// <summary>
        /// Opened position or not
        /// </summary>
        [JsonProperty("isOpen")]
        public bool IsOpen { get; set; }


        /// <summary>
        /// Manually added margin
        /// </summary>
        [JsonProperty("posCross")]
        public decimal PosCross { get; set; }


        /// <summary>
        /// Current timestamp
        /// </summary>
        [JsonProperty("currentTimestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CurrentTimestamp { get; set; }


        /// <summary>
        /// Rate of return on investment
        /// </summary>
        [JsonProperty("unrealisedRoePcnt")]
        public decimal UnrealisedRoePcnt { get; set; }


        /// <summary>
        /// Position profit and loss ratio
        /// </summary>
        [JsonProperty("unrealisedPnlPcnt")]
        public decimal UnrealisedPnlPcnt { get; set; }


        /// <summary>
        /// Currency used to clear and settle the trades
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleCurrency { get; set; }

    }


}
