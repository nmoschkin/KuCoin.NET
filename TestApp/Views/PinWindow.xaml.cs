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
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataTools.PinEntry;

namespace KuCoinApp
{
    /// <summary>
    /// Interaction logic for PinWindow.xaml
    /// </summary>
    public partial class PinWindow : Window, INotifyPropertyChanged
    {
        Settings settings = new Settings();

        /// <summary>
        /// Last window closed result.
        /// </summary>
        public static bool? LastCloseResult { get; private set; }

        public PinWindow()
        {
            
            InitializeComponent();
            this.Loaded += PinWindow_Loaded;
            Pinner.ShowHideClick += Pinner_ShowHideClick;
            Pinner.UsePasswordChar = !settings.ShowPassword;
            SaveBtn.IsEnabled = false;
        }

        private void Pinner_ShowHideClick(object sender, EventArgs e)
        {
            settings.ShowPassword = !Pinner.UsePasswordChar;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    

        private void PinWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            Pinner.Focus();

        }

        /// <summary>
        /// Opens the pin window for user pin entry.
        /// </summary>
        /// <param name="owner">The owner window.</param>
        /// <returns>The 6-digit pin in string format.</returns>
        public static async Task<string> GetPin(Window owner = null)
        {
            return await Task.Run(() =>
            {
                return App.Current?.Dispatcher?.Invoke(() =>
                {
                    PinWindow wnd = new PinWindow();
                    wnd.Owner = owner;

                    wnd.ShowDialog();

                    LastCloseResult = wnd.DialogResult;

                    if (wnd.DialogResult == true)
                    {
                        CryptoCredentials.Pin = wnd.Pinner.Pin;
                        return wnd.Pinner.Pin;
                    }
                    else
                    {
                        return null;
                    }

                });

            });

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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = true;
            }
            catch { }

            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = false;
            }
            catch { }

            this.Close();
        }

        private void Pinner_PinComplete(object sender, EventArgs e)
        {
            SaveBtn.IsEnabled = true;
        }

        private void Pinner_PinInvalidated(object sender, EventArgs e)
        {
            SaveBtn.IsEnabled = false;
        }
    }
}
