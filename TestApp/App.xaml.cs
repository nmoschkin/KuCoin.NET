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
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

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
            AppCenter.Start("6e9f9975-8980-452b-be36-b5c9b087896f",
                      typeof(Analytics), typeof(Crashes));
            
            Settings = new Settings();
            Seed = Guid.Parse(Settings.Hvcyp);
        }

    }
}
