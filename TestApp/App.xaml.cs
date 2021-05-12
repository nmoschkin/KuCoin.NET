using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Media.Animation;

using Kucoin.NET.Helpers;

namespace KuCoinApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static App Current
        {
            get => (App)Application.Current;
        }

        internal Guid Seed { get; private set; }

        internal Settings Settings { get; private set; }

        public App() : base()
        {
            Settings = new Settings();
            Seed = Guid.Parse(Settings.Hvcyp);
        }

    }
}
