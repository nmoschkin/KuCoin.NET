using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace KuCoinApp.Converters
{

    public enum BoolConverterModes
    {
        Detect,
        InverseDetect,
        InverseBool,
        Number,
        InverseNumber,
        Visibility,
        InverseVisibility
    }

    public class BoolConverter : IValueConverter
    {
        public BoolConverterModes Mode { get; set; } = BoolConverterModes.InverseBool;
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            switch (Mode)
            {
                case BoolConverterModes.InverseBool:
                    if (value is bool b) return !b;
                    else throw new InvalidCastException();

                case BoolConverterModes.Detect:
                    return value != null;

                case BoolConverterModes.InverseDetect:
                    return value == null;

                case BoolConverterModes.Number:
                    return NumDetect(value);

                case BoolConverterModes.InverseNumber:
                    return !NumDetect(value);

                case BoolConverterModes.Visibility:

                    if (value is bool b2)
                    {
                        if (b2 == true)
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                    }
                    break;

                case BoolConverterModes.InverseVisibility:

                    if (value is bool b3)
                    {
                        if (b3 == false)
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                    }
                    break;

                default:
                    return false;

            }

            return false;

        }

        private bool NumDetect(object value)
        {

            if (value is decimal de)
            {
                return de != 0;
            }
            else if (value is double db)
            {
                return db != 0;
            }
            else if (value is float f)
            {
                return f != 0;
            }
            else if (value is ulong ul)
            {
                return ul != 0;
            }
            else if (value is long l)
            {
                return l != 0;
            }
            else if (value is uint ui)
            {
                return ui != 0;
            }
            else if (value is int i)
            {
                return i != 0;
            }
            else if (value is ushort us)
            {
                return us != 0;
            }
            else if (value is short s)
            {
                return s != 0;
            }
            else if (value is byte b)
            {
                return b != 0;
            }
            else if (value is sbyte sb)
            {
                return sb != 0;
            }
            else if (value is char ch)
            {
                return ch != 0;
            }
            else if (value is BigInteger bigI)
            {
                return !bigI.Equals(BigInteger.Zero);
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
