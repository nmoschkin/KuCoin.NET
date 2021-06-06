using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kucoin.NET.Futures.Data.User
{

    [JsonConverter(typeof(EnumToStringConverter<FuturesCurrency>))]
    public enum FuturesCurrency
    {
        [EnumMember(Value = "XBD")]
        XBT,

        [EnumMember(Value = "USDT")]
        USDT
    }


    /// <summary>
    /// Futures Account Info
    /// </summary>
    public class FuturesAccount
    {


        /// <summary>
        /// Account equity = marginBalance + Unrealized PNL
        /// </summary>
        [JsonProperty("accountEquity")]
        public decimal AccountEquity { get; set; }


        /// <summary>
        /// Unrealized profit and loss
        /// </summary>
        [JsonProperty("unrealizedPNL")]
        public decimal UnrealizedPNL { get; set; }


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
        public FuturesCurrency Currency { get; set; }

    }


}
