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
using KuCoinApp.Views;
using System.Text;

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

        public MainWindow Spot { get; set; }

        public FuturesWindow Futures { get; set; }

        internal Guid Seed { get; private set; }

        internal Settings Settings { get; private set; }

        public App() : base()
        {
            AppCenter.Start("6e9f9975-8980-452b-be36-b5c9b087896f",
                      typeof(Analytics), typeof(Crashes));
            
            Settings = new Settings();

            if (Settings.RegistryPropertyExists("Hvcyp"))
            {
                Settings.Hvcyp = Settings.HvcypOld;
                Settings.DeleteRegistryProperty("Hvcyp");
            }
            var vstr = new StringBuilder();
            Seed = Guid.Parse(Settings.Hvcyp);

            vstr.AppendLine($"{Crc32.Hash("done")},done");
            vstr.AppendLine($"{Crc32.Hash("match")},match");
            vstr.AppendLine($"{Crc32.Hash("change")},change");
            vstr.AppendLine($"{Crc32.Hash("open")},open");

            Clipboard.SetText(vstr.ToString());


        }

    }
}
