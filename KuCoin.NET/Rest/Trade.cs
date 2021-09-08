using System;
using System.Collections.Generic;
using System.Text;

using KuCoin.NET.Rest;
using KuCoin.NET.Data.Order;
using System.Threading.Tasks;
using System.Net.Http;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Json;
using KuCoin.NET.Helpers;

namespace KuCoin.NET.Rest
{
    /// <summary>
    /// View, create and manipulate orders.
    /// </summary>
    public class Trade : KucoinBaseRestApi
    {

        public Trade(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        public Trade(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox)
        {
        }

#region Orders

        public async Task<IList<OrderDetails>> ListOrders(
            OrderStatus? status = null, 
            string symbol = null,
            Side? side = null,
            OrderType? type = null,
            TradeType? tradeType = null,
            DateTime? startAt = null,
            DateTime? endAt = null
            )
        {
            var lp = new OrderListParams(status, symbol, side, type, tradeType, startAt, endAt);

            return await ListOrders(lp);
        }

        public async Task<IList<OrderDetails>> ListOrders(OrderListParams listParams)
        {
            return await GetAllPaginatedResults<OrderDetails, OrderDetailsPage>(HttpMethod.Get, "/api/v1/orders", reqParams: listParams.ToDict());
        }

        public async Task<string[]> CancelAllOrders(string symbol = null, TradeType? tradeType = null)
        {
            //  /api/v1/orders

            var dict = new Dictionary<string, object>();

            if (symbol != null)
            {
                dict.Add("symbol", symbol);
            }

            if (tradeType != null)
            {
                dict.Add("tradeType", EnumToStringConverter<TradeType>.GetEnumName((TradeType)tradeType));
            }

            var jobj = await MakeRequest(HttpMethod.Delete, "/api/v1/orders", reqParams: dict);
            return jobj["cancelledOrderIds"].ToObject<string[]>();
        }

        public async Task<string> CancelOrderByClientId(string clientOid)
        {
            var jobj = await MakeRequest(HttpMethod.Delete, $"/api/v1/order/client-order/{clientOid}");
            return jobj["cancelledOrderId"].ToObject<string>();

        }
        public async Task<string[]> CancelOrderById(string orderId)
        {
            var jobj = await MakeRequest(HttpMethod.Delete, $"/api/v1/orders/{orderId}");
            return jobj["cancelledOrderIds"].ToObject<string[]>();
        }

        /// <summary>
        /// Create a market margin order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateMarketMarginOrder(MarketOrder orderDetails)
        {
            return await CreateMarginOrder(orderDetails);
        }

        /// <summary>
        /// Createa a market spot order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateMarketSpotOrder(MarketOrder orderDetails)
        {
            return await CreateSpotOrder(orderDetails);
        }

        /// <summary>
        /// Create a limit margin order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateLimitMarginOrder(LimitOrder orderDetails)
        {
            return await CreateMarginOrder(orderDetails);
        }

        /// <summary>
        /// Create a limit spot order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateLimitSpotOrder(LimitOrder orderDetails)
        {
            return await CreateSpotOrder(orderDetails);
        }

        /// <summary>
        /// Create a margin order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateMarginOrder(OrderBase orderDetails)
        {
            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/margin/order", reqParams: orderDetails.ToDict());
            return jobj.ToObject<OrderReceipt>();

        }

        /// <summary>
        /// Create a spot order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateSpotOrder(OrderBase orderDetails)
        {
            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/orders", reqParams: orderDetails.ToDict());
            return jobj.ToObject<OrderReceipt>();
        }

        public async Task<OrderDetails> GetOrder(string orderId)
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/orders/" + orderId);
            return jobj.ToObject<OrderDetails>();

        }

        public async Task<IList<OrderDetails>> GetRecentOrders()
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/limit/orders");
            return jobj.ToObject<OrderDetails[]>();
        }

        public async Task<OrderDetails> GetOrderByClientOid(string clientOid)
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/order/client-order/" + clientOid);
            return jobj.ToObject<OrderDetails>();

        }

        #endregion

        #region Fills

        public async Task<IList<Fill>> ListFills(
            string symbol = null,
            Side? side = null,
            OrderType? type = null,
            TradeType? tradeType = null,
            DateTime? startAt = null,
            DateTime? endAt = null
            )
        {
            var lp = new OrderListParams(null, symbol, side, type, tradeType, startAt, endAt);

            return await ListFills(lp);
        }

        public async Task<IList<Fill>> ListFills(OrderListParams listParams)
        {
            return await GetAllPaginatedResults<Fill, FillPage>(HttpMethod.Get, "/api/v1/fills", reqParams: listParams.ToDict());
        }

        public async Task<IList<Fill>> RecentFills()
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/limit/fills");
            return jobj.ToObject<Fill[]>();
        }

        #endregion

        #region StopOrder


        public async Task<IList<StopOrderDetails>> ListStopOrders(
            OrderStatus? status = null,
            string symbol = null,
            Side? side = null,
            OrderType? type = null,
            TradeType? tradeType = null,
            DateTime? startAt = null,
            DateTime? endAt = null
            )
        {
            var lp = new OrderListParams(status, symbol, side, type, tradeType, startAt, endAt);

            return await ListStopOrders(lp);
        }

        public async Task<IList<StopOrderDetails>> ListStopOrders(OrderListParams listParams)
        {
            return await GetAllPaginatedResults<StopOrderDetails, StopOrderDetailsPage>(HttpMethod.Get, "/api/v1/stop-order", reqParams: listParams.ToDict());
        }

        public async Task<string[]> CancelAllStopOrders(string symbol = null, TradeType? tradeType = null, IEnumerable<string> orderIds = null)
        {
            //  /api/v1/orders

            var dict = new Dictionary<string, object>();

            if (orderIds != null)
            {
                var sb = new StringBuilder();
                foreach (var o in orderIds)
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append(o);
                }

                dict.Add("orderIds", sb.ToString());
            }

            if (symbol != null)
            {
                dict.Add("symbol", symbol);
            }

            if (tradeType != null)
            {
                dict.Add("tradeType", EnumToStringConverter<TradeType>.GetEnumName((TradeType)tradeType));
            }

            var jobj = await MakeRequest(HttpMethod.Delete, "/api/v1/stop-order/cancel", reqParams: dict);
            return jobj["cancelledOrderIds"].ToObject<string[]>();
        }

        public async Task<string> CancelStopOrderByClientId(string clientOid)
        {
            var dict = new Dictionary<string, object>();

            dict.Add("clientOid", clientOid);
            var jobj = await MakeRequest(HttpMethod.Delete, $"/api/v1/stop-order/cancelOrderByClientOid", reqParams: dict);
            return jobj["cancelledOrderId"].ToObject<string>();

        }
        public async Task<string[]> CancelStopOrderById(string orderId)
        {
            var jobj = await MakeRequest(HttpMethod.Delete, $"/api/v1/stop-order/{orderId}");
            return jobj["cancelledOrderIds"].ToObject<string[]>();
        }

        /// <summary>
        /// Create a market margin order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateMarketStopOrder(MarketOrder orderDetails)
        {
            return await CreateMarginOrder(orderDetails);
        }


        /// <summary>
        /// Create a limit spot order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateLimitStopOrder(LimitOrder orderDetails)
        {
            return await CreateStopOrder(orderDetails);
        }

        /// <summary>
        /// Create a spot order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        protected async Task<OrderReceipt> CreateStopOrder<T>(T orderDetails) where T: OrderBase
        {
            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/stop-order", reqParams: orderDetails.ToDict());
            return jobj.ToObject<OrderReceipt>();
        }

        public async Task<StopOrderDetails> GetStopOrder(string orderId)
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/stop-order/" + orderId);
            return jobj.ToObject<StopOrderDetails>();

        }

        public async Task<IList<OrderDetails>> GetRecentStopOrders()
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/limit/orders");
            return jobj.ToObject<OrderDetails[]>();
        }

        public async Task<StopOrderDetails> GetStopOrderByClientOid(string clientOid)
        {
            var dict = new Dictionary<string, object>();

            dict.Add("clientOid", clientOid);
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/stop-order/queryOrderByClientOid", reqParams: dict);
            return jobj.ToObject<StopOrderDetails>();

        }


        #endregion
    }
}
