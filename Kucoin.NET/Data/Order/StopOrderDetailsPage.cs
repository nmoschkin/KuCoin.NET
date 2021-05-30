using Kucoin.NET.Data.Interfaces;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order
{
    /// <summary>
    /// Paginated stop order information
    /// </summary>
    public class StopOrderDetailsPage : IPaginated<StopOrderDetails>
    {

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<StopOrderDetails> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }
}
