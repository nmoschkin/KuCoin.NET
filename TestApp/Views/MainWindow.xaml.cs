using Kucoin.NET.Data.Market;

using KuCoinApp.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
        WindowViewModelBase vm;
        bool init = false;


        public MainWindow(WindowViewModelBase vm)
        {
            InitializeComponent();
            
            DataContext = this.vm = vm;

            MakeProps("Symbol", typeof(TradingSymbol), SymbolInfo, "Symbol");

            SymbolTip.PlacementTarget = SymbolCombo;

            this.vm.AskQuit += Vm_AskQuit;

            this.SizeChanged += MainWindow_SizeChanged;
            this.LocationChanged += MainWindow_LocationChanged;
            this.Loaded += MainWindow_Loaded;
            this.Activated += MainWindow_Activated;
            
            App.Current.Settings.ApplyWindowSettings(this);

            init = true;
        }

        public MainWindow() : this(new MainWindowViewModel())
        {

        }

        private void Vm_AskQuit(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (vm is MainWindowViewModel mb)
                mb.FocusCredWindow();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
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

        public Grid MakeProps(string binding, Type type, Panel ctrl, string titleProperty = null)
        {
            var res = Kucoin.NET.Localization.AppResources.ResourceManager;

            var pis = new List<PropertyInfo>(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            var pindx = new List<Type>(new Type[] { typeof(string), typeof(decimal), typeof(bool) });

            pis.Sort((a, b) =>
            {
                if (a.PropertyType == b.PropertyType)
                {
                    return string.Compare(a.Name, b.Name);
                }
                else
                {
                    int i = pindx.IndexOf(a.PropertyType);
                    int j = pindx.IndexOf(b.PropertyType);

                    if (i == -1 && j == -1) return string.Compare(a.PropertyType.Name, b.PropertyType.Name);
                    else if (i == -1) return 1;
                    else if (j == -1) return -1;
                    else if (i < j) return -1;
                    else if (i > j) return 1;
                    else return 0;
                }
            });

            Dictionary<string, string> proptxt = new Dictionary<string, string>();

            string vn;
            string txt;

            foreach (var pi in pis)
            {
                vn = type.Name + "." + pi.Name;
                txt = res.GetString(vn);

                if (txt != null)
                {
                    proptxt.Add(binding + "." + pi.Name, txt);
                }
            }

            var g = new Grid();


            int row = 0;
            int col = 0;

            int maxrows = 8;
            Label tp = null;

            g.ColumnDefinitions.Add(new ColumnDefinition());
            
            if (titleProperty != null)
            {
                tp = new Label()
                {
                    Foreground = Brushes.Black,
                    FontFamily = new FontFamily("Calibri"),
                    FontSize = 16,
                    Margin = new Thickness(4),
                    FontWeight = FontWeights.Bold
                    
                };

                tp.SetBinding(Label.ContentProperty, new Binding(binding + "." + titleProperty));
                tp.SetValue(Grid.RowProperty, 0);
                tp.SetValue(Grid.ColumnProperty, 0);

                g.RowDefinitions.Add(new RowDefinition());
                g.Children.Add(tp);

                row++;

            }

            
            foreach (var kv in proptxt)
            {
                g.RowDefinitions.Add(new RowDefinition());

                var sp = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0)
                };

                sp.SetValue(Grid.RowProperty, row);
                sp.SetValue(Grid.ColumnProperty, col);

                var lbl1 = new Label()
                {
                    Content = kv.Value + ":",
                    Foreground = Brushes.Blue,
                    FontFamily = new FontFamily("Calibri"),
                    FontSize = 12,
                    Margin = new Thickness(4, 2, 4, 2),
                    FontWeight = FontWeights.Bold
                };

                var lbl2 = new Label()
                {
                    Foreground = Brushes.Black,
                    FontFamily = new FontFamily("Calibri"),
                    FontSize = 12,
                    Margin = new Thickness(4, 2, 4, 2),
                    FontWeight = FontWeights.Bold
                };

                lbl2.SetBinding(Label.ContentProperty, new Binding(kv.Key));

                sp.Children.Add(lbl1);
                sp.Children.Add(lbl2);

                g.Children.Add(sp);
                row++;

                if (row >= maxrows)
                {
                    row = tp == null ? 0 : 1;
                    col++;
                    g.ColumnDefinitions.Add(new ColumnDefinition());
                }
            }

            if (tp != null)
            {
                if (row != 0) col++;
                tp.SetValue(Grid.ColumnSpanProperty, col);
            }

            ctrl.Children.Add(g);
            return g;
        }

        private void SymbolInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            SymbolTip.IsOpen = !SymbolTip.IsOpen;
        }
    }

}
