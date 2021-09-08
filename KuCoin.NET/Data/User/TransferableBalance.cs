using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.User
{
    /// <summary>
    /// Transferable balance information
    /// </summary>
    public class TransferableBalance
    {


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Balance
        /// </summary>
        [JsonProperty("balance")]
        public decimal Balance { get; set; }


        /// <summary>
        /// Available
        /// </summary>
        [JsonProperty("available")]
        public decimal Available { get; set; }


        /// <summary>
        /// Holds
        /// </summary>
        [JsonProperty("holds")]
        public decimal Holds { get; set; }


        /// <summary>
        /// Transferable
        /// </summary>
        [JsonProperty("transferable")]
        public decimal Transferable { get; set; }

    }


}
