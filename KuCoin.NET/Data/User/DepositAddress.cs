using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.User
{
    /// <summary>
    /// Deposit address base class
    /// </summary>
    public class AccountAddress : DataObject
    {
        /// <summary>
        /// Address
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }


        /// <summary>
        /// Memo
        /// </summary>
        [JsonProperty("memo")]
        public string Memo { get; set; }


    }

    public abstract class SpotAddressBase : AccountAddress
    {

        /// <summary>
        /// Chain
        /// </summary>
        [JsonProperty("chain")]
        public string Chain { get; set; }

    }

    /// <summary>
    /// Deposit address information
    /// </summary>
    public class DepositAddress : SpotAddressBase
    {
        /// <summary>
        /// Contract Address
        /// </summary>
        [JsonProperty("contractAddress")]
        public string ContractAddress { get; set; }

    }


    public class WithdrawalAddress : SpotAddressBase
    {
        /// <summary>
        /// Is inner address
        /// </summary>
        [JsonProperty("isInner")]
        public bool IsInner { get; set; }

        /// <summary>
        /// Notes and remarks
        /// </summary>
        [JsonProperty("remark")]
        public string Remark { get; set; }

    }

}
