using Kucoin.NET.Data.Interfaces;

using KuCoinApp.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KuCoinApp.Views
{
    /// <summary>
    /// Interaction logic for Accounts.xaml
    /// </summary>
    public partial class Accounts : Window
    {

        private AccountsWindowViewModel vm;
        bool init = false;

        public Accounts()
        {
            InitializeComponent();

            this.Loaded += Accounts_Loaded;
            this.SizeChanged += Accounts_SizeChanged;
            this.LocationChanged += Accounts_LocationChanged;

            vm = new AccountsWindowViewModel();
            DataContext = vm;

            App.Current.Settings.ApplyWindowSettings(this, "Accounts");
            init = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            vm?.Dispose();
        }

        public Accounts(ICredentialsProvider credProvider)
        {
            InitializeComponent();

            this.Loaded += Accounts_Loaded;
            this.SizeChanged += Accounts_SizeChanged;
            this.LocationChanged += Accounts_LocationChanged;

            vm = new AccountsWindowViewModel(credProvider);
            DataContext = vm;


            App.Current.Settings.ApplyWindowSettings(this, "Accounts");
            init = true;
        }

        private void Accounts_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Accounts_LocationChanged(object sender, EventArgs e)
        {
            if (!init) return;
            App.Current.Settings.SaveWindowSettings(this, "Accounts");
        }

        private void Accounts_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!init) return;
            App.Current.Settings.SaveWindowSettings(this, "Accounts");
        }

    }
}
