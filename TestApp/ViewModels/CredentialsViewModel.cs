
using KuCoinApp.Localization.Resources;

using Kucoin.NET.Observable;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KuCoinApp
{

    public delegate void SimpleCommandHandler(object parameter);

    public delegate bool SimpleCanExecuteHandler(object parameter);

    public class SimpleCommand : ICommand
    {

        public event EventHandler CanExecuteChanged;

        private SimpleCommandHandler handler;

        private SimpleCanExecuteHandler canExec;

        private bool? canExecStatus;

        public void ChangeExecutionStatus(bool? canExecute)
        {
            if (canExecStatus != canExecute)
            {
                canExecStatus = canExecute;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }

        public virtual bool CanExecute(object parameter)
        {
            return canExecStatus ?? canExec?.Invoke(parameter) ?? true;
        }

        public virtual void Execute(object parameter)
        {
            handler?.Invoke(parameter);
        }

        public SimpleCommand(SimpleCommandHandler handler, SimpleCanExecuteHandler canExecute = null)
        {
            this.handler = handler;
            this.canExec = canExecute;
        }
    }

    public class CloseWindowEventArgs : EventArgs
    {
        public bool Saved { get; private set; }

        public CloseWindowEventArgs(bool saved)
        {
            Saved = saved;
        }
    }

    public delegate void CloseWindowEventHandler(object sender, CloseWindowEventArgs e);


    public class CredentialsViewModel : ObservableBase
    {

        private CryptoCredentials oldCred;

        public event CloseWindowEventHandler CloseWindow;

        private CryptoCredentials cred;
                
        public ICommand SaveCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }

        public ICommand ClearCommand { get; private set; }
        public ICommand FuturesCommand { get; private set; }

        public ICommand ShowHidePasswordCommand { get; private set; }

        private string oktext = AppResources.Save;

        private string wndTitle = AppResources.CredentialsTitle;


        public string WindowTitle
        {
            get => wndTitle;
            private set
            {
                SetProperty(ref wndTitle, value);
            }
        }

        public string OKText
        {
            get => oktext;
            private set
            {
                SetProperty(ref oktext, value);
            }
        }

        public CryptoCredentials OldCredentials
        {
            get => oldCred;
            private set
            {
                SetProperty(ref oldCred, value);
            }
        }


        private bool showPassword;

        public CryptoCredentials Credential
        {
            get => cred;
            set
            {
                SetProperty(ref cred, value);
            }
        }

        public bool ShowPassword
        {
            get => showPassword;
            set
            {
                SetProperty(ref showPassword, value);
            }
        }

        private void NewToOld()
        {
            oldCred.Key = cred.Key;
            oldCred.Secret = cred.Secret;
            oldCred.Passphrase = cred.Passphrase;
        }

        private void SetupCommands()
        {
            FuturesCommand = new SimpleCommand((p) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    var futures = new Credentials((CryptoCredentials)cred.AttachedAccount);
                    futures.ShowDialog();
                });
            });

            SaveCommand = new SimpleCommand((p) =>
            {
                NewToOld();
                App.Current.Settings.ShowPassword = showPassword;

                if (!cred.Futures) Credential.SaveToStorage();

                CloseWindow?.Invoke(this, new CloseWindowEventArgs(true));
            });

            CancelCommand = new SimpleCommand((p) =>
            {
                Credential = null;
                CloseWindow?.Invoke(this, new CloseWindowEventArgs(false));
            });

            ClearCommand = new SimpleCommand((p) =>
            {
                Credential.Clear();
            });

            ShowHidePasswordCommand = new SimpleCommand((p) =>
            {
                ShowPassword = !ShowPassword;
            });

        }
        public CredentialsViewModel()
        {
            showPassword = App.Current.Settings.ShowPassword;
            oldCred = CryptoCredentials.LoadFromStorage(App.Current.Seed);
            cred = oldCred.Clone();

            SetupCommands();
        }

        public CredentialsViewModel(CryptoCredentials cred) 
        {
            oldCred = cred ?? CryptoCredentials.LoadFromStorage(App.Current.Seed);
            Credential = cred.Clone();

            if (cred.Futures)
            {
                OKText = AppResources.OK;
                WindowTitle = AppResources.FuturesCredentialsTitle;
            }

            SetupCommands();
        }

        public CredentialsViewModel(string pin) : this()
        {
            CryptoCredentials.Pin = pin;

            Credential = CryptoCredentials.LoadFromStorage(App.Current.Seed, pin);

        }

    }
}
