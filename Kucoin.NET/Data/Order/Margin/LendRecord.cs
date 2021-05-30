using Kucoin.NET.Data.Interfaces;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
    /// <summary>
    /// Paginated lend records
    /// </summary>
    public class LendRecord : IPaginated<LendOrder>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<LendOrder> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Paginated historical lend records
    /// </summary>
    public class HistoricalLendRecord : IPaginated<HistoricalLendOrder>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<HistoricalLendOrder> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Paginated unsettled lend records
    /// </summary>
    public class UnsettledLendRecord : IPaginated<UnsettledLendOrder>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<UnsettledLendOrder> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Paginated settled lend records
    /// </summary>
    public class SettledLendRecord : IPaginated<SettledLendOrder>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<SettledLendOrder> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Paginated account lend records
    /// </summary>
    public class AccountLendRecord : IPaginated<AccountLendRecordItem>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<AccountLendRecordItem> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }




}
