using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.User
{
    /// <summary>
    /// Futures Account Info
    /// </summary>
    public class FuturesAccount
    {


        /// <summary>
        /// Account equity = marginBalance + Unrealised PNL
        /// </summary>
        [JsonProperty("accountEquity")]
        public decimal AccountEquity { get; set; }


        /// <summary>
        /// Unrealised profit and loss
        /// </summary>
        [JsonProperty("unrealisedPNL")]
        public decimal UnrealisedPNL { get; set; }


        /// <summary>
        /// Margin balance = positionMargin + orderMargin + frozenFunds + availableBalance
        /// </summary>
        [JsonProperty("marginBalance")]
        public decimal MarginBalance { get; set; }


        /// <summary>
        /// Position margin
        /// </summary>
        [JsonProperty("positionMargin")]
        public decimal PositionMargin { get; set; }


        /// <summary>
        /// Order margin
        /// </summary>
        [JsonProperty("orderMargin")]
        public decimal OrderMargin { get; set; }


        /// <summary>
        /// Frozen funds for withdrawal and out-transfer
        /// </summary>
        [JsonProperty("frozenFunds")]
        public decimal FrozenFunds { get; set; }


        /// <summary>
        /// Available balance
        /// </summary>
        [JsonProperty("availableBalance")]
        public decimal AvailableBalance { get; set; }


        /// <summary>
        /// Currency code
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

    }


}
