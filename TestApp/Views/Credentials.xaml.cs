using System;
using System.Collections.Generic;
using System.IO;
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

        internal CredentialsViewModel vm;

        public Credentials()
        {
            InitializeComponent();
            var stream = new MemoryStream(CoinResources.kucoin);
            Icon = BitmapFrame.Create(stream);

            this.Loaded += Credentials_Loaded;
        }

        public Credentials(CryptoCredentials cred) : this()
        {
            if (cred != null)
            {
                vm = new CredentialsViewModel(cred);
                vm.CloseWindow += Vm_CloseWindow;

                DataContext = vm;
            }
        }

        private void Credentials_Loaded(object sender, RoutedEventArgs e)
        {
            if (vm == null)
            {
                if (CryptoCredentials.Pin != null)
                {
                    vm = new CredentialsViewModel(CryptoCredentials.Pin);
                    vm.CloseWindow += Vm_CloseWindow;
                    vm.PropertyChanged += Vm_PropertyChanged;
                    DataContext = vm;

                    InitBoxes();
                    return;
                }

                PinWindow.GetPin(App.Current.MainWindow).ContinueWith((t) =>
                {
                    var pin = t.Result;
                    if (pin == null) return;
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        vm = new CredentialsViewModel(t.Result);
                        vm.CloseWindow += Vm_CloseWindow;
                        vm.PropertyChanged += Vm_PropertyChanged;
                        DataContext = vm;

                        InitBoxes();
                    });
                });
            }
            else
            {
                vm.PropertyChanged += Vm_PropertyChanged;
                InitBoxes();
            }
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CredentialsViewModel.ShowPassword))
            {
                InitBoxes();
            }
        }

        private void InitBoxes()
        {
            if (vm.ShowPassword)
            {
                ApiKeyBox.Text = vm.Credential.Key;
                ApiSecretBox.Text = vm.Credential.Secret;
                ApiPassphraseBox.Text = vm.Credential.Passphrase;

                ApiKeyPwd.Password = "";
                ApiSecretPwd.Password = "";
                ApiPassphrasePwd.Password = "";
            }
            else
            {
                ApiKeyPwd.Password = vm.Credential.Key;
                ApiSecretPwd.Password = vm.Credential.Secret;
                ApiPassphrasePwd.Password = vm.Credential.Passphrase;

                ApiKeyBox.Text = "";
                ApiSecretBox.Text = "";
                ApiPassphraseBox.Text = "";

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

        private void ApiKeyPwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (vm.ShowPassword) return;

            if (ApiKeyPwd.Password != vm.Credential.Key)
            {
                vm.Credential.Key = ApiKeyPwd.Password;
            }

        }

        private void ApiSecretPwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (vm.ShowPassword) return;

            if (ApiSecretPwd.Password != vm.Credential.Secret)
            {
                vm.Credential.Secret = ApiSecretPwd.Password;
            }

        }

        private void ApiPassphrasePwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (vm.ShowPassword) return;

            if (ApiPassphrasePwd.Password != vm.Credential.Passphrase)
            {
                vm.Credential.Passphrase = ApiPassphrasePwd.Password;
            }

        }

        private void ApiKeyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!vm.ShowPassword) return;

            if (ApiKeyBox.Text != vm.Credential.Key)
            {
                vm.Credential.Key = ApiKeyBox.Text;
            }

        }

        private void ApiSecretBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!vm.ShowPassword) return;

            if (ApiSecretBox.Text != vm.Credential.Secret)
            {
                vm.Credential.Secret = ApiSecretBox.Text;
            }
        }

        private void ApiPassphraseBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!vm.ShowPassword) return;

            if (ApiPassphraseBox.Text != vm.Credential.Passphrase)
            {
                vm.Credential.Passphrase = ApiPassphraseBox.Text;
            }
        }
    }
}
