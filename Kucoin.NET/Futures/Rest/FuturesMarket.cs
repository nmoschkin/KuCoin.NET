using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Futures.Data.Market;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Rest
{
    public class FuturesMarket : FuturesBaseRestApi
    {
        public FuturesMarket() : base(cred: null)
        {
        }

        public async Task<IList<FuturesContract>> GetOpenContractList()
        {
            var url = "/api/v1/contracts/active";
            var jobj = await MakeRequest(HttpMethod.Get, url);

            return jobj.ToObject<FuturesContract[]>();
        }

        public async Task<FuturesContract> GetContract(string symbol)
        {
            var url = "/api/v1/contracts/" + symbol;
            var jobj = await MakeRequest(HttpMethod.Get, url);

            return jobj.ToObject<FuturesContract>();

        }

        public async Task<FuturesTicker> GetTicker(string symbol)
        {
            var dict = new Dictionary<string, object>();

            dict.Add("symbol", symbol);

            var url = "/api/v1/ticker";
            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<FuturesTicker>();

        }

    }
}
