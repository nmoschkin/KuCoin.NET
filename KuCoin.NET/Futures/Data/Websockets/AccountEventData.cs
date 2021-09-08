using KuCoin.NET.Data;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Websockets
{

    /// <summary>
    /// Account event types
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Order margin event
        /// </summary>
        OrderMargin,

        /// <summary>
        /// Available balance change event
        /// </summary>
        AvailableBalance,

        /// <summary>
        /// Withdrawal event
        /// </summary>
        WithdrawalAmount
    }

    /// <summary>
    /// Account event data
    /// </summary>
    public class AccountEventData : DataObject, IStreamableObject
    {
        /// <summary>
        /// The type of information contained in this class.
        /// </summary>
        [JsonIgnore]
        public EventType EventType { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Current available amount
        /// </summary>
        [JsonProperty("availableBalance")]
        public decimal? AvailableBalance { get; set; }


        /// <summary>
        /// Frozen amount
        /// </summary>
        [JsonProperty("holdBalance")]
        public decimal? HoldBalance { get; set; }

        /// <summary>
        /// Current order margin
        /// </summary>
        [JsonProperty("orderMargin")]
        public decimal? OrderMargin { get; set; }

        /// <summary>
        /// Current frozen amount for withdrawal
        /// </summary>
        [JsonProperty("withdrawHold")]
        public decimal? WithdrawHold { get; set; }

    }




}
