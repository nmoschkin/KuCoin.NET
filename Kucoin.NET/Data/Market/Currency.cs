using System;
using System.Collections.Generic;
using System.Text;

using Kucoin.NET.Observable;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.Market
{
    public class MarketCurrency 
    {


        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("precision")]
        public int Precision { get; set; }

        [JsonProperty("withdrawalMinSize")]
        public decimal WithdrawalMinSize { get; set; }

        [JsonProperty("withdrawalMinFee")]
        public decimal withdrawalMinFee { get; set; }

        [JsonProperty("isWithdrawEnabled")]
        public bool IsWithdrawEnabled { get; set; }

        [JsonProperty("isDepositEnabled")]
        public bool IsDepositEnabled { get; set; }

        [JsonProperty("isMarginEnabled")]
        public bool IsMarginEnabled { get; set; }

        [JsonProperty("isDebitEnabled")]
        public bool IsDebitEnabled { get; set; }

        public override string ToString()
        {
            
            if (FullName != null && Currency != null)
            {
                return $"{Currency}: {FullName}";
            }
            return FullName ?? Currency ?? base.ToString();
        }

    }
}
