using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Order;
using KuCoin.NET.Futures.Data.Trade;
using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuCoin.NET.Futures.Rest
{
    /// <summary>
    /// Futures Trading
    /// </summary>
    public class FuturesTrade : FuturesBaseRestApi
    {

        /// <summary>
        /// Create a new Futures Trading class with the specified credentials.
        /// </summary>
        /// <param name="credProvider">An object that implements <see cref="ICredentialsProvider"/>.</param>
        public FuturesTrade(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        /// <summary>
        /// Create a new Futures Trading class with the specified credentials.
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is sandbox / not real-time.</param>
        public FuturesTrade(
            string key, 
            string secret, 
            string passphrase, 
            bool isSandbox = false) 
            : base(
                  key, 
                  secret, 
                  passphrase, 
                  isSandbox)
        {
        }

        /// <summary>
        /// Create a new market order
        /// </summary>
        /// <param name="order">Order parameters</param>
        /// <returns></returns>
        public async Task<string> CreateMarketOrder(MarketFuturesOrder order)
        {
            string url = "/api/v1/orders";

            var res = await MakeRequest(HttpMethod.Post, url, reqParams: order.ToDict());
            return res["orderId"].ToObject<string>();
        }

        /// <summary>
        /// Create a new limit order
        /// </summary>
        /// <param name="order">Order parameters</param>
        /// <returns></returns>
        public async Task<string> CreateLimitOrder(LimitFuturesOrder order)
        {
            string url = "/api/v1/orders";

            var res = await MakeRequest(HttpMethod.Post, url, reqParams: order.ToDict());
            return res["orderId"].ToObject<string>();
        }

        /// <summary>
        /// Cancel order by order Id
        /// </summary>
        /// <param name="orderId">The order Id of the order to cancel.</param>
        /// <returns></returns>
        public async Task CancelOrder(string orderId)
        {
            string url = "/api/v1/orders/" + orderId;
            await MakeRequest(HttpMethod.Delete, url);
        }

        /// <summary>
        /// Cancel all orders for the specified symbol.
        /// </summary>
        /// <param name="symbol">Trading symbol to cancel.</param>
        /// <returns></returns>
        public async Task MassCancelOrders(string symbol)
        {

            string url = "/api/v1/orders";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);
            await MakeRequest(HttpMethod.Delete, url, reqParams: dict);
        }

        /// <summary>
        /// Cancel all stop orders for the specified symbol.
        /// </summary>
        /// <param name="symbol">Trading symbol to cancel.</param>
        /// <returns></returns>
        public async Task MassCancelStopOrders(string symbol)
        {

            string url = "/api/v1/stopOrders";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);
            await MakeRequest(HttpMethod.Delete, url, reqParams: dict);
        }

        /// <summary>
        /// List orders using the specified optional parameters.
        /// </summary>
        /// <param name="symbol">The trading symbol</param>
        /// <param name="status">Order status</param>
        /// <param name="side">The side (buy or sell)</param>
        /// <param name="type">The order type</param>
        /// <param name="startAt">Start time</param>
        /// <param name="endAt">End time</param>
        /// <returns></returns>
        public async Task<IList<FuturesOrderDetails>> ListOrders(string symbol = null, 
            FuturesOrderStatus? status = null,
            Side? side = null,
            OrderType? type = null,
            DateTime? startAt = null,
            DateTime? endAt = null)
        {
            var lp = new FuturesOrderListParams(status, symbol, side, type, startAt, endAt);

            return await ListOrders(lp);
        }

        /// <summary>
        /// List orders using the specified parameters object.
        /// </summary>
        /// <param name="listParams">Parameters</param>
        /// <returns></returns>
        public async Task<IList<FuturesOrderDetails>> ListOrders(FuturesOrderListParams listParams)
        {
            return await GetAllPaginatedResults<FuturesOrderDetails, FuturesOrderDetailsPage>(HttpMethod.Get, "/api/v1/orders", reqParams: listParams.ToDict());
        }

        /// <summary>
        /// List all untriggered stop orders
        /// </summary>
        /// <param name="symbol">The trading symbol</param>
        /// <param name="status">Order status</param>
        /// <param name="side">The side (buy or sell)</param>
        /// <param name="type">The order type</param>
        /// <param name="startAt">Start time</param>
        /// <param name="endAt">End time</param>
        /// <returns></returns>
        public async Task<IList<FuturesOrderDetails>> ListUntriggeredStopOrders(string symbol = null,
            FuturesOrderStatus? status = null,
            Side? side = null,
            FuturesOrderType? type = null,
            DateTime? startAt = null,
            DateTime? endAt = null)
        {
            var lp = new FuturesStopOrderListParams(status, symbol, side, type, startAt, endAt);

            return await ListOrders(lp);
        }

        /// <summary>
        /// List all untriggered stop orders.
        /// </summary>
        /// <param name="listParams">List parameters.</param>
        /// <returns></returns>
        public async Task<IList<FuturesOrderDetails>> ListUntriggeredStopOrders(FuturesStopOrderListParams listParams)
        {
            return await GetAllPaginatedResults<FuturesOrderDetails, FuturesOrderDetailsPage>(HttpMethod.Get, "/api/v1/stopOrders", reqParams: listParams.ToDict());
        }

        /// <summary>
        /// List recently completed orders
        /// </summary>
        /// <returns></returns>
        public async Task<IList<FuturesOrderDetails>> ListRecentDoneOrders()
        {
            var url = "/api/v1/recentDoneOrders";

            var res = await MakeRequest(HttpMethod.Get, url);

            return res.ToObject<FuturesOrderDetails[]>();

        }

        /// <summary>
        /// Get order details by order Id
        /// </summary>
        /// <param name="orderId">The orderId of the order to retrieve.</param>
        /// <returns></returns>
        public async Task<FuturesOrderDetails> GetOrder(string orderId)
        {

            var url = "/api/v1/orders/" + orderId;

            var res = await MakeRequest(HttpMethod.Get, url);

            return res.ToObject<FuturesOrderDetails>();
        }

        /// <summary>
        /// Get order details by clientOid
        /// </summary>
        /// <param name="clientOid">The clientOid</param>
        /// <returns></returns>
        public async Task<FuturesOrderDetails> GetOrderByClientOid(string clientOid)
        {
            var url = "/api/v1/orders/byClientOid";

            var dict = new Dictionary<string, object>();
            dict.Add("clientOid", clientOid);

            var res = await MakeRequest(HttpMethod.Get, url, reqParams: dict);
            return res.ToObject<FuturesOrderDetails>();

        }

        /// <summary>
        /// List fills
        /// </summary>
        /// <param name="symbol">The trading symbol</param>
        /// <param name="status">Order status</param>
        /// <param name="side">The side (buy or sell)</param>
        /// <param name="type">The order type</param>
        /// <param name="startAt">Start time</param>
        /// <param name="endAt">End time</param>
        /// <returns></returns>
        public async Task<IList<FuturesFillDetails>> ListFills(string symbol = null,
            FuturesOrderStatus? status = null,
            Side? side = null,
            OrderType? type = null,
            DateTime? startAt = null,
            DateTime? endAt = null)
        {
            var lp = new FuturesOrderListParams(status, symbol, side, type, startAt, endAt);

            return await ListFills(lp);
        }

        /// <summary>
        /// List fills
        /// </summary>
        /// <param name="listParams">List parameters</param>
        /// <returns></returns>
        public async Task<IList<FuturesFillDetails>> ListFills(FuturesOrderListParams listParams)
        {
            return await GetAllPaginatedResults<FuturesFillDetails, FuturesFillDetailsPage>(HttpMethod.Get, "/api/v1/fills", reqParams: listParams.ToDict());
        }

        /// <summary>
        /// List recent fills
        /// </summary>
        /// <returns></returns>
        public async Task<IList<FuturesFillDetails>> ListRecentFills()
        {
            var url = "/api/v1/recentFills";

            var jobj = await MakeRequest(HttpMethod.Get, url);

            return jobj.ToObject<FuturesFillDetails[]>();

        }

        /// <summary>
        /// Get order statistics by contract symbol.
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <returns></returns>
        public async Task<ActiveOrderValue> GetOrderStatistics(string symbol)
        {

            var url = "/api/v1/openOrderStatistics";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<ActiveOrderValue>();
        }

        /// <summary>
        /// Get position details by contract symbol.
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <returns></returns>
        public async Task<PositionDetails> GetPositionDetails(string symbol)
        {

            var url = "/api/v1/position";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<PositionDetails>();
        }

        /// <summary>
        /// Get all positions by contract symbol.
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <returns></returns>
        public async Task<IList<PositionDetails>> GetPositionList(string symbol)
        {

            var url = "/api/v1/positions";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<PositionDetails[]>();
        }

        /// <summary>
        /// Enable/Disable Auto-Deposit Margin
        /// </summary>
        /// <param name="symbol">Contract symbol</param>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns></returns>
        public async Task ToggleAutoDepositMargin(string symbol, bool enable)
        {

            var url = "/api/v1/position/margin/auto-deposit-status";
            var dict = new Dictionary<string, object>();

            dict.Add("symbol", symbol);
            dict.Add("status", enable);

            await MakeRequest(HttpMethod.Post, url, reqParams: dict);
        }

        /// <summary>
        /// Manually add funds to deposit margin
        /// </summary>
        /// <param name="symbol">Contract symbol</param>
        /// <param name="amount">Amount of funds in working currency to add</param>
        /// <returns></returns>
        public async Task AddDepositMargin(string symbol, decimal amount)
        {

            var url = "/api/v1/position/margin/deposit-margin";

            var dict = new Dictionary<string, object>();

            dict.Add("symbol", symbol);
            dict.Add("margin", amount);
            dict.Add("bizNo", Guid.NewGuid().ToString("d"));

            await MakeRequest(HttpMethod.Post, url, reqParams: dict);
        }

        /// <summary>
        /// Get funding history by contract symbol
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="forward">True if returning data in forward order.</param>
        /// <param name="offset">Offset</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="maxCount">Max returned objects</param>
        /// <returns></returns>
        public async Task<FundingHistory> GetFundingHistory(string symbol,
            bool forward = true,
            long? offset = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int maxCount = 50)
        {

            var url = "/api/v1/funding-history";

            var dict = new Dictionary<string, object>();

            dict.Add("symbol", symbol);

            dict.Add("forward", forward);
            dict.Add("maxCount", maxCount);

            long st, et;
            DateTime d;

            if (offset != null)
            {
                dict.Add("offset", offset);
            }

            if (startTime == null)
            {
                st = 0;
            }
            else
            {
                d = (DateTime)startTime;
                st = EpochTime.DateToMilliseconds(d);
                dict.Add("startAt", st);
            }

            if (endTime == null)
            {
                et = 0;
            }
            else
            {
                d = (DateTime)endTime;
                et = EpochTime.DateToMilliseconds(d);
                dict.Add("endAt", et);
            }

            if (startTime != null && endTime != null && et < st) throw new ArgumentException("End time must be greater than start time");

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);
            return jobj.ToObject<FundingHistory>();

        }



    }
}
