using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using KuCoinApp.Localization.Resources;

namespace KuCoinApp.Converters
{
    public class TextVariablesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string s)
            {
                s = AppResources.ResourceManager.GetString(s);

                var lst = new List<object>();
                var c = values.Length;

                for (int i = 1; i < c; i++)
                {
                    lst.Add(values[i]);
                }

                return string.Format(s, lst.ToArray());
            }
            else
            {
                throw new NotImplementedException();
            }

            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
