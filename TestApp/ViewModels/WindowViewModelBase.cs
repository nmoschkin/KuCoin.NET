using DataTools.PinEntry.Observable;

using Kucoin.NET.Data.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KuCoinApp.ViewModels
{
    public abstract class WindowViewModelBase : ObservableBase
    {

        protected CryptoCredentials cred;

        public abstract event EventHandler AskQuit;

        public virtual ICommand QuitCommand { get; protected set; }

        public CryptoCredentials Credentials => cred;


        protected abstract Task Initialize();


        public WindowViewModelBase()
        {
            //Initialize();
        }



    }
}
