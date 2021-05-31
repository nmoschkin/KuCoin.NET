using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kucoin.NET.Futures.Data.Market
{
    /// <summary>
    /// Futures K-Line Type
    /// </summary>
    public struct FuturesKlineType : IKlineType
    {
        /// <summary>
        /// The length of the K Line, in minutes.
        /// </summary>
        private readonly int length;

        public int Length => length;

        public KlineLengthType LengthType => KlineLengthType.Minutes;

        public static readonly int[] ValidValues = new int[] { 0, 1, 5, 15, 30, 60, 120, 240, 480, 720, 1440, 10080 };

        private FuturesKlineType(int value)
        {
            if (!ValidValues.Contains(value)) throw new ArgumentException("Invalid Futures K-Line Length");
            length = value;
        }

        public static FuturesKlineType Invalid = new FuturesKlineType(0);

        public static FuturesKlineType Min1 = new FuturesKlineType(1);

        public static FuturesKlineType Min5 = new FuturesKlineType(5);
        
        public static FuturesKlineType Min15 = new FuturesKlineType(15);
        
        public static FuturesKlineType Min30 = new FuturesKlineType(30);
        
        public static FuturesKlineType Hour1 = new FuturesKlineType(60);
        
        public static FuturesKlineType Hour2 = new FuturesKlineType(120);
        
        public static FuturesKlineType Hour4 = new FuturesKlineType(240);
        
        public static FuturesKlineType Hour8 = new FuturesKlineType(480);
        
        public static FuturesKlineType Hour12 = new FuturesKlineType(720);
        
        public static FuturesKlineType Day1 = new FuturesKlineType(1440);
        
        public static FuturesKlineType Week1 = new FuturesKlineType(10080);

        private static readonly FieldInfo[] klineFields = typeof(FuturesKlineType).GetFields(BindingFlags.Public | BindingFlags.Static);

        public bool IsInvalid => length == 0;

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
                    if (f.GetValue(null) is FuturesKlineType ktt) l.Add(ktt);
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
            return length.ToString();
        }

        public string ToString(string numberFormat)
        {
            return length.ToString(numberFormat);
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

        public string ToString(bool formatted)
        {
            if (!formatted)
            {
                return length.ToString();
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
                        if (fc.length == length)
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
                return fc.length == length;
            }
            else if (obj is int i)
            {
                return i == length;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return length;
        }

        public DateTime GetStartDate(int pieces, DateTime? endDate = null)
        {
            var et = endDate ?? DateTime.Now;
            return et.AddMinutes(-1 * length * pieces);
        }

        public static implicit operator int(FuturesKlineType val)
        {
            return val.length;
        }

        public static explicit operator FuturesKlineType(int val)
        {
            return new FuturesKlineType(val);
        }

        public static bool operator ==(int val1, FuturesKlineType val2)
        {
            return val1 == val2.length;
        }
        public static bool operator !=(int val1, FuturesKlineType val2)
        {
            return val1 != val2.length;
        }

        public static bool operator ==(FuturesKlineType val1, int val2)
        {
            return val2 == val1.length;
        }
        public static bool operator !=(FuturesKlineType val1, int val2)
        {
            return val2 != val1.length;
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
