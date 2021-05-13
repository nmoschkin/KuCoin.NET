using Kucoin.NET.Data.Interfaces;

using KuCoinApp.ViewModels;

using System;
using System.Collections.Generic;
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

        public Accounts()
        {
            InitializeComponent();

            vm = new AccountsWindowViewModel();
            DataContext = vm;
        }

        public Accounts(ICredentialsProvider credProvider)
        {
            InitializeComponent();

            vm = new AccountsWindowViewModel(credProvider);
            DataContext = vm;

        }

    }
}
