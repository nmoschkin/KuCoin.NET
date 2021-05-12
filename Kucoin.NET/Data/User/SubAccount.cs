using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.User
{
    public class SubAccount
    {
        /// <summary>
        /// UserId
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }


        /// <summary>
        /// SubName
        /// </summary>
        [JsonProperty("subName")]
        public string SubName { get; set; }


        /// <summary>
        /// Remarks
        /// </summary>
        [JsonProperty("remarks")]
        public string Remarks { get; set; }


    }
}
