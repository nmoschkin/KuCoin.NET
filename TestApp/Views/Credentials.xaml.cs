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

namespace KuCoinApp
{
    /// <summary>
    /// Interaction logic for Credentials.xaml
    /// </summary>
    public partial class Credentials : Window
    {

        private CredViewModel vm;

        public Credentials()
        {
            InitializeComponent();
            this.Loaded += Credentials_Loaded;
        }

        public Credentials(CryptoCredentials cred) : this()
        {

            if (cred != null)
            {
                vm = new CredViewModel(cred);
                vm.CloseWindow += Vm_CloseWindow;

                DataContext = vm;
            }
        }

        private void Credentials_Loaded(object sender, RoutedEventArgs e)
        {
            if (vm == null)
            {
                PinWindow.GetPin(App.Current.MainWindow).ContinueWith((t) =>
                {
                    var pin = t.Result;
                    if (pin == null) return;
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        vm = new CredViewModel(t.Result);
                        vm.CloseWindow += Vm_CloseWindow;
                        DataContext = vm;
                    });
                });
            }
        }

        private void Vm_CloseWindow(object sender, CloseWindowEventArgs e)
        {
            this.DialogResult = e.Saved;
            this.Close();
        }

        private void TitleGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void TitleGrid_MouseMove(object sender, MouseEventArgs e)
        {
        }
    }
}
