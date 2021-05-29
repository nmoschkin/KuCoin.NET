using Kucoin.NET.Observable;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KuCoinApp.Converters
{

    public class ElementFormatConverter : ObservableBase, IMultiValueConverter
    {
        private string defFmt = "#0.0########";


        /// <summary>
        /// Default format
        /// </summary>
        public string DefaultFormat
        {
            get => defFmt;
            set
            {
                SetProperty(ref defFmt, value);
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length == 1 && values[0] is decimal df)
            {
                return df.ToString(defFmt);
            }            
            else if (values?.Length >= 2)
            {
                if (values[0] is decimal de)
                {
                    if (values[1] is string f)
                    {
                        return de.ToString(f);
                    }
                    else
                    {
                        return de.ToString(defFmt);
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (value is string ds)
            {
                char[] chars = ds.ToCharArray();
                StringBuilder sb = new StringBuilder();
                CultureInfo ci = CultureInfo.CurrentCulture;

                var dec = ci.NumberFormat.NumberDecimalSeparator;

                foreach (char ch in chars)
                {

                    if (char.IsNumber(ch) || (ch == dec[0]))
                    {
                        sb.Append(ch);
                    }

                }

                return new object[] { decimal.Parse(sb.ToString()), defFmt };
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
