using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Helpers
{
    /// <summary>
    /// ISO 3309 CRC-32 Hash Calculator.
    /// </summary>
    /// <remarks></remarks>
    public sealed class Crc32
    {
        /// <summary>
        /// The ISO-3309 CRC-32 Polynomial.
        /// </summary>
        public const uint CRC32Poly = 0xedb88320u;
        
        /// <summary>
        /// The ISO-3309 CRC-32 Hash Lookup Table.
        /// </summary>
        private static readonly uint[] Crc32Table;

        /// <summary>
        /// Initialize the CRC table from the polynomial.
        /// </summary>
        /// <remarks></remarks>
        static Crc32()
        {
            Crc32Table = CreateCrc32HashTable();
        }
        /// <summary>
        /// Create an ISO-3309 CRC-32 Hash Lookup Table.
        /// </summary>
        public static uint[] CreateCrc32HashTable()
        {
            uint i;
            uint j;
            uint l;

            uint[] table = new uint[256];

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
                table[i] = j;
            }

            return table;
        }

        /// <summary>
        /// Consistently Hash a string using .NET default 2 byte Unicode enoding.
        /// </summary>
        /// <remarks>
        /// In order to conform to ISO-3309 the text is hashed as a byte array.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Hash(string text)
        {
            uint crc = 0xffffffffu;
            int j, c = text.Length;

            for (j = 0; j < c; j++)
            {
                crc = Crc32Table[(crc ^ (text[j] & 0xff)) & 0xff] ^ crc >> 8;
                crc = Crc32Table[(crc ^ (text[j] >> 8)) & 0xff] ^ crc >> 8;
            }

            return crc ^ 0xffffffffu;
        }

        /// <summary>
        /// Hash a string with the ISO-3309 CRC-32 algorithm.
        /// </summary>
        /// <remarks>
        /// In order to conform to ISO-3309 the text is hashed as a byte array.<br />
        /// Note: Encoding and decoding from the internal UTF-16 is a somewhat expensive task.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Hash(string text, Encoding encoding) => Hash(encoding.GetBytes(text));

        /// <summary>
        /// Consistently Hash characters using .NET default 2 byte Unicode enoding.
        /// </summary>
        /// <remarks>
        /// In order to conform to ISO-3309 the text is hashed as a byte array.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Hash(char[] chars)
        {
            uint crc = 0xffffffffu;
            int j, c = chars.Length;

            for (j = 0; j < c; j++)
            {
                crc = Crc32Table[(crc ^ (chars[j] & 0xff)) & 0xff] ^ crc >> 8;
                crc = Crc32Table[(crc ^ (chars[j] >> 8)) & 0xff] ^ crc >> 8;
            }

            return crc ^ 0xffffffffu;
        }

        /// <summary>
        /// Hash bytes with the ISO-3309 CRC-32 algorithm.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Hash(byte[] data)
        {
            uint crc = 0xffffffffu;
            int j, c = data.Length;

            for (j = 0; j < c; j++)
            {
                crc = Crc32Table[(crc ^ data[j]) & 0xff] ^ crc >> 8;
            }

            return crc ^ 0xffffffffu;
        }

        /// <summary>
        /// Hash bytes with the ISO-3309 CRC-32 algorithm.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint Hash(byte* data, int count)
        {
            uint crc = 0xffffffffu;
          
            for (int j = 0; j < count; j++)
            {
                crc = Crc32Table[(crc ^ data[j]) & 0xff] ^ crc >> 8;
            }

            return crc ^ 0xffffffffu;
        }

        private uint current;

        /// <summary>
        /// Gets the current calculation of the CRC-32 as an unsigned 32-bit integer.
        /// </summary>
        public uint Current => current ^ 0xffffffffu;

        /// <summary>
        /// Create a rolling CRC-32 calculation.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public Crc32(string initialValue)
        {
            current = Hash(initialValue) ^ 0xffffffffu;
        }

        /// <summary>
        /// Create a rolling CRC-32 calculation.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public Crc32(byte[] initialValue)
        {
            current = Hash(initialValue) ^ 0xffffffffu;
        }
        /// <summary>
        /// Create a rolling CRC-32 calculation.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public Crc32(char[] initialValue)
        {
            current = Hash(initialValue) ^ 0xffffffffu;
        }

        /// <summary>
        /// Create a rolling CRC-32 calculation.
        /// </summary>
        public Crc32()
        {
            current = 0xffffffffu;           
        }

        /// <summary>
        /// Hash two bytes into the current calculation (UTF-16 <see cref="char"/>.)
        /// </summary>
        /// <param name="str"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next(char str)
        {
            current = Crc32Table[(current ^ (str & 0xff)) & 0xff] ^ current >> 8;
            current = Crc32Table[(current ^ (str >> 8)) & 0xff] ^ current >> 8;
        }

        /// <summary>
        /// Hash characters into the current calculation (UTF-16 <see cref="char"/>.)
        /// </summary>
        /// <param name="str"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next(char[] str)
        {
            int c = str.Length;
            for (int i = 0; i < c; i++)
            {
                current = Crc32Table[(current ^ (str[i] & 0xff)) & 0xff] ^ current >> 8;
                current = Crc32Table[(current ^ (str[i] >> 8)) & 0xff] ^ current >> 8;
            }
        }

        /// <summary>
        /// Hash a string into the current calculation (UTF-16 <see cref="char"/>.)
        /// </summary>
        /// <param name="str"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next(string str)
        {
            int c = str.Length;
            for (int i = 0; i < c; i++)
            {
                current = Crc32Table[(current ^ (str[i] & 0xff)) & 0xff] ^ current >> 8;
                current = Crc32Table[(current ^ (str[i] >> 8)) & 0xff] ^ current >> 8;
            }
        }

        /// <summary>
        /// Hash a byte into the current calculation.
        /// </summary>
        /// <param name="b"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next(byte b)
        {
            current = Crc32Table[(current ^ b) & 0xff] ^ current >> 8;
        }

        /// <summary>
        /// Hash bytes into the current calculation.
        /// </summary>
        /// <param name="b"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next(byte[] data)
        {
            int c = data.Length;

            for (int i = 0; i < c; i++)
            {
                current = Crc32Table[(current ^ data[i]) & 0xff] ^ current >> 8;
            }
        }

        /// <summary>
        /// Reset the calculation.
        /// </summary>
        /// <returns>The CRC calculated before reset.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Reset()
        {
            uint c = current;
            current = 0xffffffffu;
            return c ^ 0xffffffffu;
        }

    }
}
