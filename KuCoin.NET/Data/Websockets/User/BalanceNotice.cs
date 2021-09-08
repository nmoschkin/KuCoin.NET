using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KuCoin.NET.Data.Websockets.User
{
    /// <summary>
    /// Balance notice event information
    /// </summary>
    public class BalanceNotice : DataObject, ICloneable, IStreamableObject
    {
        /// <summary>
        /// Total
        /// </summary>
        [JsonProperty("total")]
        public decimal Total { get; set; }

        /// <summary>
        /// Available
        /// </summary>
        [JsonProperty("available")]
        public decimal Available { get; set; }

        /// <summary>
        /// AvailableChange
        /// </summary>
        [JsonProperty("availableChange")]
        public decimal AvailableChange { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Hold
        /// </summary>
        [JsonProperty("hold")]
        public decimal Hold { get; set; }

        /// <summary>
        /// HoldChange
        /// </summary>
        [JsonProperty("holdChange")]
        public decimal HoldChange { get; set; }

        /// <summary>
        /// RelationEvent
        /// </summary>
        [JsonProperty("relationEvent")]
        public RelationEventType RelationEvent { get; set; }

        /// <summary>
        /// RelationEventId
        /// </summary>
        [JsonProperty("relationEventId")]
        public string RelationEventId { get; set; }

        /// <summary>
        /// RelationContext
        /// </summary>
        [JsonProperty("relationContext")]
        public RelationContext RelationContext { get; set; }

        /// <summary>
        /// Time Stamp
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }

        object ICloneable.Clone() => MemberwiseClone();

        public BalanceNotice Clone() => (BalanceNotice)MemberwiseClone();


    }

    public class RelationContext
    {

        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// TradeId
        /// </summary>
        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        /// <summary>
        /// OrderId
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }


    }

    [JsonConverter(typeof(EnumToStringConverter<RelationEventType>))]
    [Flags]
    public enum RelationEventType
    {
        /// <summary>
        /// Nothing
        /// </summary>
        None = 0,

        /// <summary>
        /// Accounts mask
        /// </summary>
        Accounts = 0xf00,

        /// <summary>
        /// Actions mask
        /// </summary>
        Actions = 0xff,

        /// <summary>
        /// Main account
        /// </summary>
        Main = 0x100,

        /// <summary>
        /// Trading account
        /// </summary>
        Trade = 0x200,

        /// <summary>
        /// Margin account
        /// </summary>
        Margin = 0x400,

        /// <summary>
        /// Transfer action
        /// </summary>
        Transfer = 0x1,

        /// <summary>
        /// Hold action
        /// </summary>
        Hold = 0x2,

        /// <summary>
        /// Settlement action
        /// </summary>
        Settled = 0x4,

        /// <summary>
        /// Completed action
        /// </summary>
        Done = 0x8,

        /// <summary>
        /// Deposit action
        /// </summary>
        Deposit = 0x10,

        /// <summary>
        /// Withdrawal action
        /// </summary>
        Withdraw = 0x20,

        /// <summary>
        /// Others
        /// </summary>
        [EnumMember(Value = "other")]
        Other = 0x1000,

        /// <summary>
        /// Deposit
        /// </summary>
        [EnumMember(Value = "main.deposit")]
        MainDeposit = Main | Deposit,

        /// <summary>
        /// Hold withdrawal amount
        /// </summary>
        [EnumMember(Value = "main.withdraw_hold")]
        MainWithdrawHold = Main | Withdraw | Hold,

        /// <summary>
        /// Withdrawal done
        /// </summary>
        [EnumMember(Value = "main.withdraw_done")]
        MainWithdrawDone = Main | Withdraw | Done,

        /// <summary>
        /// Transfer (Main account)
        /// </summary>
        [EnumMember(Value = "main.transfer")]
        MainTransfer = Main | Transfer,

        /// <summary>
        /// Other operations (Main account)
        /// </summary>
        [EnumMember(Value = "main.other")]
        MainOther = Main | Other,

        /// <summary>
        /// Hold (Trade account)
        /// </summary>
        [EnumMember(Value = "trade.hold")]
        TradeHold = Trade | Hold,

        /// <summary>
        /// Settlement (Trade account)
        /// </summary>
        [EnumMember(Value = "trade.setted")]
        TradeSettled = Trade | Settled,

        /// <summary>
        /// Transfer (Trade account)
        /// </summary>
        [EnumMember(Value = "trade.transfer")]
        TradeTransfer = Trade | Transfer,

        /// <summary>
        /// Other operations (Trade account)
        /// </summary>
        [EnumMember(Value = "trade.other")]
        TradeOther = Trade | Other,

        /// <summary>
        /// Hold (Margin account)
        /// </summary>
        [EnumMember(Value = "margin.hold")]
        MarginHold = Margin | Hold,

        /// <summary>
        /// Settlement (Margin account)
        /// </summary>
        [EnumMember(Value = "margin.setted")]
        MarginSettled = Margin | Settled,

        /// <summary>
        /// Transfer (Margin account)
        /// </summary>
        [EnumMember(Value = "margin.transfer")]
        MarginTransfer = Margin | Transfer,

        /// <summary>
        /// Other operations (Margin account)
        /// </summary>
        [EnumMember(Value = "margin.other")]
        MarginOther = Margin | Other,

    }

}
