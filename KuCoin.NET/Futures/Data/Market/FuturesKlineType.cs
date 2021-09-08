using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KuCoin.NET.Futures.Data.Market
{
    /// <summary>
    /// Futures K-Line Type
    /// </summary>
    public struct FuturesKlineType : IKlineType
    {
        /// <summary>
        /// The length of the K Line, in minutes.
        /// </summary>
        private readonly TimeSpan ts;

        public TimeSpan TimeSpan => ts;


        public static readonly int[] ValidValues = new int[] { 0, 1, 5, 15, 30, 60, 120, 240, 480, 720, 1440, 10080 };

        private FuturesKlineType(int value)
        {
            if (!ValidValues.Contains(value)) throw new ArgumentException("Invalid Futures K-Line Length");

            ts = new TimeSpan(0, value, 0);
        }

        /// <summary>
        /// Invalid K-Line type
        /// </summary>
        public static FuturesKlineType Invalid = new FuturesKlineType(0);

        /// <summary>
        /// 1 minute K-line
        /// </summary>
        public static FuturesKlineType Min1 = new FuturesKlineType(1);

        /// <summary>
        /// 5 minute K-line
        /// </summary>
        public static FuturesKlineType Min5 = new FuturesKlineType(5);

        /// <summary>
        /// 15 minute K-line
        /// </summary>
        public static FuturesKlineType Min15 = new FuturesKlineType(15);

        /// <summary>
        /// 30 minute K-line
        /// </summary>
        public static FuturesKlineType Min30 = new FuturesKlineType(30);

        /// <summary>
        /// 1 hour K-line
        /// </summary>
        public static FuturesKlineType Hour1 = new FuturesKlineType(60);

        /// <summary>
        /// 2 hour K-line
        /// </summary>
        public static FuturesKlineType Hour2 = new FuturesKlineType(120);

        /// <summary>
        /// 4 hour K-line
        /// </summary>
        public static FuturesKlineType Hour4 = new FuturesKlineType(240);

        /// <summary>
        /// 8 hour K-line
        /// </summary>
        public static FuturesKlineType Hour8 = new FuturesKlineType(480);

        /// <summary>
        /// 12 hour K-line
        /// </summary>
        public static FuturesKlineType Hour12 = new FuturesKlineType(720);

        /// <summary>
        /// 1 day K-line
        /// </summary>
        public static FuturesKlineType Day1 = new FuturesKlineType(1440);

        /// <summary>
        /// 1 week K-line
        /// </summary>
        public static FuturesKlineType Week1 = new FuturesKlineType(10080);

        private static readonly FieldInfo[] klineFields = typeof(FuturesKlineType).GetFields(BindingFlags.Public | BindingFlags.Static);

        public bool IsInvalid => ts == TimeSpan.Zero;

        /// <summary>
        /// Gets a list of all K-Line types.
        /// </summary>
        public static IEnumerable<IKlineType> AllTypes
        {
            get
            {
                var l = new List<IKlineType>();

                foreach (var f in klineFields)
                {
                    if (f.GetValue(null) is FuturesKlineType ktt && ktt.ts != TimeSpan.Zero) l.Add(ktt);
                }

                return l.ToArray();
            }
        }

        /// <summary>
        /// Return all valid K-Line types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IKlineType> GetAllTypes() => AllTypes;

        public override string ToString()
        {
            return ts.ToString();
        }

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <param name="format">The format to use (or null)</param>
        /// <returns></returns>
        public string ToString(string format)
        {
            if (format == "g") return ToString();
            if (format == "G") return ToString(true);
            else return ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString(format);
        }

        public string Unit
        {
            get
            {
                var ss = ToString(true).Split(' ');
                return ss[1];
            }
        }

        public int Number
        {
            get
            {
                var ss = ToString(true).Split(' ');
                return int.Parse(ss[0]);
            }
        }


        public DateTime GetCurrentKlineStartTime()
        {
            var num = Number;

            var dt = DateTime.UtcNow;
            dt = dt.AddSeconds(-dt.Second);

            if (Unit == "Min")
            {
                dt = dt.AddMinutes(-(dt.Minute % num));
            }
            else if (Unit == "Hour")
            {
                dt = dt.AddMinutes(-dt.Minute);
                dt = dt.AddHours(-(dt.Hour % num));
            }
            else if (Unit == "Day")
            {

                dt = dt.AddMinutes(-dt.Minute);
                dt = dt.AddHours(-dt.Hour);
                dt = dt.AddDays(-(dt.Day % num));
            }
            else if (Unit == "Week")
            {
                dt = dt.AddMinutes(-dt.Minute);
                dt = dt.AddHours(-dt.Hour);
                dt = dt.AddDays(-((int)dt.DayOfWeek % (num * 7)));
            }
            else
            {
                throw new NotImplementedException();
            }

            return dt;
        }

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <param name="formatted">True to return a formatted time.</param>
        /// <returns></returns>
        public string ToString(bool formatted)
        {
            if (!formatted)
            {
                return ts.ToString();
            }
            else 
            {
                StringBuilder sb1, sb2;

                sb1 = new StringBuilder();
                sb2 = new StringBuilder();

                string n = null;

                foreach (var f in klineFields)
                {
                    if (f.GetValue(null) is FuturesKlineType fc)
                    {
                        if (fc.ts == ts)
                        {
                            n = f.Name;
                            break;
                        }
                    }
                }

                if (n != null)
                {
                    foreach (char ch in n)
                    {
                        if (char.IsDigit(ch))
                        {
                            sb1.Append(ch);
                        }
                        else
                        {
                            sb2.Append(ch);
                        }
                    }

                    return $"{sb1} {sb2}";
                }
                else
                {
                    return base.ToString();
                }

            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj is FuturesKlineType fc)
            {
                return fc.ts == ts;
            }
            else if (obj is int i)
            {
                return i == ts.TotalMinutes;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ts.GetHashCode();
        }

        public DateTime GetStartDate(int pieces, DateTime? endDate = null)
        {
            var et = endDate ?? DateTime.Now;
            return et.AddMinutes(-1 * ts.TotalMinutes * pieces);
        }

        public static implicit operator int(FuturesKlineType val)
        {
            return (int)val.ts.TotalMinutes;
        }

        public static explicit operator FuturesKlineType(int val)
        {
            return new FuturesKlineType(val);
        }

        public static bool operator ==(int val1, FuturesKlineType val2)
        {
            return val1 == val2.ts.TotalMinutes;
        }
        public static bool operator !=(int val1, FuturesKlineType val2)
        {
            return val1 != val2.ts.TotalMinutes;
        }

        public static bool operator ==(FuturesKlineType val1, int val2)
        {
            return val2 == val1.ts.TotalMinutes;
        }
        public static bool operator !=(FuturesKlineType val1, int val2)
        {
            return val2 != val1.ts.TotalMinutes;
        }

        public static bool operator ==(FuturesKlineType val1, FuturesKlineType val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(FuturesKlineType val1, FuturesKlineType val2)
        {
            return !val1.Equals(val2);
        }

    }

}
