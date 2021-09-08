using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using Newtonsoft.Json;
using System.ComponentModel;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.User
{
    /// <summary>
    /// Account business type
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<BizType>))]
    public enum BizType
    {        

        /// <summary>
        /// Deposit funds from external account
        /// </summary>
        [EnumMember(Value = "DEPOSIT")]
        Deposit,

        /// <summary>
        /// Withdraw funds to external account
        /// </summary>
        [EnumMember(Value = "WITHDRAW")]
        Withdraw,

        /// <summary>
        /// Transfer funds between accounts
        /// </summary>
        [EnumMember(Value = "TRANSFER")]
        Transfer,

        /// <summary>
        /// Transfer funds between primary accounts and sub-user accounts
        /// </summary>
        [EnumMember(Value = "SUB_TRANSFER")]
        SubTransfer,

        /// <summary>
        /// Spot trade or exchange
        /// </summary>
        [EnumMember(Value = "TRADE_EXCHANGE")]
        TradeExchange,

        /// <summary>
        /// Spot margin or exchange
        /// </summary>
        [EnumMember(Value = "MARGIN_EXCHANGE")]
        MarginExchange,

        /// <summary>
        /// KuCoin bonus
        /// </summary>
        [EnumMember(Value = "KUCOIN_BONUS")]
        KucoinBonus,

        /// <summary>
        /// Referral bonus
        /// </summary>
        [EnumMember(Value = "REFERRAL_BONUS")]
        ReferralBonus,
    }
}
