using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Futures.Data.Trade;
using Kucoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Rest
{
    public class FuturesTrade : FuturesBaseRestApi
    {

        public FuturesTrade(ICredentialsProvider cred) : base(cred)
        {
        }


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


        public async Task<string> CreateMarketOrder(MarketFuturesOrder order)
        {
            string url = "/api/v1/orders";

            var res = await MakeRequest(HttpMethod.Post, url, reqParams: order.ToDict());
            return res["orderId"].ToObject<string>();
        }

        public async Task<string> CreateLimitOrder(LimitFuturesOrder order)
        {
            string url = "/api/v1/orders";

            var res = await MakeRequest(HttpMethod.Post, url, reqParams: order.ToDict());
            return res["orderId"].ToObject<string>();
        }

        public async Task CancelOrder(string orderId)
        {
            string url = "/api/v1/orders/" + orderId;
            await MakeRequest(HttpMethod.Delete, url);
        }

        public async Task MassCancelOrders(string symbol)
        {

            string url = "/api/v1/orders";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);
            await MakeRequest(HttpMethod.Delete, url, reqParams: dict);
        }



        public async Task MassCancelStopOrders(string symbol)
        {

            string url = "/api/v1/stopOrders";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);
            await MakeRequest(HttpMethod.Delete, url, reqParams: dict);
        }



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

        public async Task<IList<FuturesOrderDetails>> ListOrders(FuturesOrderListParams listParams)
        {
            return await GetAllPaginatedResults<FuturesOrderDetails, FuturesOrderDetailsPage>(HttpMethod.Get, "/api/v1/orders", reqParams: listParams.ToDict());
        }

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

        public async Task<IList<FuturesOrderDetails>> ListUntriggeredStopOrders(FuturesStopOrderListParams listParams)
        {
            return await GetAllPaginatedResults<FuturesOrderDetails, FuturesOrderDetailsPage>(HttpMethod.Get, "/api/v1/stopOrders", reqParams: listParams.ToDict());
        }


        public async Task<IList<FuturesOrderDetails>> ListRecentDoneOrders()
        {
            var url = "/api/v1/recentDoneOrders";

            var res = await MakeRequest(HttpMethod.Get, url);

            return res.ToObject<FuturesOrderDetails[]>();

        }

        public async Task<FuturesOrderDetails> GetOrder(string orderId)
        {

            var url = "/api/v1/orders/" + orderId;

            var res = await MakeRequest(HttpMethod.Get, url);

            return res.ToObject<FuturesOrderDetails>();
        }

        public async Task<FuturesOrderDetails> GetOrderByClientOid(string clientOid)
        {
            var url = "/api/v1/orders/byClientOid";

            var dict = new Dictionary<string, object>();
            dict.Add("clientOid", clientOid);

            var res = await MakeRequest(HttpMethod.Get, url, reqParams: dict);
            return res.ToObject<FuturesOrderDetails>();

        }



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

        public async Task<IList<FuturesFillDetails>> ListFills(FuturesOrderListParams listParams)
        {
            return await GetAllPaginatedResults<FuturesFillDetails, FuturesFillDetailsPage>(HttpMethod.Get, "/api/v1/fills", reqParams: listParams.ToDict());
        }

        public async Task<IList<FuturesFillDetails>> ListRecentFills()
        {
            var url = "/api/v1/recentFills";

            var jobj = await MakeRequest(HttpMethod.Get, url);

            return jobj.ToObject<FuturesFillDetails[]>();

        }

        public async Task<ActiveOrderValue> GetOrderStatistics(string symbol)
        {

            var url = "/api/v1/openOrderStatistics";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<ActiveOrderValue>();
        }

        public async Task<PositionDetails> GetPositionDetails(string symbol)
        {

            var url = "/api/v1/position";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<PositionDetails>();
        }


        public async Task<IList<PositionDetails>> GetPositionList(string symbol)
        {

            var url = "/api/v1/positions";
            var dict = new Dictionary<string, object>();
            dict.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<PositionDetails[]>();
        }

        public async Task ToggleAutoDepositMargin(string symbol, bool enable)
        {

            var url = "/api/v1/position/margin/auto-deposit-status";
            var dict = new Dictionary<string, object>();

            dict.Add("symbol", symbol);
            dict.Add("status", enable);

            await MakeRequest(HttpMethod.Post, url, reqParams: dict);
        }

        public async Task AddDepositMargin(string symbol, decimal amount)
        {

            var url = "/api/v1/position/margin/auto-deposit-status";

            var dict = new Dictionary<string, object>();

            dict.Add("symbol", symbol);
            dict.Add("margin", amount);
            dict.Add("bizNo", Guid.NewGuid().ToString("d"));

            await MakeRequest(HttpMethod.Post, url, reqParams: dict);
        }


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
