using Kucoin.NET.Data.Market;

using KuCoinApp.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace KuCoinApp.Views
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

            var stream = new MemoryStream(CoinResources.kucoin);
            Icon = BitmapFrame.Create(stream);

            DataContext = this.vm = vm;

            MakeProps("SelectedSymbol", "Data", typeof(TradingSymbol), SymbolInfo, "Symbol", "BaseCurrency.Image", "QuoteCurrency.Image");

            SymbolTip.PlacementTarget = SymbolCombo;

            this.vm.AskQuit += Vm_AskQuit;

            App.Current.Spot = this;

            this.SizeChanged += MainWindow_SizeChanged;
            this.LocationChanged += MainWindow_LocationChanged;
            this.Loaded += MainWindow_Loaded;
            this.Activated += MainWindow_Activated;
            this.Closing += MainWindow_Closing; ;
            
            
            App.Current.Settings.ApplyWindowSettings(this);

            init = true;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm?.Dispose();

            App.Current.Spot = null;
            vm = null;
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
            if (WindowState != WindowState.Normal) return;
            App.Current.Settings.SaveWindowSettings(this);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!init) return;
            if (WindowState != WindowState.Normal) return;
            App.Current.Settings.SaveWindowSettings(this);
        }

        private void ShowNewChartBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        public Grid MakeProps(string binding, string sub, Type type, Panel ctrl, string titleProperty = null, string baseCurrImg = null, string quoteCurrImg = null)
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
            string b;

            foreach (var pi in pis)
            {
                vn = type.Name + "." + pi.Name;
                txt = res.GetString(vn);

                if (txt != null)
                {
                    b = binding;
                    if (sub != null) b += "." + sub;

                    proptxt.Add(b + "." + pi.Name, txt);
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
                Image img;
                int ic = 0;
                var g2 = new Grid();

                if (baseCurrImg != null)
                {

                    g2.ColumnDefinitions.Add(new ColumnDefinition()
                    {   
                        Width = GridLength.Auto
                    });

                    img = new Image()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = 24,
                        Width = 24,
                        Margin = new Thickness(4)
                    };

                    img.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
                    img.SetBinding(Image.SourceProperty, new Binding(binding + "." + baseCurrImg));
                    img.SetValue(Grid.ColumnProperty, ic++);

                    g2.Children.Add(img);
                }

                if (quoteCurrImg != null)
                {
                    g2.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = GridLength.Auto
                    });

                    img = new Image()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = 24,
                        Width = 24,
                        Margin = new Thickness(4)
                    };

                    img.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
                    img.SetBinding(Image.SourceProperty, new Binding(binding + "." + quoteCurrImg));
                    img.SetValue(Grid.ColumnProperty, ic++);

                    g2.Children.Add(img);
                }

                tp = new Label()
                {
                    Foreground = Brushes.Black,
                    FontFamily = new FontFamily("Calibri"),
                    FontSize = 16,
                    Margin = new Thickness(4),
                    FontWeight = FontWeights.Bold
                    
                };

                g2.ColumnDefinitions.Add(new ColumnDefinition());

                b = binding;
                if (sub != null) b += "." + sub;

                tp.SetBinding(Label.ContentProperty, new Binding(b + "." + titleProperty));

                tp.SetValue(Grid.RowProperty, 0);
                tp.SetValue(Grid.ColumnProperty, ic);

                g2.Children.Add(tp);

                g.RowDefinitions.Add(new RowDefinition());
                g.Children.Add(g2);

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
