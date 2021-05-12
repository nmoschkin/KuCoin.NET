using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kucoin.NET.Localization;
using Kucoin.NET.Observable;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.User
{
    public class Account
    {
        public override string ToString()
        {
            return $"{Id} ({Type}): {Currency}: {Balance:#,##0.00#######}";
        }

        [JsonProperty("id")]
        [KeyProperty]
        public string Id { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("type")]
        public AccountType Type { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("available")]
        public decimal Available { get; set; }

        [JsonProperty("holds")]
        public decimal Holds { get; set; }

        [JsonIgnore]
        public string TypeDescription => AutoLocalizer.Localize(Type, this, nameof(Type));

    }
}
