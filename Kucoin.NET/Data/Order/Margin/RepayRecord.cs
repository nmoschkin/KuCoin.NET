using Kucoin.NET.Data.Interfaces;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
    public class RepayRecord : IPaginated<RepayItem>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<RepayItem> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }
}
