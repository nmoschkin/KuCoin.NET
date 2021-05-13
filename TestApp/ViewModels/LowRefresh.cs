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
using Kucoin.NET.Data.Interfaces;
using System.Windows.Input;
using Kucoin.NET.Websockets.Public;
using System.Security.Authentication;

namespace KuCoinApp.ViewModels.LowRefresh
{
    public class AccountsWindowViewModel : ObservableBase, IDisposable
    {
        private User user;
        private ICredentialsProvider cred;
        private ObservableCollection<AccountItemViewModel> accounts;

        public event EventHandler<EventArgs> AccountsRefreshed;

        private List<string> accountTypes;

        private CurrencyViewModel quoteCurrency;
        private ObservableCollection<CurrencyViewModel> currencies;

        private ObservableCollection<AccountItemViewModel> mainAccounts;
        private ObservableCollection<AccountItemViewModel> tradingAccounts;
        private ObservableCollection<AccountItemViewModel> marginAccounts;
        private ObservableCollection<AccountItemViewModel> poolxAccounts;

        private Dictionary<string, AccountItemViewModel> mainDict;
        private Dictionary<string, AccountItemViewModel> tradingDict;
        private Dictionary<string, AccountItemViewModel> marginDict;
        private Dictionary<string, AccountItemViewModel> poolxDict;

        public ICommand RefreshAccountsCommand { get; private set; }

        public CurrencyViewModel QuoteCurrency
        {
            get => quoteCurrency;
            set
            {
                SetProperty(ref quoteCurrency, value);
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

                CurrencyViewModel usdt = null;

                foreach (var curr in CurrencyViewModel.Currencies)
                {
                    var newCurr = new CurrencyViewModel(curr, false);

                    _ = newCurr.LoadImage();

                    currencies.Add(newCurr);
                    if (curr.Currency == "USDT") usdt = newCurr;
                }

                if (quoteCurrency == null)
                {
                    QuoteCurrency = usdt;
                }

            });

        }

        public async Task UpdateAccounts()
        {
            var accts = await user.GetAccountList();

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
                    var acctvm = new AccountItemViewModel(acct);

                    accounts.Add(acctvm);

                    if (!newTypes.Contains(acct.TypeDescription))
                    {
                        newTypes.Add(acct.TypeDescription);
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

                }
                AccountTypes = newTypes;

                AccountsRefreshed?.Invoke(this, new EventArgs());

            });

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

        object nextObj = new object();

        public void StartTickers()
        {
            _ = Task.Run(async () =>
            {
                while (!disposed)
                {
                    var market = new Market();
                    var tickers = await market.GetAllTickers();

                    lock (nextObj)
                    {
                        foreach (var ticker in tickers.Ticker.Values)
                        {
                            AccountItemViewModel avm;
                            var sym = ticker.Symbol;

                            if (mainDict.TryGetValue(sym, out avm))
                            {
                                avm.UpdateQuoteAmount((decimal)ticker.LastPrice);
                            }

                            if (tradingDict.TryGetValue(sym, out avm))
                            {
                                avm.UpdateQuoteAmount((decimal)ticker.LastPrice);
                            }
                            if (marginDict.TryGetValue(sym, out avm))
                            {
                                avm.UpdateQuoteAmount((decimal)ticker.LastPrice);
                            }

                            if (poolxDict.TryGetValue(sym, out avm))
                            {
                                avm.UpdateQuoteAmount((decimal)ticker.LastPrice);
                            }
                        }
                    }

                    await Task.Delay(5000);

                }
            });

        }

        public AccountsWindowViewModel(ICredentialsProvider credProvider)
        {
            MakeCommands();

            _ = Task.Run(async () =>
            {
                cred = credProvider;
                user = new User(cred);

                await UpdateCurrencies().ContinueWith((t) => UpdateAccounts());

                StartTickers();
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

                await UpdateCurrencies().ContinueWith((t) => UpdateAccounts());

                StartTickers();

            });

        }

        bool disposed;
        public void Dispose()
        {
            disposed = true;
        }

        ~AccountsWindowViewModel()
        {
            disposed = true;
        }


    }

}
