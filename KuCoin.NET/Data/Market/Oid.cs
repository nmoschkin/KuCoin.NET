using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Data.Market
{
#if DOTNETSTD

#else
    [SkipLocalsInit]
    [StructLayout(LayoutKind.Explicit, Size = 52)]
    public struct Oid : IEquatable<Oid>, IComparable<Oid>
    {
        public string OrderId
        {
            get
            {
                unsafe
                {
                    Oid val = this;
                    return new string((char*)&val, 0, 24);
                }
            }
            set
            {
                this = value;
            }
        }

        public uint Hash
        {
            get
            {
                unsafe
                {
                    Oid o = this;
                    return *(uint*)((byte*)&o + 48);
                }
            }
        }

        public static implicit operator Oid(string val)
        {
            if (val.Length != 24) throw new InvalidCastException();

            unsafe
            {
                Oid oid = new Oid();
                
                val.CopyTo(new Span<char>((char*)&oid, 24));
                uint crc = Crc32.Hash((byte*)&oid, 48);
                *(uint*)((byte*)&oid + 48) = crc;

                return oid;
            }
        }

        public static implicit operator string(Oid val)
        {
            unsafe
            {
                return new string((char*)&val, 0, 24);
            }
        }

        public static implicit operator uint(Oid val)
        {
            unsafe
            {
                return *(uint*)((byte*)&val + 48);
            }
        }

        public static implicit operator int(Oid val)
        {
            unsafe
            {
                return *(int*)((byte*)&val + 48);
            }
        }

        public static bool operator ==(Oid v1, Oid v2)
        {
            return ((uint)v1 == (uint)v2);
        }
        public static bool operator !=(Oid v1, Oid v2)
        {
            return ((uint)v1 != (uint)v2);
        }

        public override string ToString()
        {
            string s = "" + this;
            uint x = this;
            return $"Order Id: {s}, Hash: {x:x}";
        }

        public bool Equals(Oid obj)
        {
            return this == obj;
        }

        public int CompareTo(Oid obj)
        {
            return ((uint)this).CompareTo(obj);
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is Oid other)
            {
                return this == other;
            }
            else if (obj is string s)
            {
                return this == s;
            }
            else if (obj is uint ui)
            {
                return this == ui;
            }
            else if (obj is int i)
            {
                return this == i;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (int)this;
        }

    }

#endif
}
