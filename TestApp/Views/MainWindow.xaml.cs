using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace KuCoinApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel vm;
        bool init = false;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = vm = new MainWindowViewModel();
            
            this.SizeChanged += MainWindow_SizeChanged;
            this.LocationChanged += MainWindow_LocationChanged;
            this.Loaded += MainWindow_Loaded;
            this.Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            vm.FocusCredWindow();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.Settings.ApplyWindowSettings(this);
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (!init) return;
            App.Current.Settings.SaveWindowSettings(this);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!init) return;
            App.Current.Settings.SaveWindowSettings(this);
        }

        private void ShowNewChartBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
