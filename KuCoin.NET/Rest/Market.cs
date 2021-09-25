using System.Net.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using KuCoin.NET.Observable;
using KuCoin.NET.Data.Market;
using System.IO;
using System.Net;
using KuCoin.NET.Helpers;
using System.ComponentModel;
using System.Threading;

namespace KuCoin.NET.Rest
{
    public class MarketCreatedEventArgs : EventArgs
    {
        public Market Market { get; }

        public MarketCreatedEventArgs(Market market)
        {
            Market = market;
        }
    }


    /// <summary>
    /// A class to retrieve public market data from KuCoin.
    /// </summary>
    /// <remarks>
    /// The methods in this class do not require authentication.
    /// </remarks>
    public class Market : KucoinBaseRestApi
    {
        protected ObservableDictionary<string, TradingSymbol> symbols = null;

        protected AllSymbolsTicker tickers = null;

        protected ObservableDictionary<string, MarketCurrency> currencies;

        static Market inst = new Market();

        static bool creating = false;
        
        public event EventHandler MarketLoaded;

        public static event EventHandler MarketRefreshed;


        public static event EventHandler<MarketCreatedEventArgs> MarketCreated;

        static object lockObj = new object();

        /// <summary>
        /// Create the market instance if it doesn't already exist.
        /// </summary>
        /// <param name="getMarket">True to populate market data after creating.</param>
        /// <returns></returns>
        public static async Task CreateMarket(bool getMarket, bool forceRecreate = false)
        {
            if (creating) return;
            creating = true;

            lock (lockObj)
            {
                if (inst == null || forceRecreate)
                {
                    if (inst != null)
                    {
                        inst.MarketLoaded -= Inst_MarketLoaded;
                    }

                    inst = new Market();
                    inst.MarketLoaded += Inst_MarketLoaded;
                }

            }

            if (getMarket)
            {
                await inst.RefreshMarket();
            }

            creating = false;
            MarketCreated?.Invoke(inst, new MarketCreatedEventArgs(inst));
        }

        private static void Inst_MarketLoaded(object sender, EventArgs e)
        {
            MarketRefreshed?.Invoke(sender, e);
        }

        public bool IsMarketDataPopulated
        {
            get => symbols != null && symbols.Count > 0 && currencies != null && currencies.Count > 0;
        }
       
        /// <summary>
        /// Gets the singleton instance of <see cref="Market"/>.
        /// </summary>
        public static Market Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (inst == null)
                    {
                        inst = new Market();
                    }

                    return inst;
                }
            }
        }

        /// <summary>
        /// Create a new instance of the MarketData class.
        /// </summary>
        internal Market() : base(null, null, null)
        {
        }

        /// <summary>
        /// Gets the dictionary of currencies keyed on currency.
        /// </summary>
        public ObservableDictionary<string, MarketCurrency> Currencies
        {
            get => currencies;
            set
            {
                SetProperty(ref currencies, value);
            }
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

        /// <summary>
        /// Gets the all-symbols ticker.
        /// </summary>
        public AllSymbolsTicker Tickers
        {
            get => tickers;
            set
            {
                SetProperty(ref tickers, value);
            }
        }

        /// <summary>
        /// Get a list of all currencies that appear on the right-hand side of a trading symbol.
        /// </summary>
        /// <returns></returns>
        public async Task<MarketCurrency[]> GetAllQuoteCurrencies()
        {

            if (currencies == null || currencies.Count == 0)
            {
                await RefreshCurrenciesAsync();
            }

            if (symbols == null || symbols.Count == 0)
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

        /// <summary>
        /// Refresh trading symbols
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TradingSymbol>> RefreshSymbolsAsync()
        {

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/symbols", 5, false);
            var results = jobj.ToObject<List<TradingSymbol>>();


            Symbols = new ObservableDictionary<string, TradingSymbol>(
                new Comparison<TradingSymbol>((a, b) => {
                    return string.Compare(a.Symbol, b.Symbol);
                }), ListSortDirection.Ascending, results);


            return results;

        }


        /// <summary>
        /// Gets all the currencies
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MarketCurrency>> RefreshCurrenciesAsync()
        {
            var jobj = await MakeRequest(HttpMethod.Get, "api/v1/currencies", auth: false);

            var results = jobj.ToObject<List<MarketCurrency>>();

            Currencies = new ObservableDictionary<string, MarketCurrency>(
                new Comparison<MarketCurrency>((a, b) => {
                    return string.Compare(a.Currency, b.Currency);
                }), ListSortDirection.Ascending, results);

            return results;

        }

        /// <summary>
        /// Reload the market symbols and currencies lists from the server.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshMarket()
        {
            await RefreshSymbolsAsync();
            await RefreshCurrenciesAsync();

            MarketLoaded?.Invoke(this, new EventArgs());
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

            try
            {
                var obj = jobj.ToObject<Ticker>();
                if (obj != null)
                {
                    obj.Symbol = symbol;
                }
                return obj;
            }
            catch
            {
                return null;
            }

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


        /// <summary>
        /// Get the list of all tickers.
        /// </summary>
        /// <returns></returns>
        public async Task<AllSymbolsTicker> GetAllTickers()
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/market/allTickers", auth: false);


            //var l = new TickerList();
            //l.Populate(jobj);
            var l = jobj.ToObject<AllSymbolsTicker>();

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
        public async Task<AllSymbolsTickerItem> Get24HourStats(string symbol)
        {
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/market/stats", 5, false, param);

            var obj = jobj.ToObject<AllSymbolsTickerItem>();
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

        /// <summary>
        /// Get recent trade histories for the specified symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<List<TradeHistoryItem>> GetTradeHistories(string symbol)
        {
            var curl = "/api/v1/market/histories";
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            return jobj.ToObject<List<TradeHistoryItem>>();
        }

        /// <summary>
        /// Get the K-Line for the specified ticker symbol and the specified time range.
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="type">The K-Line type (the length of time represented by a single candlestick)</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>A list of candlesticks</returns>
        public async Task<List<Candle>> GetKline(string symbol, KlineType type, DateTime? startTime = null, DateTime? endTime = null) 
        {
            return await GetKline<Candle, Candle, List<Candle>>(symbol, type, startTime, endTime);
        }


        /// <summary>
        /// Get a customized K-Line for the specified ticker symbol and the specified time range.
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="type">The K-Line type (the length of time represented by a single candlestick)</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <typeparam name="TCandle">The type of the K-Line objects to create that implements both <see cref="IFullCandle"/> and <typeparamref name="TCustom"/>.</typeparam>
        /// <typeparam name="TCustom">
        /// The type of the objects to return.<br />
        /// This is usually a third-party interface.<br />  
        /// Objects of this type will be returned in the newly created collection.
        /// </typeparam>
        /// <typeparam name="TCol">The type of the collection that contains <typeparamref name="TCustom"/> objects.</typeparam>
        /// <returns>A list of candlesticks</returns>
        public async Task<TCol> GetKline<TCandle, TCustom, TCol>(
            string symbol, 
            KlineType type, 
            DateTime? startTime = null, 
            DateTime? endTime = null
            ) 
            where TCandle: IFullCandle, TCustom, new() 
            where TCol: IList<TCustom>, new()
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

            var results = new TCol();
            klineRaw.Reverse();

            foreach (var values in klineRaw)
            {
                var candle = new TCandle
                {
                    Timestamp = EpochTime.SecondsToDate(long.Parse(values[0])),

                    OpenPrice = decimal.Parse(values[1]),
                    ClosePrice = decimal.Parse(values[2]),

                    HighPrice = decimal.Parse(values[3]),
                    LowPrice = decimal.Parse(values[4]),

                    Amount = decimal.Parse(values[5]),
                    Volume = decimal.Parse(values[6])
                };

                if (candle is IFullKlineCandle<KlineType> tc)
                {
                    tc.Type = type;
                }

                results.Add(candle);
            }

            return results;

        }

    }
}
