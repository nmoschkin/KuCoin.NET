﻿using Kucoin.NET.Observable;

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

        public event CloseWindowEventHandler CloseWindow;

        private CryptoCredentials cred;
                
        public ICommand SaveCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }

        public ICommand ClearCommand { get; private set; }

        public ICommand ShowHidePasswordCommand { get; private set; }



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
                if (SetProperty(ref showPassword, value))
                {
                    App.Current.Settings.ShowPassword = value;
                }
            }
        }

        public CredentialsViewModel()
        {
            showPassword = App.Current.Settings.ShowPassword;

            SaveCommand = new SimpleCommand((p) =>
            {
                Credential.SaveToStorage();
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

        public CredentialsViewModel(CryptoCredentials cred) : this()
        {
            Credential = cred;
        }

        public CredentialsViewModel(string pin) : this()
        {
            CryptoCredentials.Pin = pin;

            Credential = CryptoCredentials.LoadFromStorage(App.Current.Seed, pin);

        }

    }
}