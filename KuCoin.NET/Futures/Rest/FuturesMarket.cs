using KuCoin.NET.Data.Market;
using KuCoin.NET.Futures.Data.Market;
using KuCoin.NET.Helpers;
using KuCoin.NET.Observable;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KuCoin.NET.Futures.Rest
{
    /// <summary>
    /// Futures market information
    /// </summary>
    public class FuturesMarket : FuturesBaseRestApi
    {

        private ObservableDictionary<string, FuturesContract> contractList;


        public FuturesMarket() : base(credProvider: null)
        {
        }

        public ObservableDictionary<string, FuturesContract> ContractList => contractList;

        /// <summary>
        /// Get all open futures contracts.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<FuturesContract>> GetOpenContractList()
        {
            var url = "/api/v1/contracts/active";
            var jobj = await MakeRequest(HttpMethod.Get, url);

            return jobj.ToObject<FuturesContract[]>();
        }

        /// <summary>
        /// Get all open futures contracts.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshOpenContractList()
        {
            var f = await GetOpenContractList();


            if (contractList == null)
            {
                contractList = new ObservableDictionary<string, FuturesContract>();
            }
            else
            {
                contractList.Clear();
            }
            foreach (var fc in f)
            {
                contractList.Add(fc);
            }
        }

        /// <summary>
        /// Get the current futures contract for the specified symbol.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns></returns>
        public async Task<FuturesContract> GetContract(string symbol)
        {
            var url = "/api/v1/contracts/" + symbol;
            var jobj = await MakeRequest(HttpMethod.Get, url);

            return jobj.ToObject<FuturesContract>();

        }

        /// <summary>
        /// Get the futures symbol ticker.
        /// </summary>
        /// <param name="symbol">The futures contract symbol.</param>
        /// <returns>A ticker object.</returns>
        public async Task<FuturesTicker> GetTicker(string symbol)
        {
            var dict = new Dictionary<string, object>();

            dict.Add("symbol", symbol);

            var url = "/api/v1/ticker";
            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<FuturesTicker>();

        }

        /// <summary>
        /// Get the K-Line for the specified ticker symbol and the specified time range.
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="type">The K-Line type (the length of time represented by a single candlestick)</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>A list of candlesticks</returns>
        public async Task<List<FuturesCandle>> GetKline(string symbol, FuturesKlineType type, DateTime? startTime = null, DateTime? endTime = null)
        {
            return await GetKline<FuturesCandle, FuturesCandle, List<FuturesCandle>>(symbol, type, startTime, endTime);
        }


        /// <summary>
        /// Get a customized K-Line for the specified ticker symbol and the specified time range.
        /// </summary>
        /// <typeparam name="TCandle">The type of the K-Line objects to create that implements both <see cref="IFullCandle"/> and <typeparamref name="TCustom"/>.</typeparam>
        /// <typeparam name="TCustom">
        /// The (usually user-provided) type of the objects to return.  
        /// Objects of this type will be returned in the newly created collection.
        /// </typeparam>
        /// <typeparam name="TCol">The type of the collection that contains <typeparamref name="TCustom"/> objects.</typeparam>
        /// <param name="symbol">The symbol</param>
        /// <param name="type">The K-Line type (the length of time represented by a single candlestick)</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>A list of candlesticks</returns>
        public async Task<TCol> GetKline<TCandle, TCustom, TCol>(
            string symbol, 
            FuturesKlineType type, 
            DateTime? startTime = null, 
            DateTime? endTime = null)
            where TCandle : ICandle, TCustom, new()
            where TCol : IList<TCustom>, new()
        {
            var curl = "/api/v1/kline/query";

            long st, et;
            var param = new Dictionary<string, object>();

            DateTime d;

            param.Add("symbol", symbol);
            param.Add("granularity", (int)type);


            if (startTime == null)
            {
                st = 0;
            }
            else
            {
                d = (DateTime)startTime;
                st = EpochTime.DateToMilliseconds(d);
                param.Add("from", st.ToString());
            }

            if (endTime == null)
            {
                et = 0;
            }
            else
            {
                d = (DateTime)startTime;
                et = EpochTime.DateToMilliseconds(d);
                param.Add("to", et.ToString());
            }

            if (startTime != null && endTime != null && et < st) throw new ArgumentException("End time must be greater than start time");

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            var klineRaw = jobj.ToObject<List<List<string>>>();

            var results = new TCol();
            //klineRaw.Reverse();

            foreach (var values in klineRaw)
            {
                var candle = new TCandle
                {
                    Timestamp = EpochTime.MillisecondsToDate(long.Parse(values[0])),
                    OpenPrice = decimal.Parse(values[1]),
                    HighPrice = decimal.Parse(values[2]),
                    LowPrice = decimal.Parse(values[3]),
                    ClosePrice = decimal.Parse(values[4]),
                    Volume = decimal.Parse(values[5])
                };

                if (candle is IKlineCandle<FuturesKlineType> tc)
                {
                    tc.Type = type;
                }
                else if (candle is IFullKlineCandle<FuturesKlineType> tce)
                {
                    tce.Type = type;
                }

                results.Add(candle);
            }

            return results;

        }

        /// <summary>
        /// Get historical trade data for the specified symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol to query.</param>
        /// <returns>A list of historical data points.</returns>
        public async Task<IList<HistoricalData>> GetHistoricalData(string symbol)
        {

            var url = "/api/v1/trade/history";

            var dict = new Dictionary<string, object>();

            dict.Add("symbol", symbol);


            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<HistoricalData[]>();
        }

        /// <summary>
        /// Get the interest rate list.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="forward">True if returning data in forward order.</param>
        /// <param name="offset">Offset</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="maxCount">Max returned objects</param>
        /// <returns></returns>
        public async Task<InterestHistory> GetInterestRateList(string symbol,
            bool forward = true,
            long? offset = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int maxCount = 50)
        {

            var url = "/api/v1/interest/query";

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
            return jobj.ToObject<InterestHistory>();

        }


        /// <summary>
        /// Get the market index list
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="forward">True if returning data in forward order.</param>
        /// <param name="offset">Offset</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="maxCount">Max returned objects</param>
        /// <returns></returns>
        public async Task<IndexList> GetIndexList(string symbol,
            bool forward = true,
            long? offset = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int maxCount = 50)
        {

            var url = "/api/v1/index/query";

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
            return jobj.ToObject<IndexList>();

        }

        /// <summary>
        /// Get the mark price info for the specified symbol
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns></returns>
        public async Task<MarkPrice> GetMarkPrice(string symbol)
        {
            var url = $"/api/v1/mark-price/{symbol}/current";
            var jobj = await MakeRequest(HttpMethod.Get, url);

            return jobj.ToObject<MarkPrice>();
        }

        /// <summary>
        /// Get the premium index for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="forward">True if returning data in forward order.</param>
        /// <param name="offset">Offset</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="maxCount">Max returned objects</param>
        /// <returns></returns>
        public async Task<PremiumIndex> GetPremiumIndex(string symbol,
            bool forward = true,
            long? offset = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int maxCount = 50)
        {

            var url = "/api/v1/premium/query";

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
            return jobj.ToObject<PremiumIndex>();

        }

        /// <summary>
        /// Get the current funding rate for the specified symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol</param>
        /// <returns></returns>
        public async Task<FundingRate> GetCurrentFundingRate(string symbol)
        {
            var url = $"/api/v1/funding-rate/{symbol}/current";
            var jobj = await MakeRequest(HttpMethod.Get, url);

            return jobj.ToObject<FundingRate>();
        }

        /// <summary>
        /// Get the system time stamp
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> GetTimestamp()
        {

            var url = "/api/v1/timestamp";
            var jobj = await MakeRequest(HttpMethod.Get, url);

            var l = jobj.ToObject<long>();

            return EpochTime.MillisecondsToDate(l);

        }

        /// <summary>
        /// Get the current KuCoin Futures service status and message
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceInfo> GetServiceStatus()
        {

            var url = "/api/v1/status";
            var jobj = await MakeRequest(HttpMethod.Get, url);

            return jobj.ToObject<ServiceInfo>();

        }

    }
}
