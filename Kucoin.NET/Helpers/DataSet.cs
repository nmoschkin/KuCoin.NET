using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Kucoin.NET.Helpers
{
    public class DataSet<T> : List<T>
    {

        public DataSet() : base()
        {
        }

        public DataSet(IEnumerable<T> items) : base(items)
        {
        }

        public DataSet(int capacity) : base(capacity)
        {
        }


        public static DataSet<T> operator &(DataSet<T> val1, DataSet<T> val2)
        {
            int i, c;

            DataSet<T> result = new DataSet<T>();

            c = val1.Count;

            for (i = 0; i < c; i++)
            {
                if (result.Contains(val1[i])) continue;

                if (val2.Contains(val1[i]))
                {
                    result.Add(val1[i]);
                }
            }

            return result;
        }

        public static DataSet<T> operator ^(DataSet<T> val1, DataSet<T> val2)
        {
            int i, c;

            DataSet<T> result = new DataSet<T>();

            c = val1.Count;

            for (i = 0; i < c; i++)
            {
                if (result.Contains(val1[i])) continue;

                if (!val2.Contains(val1[i]))
                {
                    result.Add(val1[i]);
                }
            }

            c = val2.Count;

            for (i = 0; i < c; i++)
            {
                if (result.Contains(val2[i])) continue;

                if (!val1.Contains(val2[i]))
                {
                    result.Add(val2[i]);
                }
            }

            return result;
        }

        public static DataSet<T> operator |(DataSet<T> val1, DataSet<T> val2)
        {
            int i, c;

            DataSet<T> result = new DataSet<T>();

            c = val1.Count;

            for (i = 0; i < c; i++)
            {
                if (result.Contains(val1[i])) continue;
                result.Add(val1[i]);
            }

            c = val2.Count;

            for (i = 0; i < c; i++)
            {
                if (result.Contains(val2[i])) continue;
                result.Add(val2[i]);
            }

            return result;
        }

        public static DataSet<T> operator %(DataSet<T> val1, DataSet<T> val2)
        {
            int i, c;

            DataSet<T> result = new DataSet<T>();

            c = val1.Count;

            for (i = 0; i < c; i++)
            {
                if (result.Contains(val1[i])) continue;

                if (!val2.Contains(val1[i]))
                {
                    result.Add(val1[i]);
                }
            }

            return result;
        }

        public static bool operator ==(DataSet<T> val1, DataSet<T> val2)
        {
            if (val1.Count != val2.Count) return false;

            int i, c;
            c = val1.Count;

            for (i = 0; i < c; i++)
            {
                if (val1[i] is object)
                {
                    if (!val1[i].Equals(val2[i])) return false;
                }
                else if (val2[i] is object)
                {
                    if (!val2[i].Equals(val1[i])) return false;
                }
            }

            return true;

        }


        public static bool operator !=(DataSet<T> val1, DataSet<T> val2)
        {
            if (val1.Count != val2.Count) return true;

            int i, c;
            c = val1.Count;

            for (i = 0; i < c; i++)
            {
                if (val1[i] is object)
                {
                    if (!val1[i].Equals(val2[i])) return true;
                }
                else if (val2[i] is object)
                {
                    if (!val2[i].Equals(val1[i])) return true;
                }
            }

            return false;

        }

        public override bool Equals(object obj)
        {
            if (obj is DataSet<T> d)
            {
                return d == this;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hc = 0;
            foreach(var item in this)
            {
                hc = hc << 1 | item.GetHashCode();
            }

            return hc;
        }

    }
}
