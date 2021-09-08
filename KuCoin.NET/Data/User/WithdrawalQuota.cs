using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.User
{
    /// <summary>
    /// Withdrawal quota information
    /// </summary>
    public class WithdrawalQuota
    {


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Limit BTC Amount
        /// </summary>
        [JsonProperty("limitBTCAmount")]
        public decimal LimitBTCAmount { get; set; }


        /// <summary>
        /// Used BTC Amount
        /// </summary>
        [JsonProperty("usedBTCAmount")]
        public long UsedBTCAmount { get; set; }


        /// <summary>
        /// Limit Amount
        /// </summary>
        [JsonProperty("limitAmount")]
        public decimal LimitAmount { get; set; }


        /// <summary>
        /// Remain Amount
        /// </summary>
        [JsonProperty("remainAmount")]
        public decimal RemainAmount { get; set; }


        /// <summary>
        /// Available Amount
        /// </summary>
        [JsonProperty("availableAmount")]
        public decimal AvailableAmount { get; set; }


        /// <summary>
        /// Withdraw Min Fee
        /// </summary>
        [JsonProperty("withdrawMinFee")]
        public decimal WithdrawMinFee { get; set; }


        /// <summary>
        /// Inner Withdraw Min Fee
        /// </summary>
        [JsonProperty("innerWithdrawMinFee")]
        public decimal InnerWithdrawMinFee { get; set; }


        /// <summary>
        /// Withdraw Min Size
        /// </summary>
        [JsonProperty("withdrawMinSize")]
        public decimal WithdrawMinSize { get; set; }


        /// <summary>
        /// Is Withdraw Enabled
        /// </summary>
        [JsonProperty("isWithdrawEnabled")]
        public bool IsWithdrawEnabled { get; set; }


        /// <summary>
        /// Precision
        /// </summary>
        [JsonProperty("precision")]
        public long Precision { get; set; }


        /// <summary>
        /// Chain
        /// </summary>
        [JsonProperty("chain")]
        public string Chain { get; set; }

    }


}
