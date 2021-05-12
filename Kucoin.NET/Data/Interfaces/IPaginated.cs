using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.Interfaces
{
    /// <summary>
    /// Interface for all KuCoin requests that support pagination.
    /// </summary>
    /// <typeparam name="T">The type parameter</typeparam>
    public interface IPaginated<T> where T : class, new()
    {
        [JsonProperty("pageSize")]
        int PageSize { get; set; }

        [JsonProperty("currentPage")]
        int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        int TotalNumber { get; set; }

        [JsonProperty("items")]
        IList<T> Items { get; set; }

        [JsonProperty("totalPages")]
        int TotalPages { get; set; }

    }
}
