﻿using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Observable;
using KuCoin.NET.Rest;
using KuCoin.NET.Websockets;
using KuCoin.NET.Websockets.Observations;
using KuCoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KuCoin.NET.Services
{
    /// <summary>
    /// Implements the contract for a trading symbol data provider service.
    /// </summary>
    public class SymbolDataService : ObservableBase, ISymbolDataService
    {
        public event EventHandler<SymbolChangedEventArgs> SymbolChanged;
        public event EventHandler<SymbolDataServiceEventArgs<Ticker>> TickerCalled;
        public event EventHandler<SymbolDataServiceEventArgs<AllSymbolsTickerItem>> Stats24HrCalled;


        protected TradingSymbol symbol;

        protected MarketCurrency baseCurrency;

        protected MarketCurrency quoteCurrency;

        protected Level2OrderBook level2obs;

        protected Level3OrderBook level3obs;

        protected Level2 level2feed;

        protected Level3 level3feed;

        protected Level3Direct level3directfeed;

        protected ICredentialsProvider cred;

        protected Level2Depth5 level2d5;

        protected Level2Depth50 level2d50;

        protected ObservableStaticMarketDepthUpdate level2d5update;

        protected ObservableStaticMarketDepthUpdate level2d50update;

        protected KlineFeed<Candle> klinefeed;

        protected TickerFeed tickerfeed;

        protected bool level2enabled;

        protected bool level3enabled;
        
        protected bool level3directenabled;

        protected bool level2d5enabled;

        protected bool level2d50enabled;

        protected bool klineEnabled;

        protected bool tickerEnabled;

        protected bool ownmarket = true;

        public virtual Level2OrderBook Level2Observation => level2obs;

        public virtual Level3OrderBook Level3Observation => level3obs;

        public virtual bool Connected
        {
            get
            {
                if (cred == null) return false;

                bool b = true;

                if (tickerEnabled) b &= tickerfeed?.Connected ?? false;

                if (klineEnabled) b &= klinefeed?.Connected ?? false;

                if (level2d5enabled) b &= level2d5?.Connected ?? false;

                if (level2d50enabled) b &= level2d50?.Connected ?? false;

                if (level2enabled) b &= level2feed?.Connected ?? false;

                if (level3enabled) b &= level3feed?.Connected ?? false;

                return b;
            }
        }

        public virtual string Symbol
        {
            get => (string)symbol;
            set
            {
                if ((string)symbol != value)
                {
                    _ = ChangeSymbol(value);
                }
            }
        }

        public virtual Level2 Level2Feed => level2feed;

        public virtual Level2Depth5 Level2Depth5 => level2d5;

        public virtual Level2Depth50 Level2Depth50 => level2d50;

        public virtual ObservableStaticMarketDepthUpdate Level2Depth5Update => level2d5update;

        public virtual ObservableStaticMarketDepthUpdate Level2Depth50Update => level2d50update;

        public virtual Level3 Level3Feed => level3feed ?? level3directfeed;

        public virtual Level3Direct Level3DirectFeed => level3directfeed;

        public virtual TradingSymbol TradingSymbolInfo => symbol;

        public virtual MarketCurrency BaseCurrency => baseCurrency;
        
        public virtual MarketCurrency QuoteCurrency => quoteCurrency;

        string IReadOnlySymbolicated.BaseCurrency => baseCurrency?.Currency;

        string IReadOnlySymbolicated.QuoteCurrency => quoteCurrency?.Currency;

        string IReadOnlySymbol.Symbol => (string)symbol;

        public SymbolDataService()
        {
            try
            {
                _ = Market.Instance;
            }
            catch
            {
            }
        }

        public virtual async Task<bool> Connect(ICredentialsProvider credentialsProvider)
        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            if (cred != null)
            {
                Reset();
            }

            cred = credentialsProvider;
            await Initialize();

            return Connected;
        }

        public virtual async Task<bool> Reconnect(bool flushSubscriptions)
        {
            if (flushSubscriptions) Reset();

            await Initialize();

            if (!flushSubscriptions)
            {
                if (level2enabled)
                {
                    await level2feed?.Reconnect();
                }

                if (level3enabled)
                {
                    await level3feed?.Reconnect();
                }

                if (level3directenabled)
                {
                    await level3directfeed?.Reconnect();
                }
            }

            return Connected;
        }

        protected virtual async Task Initialize(KucoinBaseWebsocketFeed connection = null)
        {
            if (tickerEnabled || klineEnabled || level2d5enabled || level2d50enabled)
            {
                if (tickerfeed == null || tickerfeed.Disposed)
                {
                    tickerfeed = new TickerFeed();
                    tickerfeed.FeedDisconnected += OnTickerDisconnected;
                }

                if (klinefeed == null || klinefeed.Disposed || connection != null)
                    klinefeed = new KlineFeed<Candle>();

                if (Dispatcher.Initialized)
                {
                    if (level2d5 == null || level2d5.Disposed || connection != null)
                        level2d5 = new Level2Depth5();
                    if (level2d50 == null || level2d50.Disposed || connection != null)
                        level2d50 = new Level2Depth50();
                }

                if (connection != null)
                {

                    await tickerfeed.MultiplexInit(connection);
                    await klinefeed.MultiplexInit(connection);

                    if (Dispatcher.Initialized)
                    {
                        await level2d5.MultiplexInit(connection);
                        await level2d50.MultiplexInit(connection);
                    }
                }
                else
                {
                    if (tickerfeed.Connected)
                    {
                        tickerfeed.Disconnect();
                    }

                    await tickerfeed.Connect(true);
                    await klinefeed.MultiplexInit(tickerfeed);

                    if (Dispatcher.Initialized)
                    {
                        await level2d5?.MultiplexInit(tickerfeed);
                        await level2d50?.MultiplexInit(tickerfeed);
                    }

                }
            }

            if (cred != null)
            {
                if (level2feed == null || level2feed.Disposed || connection != null)
                {
                    level2feed = new Level2(cred);
                    level2feed.FeedDisconnected += OnLevel2Disconnected;
                }

                if (level3feed == null || level3feed.Disposed || connection != null)
                {
                    level3feed = new Level3(cred);
                    level3feed.FeedDisconnected += OnLevel3Disconnected;
                }

                if (level3directfeed == null || level3directfeed.Disposed || connection != null)
                {
                    level3directfeed = new Level3Direct(cred);
                    level3directfeed.FeedDisconnected += OnLevel3Disconnected;
                }
            }
        }

        protected void OnTickerDisconnected(object sender, FeedDisconnectedEventArgs e)
        {
            klineEnabled = tickerEnabled = level2d5enabled = level2d50enabled = false;

            klinefeed = null;
            tickerfeed = null;
            level2d5 = null;
            level2d50 = null;
            level2d50update = null;
            level2d5update = null;
        }

        public virtual async Task<ISymbolDataService> AddSymbol(string symbol, bool shareConnection)
        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            var sym = new SymbolDataService();

            if (shareConnection)
            {
                sym.cred = cred;
                await sym.Initialize(tickerfeed);

                sym.klineEnabled = klineEnabled;
                sym.tickerEnabled = tickerEnabled;
                sym.level2d50enabled = level2d50enabled;
                sym.level2d5enabled = level2d5enabled;

                if (cred != null) 
                {
                    if (level2feed != null && level2feed.Connected)
                    {
                        sym.level2feed = level2feed;
                        sym.level2enabled = true;
                    }

                    if (level3feed != null && level3feed.Connected)
                    {
                        sym.level3feed = level3feed;
                        sym.level3enabled = true;
                        sym.level3directfeed = null;
                        sym.level3directenabled = false;
                    }

                    if (level3directfeed != null && level3directfeed.Connected)
                    {
                        sym.level3directfeed = level3directfeed;
                        sym.level3directenabled = true;
                        sym.level3feed = null;
                        sym.level3enabled = false;
                    }

                }

                sym.ownmarket = false;
            }
            else
            {
                await sym.Connect(cred);
            }

            await sym.ChangeSymbol(symbol);
            return sym;
        }

        public async Task ClearSymbol()
        {
            // market data feeds default to a disconnected state.
            if (level2enabled)
            {
                level2obs?.Dispose();

                if (ownmarket)
                {
                    level2feed?.Disconnect();
                    level2enabled = false;
                }
            }
            if (level3enabled || level3directenabled)
            {
                level3obs?.Dispose();

                if (ownmarket)
                {
                    level3directfeed?.Disconnect();
                    level3feed?.Disconnect();
                    level3enabled = false;
                    level3directenabled = false;
                }
            }

            // these are multiplexed, and always 'enabled'
            if (klineEnabled)
            {
                await klinefeed.ClearAllTickers();
            }

            if (tickerEnabled)
            {
                await tickerfeed.UnsubscribeAll();
            }

            // these are multiplexed but require the enabled state to be unset
            if (level2d5enabled)
            {
                await level2d5.UnsubscribeAll();
                level2d5enabled = false;
            }

            if (level2d50enabled)
            {
                await level2d50.UnsubscribeAll();
                level2d50enabled = false;
            }
            var oldsymbol = symbol;

            symbol = null;
            baseCurrency = quoteCurrency = null;

            SymbolChanged?.Invoke(this, new SymbolChangedEventArgs(null, oldsymbol));
        }

        public virtual async Task<string> ChangeSymbol(TradingSymbol newSymbol)
            => await ChangeSymbol((string)newSymbol);

        public virtual async Task<string> ChangeSymbol(IReadOnlySymbol newSymbol)
            => await ChangeSymbol(newSymbol.Symbol);

        public virtual async Task<string> ChangeSymbol(ISymbolicated newSymbol)
            => await ChangeSymbol(newSymbol.Symbol);

        public virtual async Task<string> ChangeSymbol(string newSymbol)
        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            var oldSym = symbol;
            
            symbol = Market.Instance.Symbols[newSymbol];

            if (oldSym?.Symbol == symbol?.Symbol)
            {
                return newSymbol;
            }
            else if (newSymbol == null)
            {
                await ClearSymbol();
                return null;
            }

            if (Market.Instance.Currencies.Contains(symbol.BaseCurrency))
            {
                baseCurrency = Market.Instance.Currencies[symbol.BaseCurrency];
            }
            else
            {
                baseCurrency = new MarketCurrency()
                {
                    Name = symbol.BaseCurrency,
                    FullName = symbol.BaseCurrency
                };
            }


            if (Market.Instance.Currencies.Contains(symbol.QuoteCurrency))
            {
                quoteCurrency = Market.Instance.Currencies[symbol.QuoteCurrency];
            }
            else
            {
                quoteCurrency = new MarketCurrency()
                {
                    Name = symbol.QuoteCurrency,
                    FullName = symbol.QuoteCurrency
                };
            }

            if (level2enabled)
            {
                level2obs?.Dispose();
                level2obs = await level2feed.SubscribeOne(newSymbol);
            }

            if (level3enabled || level3directenabled)
            {
                level3obs?.Dispose();
                if (level3directenabled)
                {
                    level3obs = await level3directfeed.SubscribeOne(newSymbol);
                }
                else
                {
                    level3obs = await level3feed.SubscribeOne(newSymbol);
                }
            }

            if (klineEnabled)
            {
                var kt = new List<KlineType>();

                foreach (var sym in klinefeed.ActiveTickers)
                {
                    kt.Add(sym.KlineType);
                }

                await klinefeed.ClearAllTickers();

                foreach (var k in kt)
                {
                    await klinefeed.SubscribeOne((string)symbol, k);
                }
            }

            if (tickerEnabled)
            {
                await tickerfeed.UnsubscribeAll();
                await tickerfeed.SubscribeOne((string)symbol);
            }

            if (level2d5enabled)
            {
                await level2d5.UnsubscribeAll();
                level2d5update = await level2d5.AddSymbol((string)symbol);
            }

            if (level2d50enabled)
            {
                await level2d50.UnsubscribeAll();
                level2d50update = await level2d5.AddSymbol((string)symbol);
            }

            SymbolChanged?.Invoke(this, new SymbolChangedEventArgs(symbol, oldSym));
            return (string)oldSym;
        }

        public virtual async Task EnableLevel2()
        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            if (level2enabled) return;

            if (cred == null)
            {
                throw new AuthenticationException();
            }

            if (level2feed == null)
            {
                level2feed = new Level2(cred);
            }

            if (level2feed.Connected == false)
            {
                await level2feed.Connect();
            }

            OnPropertyChanged(nameof(Level2Feed));
            level2feed.FeedDisconnected += OnLevel2Disconnected;

            level2obs = await level2feed.SubscribeOne((string)symbol);
            OnPropertyChanged(nameof(Level2Observation));
            level2enabled = true;
        }

        protected virtual void OnLevel2Disconnected(object sender, KuCoin.NET.Websockets.FeedDisconnectedEventArgs e)
        {
            level2enabled = false;
            level2feed = null;
            level2obs = null;
        }

        public virtual async Task EnableLevel3()
        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            if (level3enabled || level3directenabled) return;

            if (cred == null)
            {
                throw new AuthenticationException();
            }

            if (level3feed == null)
            {
                level3feed = new Level3(cred);
            }

            level3directfeed = null;

            if (level3feed.Connected == false)
            {
                await level3feed.Connect();
            }
            OnPropertyChanged(nameof(Level3Feed));
            level3feed.FeedDisconnected += OnLevel3Disconnected;

            level3obs = await level3feed.SubscribeOne((string)symbol);
            OnPropertyChanged(nameof(Level3Observation));
            level3enabled = true;
        }

        public virtual async Task EnableLevel3Direct()
        {

            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            if (level3enabled || level3directenabled) return;

            if (cred == null)
            {
                throw new AuthenticationException();
            }

            if (level3directfeed == null)
            {
                level3directfeed = new Level3Direct(cred);
            }

            level3feed = null;

            if (level3directfeed.Connected == false)
            {
                await level3directfeed.Connect();
            }
            OnPropertyChanged(nameof(Level3DirectFeed));
            level3directfeed.FeedDisconnected += OnLevel3Disconnected;

            level3obs = await level3directfeed.SubscribeOne((string)symbol);
            OnPropertyChanged(nameof(Level3Observation));
            level3directenabled = true;
        }

        protected virtual void OnLevel3Disconnected(object sender, KuCoin.NET.Websockets.FeedDisconnectedEventArgs e)
        {
            level3enabled = false;
            level3directenabled = false;
            level3feed = null;
            level3directfeed = null;
            level3obs = null;
        }

        public virtual async Task<AllSymbolsTickerItem> Get24HourStats()
        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            var results = await Market.Instance.Get24HourStats(Symbol);
            Stats24HrCalled?.Invoke(this, new SymbolDataServiceEventArgs<AllSymbolsTickerItem>(symbol, results));
            return results;
        }

        public async Task<TCol> GetKline<TCandle, TCustom, TCol>(KlineType klineType, int pieces)
            where TCandle : IFullCandle, TCustom, new()
            where TCol : IList<TCustom>, new()

        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            return await Market.Instance.GetKline<TCandle, TCustom, TCol>((string)symbol, klineType, startTime: klineType.GetStartDate(pieces));
        }

        public async Task<Ticker> GetTicker()
        {
            var results = await Market.Instance.GetTicker((string)symbol);
            TickerCalled?.Invoke(this, new SymbolDataServiceEventArgs<Ticker>(symbol, results));
            return results;
        }

        public virtual async Task<ObservableStaticMarketDepthUpdate> SubscribeLevel2StaticDepth(Level2Depth depth, IObserver<StaticMarketDepthUpdate> observer)
        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            if (depth == Level2Depth.Depth5)
            {
                if (level2d5enabled) return level2d5update;

                if (level2d5 == null)
                {
                    level2d5 = new Level2Depth5();
                    await level2d5.MultiplexInit(tickerfeed);
                }

                level2d5update = await level2d5.AddSymbol(Symbol);
                level2d5enabled = true;

                return level2d5update;
            }
            else if (depth == Level2Depth.Depth50)
            {
                if (level2d50enabled) return level2d50update;

                if (level2d50 == null)
                {
                    level2d50 = new Level2Depth50();
                    await level2d50.MultiplexInit(tickerfeed);
                }

                level2d50update = await level2d50.AddSymbol(Symbol);
                level2d50enabled = true;

                return level2d50update;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public virtual IDisposable SubscribeKline(KlineType klineType, IObserver<KlineFeedMessage<Candle>> observer)
        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);

            _ = Task.Run(async () => await klinefeed.SubscribeOne(Symbol, klineType));

            klineEnabled = true;
            return klinefeed.Subscribe(observer); 
        }

        public virtual IDisposable SubscribeTicker(IObserver<Ticker> observer)
        {
            if (disposed) throw new ObjectDisposedException(this.GetType().FullName);
            tickerEnabled = true;
            return tickerfeed.Subscribe(observer);
        }

        #region IDisposable

        protected bool disposed;

        protected bool disposing;

        protected virtual void Reset()
        {
            if (level2feed != null)
            {
                level2feed.FeedDisconnected -= OnLevel2Disconnected;
            }

            if (level3feed != null)
            {
                level3feed.FeedDisconnected -= OnLevel3Disconnected;
            }

            level2obs?.Dispose();
            level3obs?.Dispose();
            level2feed?.Dispose();
            level3feed?.Dispose();
            klinefeed?.Dispose();
            tickerfeed?.Dispose();
            level2d5?.Dispose();
            level2d50?.Dispose();

            if (disposing)
            {
                disposed = true;
                level3enabled = level2enabled = klineEnabled = tickerEnabled = level2d5enabled = level2d50enabled = false;

                level2obs = null;
                level3obs = null;
                level2feed = null;
                level3feed = null;
                klinefeed = null;
                tickerfeed = null;
                level2d5 = null;
                level2d50 = null;
                level2d50update = null;
                level2d5update = null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        public virtual bool Disposed => disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed || this.disposing) return;
            this.disposing = true;

            Reset();

            this.disposing = false;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~SymbolDataService()
        {
            Dispose(false);
        }

        #endregion IDisposable



    }
}