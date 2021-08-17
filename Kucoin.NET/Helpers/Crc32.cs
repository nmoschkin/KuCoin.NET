using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Helpers
{
    /// <summary>
    /// ISO 3309 CRC-32 Hash Calculator.
    /// </summary>
    /// <remarks></remarks>
    public sealed class Crc32
    {

        private static readonly uint CRC32Poly = 0xedb88320u;

        private static uint[] Crc32Table = new uint[256];

        private Crc32()
        {
            // this is not a creatable object.
        }

        /// <summary>
        /// Initialize the CRC table from the polynomial.
        /// </summary>
        /// <remarks></remarks>
        static Crc32()
        {
            uint i = 0;
            uint j = 0;
            uint l = 0;

            for (i = 0; i <= 255; i++)
            {
                j = i;
                for (l = 0; l <= 7; l++)
                {
                    if ((j & 1) == 1)
                    {
                        j = j >> 1 ^ CRC32Poly;
                    }
                    else
                    {
                        j >>= 1;
                    }
                }
                Crc32Table[i] = j;
            }

        }

        /// <summary>
        /// Consistently Hash a String
        /// </summary>
        /// <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash(string data)
        {
            uint crc = 0xffffffffu;
            int j, c = data.Length;

            for (j = 0; j < c; j++)
            {
                crc = Crc32Table[(crc ^ data[j]) & 0xff] ^ crc >> 8;
            }

            return (int)(crc ^ 0xffffffffu);
        }

    }
}
