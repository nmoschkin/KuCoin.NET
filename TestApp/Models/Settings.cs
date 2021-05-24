using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Kucoin.NET.Observable;

using Microsoft.Win32;


namespace KuCoinApp
{

    public class WindowSettings
    {
        public double Left { get; set; }

        public double Top { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public WindowState State { get; set; }

        public WindowStartupLocation Location { get; set; }

        public WindowSettings()
        {

        }

        public WindowSettings(Window window)
        {
            Left = window.Left;
            Top = window.Top;
            Width = window.Width;
            Height = window.Height;
            State = window.WindowState;
            Location = window.WindowStartupLocation;
        }

        public void Apply(Window window)
        {
            window.Left = Left;
            window.Top = Top;
            window.Width = Width;
            window.Height = Height;
            window.WindowState = State;
            window.WindowStartupLocation = Location;
        }

        public static WindowSettings Parse(string value, Window window = null)
        {
            try
            {
                string[] s = value.Split(',');
                
                var nobj = new WindowSettings();

                if (s.Length >= 1)
                {
                    nobj.Left = double.Parse(s[0].Trim());
                }
                if (s.Length >= 2)
                {
                    nobj.Top = double.Parse(s[1].Trim());
                }
                if (s.Length >= 3)
                {
                    nobj.Width = double.Parse(s[2].Trim());
                }
                if (s.Length >= 4)
                {
                    nobj.Height = double.Parse(s[3].Trim());
                }
                if (s.Length >= 5)
                {
                    try
                    {
                        nobj.State = (WindowState)(Enum.Parse(typeof(WindowState), s[4].Trim()));
                    }
                    catch { }
                }
                if (s.Length >= 6)
                {
                    try
                    {
                        nobj.Location = (WindowStartupLocation)(Enum.Parse(typeof(WindowStartupLocation), s[5].Trim()));
                    }
                    catch { }
                }

                return nobj;

            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
        {
            return $"{Left}, {Top}, {Width}, {Height}, {State}, {Location}";
        }

    }
    public class Settings : INotifyPropertyChanged
    {
        protected string subKey = "SOFTWARE\\Nathaniel Moschkin\\KuCoin.NET";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Settings(string subKey = null)
        {
            if (subKey != null)
            {
                this.subKey = subKey;
            }
        }

        public string SubKey => subKey;

        public bool ShowPassword
        {
            get
            {
                return GetProperty<bool>(false);
            }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }

        public string QuoteCurrency
        {
            get
            {
                return GetProperty<string>("USDT");
            }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }

        public string Hvcyp
        {
            get
            {
                return CryptoCredentials.GetHvcyp().ToString("d");
            }
            set
            {
                var g = Guid.Parse(value);
                CryptoCredentials.GetHvcyp(g);
            }
        }

        public string HvcypOld
        {
            get
            {
                return GetProperty<string>(Guid.NewGuid().ToString("d"), "Hvcyp");
            }
            set
            {
                SetProperty(value, "Hvcyp");
                OnPropertyChanged();
            }
        }

        public int SymbolLimit
        {
            get
            {
                return GetProperty<int>(15);
            }
            set
            {
                if (value < 1) value = 1;
                else if (value > 255) value = 255;

                SetProperty(value);
                OnPropertyChanged();
            }
        }

        public string[] Symbols 
        {
            get
            {
                return GetProperty<string[]>();
            }
            set
            {
                SetProperty(value);
                OnPropertyChanged();
            }
        }

        public void PushSymbol(string symbol)
        {
            string[] syms = Symbols;

            
            if (syms == null)
            {
                syms = new string[] { symbol };
                Symbols = syms;
                
                return;
            }
            else
            {
                int sl = SymbolLimit;
                var snew = new List<string>(syms);

                snew.Remove(symbol);

                while (snew.Count >= sl)
                {
                    snew.RemoveAt(0);
                }
                snew.Add(symbol);

                Symbols = snew.ToArray();
            }

            OnPropertyChanged(nameof(Symbols));
        }

        public string MostRecentSymbol
        {
            get
            {
                var s = Symbols;
                if (s != null && s.Length > 0)
                    return s[s.Length - 1];
                else
                    return null;
            }
        }

        public void ApplyWindowSettings(Window window, string windowName = null)
        {
            if (windowName == null)
            {
                if (string.IsNullOrEmpty(window.Title))
                {
                    windowName = window.Name;
                }
                else
                {
                    windowName = window.Title;
                }

                if (string.IsNullOrEmpty(windowName))
                {
                    windowName = "Window";
                }

                windowName = windowName.Replace(" ", "");
            }

            var ws = new WindowSettings(window);

            ws = GetProperty<WindowSettings>(ws, windowName + "Geometry");
            ws.Apply(window);
        }

        public void SaveWindowSettings(Window window, string windowName = null)
        {
            if (windowName == null)
            {
                if (string.IsNullOrEmpty(window.Title))
                {
                    windowName = window.Name;
                }
                else
                {
                    windowName = window.Title;
                }

                if (string.IsNullOrEmpty(windowName))
                {
                    windowName = "Window";
                }

                windowName = windowName.Replace(" ", "");
            }

            var ws = new WindowSettings(window);
            SetProperty<WindowSettings>(ws, windowName + "Geometry");
        }


        protected virtual T GetProperty<T>(object defaultValue = null, [CallerMemberName] string propertyName = null)
        {

            MethodInfo f = null;

            if (defaultValue != null && !defaultValue.GetType().IsAssignableFrom(typeof(T)))
            {
                throw new InvalidCastException($"{nameof(defaultValue)} must be parse-able, or assignable to T.");
            }

            var parsables = new List<Type>(new Type[] { typeof(Guid), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(bool) });

            if (!typeof(T).IsAssignableFrom(typeof(string)) && !typeof(T).IsValueType)
            {
                try
                {
                    f = typeof(T).GetMethod("Parse");
                }
                catch
                {
                    try
                    {
                        f = typeof(T).GetMethod("Parse", new Type[] { typeof(string) });
                    }
                    catch
                    {

                    }
                }

                if (f != null && (!f.IsStatic || !f.IsPublic))
                {
                    f = null;
                }
            }
            else if (parsables.Contains(typeof(T)))
            {
                f = typeof(T).GetMethod("Parse", new Type[] { typeof(string) });

                if (f != null && (!f.IsStatic || !f.IsPublic))
                {
                    f = null;
                }
            }

            using (var rkey = Registry.CurrentUser.CreateSubKey(SubKey, true))
            {
                object ret = rkey.GetValue(propertyName);

                if (ret == null)
                {
                    if (defaultValue != null)
                    {
                        SetProperty((T)defaultValue, propertyName);
                        return (T)defaultValue;
                    }
                    else
                    {
                        return default;
                    }
                }

                if (f != null && ret is string)
                {
                    List<object> lparam = new List<object>();
                    lparam.Add(ret);

                    var p = f.GetParameters();
                    if (p.Length > 1)
                    {
                        int c = p.Length;
                        for (int i = 1; i < c; i++)
                        {
                            lparam.Add(null);
                        }
                    }

                    ret = f.Invoke(null, lparam.ToArray());
                }

                return (T)ret;
            }
        }

        public void DeleteRegistryProperty(string propertyName)
        {
            using (var rkey = Registry.CurrentUser.CreateSubKey(SubKey, true))
            {
                var l = new List<string>(rkey.GetValueNames());

                if (l.Contains(propertyName))
                {
                    rkey.DeleteValue(propertyName);
                }
            }
        }

        public bool RegistryPropertyExists(string propertyName)
        {
            using (var rkey = Registry.CurrentUser.CreateSubKey(SubKey, true))
            {
                var l = new List<string>(rkey.GetValueNames());

                if (l.Contains(propertyName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        protected virtual bool SetProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            using (var rkey = Registry.CurrentUser.CreateSubKey(SubKey, true))
            {
                RegistryValueKind vk;

                if (typeof(T) == typeof(string))
                {
                    if (value.ToString().Contains("%"))
                    {
                        vk = RegistryValueKind.ExpandString;
                    }
                    else
                    {
                        vk = RegistryValueKind.String;
                    }

                    rkey.SetValue(propertyName, value, vk);

                }
                else if (typeof(T) == typeof(string[]))
                {
                    vk = RegistryValueKind.MultiString;
                    rkey.SetValue(propertyName, value, vk);

                }
                else if (typeof(int).IsAssignableFrom(typeof(T)))
                {
                    vk = RegistryValueKind.DWord;
                    rkey.SetValue(propertyName, value, vk);
                }
                else if (typeof(T) == typeof(bool))
                {
                    vk = RegistryValueKind.String;
                    rkey.SetValue(propertyName, value.ToString(), vk);
                }
                else if (typeof(T) == typeof(byte[]))
                {
                    vk = RegistryValueKind.Binary;
                    rkey.SetValue(propertyName, value, vk);

                }
                else
                {
                    MethodInfo f;
                    f = typeof(T).GetMethod("Parse");

                    if (f != null && f.IsStatic && f.IsPublic)
                    {
                        vk = RegistryValueKind.String;
                        rkey.SetValue(propertyName, value.ToString(), vk);

                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }


    }
}
