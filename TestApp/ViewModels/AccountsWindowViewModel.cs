using DataTools.PinEntry.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kucoin.NET.Data.User;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Rest;
using System.Windows.Input;
using Kucoin.NET.Websockets.Public;
using System.Security.Authentication;
using Kucoin.NET.Websockets;
using Kucoin.NET.Websockets.Private;
using Kucoin.NET.Data.Websockets.User;
using Kucoin.NET.Helpers;

namespace KuCoinApp.ViewModels
{
    public class AccountsWindowViewModel : ObservableBase, IObserver<Ticker>, IObserver<BalanceNotice>, IDisposable
    {
        private User user;
        private ICredentialsProvider cred;
        private ObservableCollection<AccountItemViewModel> accounts;

        private TickerFeed ticker;
        private BalanceNoticeFeed balances; 

        private SymbolDistributable<Ticker> feedContext;
        private FeedObject<BalanceNotice> balanceContext;

        public event EventHandler<EventArgs> AccountsRefreshed;

        private List<string> accountTypes;

        private CurrencyViewModel quoteCurrency;
        private ObservableCollection<CurrencyViewModel> currencies;
        private ObservableCollection<CurrencyViewModel> quoteCurrencies;

        private ObservableCollection<AccountItemViewModel> mainAccounts;
        private ObservableCollection<AccountItemViewModel> tradingAccounts;
        private ObservableCollection<AccountItemViewModel> marginAccounts;
        private ObservableCollection<AccountItemViewModel> poolxAccounts;

        private Dictionary<string, AccountItemViewModel> mainDict;
        private Dictionary<string, AccountItemViewModel> tradingDict;
        private Dictionary<string, AccountItemViewModel> marginDict;
        private Dictionary<string, AccountItemViewModel> poolxDict;

        private Market market = Market.Instance;

        public ICommand RefreshAccountsCommand { get; private set; }

        public CurrencyViewModel QuoteCurrency
        {
            get => quoteCurrency;
            set
            {
                if (SetProperty(ref quoteCurrency, value))
                {
                    App.Current.Settings.QuoteCurrency = value.Currency.Currency;
                    _ = UpdateAccounts();
                }
            }
        }

        public ObservableCollection<CurrencyViewModel> QuoteCurrencies
        {
            get => quoteCurrencies;
            set
            {
                SetProperty(ref quoteCurrencies, value);
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

        public ObservableCollection<AccountItemViewModel> MainAccounts
        {
            get => mainAccounts;
            set
            {
                SetProperty(ref mainAccounts, value);
            }
        }


        public ObservableCollection<AccountItemViewModel> TradingAccounts
        {
            get => tradingAccounts;
            set
            {
                SetProperty(ref tradingAccounts, value);
            }
        }


        public ObservableCollection<AccountItemViewModel> MarginAccounts
        {
            get => marginAccounts;
            set
            {
                SetProperty(ref marginAccounts, value);
            }
        }


        public ObservableCollection<AccountItemViewModel> PoolXAccounts
        {
            get => poolxAccounts;
            set
            {
                SetProperty(ref poolxAccounts, value);
            }
        }

        public List<string> AccountTypes
        {
            get => accountTypes;
            private set
            {
                SetProperty(ref accountTypes, value);
            }
        }

        public ObservableCollection<AccountItemViewModel> Accounts
        {
            get => accounts;
            set
            {
                SetProperty(ref accounts, value);
            }
        }

        public async Task UpdateCurrencies()
        {
            if (CurrencyViewModel.Currencies == null)
            {
                await CurrencyViewModel.UpdateCurrencies();
            }
            
            App.Current?.Dispatcher?.Invoke(() =>
            {
                Currencies = new ObservableCollection<CurrencyViewModel>();
                QuoteCurrencies = new ObservableCollection<CurrencyViewModel>();

                CurrencyViewModel usdt = null;
                
                var cq = App.Current.Settings.QuoteCurrency;

                foreach (var curr in CurrencyViewModel.Currencies)
                {
                    var newCurr = new CurrencyViewModel(curr, false);

                    _ = newCurr.LoadImage();

                    currencies.Add(newCurr);

                    foreach (var curr2 in CurrencyViewModel.QuoteCurrencies)
                    {
                        if (curr2.Currency == curr.Currency)
                        {
                            quoteCurrencies.Add(newCurr);
                            break;
                        }
                    }

                    if (curr.Currency == cq) usdt = newCurr;
                }

                if (quoteCurrency == null)
                {
                    QuoteCurrency = usdt;
                }

            });

        }

        private void InsertAccount(Account acct, bool addTicker = false)
        {
            bool isquote = acct.Currency == QuoteCurrency.Currency.Currency;
            var pair = $"{acct.Currency}-{QuoteCurrency.Currency.Currency}";
            var acctvm = new AccountItemViewModel(acct);

            accounts.Add(acctvm);

            if (isquote)
            {
                acctvm.UpdateQuoteAmount(1.0M);
            }
            else
            {
                acctvm.UpdateQuoteAmount((decimal?)null);
            }

            if (acct.Type == AccountType.Main)
            {
                mainAccounts.Add(acctvm);
                mainDict.Add(pair, acctvm);
            }
            else if (acct.Type == AccountType.Trading)
            {
                tradingAccounts.Add(acctvm);
                tradingDict.Add(pair, acctvm);
            }
            if (acct.Type == AccountType.Margin)
            {
                marginAccounts.Add(acctvm);
                marginDict.Add(pair, acctvm);
            }
            if (acct.Type == AccountType.PoolX)
            {
                poolxAccounts.Add(acctvm);
                poolxDict.Add(pair, acctvm);
            }

            if (addTicker)
            {
                _ = ticker.SubscribeOne(pair);
            }
        }

        public async Task UpdateAccounts()
        {
            var accts = await user.GetAccountList();

            feedContext?.Dispose();
            balanceContext?.Dispose();
            ticker?.Dispose();

            var pairs = new List<string>();

            App.Current?.Dispatcher?.Invoke(() =>
            {
                var newTypes = new List<string>();

                mainDict = new Dictionary<string, AccountItemViewModel>();
                tradingDict = new Dictionary<string, AccountItemViewModel>();
                marginDict = new Dictionary<string, AccountItemViewModel>();
                poolxDict = new Dictionary<string, AccountItemViewModel>();

                MainAccounts = new ObservableCollection<AccountItemViewModel>();
                TradingAccounts = new ObservableCollection<AccountItemViewModel>();
                MarginAccounts = new ObservableCollection<AccountItemViewModel>();
                PoolXAccounts = new ObservableCollection<AccountItemViewModel>();
                Accounts = new ObservableCollection<AccountItemViewModel>();

                foreach (var acct in accts)
                {
                    var pair = $"{acct.Currency}-{QuoteCurrency.Currency.Currency}";

                    if (acct.Balance != 0M)
                    {
                        if (!pairs.Contains(pair)) pairs.Add(pair);
                    }

                    if (!newTypes.Contains(acct.TypeDescription))
                    {
                        newTypes.Add(acct.TypeDescription);
                    }

                    InsertAccount(acct);
                }

                AccountTypes = newTypes;
                AccountsRefreshed?.Invoke(this, new EventArgs());

            });

            balances = new BalanceNoticeFeed(cred);
            await balances.Connect(true);

            ticker = new TickerFeed();
            await ticker.MultiplexInit(balances);
            
            await balances.StartFeed();

            balanceContext = (FeedObject<BalanceNotice>)balances.Subscribe(this);

            feedContext = (SymbolDistributable<Ticker>)ticker.Subscribe(this);
            feedContext.ActiveSymbols = pairs;

            foreach (var pair in pairs)
            {
                var ps = pair.Split('-');

                if (ps[0] == ps[1]) continue;

                try
                {
                    var tick = await market.GetTicker(pair);

                    if (tick != null)
                    {
                        ((IObserver<Ticker>)this).OnNext(tick);
                        await ticker.SubscribeOne(pair);
                    }
                }
                catch
                {

                }
            }
        }

        private void MakeCommands()
        {
            RefreshAccountsCommand = new SimpleCommand(async (o) =>
            {
                await UpdateAccounts();
            });
        }

        public void OnCompleted()
        {
            return;
        }

        public void OnError(Exception error)
        {
            return;
        }

        object lockObj = new object();
        Dictionary<string, decimal> lastPrices = new Dictionary<string, decimal>();
        DateTime updateTimeout = DateTime.Now;

        void IObserver<BalanceNotice>.OnNext(BalanceNotice value)
        {
            var t = value.RelationEvent & RelationEventType.Accounts;

            switch(t)
            {
                case RelationEventType.Main:

                    foreach (var acct in mainAccounts)
                    {
                        if (acct.Currency == null) continue;

                        if (acct.Currency.Currency.Currency == value.Currency)
                        {
                            lock(lockObj)
                            {
                                acct.Available = value.Available;
                                acct.Balance = value.Total;
                                acct.Holds = value.Hold;

                                return;
                            }
                        }
                    }

                    Task.Run(async () =>
                    {
                        var newacct = await user.GetAccountList(value.Currency, AccountType.Main);
                        lock (lockObj) InsertAccount(newacct.FirstOrDefault(), true);
                    });

                    break;

                case RelationEventType.Trade:

                    foreach (var acct in tradingAccounts)
                    {
                        if (acct.Currency.Currency.Currency == value.Currency)
                        {
                            lock (lockObj)
                            {
                                acct.Available = value.Available;
                                acct.Balance = value.Total;
                                acct.Holds = value.Hold;

                                return;
                            }
                        }
                    }

                    Task.Run(async () =>
                    {
                        var newacct = await user.GetAccountList(value.Currency, AccountType.Trading);
                        
                        lock(lockObj) InsertAccount(newacct.FirstOrDefault(), true);
                    });

                    break;

                case RelationEventType.Margin:


                    foreach (var acct in marginAccounts)
                    {
                        if (acct.Currency.Currency.Currency == value.Currency)
                        {
                            lock(lockObj)
                            {
                                acct.Available = value.Available;
                                acct.Balance = value.Total;
                                acct.Holds = value.Hold;

                                return;
                            }
                        }
                    }

                    Task.Run(async () =>
                    {
                        var newacct = await user.GetAccountList(value.Currency, AccountType.Margin);
                        lock(lockObj) InsertAccount(newacct.FirstOrDefault(), true);
                    });

                    break;

                default:
                    _ = UpdateAccounts();
                    return;
            }
        }

        void IObserver<Ticker>.OnNext(Ticker value)
        {
            lastPrices[value.Symbol] = value.Price;

            if ((DateTime.Now - updateTimeout).TotalMilliseconds < 500) return;
            updateTimeout = DateTime.Now;

            lock (lockObj)
            {
                foreach (var varsym in lastPrices)
                {
                    AccountItemViewModel avm;

                    var sym = varsym.Key;
                    var price = varsym.Value;

                    if (mainDict.TryGetValue(sym, out avm))
                    {
                        avm.UpdateQuoteAmount(price);
                    }

                    if (tradingDict.TryGetValue(sym, out avm))
                    {
                        avm.UpdateQuoteAmount(price);
                    }
                    if (marginDict.TryGetValue(sym, out avm))
                    {
                        avm.UpdateQuoteAmount(price);
                    }

                    if (poolxDict.TryGetValue(sym, out avm))
                    {
                        avm.UpdateQuoteAmount(price);
                    }

                }
            }
        }

        public AccountsWindowViewModel(ICredentialsProvider credProvider)
        {
            MakeCommands();

            _ = Task.Run(async () =>
            {
                cred = credProvider;
                user = new User(cred);

                await UpdateCurrencies().ContinueWith((t) => UpdateAccounts());

            });
        }

        public AccountsWindowViewModel()
        {
            MakeCommands();

            _ = Task.Run(async () =>
            {
                string pin;

                if (CryptoCredentials.Pin == null)
                {
                    pin = await PinWindow.GetPin(App.Current.MainWindow);
                }
                else
                {
                    pin = CryptoCredentials.Pin;
                }

                if (pin == null) throw new InvalidCredentialException();

                cred = CryptoCredentials.LoadFromStorage(App.Current.Seed, pin, false);
                user = new User(cred);

                await UpdateCurrencies(); //.ContinueWith((t) => UpdateAccounts());

            });

        }

        public void Dispose()
        {
            feedContext?.Dispose();
            ticker?.Dispose();
        }

        ~AccountsWindowViewModel()
        {
            Dispose();
        }


    }

}
