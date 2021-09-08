using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Data
{
    public abstract class PaginatedData<T> : IPaginated<T> where T: class, IDataObject, new()
    {

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<T> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        IList<T> IDataSeries<T, IList<T>>.Data => Items;

    }
}
