using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace KuCoin.NET.Data
{
    /// <summary>
    /// Interface for all KuCoin requests that support pagination.
    /// </summary>
    /// <typeparam name="T">The type parameter</typeparam>
    public interface IPaginated<T> : IDataSeries<T, IList<T>> where T : class, IDataObject, new()
    {
        /// <summary>
        /// Gets or sets the number of items on each page.
        /// </summary>
        [JsonProperty("pageSize")]
        int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        [JsonProperty("currentPage")]
        int CurrentPage { get; set; }

        /// <summary>
        /// The total number of items in the list.
        /// </summary>
        [JsonProperty("totalNum")]
        int TotalNumber { get; set; }

        /// <summary>
        /// Gets or sets the items on the current page.
        /// </summary>
        [JsonProperty("items")]
        IList<T> Items { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        [JsonProperty("totalPages")]
        int TotalPages { get; set; }

    }
}
