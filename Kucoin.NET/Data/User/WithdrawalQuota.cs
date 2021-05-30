using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.User
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
        /// LimitBTCAmount
        /// </summary>
        [JsonProperty("limitBTCAmount")]
        public decimal LimitBTCAmount { get; set; }


        /// <summary>
        /// UsedBTCAmount
        /// </summary>
        [JsonProperty("usedBTCAmount")]
        public long UsedBTCAmount { get; set; }


        /// <summary>
        /// LimitAmount
        /// </summary>
        [JsonProperty("limitAmount")]
        public decimal LimitAmount { get; set; }


        /// <summary>
        /// RemainAmount
        /// </summary>
        [JsonProperty("remainAmount")]
        public decimal RemainAmount { get; set; }


        /// <summary>
        /// AvailableAmount
        /// </summary>
        [JsonProperty("availableAmount")]
        public decimal AvailableAmount { get; set; }


        /// <summary>
        /// WithdrawMinFee
        /// </summary>
        [JsonProperty("withdrawMinFee")]
        public decimal WithdrawMinFee { get; set; }


        /// <summary>
        /// InnerWithdrawMinFee
        /// </summary>
        [JsonProperty("innerWithdrawMinFee")]
        public decimal InnerWithdrawMinFee { get; set; }


        /// <summary>
        /// WithdrawMinSize
        /// </summary>
        [JsonProperty("withdrawMinSize")]
        public decimal WithdrawMinSize { get; set; }


        /// <summary>
        /// IsWithdrawEnabled
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
