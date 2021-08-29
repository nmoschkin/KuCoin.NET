using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.User;
using Kucoin.NET.Observable;
using Kucoin.NET.Rest;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Websockets.Public;

using Kucoin.NET.Futures.Websockets;
using Kucoin.NET.Futures.Data.Market;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

using FancyCandles;

using KuCoinApp.Localization.Resources;
using KuCoinApp.ViewModels;
using KuCoinApp.Views;
using Kucoin.NET.Futures.Rest;
using Microsoft.AppCenter.Ingestion.Models;
using Kucoin.NET.Futures.Data.User;
using Kucoin.NET.Futures.Websockets.Observations;

namespace KuCoinApp
{

    public class FuturesWindowViewModel : WindowViewModelBase, IObserver<FuturesTicker>
    {        
        protected FuturesLevel2 futuresl2;

        protected Accounts accountWnd;

        protected FuturesTickerFeed tickerFeed;
                
        protected IDisposable tickerSubscription;

        protected IDisposable klineSubscription;

        protected Credentials credWnd;

        protected IntRange lastRange;

        protected bool isCredWndShowing;

        protected Market market;

        protected FuturesMarket fmarket;

        protected User user;

        protected bool isLoggedIn;

        protected ObservableCollection<Account> accounts;

        protected ObservableDictionary<string, FuturesContract> symbols;

        protected FuturesContract symbol;

        protected FuturesKlineType kt = FuturesKlineType.Min1;

        FuturesKlineCandle lastCandle = new FuturesKlineCandle(new FuturesCandle() { Timestamp = DateTime.Now, Type = FuturesKlineType.Min1 });

        protected Ticker nowTicker = null;

        protected decimal? nowPrice = null;

        protected decimal? volume = null;

        protected DateTime? volumeTime = null;

        protected CurrencyViewModel currency, quoteCurrency;

        protected ObservableCollection<FancyCandles.ICandle> data = new ObservableCollection<FancyCandles.ICandle>();

        protected ObservableCollection<CurrencyViewModel> currencies = new ObservableCollection<CurrencyViewModel>();

        protected FuturesLevel2Observation futureslevel2;
        
        protected List<SymbolViewModel> recentSymbols = new List<SymbolViewModel>();


        protected ObservableStaticMarketDepthUpdate marketUpdate;

        protected string priceFormat = "0.00";

        protected string sizeFormat = "0.00";

        public override event EventHandler AskQuit;

        public FuturesLevel2Observation FuturesLevel2
        {
            get => futureslevel2;
            protected set
            {
                SetProperty(ref futureslevel2, value);
            }
        }

        public ICommand RefreshSymbolsCommand { get; protected set; }
        public ICommand RefreshKlineCommand { get; protected set; }
        public ICommand RefreshPriceCommand { get; protected set; }
        public ICommand EditCredentialsCommand { get; protected set; }

        public ICommand ShowSpotCommand { get; protected set; }
        public ICommand ShowAccountsCommand { get; protected set; }

        public ObservableStaticMarketDepthUpdate MarketUpdate
        {
            get => marketUpdate;
            set
            {
                SetProperty(ref marketUpdate, value);
            }
        }

        public IntRange LastCandleRange
        {
            get => lastRange;
            set
            {
                if (SetProperty(ref lastRange, value))
                {
                    App.Current.Settings.LastCandleRange = value;
                }
            }
        }

        public bool IsCredShowing
        {
            get => isCredWndShowing;
            protected set
            {
                SetProperty(ref isCredWndShowing, value);
            }
        }

        public FuturesLevel2 FuturesLevel2Feed
        {
            get => futuresl2;
            set
            {
                SetProperty(ref futuresl2, value);
            }
        }

        public ObservableCollection<Account> Accounts
        {
            get => accounts;
            set
            {
                SetProperty(ref accounts, value);
            }
        }

        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set
            {
                SetProperty(ref isLoggedIn, value);
            }
        }


        public User User
        {
            get => user;
            protected set
            {
                SetProperty(ref user, value);
            }
        }

        public ObservableCollection<CurrencyViewModel> Currencies
        {
            get => currencies;
            set
            {
                SetProperty(ref currencies, value);
            }
        }

        public CurrencyViewModel Currency
        {
            get => currency;
            set
            {
                SetProperty(ref currency, value);
            }
        }
        public CurrencyViewModel QuoteCurrency
        {
            get => quoteCurrency;
            set
            {
                SetProperty(ref quoteCurrency, value);
            }
        }

        public string PriceFormat
        {
            get => priceFormat;
            set
            {
                SetProperty(ref priceFormat, value);
            }
        }

        public string SizeFormat
        {
            get => sizeFormat;
            set
            {
                SetProperty(ref sizeFormat, value);
            }
        }

        public FuturesKlineType KlineType
        {
            get => kt;
            set
            {
                var oval = kt;

                if (SetProperty(ref kt, value))
                {
                    UpdateSymbol((string)Symbol, (string)Symbol, true, true, oval);

                    Volume = null;
                    VolumeTime = null;
                }
            }
        }


        public Ticker RealTimeTicker
        {
            get => nowTicker;
            set
            {
                SetProperty(ref nowTicker, value);
            }
        }

        public decimal? RealTimePrice
        {
            get => nowPrice;
            set
            {
                SetProperty(ref nowPrice, value);
            }
        }

        public decimal? Volume
        {
            get => volume;
            set
            {
                SetProperty(ref volume, value);
            }
        }

        public DateTime? VolumeTime
        {
            get => volumeTime;
            set
            {
                SetProperty(ref volumeTime, value);
            }
        }

        public ObservableDictionary<string, FuturesContract> Symbols
        {
            get => symbols;
            set
            {
                SetProperty(ref symbols, value);
            }
        }

        public ObservableCollection<FancyCandles.ICandle> Data
        {
            get => data;
            set
            {
                SetProperty(ref data, value);
            }
        }

        public FuturesContract Symbol
        {
            get => symbol;
            set
            {
                var ot = symbol;

                if (SetProperty(ref symbol, value))
                {
                    UpdateSymbol((string)ot, (string)value);

                    var tp = new string[] { symbol.BaseCurrency, symbol.QuoteCurrency };

                    foreach (var curr in currencies)
                    {
                        if (curr.Currency.Currency == tp[0])
                        {
                            if (!curr.ImageLoaded) _ = curr.LoadImage();
                            Currency = curr;
                        }
                        else if (curr.Currency.Currency == tp[1])
                        {
                            if (!curr.ImageLoaded) _ = curr.LoadImage();
                            QuoteCurrency = curr;
                        }
                    }

                    OnPropertyChanged(nameof(SymbolDescription));
                }
                else
                {
                    Data?.Clear();
                }
            }
        }

        public string SymbolDescription
        {
            get
            {
                if (market.Currencies == null || ((string)symbol) == null || !((string)symbol).Contains("-")) return null;

                var tsp = ((string)symbol).Split("-");

                MarketCurrency c1, c2;
                c1 = c2 = null;

                foreach (var c in market.Currencies)
                {
                    if (c.Currency == tsp[0])
                    {
                        c1 = c;
                    }
                    else if (c.Currency == tsp[1])
                    {
                        c2 = c;
                    }
                }

                if (c1 != null && c2 != null)
                {
                    return string.Format("{0} - {1}", c1.FullName, c2.FullName);
                }
                else
                {
                    return (string)symbol;
                }
            }
        }

        protected void MakeFormats(string symbol)
        {
            var fsym = fmarket.ContractList[symbol];

            var lsym = $"{fsym.BaseCurrency}-{fsym.QuoteCurrency}";
            TradingSymbol sym;

            if (market.Symbols.Contains(lsym))
            {
                sym = market.Symbols[lsym];
            }
            else
            {
                return;
            }

            var inc = "0" + sym.QuoteIncrement.ToString().Replace("1", "0");

            SizeFormat = inc;

            inc = sym.PriceIncrement.ToString().Replace("1", "0");

            PriceFormat = inc;
        }



        protected void UpdateSymbol(
            string oldSymbol,
            string newSymbol,
            bool force = false,
            bool isKlineChange = false,
            FuturesKlineType? oldKline = null)
        {

            PushSymbol(newSymbol);
            MakeFormats(newSymbol);

            foreach (var sym in recentSymbols)
            {
                if (sym.Data.Symbol == newSymbol)
                {
                    SelectedSymbol = sym;
                    break;
                }
            }


            if (force || (oldSymbol != (string)symbol && oldSymbol != null))
            {
                tickerFeed.RemoveSymbol(oldSymbol).ContinueWith((t) =>
                {
                    RefreshData().ContinueWith(async (t) =>
                    {

                        await tickerFeed.AddSymbol(newSymbol);

                        Volume = null;
                        VolumeTime = null;

                        if (isKlineChange) return;

                        if (cred != null && (futuresl2 == null || futuresl2.Connected == false))
                        {
                            FuturesLevel2Feed = new FuturesLevel2(cred);
                            await futuresl2.Connect();
                        }

                        if (FuturesLevel2 != null)
                        {
                            FuturesLevel2.Dispose();
                        }

                        await futuresl2.AddSymbol(newSymbol).ContinueWith((t) =>
                        {
                            App.Current?.Dispatcher?.Invoke(() =>
                            {
                                FuturesLevel2 = t.Result;
                            });

                        });

                    });
                });

            }
            else
            {
                RefreshData().ContinueWith(async (t2) =>
                {
                    if (cred != null && (futuresl2 == null || futuresl2.Connected == false))
                    {
                        FuturesLevel2Feed = new FuturesLevel2(cred);
                        await futuresl2.Connect();
                    }

                    await futuresl2.AddSymbol(newSymbol).ContinueWith((t) =>
                    {
                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            FuturesLevel2 = t.Result;
                        });

                    });

                    await tickerFeed.AddSymbol(newSymbol);

                });
            }
        }

        bool finit = false;

        public async Task RefreshData()
        {
            if (!Kucoin.NET.Helpers.Dispatcher.Initialized)
            {
                Kucoin.NET.Helpers.Dispatcher.Initialize(new DispatcherSynchronizationContext(App.Current.Dispatcher));
            }

            if (cred != null && (futuresl2 == null || futuresl2.Connected == false))
            {
                if (finit) return;

                FuturesLevel2Feed = new FuturesLevel2(cred);
                await futuresl2.Connect();

                if (futuresl2.Connected == false)
                {
                    System.Windows.MessageBox.Show(
                        AppResources.ErrorNoInternet,
                        "",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);

                    return;
                }
                finit = true;
            }

            if (tickerFeed == null || tickerFeed.Connected == false)
            {
                if (finit) return;

                tickerFeed = new FuturesTickerFeed();

                await tickerFeed.Connect(true);

                if (tickerFeed.Connected == false)
                {
                    System.Windows.MessageBox.Show(
                        AppResources.ErrorNoInternet,
                        "",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);

                    return;

                }

                finit = true;
            }

            if (finit)
            {
                UpdateSymbol((string)symbol, (string)symbol, true);
            }
            else
            {
                var kc = await fmarket.GetKline<FuturesKlineCandle, FancyCandles.ICandle, ObservableCollection<FancyCandles.ICandle>>((string)symbol, KlineType, startTime: KlineType.GetStartDate(200));

                Volume = null;
                VolumeTime = null;

                Data = kc;

                App.Current?.Dispatcher?.Invoke(() =>
                {
                    LastCandle = (FuturesKlineCandle)kc?.LastOrDefault();
                });
            }
        }

        public void FocusCredWindow()
        {
            if (credWnd != null)
            {
                credWnd.Focus();
            }
        }

        public bool EditCredentials()
        {
            credWnd = new Credentials(cred);

            IsCredShowing = true;

            var b = credWnd.ShowDialog();

            IsCredShowing = false;
            credWnd = null;

            if (b == true)
            {
                cred.Parent.SaveToStorage();
                return true;
            }
            else
            {
                return false;
            }


        }


        SymbolViewModel selSym;

        protected void PushSymbol(string symbol)
        {
            App.Current.Settings.PushFuturesSymbol(symbol);

            App.Current.Dispatcher.Invoke(() =>
            {
                MakeRecentSymbols();
            });
        }

        protected void MakeRecentSymbols()
        {
            var sym = App.Current.Settings.FuturesSymbols;

            var rc = new List<SymbolViewModel>();

            if (symbols == null || symbols.Count == 0 || sym == null || sym.Length == 0) return;

            foreach (var s in sym)
            {
                foreach (var sobj in symbols)
                {
                    if (s == sobj.Symbol)
                    {
                        var snew = new SymbolViewModel(sobj);
                        rc.Add(snew);
                    }
                }
            }

            RecentSymbols = rc;
        }

        public SymbolViewModel SelectedSymbol
        {
            get => selSym;
            set
            {
                if (SetProperty(ref selSym, value))
                {
                    if ((symbol?.Symbol ?? null) != (selSym?.Symbol ?? null))
                    {
                        foreach (var sym in Symbols)
                        {
                            if (sym.Symbol == selSym?.Symbol)
                            {
                                Symbol = sym;
                                return;
                            }
                        }
                    }
                }
            }
        }

        public List<SymbolViewModel> RecentSymbols
        {
            get => recentSymbols;
            set
            {
                SetProperty(ref recentSymbols, value);
            }
        }

        public FuturesKlineCandle LastCandle
        {
            get => lastCandle;
            set
            {
                if (SetProperty(ref lastCandle, value))
                {
                    if (data.Count == 0 || !Candle.IsTimeInCandle((ITypedCandle<FuturesKlineType>)data.LastOrDefault(), lastCandle.Timestamp))
                    {
                        data.Add(lastCandle);
                    }
                    else
                    {
                        data[data.Count - 1] = lastCandle;
                    }
                }

            }
        }

        void IObserver<FuturesTicker>.OnNext(FuturesTicker ticker)
        {
            if ((string)ticker.Symbol != (string)symbol) return;
            Task.Run(() =>
            {
                decimal p = ticker.Price;
                var n = DateTime.Now;

                n = n.AddSeconds(-1 * n.Second);

                FuturesKlineCandle kl = null;

                if ((data?.Count ?? 0) == 0 || !Candle.IsTimeInCandle(LastCandle, ticker.Timestamp))
                {
                    RefreshData().Wait();
                }

                kl = ((FuturesKlineCandle)data.LastOrDefault()) ?? new FuturesKlineCandle() { Timestamp = n, Type = this.KlineType };

                if (p < kl.LowPrice)
                {
                    kl.LowPrice = p;
                }
                else if (p > kl.HighPrice)
                {
                    kl.HighPrice = p;
                }

                kl.ClosePrice = p;

                if (VolumeTime != null && Candle.IsTimeInCandle(kl, (DateTime)VolumeTime))
                {
                    kl.Volume = Volume ?? 0;
                }

                RealTimeTicker = ticker;
                RealTimePrice = ticker.Price;

            }).Wait();

        }

        public async Task<bool> LoginUser()
        {
            cred = CryptoCredentials.LoadFromStorage(App.Current.Seed);

            if (cred.IsFilled)
            {
                try
                {
                    user = new User(cred);
                    Accounts = await user.GetAccountList();

                    IsLoggedIn = true;

                    Kucoin.NET.Helpers.Dispatcher.InvokeOnMainThread(async (o) =>
                    {
                        try
                        {
                            tickerFeed?.Dispose();
                        }
                        catch { }
                        try
                        {
                            futuresl2?.Dispose();
                        }
                        catch { }
                        try
                        {
                            futuresl2?.Dispose();
                        }
                        catch { }

                        FuturesLevel2Feed = new FuturesLevel2(cred);

                        tickerFeed = new FuturesTickerFeed();
                        tickerFeed.Subscribe(this);

                        symbol = null;

                        await Initialize();
                    });

                    return true;
                }
                catch
                {
                }
            }

            Accounts = null;
            IsLoggedIn = false;

            return false;

        }

        public void OnError(Exception ex)
        {
            throw ex;
        }

        public void OnCompleted()
        {
        }

        public FuturesWindowViewModel()
        {


            App.Current.Settings.PropertyChanged += Settings_PropertyChanged;
            market = Market.Instance;
            lastRange = App.Current.Settings.LastCandleRange;

            // Make sure you call this from the main thread,
            // alternately, create the feeds on the main thread
            // this will allow them to synchronize.
            Kucoin.NET.Helpers.Dispatcher.Initialize();

            // another way to initialize the dispatcher is
            // to create a new feed on the main thread, like this.
            tickerFeed = new FuturesTickerFeed();

            // The IObservable<T> / IObserver<T> pattern:
            tickerSubscription = tickerFeed.Subscribe(this);

            RefreshSymbolsCommand = new SimpleCommand(async (obj) =>
            {
                await market.RefreshSymbolsAsync();
                await market.RefreshCurrenciesAsync();

                var rec = App.Current.Settings.MostRecentSymbol;

                OnPropertyChanged(nameof(RecentSymbols));

                if (string.IsNullOrEmpty(rec)) return;

                foreach (var sym in symbols)
                {
                    if (sym.Symbol == rec)
                    {
                        Symbol = sym;
                        return;
                    }
                }

            });

            RefreshKlineCommand = new SimpleCommand(async (obj) =>
            {
                await RefreshData();
            });

            ShowAccountsCommand = new SimpleCommand((obj) =>
            {

                if (accountWnd == null || !accountWnd.IsLoaded)
                {
                    accountWnd = new Accounts(cred);
                    accountWnd.Closed += AccountWnd_Closed;
                }

                accountWnd.Show();
                accountWnd.Focus();


            });

            EditCredentialsCommand = new SimpleCommand(async (obj) =>
            {
                if (EditCredentials())
                {
                    await LoginUser();
                }
            });

            ShowSpotCommand = new SimpleCommand((obj) =>
            {
                if (App.Current.Spot != null)
                {
                    App.Current.Spot.Show();
                    App.Current.Spot.Focus();
                }
                else
                {
                    App.Current.Spot = new MainWindow();
                    App.Current.Spot.Show();
                }

            });

            QuitCommand = new SimpleCommand((obj) =>
            {
                AskQuit?.Invoke(this, new EventArgs());
            });

            Task.Run(async () =>
            {
                await Initialize();
            });
        }

        protected void AccountWnd_Closed(object sender, EventArgs e)
        {
            accountWnd.Closed -= AccountWnd_Closed;
            accountWnd = null;

            GC.Collect(0);
        }

        protected override async Task Initialize()
        {
            if (market == null) market = Market.Instance;
            if (fmarket == null) fmarket = new FuturesMarket();

            await market.RefreshSymbolsAsync().ContinueWith(async (t) =>
            {
                await market.RefreshCurrenciesAsync();

                foreach (var c in market.Currencies)
                {
                    currencies.Add(new CurrencyViewModel(c, false));
                }

                await App.Current?.Dispatcher?.Invoke(async () => {

                    string pin;

                    if (CryptoCredentials.Pin == null)
                    {
                        pin = await PinWindow.GetPin(App.Current.MainWindow);
                    }
                    else
                    {
                        pin = CryptoCredentials.Pin;
                    }

                    cred = CryptoCredentials.LoadFromStorage(App.Current.Seed, pin, false);
                    cred = (CryptoCredentials)cred?.AttachedAccount;

                    if (cred == null && PinWindow.LastCloseResult == false)
                    {
                        AskQuit?.Invoke(this, new EventArgs());
                        return;
                    }

                    var fuser = new FuturesUser(cred);
                    var over = await fuser.GetAccountOverview(FuturesCurrency.USDT);


                    await fmarket.RefreshOpenContractList();
                    Symbols = fmarket.ContractList;

                    var rec = App.Current.Settings.MostRecentFuturesSymbol;
                    OnPropertyChanged(nameof(RecentSymbols));

                    if (cred != null)
                    {
                        FuturesLevel2Feed = new FuturesLevel2(cred);
                        futuresl2.MonitorThroughput = true;

                        // for testing futures

                        //if (cred.AttachedAccount != null)
                        //{
                        //    futuresl2 = new FuturesLevel2(cred.AttachedAccount);
                        //    futuresl2.UpdateInterval = 50;
                        //}

                        if (!cred.Sandbox)
                        {

                            // connection sharing / multiplexing

                            // you can attach a public client feed
                            // to a protected host feed, but not vice-versa.
                            // the protected feed needs credentials, 
                            // and the public feed does not include them.

                            // we connect level2Feed.

                            // give its own socket because of the speed of data.
                            await futuresl2.Connect();

                            // for testing futures
                            //await futuresl2.Connect();

                            // we attach tickerFeed and klineFeed 
                            // by calling MultiplexInit with the host feed.
                            await tickerFeed.Connect(true);

                            // Now we have the multiplex host (tickerfeed)
                            // and the multiplexed client (klineFeed)

                            // the connection only exists on the multiplex host
                            // which serves as the connection maintainer.
                        }
                        else
                        {
                            await futuresl2.Connect();
                            await tickerFeed.Connect(true);
                        }
                    }
                    else
                    {
                        CryptoCredentials.Pin = pin;

                        await futuresl2.Connect();
                        await tickerFeed.Connect(true);
                    }

                    if (string.IsNullOrEmpty(rec)) return;

                    foreach (var sym in symbols)
                    {
                        if (sym.Symbol == rec)
                        {
                            Symbol = sym;

                            if (cred == null)
                            {
                                _ = Task.Run(() =>
                                {
                                    Kucoin.NET.Helpers.Dispatcher.InvokeOnMainThread(async (obj) =>
                                    {
                                        if (EditCredentials())
                                        {
                                            await LoginUser();
                                        }
                                    });
                                });

                                return;
                            }

                            // Bring up the testing console.

                            // Uncomment to use.
                            // _ = Program.TestMain(cred);

                            // Note, closing the testing console once it is open will close the program.

                            return;
                        }
                    }

                });
            });


        }

        public override void Dispose()
        {
            futuresl2?.Dispose();

            tickerSubscription.Dispose();
            tickerFeed.Dispose();

            try
            {
                App.Current.Settings.PropertyChanged -= Settings_PropertyChanged;
            }
            catch { }
        }

        ~FuturesWindowViewModel()
        {
            Dispose();
        }

        protected void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.Symbols))
            {
                OnPropertyChanged(nameof(RecentSymbols));
            }
        }
    }
}
