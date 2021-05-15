using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.User;
using Kucoin.NET.Observable;
using Kucoin.NET.Rest;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Websockets.Private;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KuCoinApp.Views;
using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Helpers;

namespace KuCoinApp
{

    public class MainWindowViewModel : ObservableBase, IObserver<Ticker>, IObserver<KlineFeedMessage<KlineCandle>>
    {
        private Level2 level2Feed;

        private Accounts accountWnd;

        private Level2Depth50 level2Feed50;

        private TickerFeed tickerFeed;

        private KlineFeed<KlineCandle> klineFeed;

        private IDisposable tickerSubscription;

        private IDisposable klineSubscription;

        private Credentials credWnd;

        private CryptoCredentials cred;

        private bool isCredWndShowing;

        private Market market;

        private User user;

        private bool isLoggedIn;

        private ObservableCollection<Account> accounts;

        private ObservableDictionary<string, TradingSymbol> symbols;

        private TradingSymbol symbol;

        private KlineType kt = KlineType.Min1;

        KlineCandle lastCandle = new KlineCandle(new Candle() { Timestamp = DateTime.Now, Type = KlineType.Min1 });

        private Ticker nowTicker = null;
        
        private decimal? nowPrice = null;

        private decimal? volume = null;

        private DateTime? volumeTime = null;

        private CurrencyViewModel currency, quoteCurrency;

        private ObservableCollection<FancyCandles.ICandle> data = new ObservableCollection<FancyCandles.ICandle>();

        private ObservableCollection<CurrencyViewModel> currencies = new ObservableCollection<CurrencyViewModel>();

        private ILevel2OrderBookProvider level2;

        private ObservableStaticMarketDepthUpdate marketUpdate;

        public ICommand RefreshSymbolsCommand { get; private set; }
        public ICommand RefreshKlineCommand { get; private set; }
        public ICommand RefreshPriceCommand { get; private set; }
        public ICommand EditCredentialsCommand { get; private set; }

        public ICommand ShowAccountsCommand { get; private set; }

        public ObservableStaticMarketDepthUpdate MarketUpdate
        {
            get => marketUpdate;
            set
            {
                SetProperty(ref marketUpdate, value);
            }
        }

        public bool IsCredShowing
        {
            get => isCredWndShowing;
            private set
            {
                SetProperty(ref isCredWndShowing, value);
            }
        }

        public ILevel2OrderBookProvider Level2
        {
            get => level2;
            set
            {
                SetProperty(ref level2, value);
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
            private set
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

        public KlineType KlineType
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

        public ObservableDictionary<string, TradingSymbol> Symbols
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

        public TradingSymbol Symbol
        {
            get => symbol;
            set
            {
                var ot = symbol;

                if (SetProperty(ref symbol, value))
                {
                    UpdateSymbol((string)ot, (string)value);
                    
                    foreach (var curr in currencies)
                    {
                        if (curr.Currency.Currency == symbol.TradingPair[0])
                        {
                            if (!curr.ImageLoaded) _ = curr.LoadImage();
                            Currency = curr;
                        }
                        else if (curr.Currency.Currency == symbol.TradingPair[1])
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

        private void UpdateSymbol(string oldSymbol, string newSymbol, bool force = false, bool isKlineChange = false, KlineType? oldKline = null)
        {

            App.Current.Settings.PushSymbol(newSymbol);

            if (force || (oldSymbol != (string)symbol && oldSymbol != null))
            {
                tickerFeed.RemoveSymbol(oldSymbol).ContinueWith((t) =>
                {
                    klineFeed.RemoveSymbol(oldSymbol, oldKline ?? KlineType).ContinueWith((t) =>
                    {
                        RefreshData().ContinueWith(async (t) =>
                        {

                            await tickerFeed.AddSymbol(newSymbol);
                            await klineFeed.AddSymbol(newSymbol, KlineType);

                            Volume = null;
                            VolumeTime = null;

                            if (isKlineChange) return;

                            //level2Feed50?.RemoveSymbol(oldSymbol).ContinueWith(async (t) =>
                            //{
                            //    MarketUpdate = await level2Feed50.AddSymbol(newSymbol);
                            //});


                            Level2?.Dispose();

                            if (isKlineChange) return;

                            await level2Feed.AddSymbol(newSymbol, 20).ContinueWith((t) =>
                            {
                                App.Current?.Dispatcher?.Invoke(() =>
                                {
                                    Level2 = t.Result;
                                });

                            });

                        });
                    });
                });

            }
            else
            {
                RefreshData().ContinueWith(async (t2) =>
                {


                    level2Feed?.AddSymbol(newSymbol, 50).ContinueWith((t) =>
                    {
                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            Level2 = t.Result;
                        });

                    });

                    //level2Feed50?.AddSymbol(newSymbol).ContinueWith((t) =>
                    //{
                    //    App.Current?.Dispatcher?.Invoke(() =>
                    //    {
                    //        MarketUpdate = t.Result;
                    //    });

                    //});

                    await tickerFeed.AddSymbol(newSymbol);
                    await klineFeed.AddSymbol(newSymbol, KlineType);

                });
            }


        }

        public async Task RefreshData()
        {
            var kc = await market.GetKline<KlineCandle, FancyCandles.ICandle, ObservableCollection<FancyCandles.ICandle>>((string)symbol, KlineType, startTime: KlineType.GetStartDate(200));

            Volume = null;
            VolumeTime = null;

            Data = kc;

            App.Current?.Dispatcher?.Invoke(() =>
            {
                LastCandle = (KlineCandle)kc?.LastOrDefault();
            });
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
                return true;
            }
            else
            {
                return false;
            }

            
        }

        public TradingSymbol[] RecentSymbols
        {
            get
            {
                List<TradingSymbol> output = new List<TradingSymbol>();
                var sym = App.Current.Settings.Symbols;

                if (symbols == null || symbols.Count == 0) return null;

                foreach (var s in sym)
                {
                    foreach (var sobj in symbols)
                    {
                        if (s == sobj.Symbol)
                        {
                            output.Add(sobj);
                        }
                    }
                }

                return output.ToArray();
            }
        }

        public KlineCandle LastCandle
        {
            get => lastCandle;
            set
            {
                if (SetProperty(ref lastCandle, value))
                {
                    if (data.Count == 0 || !Candle.IsTimeInCandle((ITypedCandle)data.LastOrDefault(), lastCandle.Timestamp))
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

        void IObserver<KlineFeedMessage<KlineCandle>>.OnNext(KlineFeedMessage<KlineCandle> ticker)
        {
            if (ticker.Symbol == (string)symbol && KlineType == ticker.Candles.Type)
            {
                App.Current?.Dispatcher.Invoke(() =>
                {
                    Volume = ticker.Candles.Volume;
                    VolumeTime = ticker.Timestamp;
                });
            }
            else
            {
                return;
            }

        }

        void IObserver<Ticker>.OnNext(Ticker ticker)
        {
            if ((string)ticker.Symbol != (string)symbol) return;
            Task.Run(async () =>
            {
                decimal p = ticker.Price;

                KlineCandle kl;

                if ((data?.Count ?? 0) == 0 || !Candle.IsTimeInCandle(LastCandle, ticker.Timestamp))
                {
                    await RefreshData();
                }
                else
                {
                    var n = DateTime.Now;
                    
                    n = n.AddSeconds(-1 * n.Second);

                    kl = ((KlineCandle)data.LastOrDefault()) ?? new KlineCandle() { Timestamp = n, Type = this.KlineType };

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

                    //App.Current?.Dispatcher?.Invoke(() => {
                    //    LastCandle = kl.Clone();
                    //});
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
                    user = new User(cred.Key, cred.Secret, cred.Passphrase);
                    Accounts = await user.GetAccountList();

                    IsLoggedIn = true;

                    await Initialize();

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

        public MainWindowViewModel()
        {
            KucoinBaseRestApi.GlobalSandbox = false;
            Dispatcher.Initialize();

            App.Current.Settings.PropertyChanged += Settings_PropertyChanged;
            market = new Market();
            
            tickerFeed = new TickerFeed();
            klineFeed = new KlineFeed<KlineCandle>();

            //Program.Feed = snapshotFeed;

            tickerSubscription = tickerFeed.Subscribe(this);
            klineSubscription = klineFeed.Subscribe(this);

            RefreshSymbolsCommand = new SimpleCommand(async (obj) =>
            {
                await market.RefreshSymbolsAsync();
                await market.RefreshCurrenciesAsync();

                var rec = App.Current.Settings.MostRecentSymbol;

                OnPropertyChanged(nameof(RecentSymbols));

                if (string.IsNullOrEmpty(rec)) return;

                foreach(var sym in symbols)
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

            Task.Run(async () =>
            {
                await Initialize();
            });
        }

        protected async Task Initialize()
        {
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

                    Symbols = market.Symbols;

                    var rec = App.Current.Settings.MostRecentSymbol;
                    OnPropertyChanged(nameof(RecentSymbols));

                    await tickerFeed.Connect(true);
                    await klineFeed.MultiplexInit(tickerFeed);

                    if (cred != null)
                    {
                        level2Feed = new Level2(cred);
                        level2Feed50 = new Level2Depth50(cred);

                        level2Feed.UpdateInterval = 50;

                        await level2Feed.Connect();
                        await level2Feed50.Connect();

                    }

                    if (string.IsNullOrEmpty(rec)) return;

                    foreach (var sym in symbols)
                    {
                        if (sym.Symbol == rec)
                        {
                            Symbol = sym;
                            //_ = Program.TestMain(cred);

                            return;
                        }
                    }

                    if (!await LoginUser())
                    {
                        EditCredentials();
                    }

                });
            });


        }

        ~MainWindowViewModel()
        {
            tickerSubscription.Dispose();
            tickerFeed.Dispose();

            try
            {
                App.Current.Settings.PropertyChanged -= Settings_PropertyChanged;
            }
            catch { }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.Symbols))
            {
                OnPropertyChanged(nameof(RecentSymbols));
            }
        }
    }
}
