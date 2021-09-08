using KuCoin.NET.Rest;
using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Data.Common;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// A structure that represents the different supported candlestick time lengths.
    /// </summary>
    public struct KlineType : IKlineType, IEquatable<IKlineType>, IComparable<IKlineType>
    {
        /// <summary>
        /// The internal "kline-type" string value that the KuCoin API recognizes.
        /// </summary>
        private readonly string value;

        private readonly TimeSpan ts;

        public TimeSpan TimeSpan => ts;

        private static readonly FieldInfo[] klineFields = typeof(KlineType).GetFields(BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        /// Invalid or undefined KlineType value.
        /// </summary>
        public static readonly KlineType Invalid = new KlineType(null);

        /// <summary>
        /// Candle has a length of 1 minute.
        /// </summary>
        public static readonly KlineType Min1 = new KlineType("1min");

        /// <summary>
        /// Candle has a length of 3 minutes.
        /// </summary>
        public static readonly KlineType Min3 = new KlineType("3min");

        /// <summary>
        /// Candle has a length of 5 minutes.
        /// </summary>
        public static readonly KlineType Min5 = new KlineType("5min");

        /// <summary>
        /// Candle has a length of 15 minutes.
        /// </summary>
        public static readonly KlineType Min15 = new KlineType("15min");

        /// <summary>
        /// Candle has a length of 30 minutes.
        /// </summary>
        public static readonly KlineType Min30 = new KlineType("30min");

        /// <summary>
        /// Candle has a length of 1 hour.
        /// </summary>
        public static readonly KlineType Hour1 = new KlineType("1hour");

        /// <summary>
        /// Candle has a length of 2 hours.
        /// </summary>
        public static readonly KlineType Hour2 = new KlineType("2hour");

        /// <summary>
        /// Candle has a length of 4 hours.
        /// </summary>
        public static readonly KlineType Hour4 = new KlineType("4hour");

        /// <summary>
        /// Candle has a length of 6 hours.
        /// </summary>
        public static readonly KlineType Hour6 = new KlineType("6hour");

        /// <summary>
        /// Candle has a length of 8 hours.
        /// </summary>
        public static readonly KlineType Hour8 = new KlineType("8hour");

        /// <summary>
        /// Candle has a length of 12 hours.
        /// </summary>
        public static readonly KlineType Hour12 = new KlineType("12hour");

        /// <summary>
        /// Candle has a length of 1 day.
        /// </summary>
        public static readonly KlineType Day1 = new KlineType("1day");

        /// <summary>
        /// Candle has a length of 1 week.
        /// </summary>
        public static readonly KlineType Week1 = new KlineType("1week");

        /// <summary>
        /// Parse a string into a <see cref="KlineType"/>.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>A new kline type based on the string.</returns>
        /// <remarks>
        /// Only valid KuCoin kline type strings are supported.
        /// If this method is unsuccessful, a <see cref="InvalidDataException"/> will be thrown.
        /// </remarks>
        public static KlineType Parse(string s)
        {

            foreach (var f in klineFields)
            {
                if (f.GetValue(null) is KlineType ktt)
                {
                    if (ktt.value == null) continue;

                    if (ktt.value == s)
                    {
                        return ktt;
                    }
                }
            }

            throw new InvalidDataException();
        }

        /// <summary>
        /// Try to parse a string into a <see cref="KlineType"/>.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="value">The parameter that receives the parsed <see cref="KlineType"/>.</param>
        /// <returns>True if the parse was successful. The <see cref="value"/> parameter will contain the newly parsed <see cref="KlineType"/>.</returns>
        /// <remarks>
        /// Only valid KuCoin kline type strings are supported.
        /// </remarks>
        public static bool TryParse(string s, out KlineType value)
        {
            try
            {
                foreach (var f in klineFields)
                {
                    if (f.GetValue(null) is KlineType ktt)
                    {
                        if (ktt.value == null) continue;

                        if (ktt.value == s)
                        {
                            value = ktt;
                            return true;
                        }
                    }
                }

            }
            catch
            {
            }

            value = Invalid;
            return false;
        }

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
                    if (f.GetValue(null) is KlineType ktt && !string.IsNullOrEmpty(ktt.value)) l.Add(ktt);
                }

                return l.ToArray();
            }
        }

        /// <summary>
        /// Return all valid K-Line types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IKlineType> GetAllTypes() => AllTypes;


        /// <summary>
        /// Returns true if the current value is invalid.
        /// </summary>
        public bool IsInvalid
        {
            get => value == null;
        }

        /// <summary>
        /// Gets the start date for the number of requested pieces of data.
        /// </summary>
        /// <param name="pieces">The number of pieces to go back.</param>
        /// <param name="endDate">The time to work back from (now is the default)</param>
        /// <returns>The date calculated date.</returns>
        /// <remarks>
        /// The function computes the date that would need to be passed to get the specified number
        /// of pieces based on the length of the current KlineType value.
        /// </remarks>
        public DateTime GetStartDate(int pieces, DateTime? endDate = null)
        {
            var et = endDate ?? DateTime.Now;
            return et.AddSeconds(-1 * ts.TotalSeconds * pieces);
        }

        public DateTime GetCurrentKlineStartTime()
        {
            var dt = DateTime.UtcNow;
            var num = Number;

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

        public int Number
        {
            get
            {
                if (value == null) return 0;
                StringBuilder sb = new StringBuilder();
                foreach (char ch in value)
                {
                    if (char.IsDigit(ch)) sb.Append(ch);
                }

                return int.Parse(sb.ToString());
            }
        }

        /// <summary>
        /// Formatted time unit name.
        /// </summary>
        public string Unit
        {
            get
            {
                if (value == null) return null;
                StringBuilder sb = new StringBuilder();

                bool x = true;

                foreach (char ch in value)
                {
                    if (!char.IsDigit(ch))
                    {
                        sb.Append(x ? char.ToUpper(ch) : ch);
                        if (x) x = false;
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Parse and initialize the candlestick length code.
        /// </summary>
        /// <param name="type">The KuCoin API code.</param>
        private KlineType(string type)
        {
            value = type;

            if (value == null)
            {
                ts = TimeSpan.Zero;
                return;
            }

            var m = 1;

            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            char[] chars = value.ToCharArray();

            foreach(char ch in chars)
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

            var timenum = int.Parse(sb1.ToString());
            var timeunit = sb2.ToString();

            switch (timeunit)
            {
                case "hour":
                    m = 60;
                    break;

                case "min":
                    m = 1;
                    break;

                case "day":
                    m = 60 * 24;
                    break;

                case "week":
                    m = 60 * 24 * 7;
                    break;
            }

            m *= timenum;
            m *= 60;

            ts = new TimeSpan(0, 0, m);
        }

        public override bool Equals(object obj)
        {
            if (obj is IKlineType kt)
            {
                return Equals(kt);
            }
            else if (obj is string s)
            {
                return s == value;
            }
            else if (obj is int i)
            {
                return i == ts.TotalSeconds;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return value?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Returns the string representation of this object as the valid KuCoin API code.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return value;
        }

        /// <summary>
        /// Returns the string representation of this object as either the valid KuCoin API code, or a formatted string.
        /// </summary>
        /// <param name="formatted">True to return a formatted string.</param>
        /// <returns>If formatted == true, a formatted string, otherwise the valid KuCoin API string code.</returns>
        public string ToString(bool formatted)
        {
            if (formatted)
            {
                var n = Number;
                var u = Unit;

                if (n > 1) u += "s";
                return $"{n} {u}";
            }
            else
            {
                return ToString();
            }
        }

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

        public bool Equals(IKlineType other)
        {
            return TimeSpan.Equals(other.TimeSpan);
        }

        public int CompareTo(IKlineType other)
        {
            return TimeSpan.CompareTo(other.TimeSpan);
        }

        public static bool operator ==(KlineType val1, KlineType val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(KlineType val1, KlineType val2)
        {
            return !val1.Equals(val2);
        }

        public static bool operator >(KlineType val1, KlineType val2)
        {
            return val1.CompareTo(val2) > 0;
        }
        public static bool operator <(KlineType val1, KlineType val2)
        {
            return val1.CompareTo(val2) < 0;
        }

        public static bool operator >=(KlineType val1, KlineType val2)
        {
            return val1.CompareTo(val2) >= 0;
        }
        public static bool operator <=(KlineType val1, KlineType val2)
        {
            return val1.CompareTo(val2) <= 0;
        }

        public static implicit operator TimeSpan(KlineType val) => val.TimeSpan;

        public static explicit operator KlineType(TimeSpan val)
        {
            foreach (KlineType kt in AllTypes)
            {
                if (kt.TimeSpan == val) return kt;
            }
            return Invalid;
        }

        public static explicit operator string(KlineType val)
        {
            return val.value;
        }

        public static explicit operator KlineType(string val)
        {
            return new KlineType(val);
        }
    }
}
