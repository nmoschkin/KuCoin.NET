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
            var url = $"/api/v1/margin/borrow";

            var jobj = await MakeRequest(HttpMethod.Post, url, reqParams: borrowParams.ToDict());
            return jobj.ToObject<BorrowReceipt>();

        }

        public async Task<BorrowOrder> GetBorrowOrder(string orderId)
        {
            var url = $"/api/v1/margin/borrow";
            var dict = new Dictionary<string, object>();
            
            dict.Add("orderId", orderId);

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);
            return jobj.ToObject<BorrowOrder>();

        }

        public async Task<IList<RepayItem>> GetRepayRecord(string currency = null)
        {
            var url = "/api/v1/margin/borrow/outstanding";

            var dict = new Dictionary<string, object>();
            
            if (currency != null)
            {
                dict.Add("currency", currency);
            }
            return await GetAllPaginatedResults<RepayItem, RepayRecord>(HttpMethod.Get, url, reqParams: dict);


        }


        public async Task<IList<RepayItem>> GetRepaymentRecord(string currency = null)
        {
            var url = "/api/v1/margin/borrow/repaid";

            var dict = new Dictionary<string, object>();

            if (currency != null)
            {
                dict.Add("currency", currency);
            }
            return await GetAllPaginatedResults<RepayItem, RepayRecord>(HttpMethod.Get, url, reqParams: dict);


        }

        public async Task OneClickRepay(string currency, RepaySequence sequence, decimal size)
        {
            var url = "/api/v1/margin/repay/all";
            var dict = new Dictionary<string, object>();

            dict.Add("currency", currency);
            dict.Add("sequence", sequence);
            dict.Add("size", size);

            await MakeRequest(HttpMethod.Post, url, reqParams: dict);

        }

        public async Task RepaySingle(string currency, string tradeId, decimal size)
        {
            var url = "/api/v1/margin/repay/single";
            var dict = new Dictionary<string, object>();

            dict.Add("currency", currency);
            dict.Add("tradeId", tradeId);
            dict.Add("size", size);

            await MakeRequest(HttpMethod.Post, url, reqParams: dict);

        }

        public async Task<string> CreateLendOrder(LendParams lendParams)
        {
            var url = "/api/v1/margin/lend";
            var jobj = await MakeRequest(HttpMethod.Post, url, reqParams: lendParams.ToDict());

            return jobj["orderId"].ToObject<string>();

        }

        public async Task CancelLendOrder(string lendOrderId)
        {
            var url = "/api/v1/margin/lend/" + lendOrderId;

            await MakeRequest(HttpMethod.Delete, url);
        }

        public async Task ToggleAutoLend(AutoLendParams autoLendParams)
        {
            var url = "/api/v1/margin/toggle-auto-lend";
            await MakeRequest(HttpMethod.Post, url, reqParams: autoLendParams.ToDict());
        }

        public async Task<IList<LendOrder>> GetActiveLendOrders(string currency = null)
        {
            var url = "/api/v1/margin/lend/active";
            var dict = new Dictionary<string, object>();
            if (currency != null)
            {
                dict.Add("currency", currency);
            }
            
            return await GetAllPaginatedResults<LendOrder, LendRecord>(HttpMethod.Get, url, reqParams: dict);
        }

        public async Task<IList<HistoricalLendOrder>> GetHistoricalLendOrderHistory(string currency = null)
        {
            var url = "/api/v1/margin/lend/done";
            var dict = new Dictionary<string, object>();
            if (currency != null)
            {
                dict.Add("currency", currency);
            }

            return await GetAllPaginatedResults<HistoricalLendOrder, HistoricalLendRecord>(HttpMethod.Get, url, reqParams: dict);
        }

        public async Task<IList<UnsettledLendOrder>> GetUnsettledLendOrders(string currency = null)
        {
            var url = "/api/v1/margin/lend/trade/unsettled";
            var dict = new Dictionary<string, object>();
            if (currency != null)
            {
                dict.Add("currency", currency);
            }

            return await GetAllPaginatedResults<UnsettledLendOrder, UnsettledLendRecord>(HttpMethod.Get, url, reqParams: dict);
        }
        public async Task<IList<UnsettledLendOrder>> GetSettledLendOrders(string currency = null)
        {
            var url = "/api/v1/margin/lend/trade/settled";
            var dict = new Dictionary<string, object>();
            if (currency != null)
            {
                dict.Add("currency", currency);
            }

            return await GetAllPaginatedResults<UnsettledLendOrder, UnsettledLendRecord>(HttpMethod.Get, url, reqParams: dict);
        }

        public async Task<IList<AccountLendRecordEntry>> GetLendRecord(string currency = null)
        {
            var url = "/api/v1/margin/lend/assets";
            var dict = new Dictionary<string, object>();
            if (currency != null)
            {
                dict.Add("currency", currency);
            }

            return await GetAllPaginatedResults<AccountLendRecordEntry, AccountLendRecord>(HttpMethod.Get, url, reqParams: dict);
        }

        /// <summary>
        /// Get the lending market data. 
        /// The returned value is sorted based on the descending sequence of the daily interest rate and terms.
        /// </summary>
        /// <param name="currency">Currency</param>
        /// <param name="term">(Optional) Term (in days)</param>
        /// <returns></returns>
        public async Task<IList<LendingMarketData>> GetLendingMarketData(string currency, int term = 0)
        {
            var dict = new Dictionary<string, object>();
            var url = "/api/v1/margin/market";

            dict.Add("currency", currency);

            if (term > 0)
            {
                dict.Add("term", term); 
            }

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);
            return jobj.ToObject<LendingMarketData[]>();

        }

        /// <summary>
        /// Get the last 300 fills in the lending and borrowing market.
        /// The returned value is sorted based on the descending sequence of the order execution time.
        /// </summary>
        /// <param name="currency">Currency</param>
        /// <returns></returns>
        public async Task<IList<MarginTradeData>> GetMarginTradeData(string currency)
        {
            var dict = new Dictionary<string, object>();
            var url = "/api/v1/margin/trade/last";

            dict.Add("currency", currency);

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);
            return jobj.ToObject<MarginTradeData[]>();

        }

    }
}
