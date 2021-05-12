using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.User
{
    public class DepositAddress
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


        /// <summary>
        /// ContractAddress
        /// </summary>
        [JsonProperty("contractAddress")]
        public string ContractAddress { get; set; }

    }


}
