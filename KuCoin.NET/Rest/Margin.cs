using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Order.Margin;
using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Rest
{
    /// <summary>
    /// Margin trading
    /// </summary>
    public class Margin : KucoinBaseRestApi
    {
        /// <summary>
        /// Gets the list of supported mark price symbols
        /// </summary>
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

        /// <summary>
        /// Gets the mark price for the supported symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<Price> GetMarkPrice(string symbol)
        {
            if (!(MarkSupportedSymbols as List<string>).Contains(symbol)) return null;

            var url = $"/api/v1/mark-price/{symbol}/current";

            var jobj = await MakeRequest(HttpMethod.Get, url);
            return jobj.ToObject<Price>();
        }


        /// <summary>
        /// Get the user's margin configuration
        /// </summary>
        /// <returns></returns>
        public async Task<MarginConfiguration> GetMarginConfiguration()
        {
            var url = $"/api/v1/margin/config";

            var jobj = await MakeRequest(HttpMethod.Get, url);
            return jobj.ToObject<MarginConfiguration>();
        }

        /// <summary>
        /// Get a list of margin accounts
        /// </summary>
        /// <returns></returns>
        public async Task<MarginAccountSummary> GetMarginAccounts()
        {
            var url = $"/api/v1/margin/account";

            var jobj = await MakeRequest(HttpMethod.Get, url);
            return jobj.ToObject<MarginAccountSummary>();

        }

        /// <summary>
        /// Create a borrow order with the specified parameters
        /// </summary>
        /// <param name="borrowParams">Borrow order parameters</param>
        /// <returns>A receipt</returns>
        public async Task<BorrowReceipt> CreateBorrowOrder(BorrowParams borrowParams)
        {
            var url = $"/api/v1/margin/borrow";

            var jobj = await MakeRequest(HttpMethod.Post, url, reqParams: borrowParams.ToDict());
            return jobj.ToObject<BorrowReceipt>();

        }

        /// <summary>
        /// Get borrow order by orderId
        /// </summary>
        /// <param name="orderId">The Id of the borrow order</param>
        /// <returns>Borrow order information</returns>
        public async Task<BorrowOrder> GetBorrowOrder(string orderId)
        {
            var url = $"/api/v1/margin/borrow";
            var dict = new Dictionary<string, object>();
            
            dict.Add("orderId", orderId);

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);
            return jobj.ToObject<BorrowOrder>();

        }

        /// <summary>
        /// Get the outstanding repayment record
        /// </summary>
        /// <param name="currency">(Optional) currency</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the closed repayment record
        /// </summary>
        /// <param name="currency">(Optional) currency</param>
        /// <returns></returns>
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

        /// <summary>
        /// One-click auto-repay all.
        /// </summary>
        /// <param name="currency">The currency to repay</param>
        /// <param name="sequence">The repay sequence to use</param>
        /// <param name="size">The size of the repayment</param>
        /// <returns></returns>
        public async Task OneClickRepay(string currency, RepaySequence sequence, decimal size)
        {
            var url = "/api/v1/margin/repay/all";
            var dict = new Dictionary<string, object>();

            dict.Add("currency", currency);
            dict.Add("sequence", sequence);
            dict.Add("size", size);

            await MakeRequest(HttpMethod.Post, url, reqParams: dict);

        }

        /// <summary>
        /// Repay a single borrow
        /// </summary>
        /// <param name="currency">The currency</param>
        /// <param name="tradeId">The trade Id to repay</param>
        /// <param name="size">The size to repay</param>
        /// <returns></returns>
        public async Task RepaySingle(string currency, string tradeId, decimal size)
        {
            var url = "/api/v1/margin/repay/single";
            var dict = new Dictionary<string, object>();

            dict.Add("currency", currency);
            dict.Add("tradeId", tradeId);
            dict.Add("size", size);

            await MakeRequest(HttpMethod.Post, url, reqParams: dict);

        }

        /// <summary>
        /// Create a lend order with the specified parameters
        /// </summary>
        /// <param name="lendParams">The lend order parameters</param>
        /// <returns>An order Id</returns>
        public async Task<string> CreateLendOrder(LendParams lendParams)
        {
            var url = "/api/v1/margin/lend";
            var jobj = await MakeRequest(HttpMethod.Post, url, reqParams: lendParams.ToDict());

            return jobj["orderId"].ToObject<string>();

        }

        /// <summary>
        /// Cancel lend order by Id.
        /// </summary>
        /// <param name="lendOrderId">The Id of the order to cancel.</param>
        /// <returns></returns>
        public async Task CancelLendOrder(string lendOrderId)
        {
            var url = "/api/v1/margin/lend/" + lendOrderId;

            await MakeRequest(HttpMethod.Delete, url);
        }

        /// <summary>
        /// Configure auto lending with the specified parameters
        /// </summary>
        /// <param name="autoLendParams">Auto-lend parameters</param>
        /// <returns></returns>
        public async Task ToggleAutoLend(AutoLendParams autoLendParams)
        {
            var url = "/api/v1/margin/toggle-auto-lend";
            await MakeRequest(HttpMethod.Post, url, reqParams: autoLendParams.ToDict());
        }

        /// <summary>
        /// Get all active lend orders.
        /// </summary>
        /// <param name="currency">(Optional) currency</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get historical lend orders
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get unsettled lend orders
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get settled lend orders
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the entire lending record
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public async Task<IList<AccountLendRecordItem>> GetLendRecord(string currency = null)
        {
            var url = "/api/v1/margin/lend/assets";
            var dict = new Dictionary<string, object>();
            if (currency != null)
            {
                dict.Add("currency", currency);
            }

            return await GetAllPaginatedResults<AccountLendRecordItem, AccountLendRecord>(HttpMethod.Get, url, reqParams: dict);
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
