using Kucoin.NET.Data;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.User
{
    public class DepositList : IPaginated<Deposit>
    {

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<Deposit> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }
}
