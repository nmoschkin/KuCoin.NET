
using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.Order
{
    /// <summary>
    /// Paginated order details
    /// </summary>
    public class OrderDetailsPage : IPaginated<OrderDetails>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<OrderDetails> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

    }

    /// <summary>
    /// Paginated order fills
    /// </summary>
    public class FillPage : IPaginated<Fill>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<Fill> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

    }
}
