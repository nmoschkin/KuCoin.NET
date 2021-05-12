using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using Newtonsoft.Json;
using System.ComponentModel;
using Kucoin.NET.Json;

namespace Kucoin.NET.Data.User
{

    [JsonConverter(typeof(EnumToStringConverter<BizType>))]
    public enum BizType
    {

        [EnumMember(Value = "DEPOSIT")]
        Deposit,

        [EnumMember(Value = "WITHDRAW")]
        Withdraw,

        [EnumMember(Value = "TRANSFER")]
        Transfer,

        [EnumMember(Value = "SUB_TRANSFER")]
        SubTransfer,

        [EnumMember(Value = "TRADE_EXCHANGE")]
        TradeExchange,

        [EnumMember(Value = "MARGIN_EXCHANGE")]
        MarginExchange,

        [EnumMember(Value = "KUCOIN_BONUS")]
        KucoinBonus,

    }
}
