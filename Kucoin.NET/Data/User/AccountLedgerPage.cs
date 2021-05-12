using System;
using System.Collections.Generic;
using System.Text;

using Kucoin.NET.Data.Interfaces;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.User
{
    /// <summary>
    /// Page of account ledgers
    /// </summary>
    public class AccountLedgerPage : IPaginated<AccountLedgerItem>
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
        public IList<AccountLedgerItem> Items { get; set; }

    }

}
