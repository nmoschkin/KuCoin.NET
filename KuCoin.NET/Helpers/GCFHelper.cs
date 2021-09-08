using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Helpers
{
    public static class GCFHelper
    {
        /// <summary>
        /// Find the greatest common factor from a set of integers.
        /// </summary>
        /// <param name="values">The values to perform the operation on.</param>
        /// <returns>The greatest common factor.</returns>
        public static int FindGCF(params int[] values)
        {
            if (values == null || values.Length == 1) throw new ArgumentOutOfRangeException("Function takes 2 or more numbers.");
            int candidate = values[0];

            int i, c = values.Length;
            int min, max, mod;

            int gcf = -1;

            for (i = 1; i < c; i++)
            {
                min = candidate;
                max = values[i];

                if (min > max)
                {
                    min = max;
                    max = values[i - 1];
                }

                mod = max % min;

                while (mod != 0)
                {
                    max = min;
                    min = mod;
                    mod = max % min;
                }

                candidate = min;

                if (gcf == -1 || gcf > candidate) gcf = candidate;
                if (candidate == 1) break;
            }

            return gcf;
        }

    }
}
