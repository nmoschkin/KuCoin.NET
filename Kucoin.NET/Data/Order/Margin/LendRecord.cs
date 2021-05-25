using Kucoin.NET.Data.Interfaces;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
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

    public class AccountLendRecord : IPaginated<AccountLendRecordEntry>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<AccountLendRecordEntry> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }




}
