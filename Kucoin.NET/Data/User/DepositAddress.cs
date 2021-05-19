using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.User
{

    public class AddressBase : JsonDictBase
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


        /// <summary>
        /// Chain
        /// </summary>
        [JsonProperty("chain")]
        public string Chain { get; set; }

    }

    public class DepositAddress : AddressBase
    {
        /// <summary>
        /// ContractAddress
        /// </summary>
        [JsonProperty("contractAddress")]
        public string ContractAddress { get; set; }

    }


    public class WithdrawalAddress : AddressBase
    {
        [JsonProperty("isInner")]
        public bool IsInner { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

    }

}
