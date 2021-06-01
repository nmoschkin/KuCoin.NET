using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KuCoinApp.Converters
{
    public class FriendlySpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value is long i)
            {
                return PrintFriendlySpeed((ulong)i);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prints a number value as a friendly byte speed in TiB, GiB, MiB, KiB or B.
        /// </summary>
        /// <param name="speed">The speed to format.</param>
        /// <param name="format">Optional numeric format for the resulting value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string PrintFriendlySpeed(ulong speed, string format = null, bool binary = false)
        {
            double fs;
            double spd = speed;
            string nom;

            if (binary)
            {
                if ((spd >= (1024L * 1024 * 1024 * 1024 * 1024 * 1024)))
                {
                    fs = (spd / (1024L * 1024 * 1024 * 1024 * 1024 * 1024));
                    nom = "Eb/s";
                    //wow
                }
                else if ((spd >= (1024L * 1024 * 1024 * 1024 * 1024)))
                {
                    fs = (spd / (1024L * 1024 * 1024 * 1024 * 1024));
                    nom = "Pb/s";
                    //wow
                }
                else if ((spd >= (1024L * 1024 * 1024 * 1024)))
                {
                    fs = (spd / (1024L * 1024 * 1024 * 1024));
                    nom = "Tb/s";
                    //wow
                }
                else if ((spd >= (1024 * 1024 * 1024)))
                {
                    fs = (spd / (1024 * 1024 * 1024));
                    nom = "Gb/s";
                    // still wow
                }
                else if ((spd >= (1024 * 1024)))
                {
                    fs = (spd / (1024 * 1024));
                    nom = "Mb/s";
                    // okay
                }
                else if ((spd >= (1024)))
                {
                    fs = (spd / (1024));
                    nom = "Kb/s";
                    // fine.
                }
                else
                {
                    fs = spd;
                    nom = "b/s";
                    // wow.
                }

            }
            else
            {
                if ((spd >= (1000L * 1000 * 1000 * 1000 * 1000 * 1000)))
                {
                    fs = (spd / (1000L * 1000 * 1000 * 1000 * 1000 * 1000));
                    nom = "Eb/s";
                    //wow
                }
                else if ((spd >= (1000L * 1000 * 1000 * 1000 * 1000)))
                {
                    fs = (spd / (1000L * 1000 * 1000 * 1000 * 1000));
                    nom = "Pb/s";
                    //wow
                }
                else if ((spd >= (1000L * 1000 * 1000 * 1000)))
                {
                    fs = (spd / (1000L * 1000 * 1000 * 1000));
                    nom = "Tb/s";
                    //wow
                }
                else if ((spd >= (1000 * 1000 * 1000)))
                {
                    fs = (spd / (1000 * 1000 * 1000));
                    nom = "Gb/s";
                    // still wow
                }
                else if ((spd >= (1000 * 1000)))
                {
                    fs = (spd / (1000 * 1000));
                    nom = "Mb/s";
                    // okay
                }
                else if ((spd >= (1000)))
                {
                    fs = (spd / (1000));
                    nom = "Kb/s";
                    // fine.
                }
                else
                {
                    fs = spd;
                    nom = "b/s";
                    // wow.
                }

            }

            if (format != null)
            {
                return System.Math.Round(fs, 2).ToString(format) + " " + nom;
            }
            else
            {
                return System.Math.Round(fs, 2) + " " + nom;
            }

        }
    }
}
