using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order.Margin;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Rest
{
    public class Margin : KucoinBaseRestApi
    {
        public static readonly IReadOnlyList<string> MarkSupportedSymbols = new List<string>(new string[] {
            "USDT-BTC", "ETH-BTC", "LTC-BTC", "EOS-BTC", "XRP-BTC",
            "KCS-BTC", "DIA-BTC", "VET-BTC", "DASH-BTC", "DOT-BTC", "XTZ-BTC", "ZEC-BTC",
            "BCHSV-BTC", "ADA-BTC", "ATOM-BTC", "LINK-BTC", "LUNA-BTC", "NEO-BTC", "UNI-BTC",
            "ETC-BTC", "BNB-BTC", "TRX-BTC", "XLM-BTC" 
        });

        public Margin(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase,isSandbox)
        {
        }

        public Margin(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        public async Task<Price> GetMarkPrice(string symbol)
        {
            if (!(MarkSupportedSymbols as List<string>).Contains(symbol)) return null;

            var url = $"/api/v1/mark-price/{symbol}/current";

            var jobj = await MakeRequest(HttpMethod.Get, url);
            return jobj.ToObject<Price>();
        }

        public async Task<MarginConfiguration> GetMarginConfiguration()
        {
            var url = $"/api/v1/margin/config";

            var jobj = await MakeRequest(HttpMethod.Get, url);
            return jobj.ToObject<MarginConfiguration>();
        }

        public async Task<MarginAccountSummary> GetMarginAccounts()
        {
            var url = $"/api/v1/margin/account";

            var jobj = await MakeRequest(HttpMethod.Get, url);
            return jobj.ToObject<MarginAccountSummary>();

        }

        public async Task<BorrowReceipt> CreateBorrowOrder(BorrowParams borrowParams)
        {
            var url = $"/api/v1/margin/barrow";

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: borrowParams.ToDict());
            return jobj.ToObject<BorrowReceipt>();

        }

    }
}
