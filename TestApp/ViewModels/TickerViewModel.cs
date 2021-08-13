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
using Windows.ApplicationModel.Activation;
using Kucoin.NET.Websockets;
using System.Net.WebSockets;
using System.Windows;
using Kucoin.NET.Helpers;
using Kucoin.NET.Services;

namespace KuCoinApp
{

    public class TickerViewViewModel : WindowViewModelBase
    {

        Credentials credWnd;

        private ObservableCollection<ISymbolDataService> activeServices;


        public ObservableCollection<ISymbolDataService> ActiveServices
        {
            get => activeServices;
            set
            {
                SetProperty(ref activeServices, value);
            }
        }


        public override event EventHandler AskQuit;

        public ICommand EditCredentialsCommand { get; protected set; }

        public void FocusCredWindow()
        {
            credWnd?.Focus();
        }
        public void OnError(Exception ex)
        {
            throw ex;
        }

        public void OnCompleted()
        {
        }

        public bool EditCredentials()
        {
            if (cred == null)
            {
                cred = CryptoCredentials.LoadFromStorage(App.Current.Seed);
            }

            credWnd = new Credentials(cred);

            var b = credWnd.ShowDialog();

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

        public TickerViewViewModel(IList<ISymbolDataService> activeServices)
        {
            ActiveServices = new ObservableCollection<ISymbolDataService>(activeServices);

            EditCredentialsCommand = new SimpleCommand(async (obj) =>
            {
                if (EditCredentials())
                {
                    
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

        protected override async Task Initialize()
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

            cred = CryptoCredentials.LoadFromStorage(App.Current.Seed, pin, false);

            if (cred != null && !cred.IsFilled) cred = null;

            if (cred == null && PinWindow.LastCloseResult == false)
            {
                AskQuit?.Invoke(this, new EventArgs());
                return;
            }

        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
