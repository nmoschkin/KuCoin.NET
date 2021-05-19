using Kucoin.NET.Data.Interfaces;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.User
{
    class DepositListPage : IPaginated<DepositListItem>
    {
        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("items")]
        public IList<DepositListItem> Items { get; set; }
    }


    class WithdrawalListPage : IPaginated<Withdrawal>
    {
        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("items")]
        public IList<Withdrawal> Items { get; set; }
    }
}
