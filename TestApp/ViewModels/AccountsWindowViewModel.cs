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

namespace KuCoinApp.ViewModels
{
    public class AccountsWindowViewModel : ObservableBase
    {
        User user;
        ICredentialsProvider cred;
        ObservableCollection<Account> accounts;

        public event EventHandler<EventArgs> AccountsRefreshed;

        private List<string> accountTypes;

        public List<string> AccountTypes
        {
            get => accountTypes;
            set
            {
                SetProperty(ref accountTypes, value);
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


        public async Task UpdateAccounts()
        {
            
            if (accounts == null)
            {
                Accounts = await user.GetAccountList();
            }
            else
            {
                await user.UpdateAccountList(accounts);
            }
            
            var newTypes = new List<string>();

            foreach (var acct in accounts)
            {
                if (!newTypes.Contains(acct.TypeDescription))
                {
                    newTypes.Add(acct.TypeDescription);
                }
            }

            AccountTypes = newTypes;
            AccountsRefreshed?.Invoke(this, new EventArgs());

        }

        public AccountsWindowViewModel(ICredentialsProvider credProvider)
        {
            _ = Task.Run(async () =>
            {
                cred = credProvider;
                user = new User(cred);

                await UpdateAccounts();
            });
        }

        public AccountsWindowViewModel()
        {
            _ = Task.Run(async () =>
            {
                string pin;

                if (CryptoCredentials.Pin == null)
                {
                    pin = await PinWindow.GetPin();
                }
                else
                {
                    pin = CryptoCredentials.Pin;
                }

                cred = CryptoCredentials.LoadFromStorage(App.Current.Seed, pin, false);
                user = new User(cred);
                await UpdateAccounts();
            });

        }
    
    }

}
