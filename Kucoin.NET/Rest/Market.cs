using System.Net.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Kucoin.NET.Observable;
using Kucoin.NET.Data.Market;
using System.IO;
using System.Net;
using Kucoin.NET.Helpers;
using Kucoin.NET.Data.Interfaces;
using System.ComponentModel;

namespace Kucoin.NET.Rest
{

    /// <summary>
    /// A class to retrieve public market data from KuCoin.
    /// </summary>
    /// <remarks>
    /// The methods in this class do not require authentication.
    /// </remarks>
    public class Market : KucoinBaseRestApi
    {
        protected ObservableDictionary<string, TradingSymbol> symbols = null;

        protected TickerList tickers = null;

        protected ObservableDictionary<string, MarketCurrency> currencies;

        public async Task<MarketCurrency[]> GetAllQuoteCurrencies()
        {

            if (currencies == null)
            {
                await RefreshCurrenciesAsync();
            }

            if (symbols == null)
            {
                await RefreshSymbolsAsync();
            }

            var syms = symbols.ToArray();

            var curs = currencies.ToArray();

            var distinct = new Dictionary<string, MarketCurrency>();

            foreach (var sym in syms) 
            {
                if (distinct.ContainsKey(sym.QuoteCurrency)) continue;

                foreach (var cur in curs)
                {
                    if (cur.Currency == sym.QuoteCurrency)
                    {
                        distinct.Add(cur.Currency, cur);
                        break;
                    }
                }

            }

            return distinct.Values.ToArray();
        }

        public ObservableDictionary<string, MarketCurrency> Currencies
        {
            get => currencies;
            set
            {
                SetProperty(ref currencies, value);
            }
        }

        /// <summary>
        /// Create a new instance of the MarketData class.
        /// </summary>
        public Market() : base(null, null, null)
        {
        }

        /// <summary>
        /// Gets the list of known ticker symbols.
        /// </summary>
        public ObservableDictionary<string, TradingSymbol> Symbols
        {
            get => symbols;
            protected set
            {
                SetProperty(ref symbols, value);
            }
        }

        public TickerList Tickers
        {
            get => tickers;
            set
            {
                SetProperty(ref tickers, value);
            }
        }

        /// <summary>
        /// Gets a ticker for the specified symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<Ticker> GetTicker(string symbol)
        {
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/market/orderbook/level1", 5, false, param);

            var obj = jobj.ToObject<Ticker>();
            obj.Symbol = symbol;

            return obj;
        }

        /// <summary>
        /// Refresh the ticker for the specified symbol.
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        public async Task RefreshTicker(Ticker ticker)
        {
            var param = new Dictionary<string, object>();

            param.Add("symbol", ticker.Symbol);

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/market/orderbook/level1", 5, false, param);

            JsonConvert.PopulateObject(jobj.ToString(), ticker);
        }


        /// <summary>
        /// Returns a list of all symbols where the specified currency symbol is one part of a trading pair.
        /// </summary>
        /// <param name="currencySymbol">The currency symbol to filter.</param>
        /// <returns></returns>
        public IDictionary<string, TradingSymbol> FilterSymbolsByCurrency(string currencySymbol)
        {
            var dict = new Dictionary<string, TradingSymbol>();

            if (symbols == null) return null;

            foreach (var value in (IEnumerable<TradingSymbol>)symbols)
            {
                var tp = value.TradingPair;

                if (tp.Contains(currencySymbol))
                {
                    dict.Add(value.Symbol, value);
                }
            }

            return dict;
        }

        public async Task RefreshSymbolsAsync()
        {

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/symbols", 5, false);
            var slist = jobj.ToObject<List<TradingSymbol>>();

            if (symbols == null)
            {
                Symbols = new ObservableDictionary<string, TradingSymbol>();
            }

            foreach (var item in slist)
            {
                if (!symbols.Contains(item.Symbol))
                {
                    symbols.Add(item);
                }
                else
                {
                    ((IDictionary<string, TradingSymbol>)symbols)[item.Symbol] = item;
                }
            }

            //symbols.SortByKey();
        }


        /// <summary>
        /// Get the list of all tickers.
        /// </summary>
        /// <returns></returns>
        public async Task<TickerList> GetAllTickers()
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/market/allTickers", auth: false);


            //var l = new TickerList();
            //l.Populate(jobj);
            var l = jobj.ToObject<TickerList>();

            Tickers = l;
            return l;
        }

        /// <summary>
        /// Refresh the list of all tickers.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAllTickers()
        {
            if (tickers == null)
            {
                await GetAllTickers();
                return;
            }

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/market/allTickers");

            JsonConvert.PopulateObject(jobj.ToString(), tickers);
        }


        /// <summary>
        /// Gets a ticker for the specified symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<TickerListItem> Get24HourStats(string symbol)
        {
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/market/stats", 5, false, param);

            var obj = jobj.ToObject<TickerListItem>();
            obj.Symbol = symbol;

            return obj;
        }

        /// <summary>
        /// Gets all markets.
        /// </summary>
        /// <returns></returns>
        public async Task<string[]> GetMarketList()
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/markets");
            return jobj.ToObject<string[]>();
        }


        public async Task<List<Trade>> GetPartList(string symbol, int pieces)
        {
            var curl = pieces > 0 ? string.Format("/api/v3/market/orderbook/level2_{0}", pieces) : "/api/v3/market/orderbook/level2";
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            return jobj.ToObject<List<Trade>>();
        }

        public Task<List<Trade>> GetAggregatedOrder(string symbol) => GetPartList(symbol, 0);

        public async Task<Trade> GetAtomicOrder(string symbol)
        {
            var curl = string.Format("/api/v3/market/orderbook/level3?symbol={0}", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false);
            return jobj.ToObject<Trade>();

        }

        public async Task<List<TradeHistoryUnit>> GetTradeHistories(string symbol)
        {
            var curl = "/api/v1/market/histories";
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            return jobj.ToObject<List<TradeHistoryUnit>>();
        }

        public async Task<List<Candle>> GetKline(string symbol, KlineType type, DateTime? startTime = null, DateTime? endTime = null) 
        {
            return await GetKline<Candle, Candle, List<Candle>>(symbol, type, startTime, endTime);
        }


        /// <summary>
        /// Get the Kline for the specified ticker symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="type"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<V> GetKline<T, U, V>(
            string symbol, 
            KlineType type, 
            DateTime? startTime = null, 
            DateTime? endTime = null
            ) 
            where T: IWriteableCandle, U, new() 
            where V: IList<U>, new()
        {
            var curl = "/api/v1/market/candles";

            long st, et;
            var param = new Dictionary<string, object>();

            DateTime d;

            param.Add("symbol", symbol);
            param.Add("type", (string)type);

            
            if (startTime == null)
            {
                st = 0;
            }
            else
            {
                d = (DateTime)startTime;
                st = EpochTime.DateToSeconds(d);
                param.Add("startAt", st.ToString());
            }

            if (endTime == null)
            {
                et = 0;
            }
            else
            {
                d = (DateTime)startTime;
                et = EpochTime.DateToSeconds(d);
                param.Add("endAt", et.ToString());
            }

            if (startTime != null && endTime != null && et < st) throw new ArgumentException("End time must be greater than start time");


            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            var klineRaw = jobj.ToObject<List<List<string>>>();

            var results = new V();
            klineRaw.Reverse();

            foreach (var values in klineRaw)
            {
                var candle = new T();
                
                if (candle is IWriteableTypedCandle tc)
                {
                    tc.Type = type;
                }

                candle.Timestamp = EpochTime.SecondsToDate(long.Parse(values[0]));

                candle.OpenPrice = decimal.Parse(values[1]);
                candle.ClosePrice = decimal.Parse(values[2]);

                candle.HighPrice = decimal.Parse(values[3]);
                candle.LowPrice = decimal.Parse(values[4]);

                candle.Amount = decimal.Parse(values[5]);
                candle.Volume = decimal.Parse(values[6]);

                results.Add(candle);
            }

            return results;

        }

        /// <summary>
        /// Gets all the currencies
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MarketCurrency>> RefreshCurrenciesAsync()
        {
            // api/v1/currencies

            var jobj = await MakeRequest(HttpMethod.Get, "api/v1/currencies", auth: false);

            var results = jobj.ToObject<List<MarketCurrency>>();

            Currencies = new ObservableDictionary<string, MarketCurrency>(
                new Comparison<MarketCurrency>((a, b) => {
                return string.Compare(a.Currency, b.Currency);
            }), ListSortDirection.Ascending, results);

            return results;
        }

    }
}
