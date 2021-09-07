using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Kucoin.NET.Data.Order
{
    [JsonConverter(typeof(KeyedBookConverter))]
    public sealed class KeyedBook<TUnit> : Collection<TUnit>, IReadOnlyDictionary<string, TUnit> where TUnit : IAtomicOrderUnit, new()
    {
        private object lockObj = new object();
        internal SortedDictionary<string, TUnit> orderIds = new SortedDictionary<string, TUnit>();

        private bool descending;

        public bool Descending => descending;

        public KeyedBook() : this(false)
        {
        }

        public TUnit this[string orderId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (lockObj)
                {

                    if (orderIds.TryGetValue(orderId, out TUnit found))
                    {
                        return found;
                    }
                    else
                    {
                        return default;
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (lockObj)
                {
                    int i = FindItem(value);

                    if (i != -1)
                    {
                        SetItem(i, value);
                    }
                    else
                    {
                        InsertItem(i, value);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindItem(TUnit item)
        {

            int z = GetInsertIndex(item);
            if (z < 0 || z >= Count) return -1;

            if (this[z].OrderId == item.OrderId) return z;
            int oz = z;

            while (++z < Count && this[z].Price == item.Price)
            {
                if (this[z].OrderId == item.OrderId) return z;
            }

            z = oz;

            while (--z >= 0 && this[z].Price == item.Price)
            {
                if (this[z].OrderId == item.OrderId) return z;
            }

            return -1;
        }

        public KeyedBook(bool descending) : base()
        {
            this.descending = descending;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void InsertItem(int index, TUnit item)
        {
            lock (lockObj)
            {
                index = GetInsertIndex(item);

                orderIds.Add(item.OrderId, item);
                base.InsertItem(index, item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void RemoveItem(int index)
        {
            lock (lockObj)
            {
                orderIds.Remove(this[index].OrderId);
                base.RemoveItem(index);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ClearItems()
        {
            lock (lockObj)
            {
                orderIds.Clear();
                base.ClearItems();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(string orderId)
        {
            lock (lockObj)
            {
                if (!orderIds.ContainsKey(orderId)) return;
                var item = orderIds[orderId];

                orderIds.Remove(orderId);

                var i = FindItem(item);
                if (i != -1)
                    base.RemoveItem(i);
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                lock (lockObj)
                {
                    return orderIds.Keys.ToArray();
                }
            }
        }

        public IEnumerable<TUnit> Values
        {
            get
            {
                lock (lockObj)
                {
                    return this.ToArray();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(string orderId, out TUnit value)
        {
            lock (lockObj)
            {
                return orderIds.TryGetValue(orderId, out value);
            }
        }

        /// <summary>
        /// Get the appropriate insert index for the configured sort direction, based on price.
        /// </summary>
        /// <param name="unit">The order unit to test.</param>
        /// <returns>The calculated insert index based on the sort direction.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetInsertIndex(TUnit unit)
        {
            if (Count == 0) return 0;

            int hi = Count - 1;
            int lo = 0;
            int mid;

            var l = this as IList<TUnit>;
            var uprice = unit.Price;

            long utime = unit.Timestamp.Ticks;
            var usize = unit.Size;

            decimal cprice;
            decimal csize;

            long ctime;

            if (!descending)
            {
                while (true)
                {
                    if (hi < lo)
                    {
                        return lo;
                    }

                    mid = (hi + lo) / 2;

                    cprice = l[mid].Price;
                    ctime = l[mid].Timestamp.Ticks;
                    csize = l[mid].Size;

                    if (uprice > cprice)
                    {
                        lo = mid + 1;
                    }
                    else if (uprice < cprice)
                    {
                        hi = mid - 1;
                    }
                    else
                    {
                        //return mid;

                        if (usize < csize)
                        {
                            lo = mid + 1;
                        }
                        else if (usize > csize)
                        {
                            hi = mid - 1;
                        }
                        else
                        {
                            if (utime < ctime)
                            {
                                lo = mid + 1;
                            }
                            else if (utime > ctime)
                            {
                                hi = mid - 1;
                            }
                            else
                            {
                                return mid;
                            }
                        }
                    }
                }
            }
            else
            {
                while (true)
                {
                    if (hi < lo)
                    {
                        return lo;
                    }

                    mid = (hi + lo) / 2;

                    cprice = l[mid].Price;
                    ctime = l[mid].Timestamp.Ticks;
                    csize = l[mid].Size;

                    if (uprice < cprice)
                    {
                        lo = mid + 1;
                    }
                    else if (uprice > cprice)
                    {
                        hi = mid - 1;
                    }
                    else
                    {
                        //return mid;

                        if (usize < csize)
                        {
                            lo = mid + 1;
                        }
                        else if (usize > csize)
                        {
                            hi = mid - 1;
                        }
                        else
                        {
                            if (utime < ctime)
                            {
                                lo = mid + 1;
                            }
                            else if (utime > ctime)
                            {
                                hi = mid - 1;
                            }
                            else
                            {
                                return mid;
                            }
                        }
                    }

                }

            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(string orderId)
        {
            lock (lockObj)
            {
                return orderIds.ContainsKey(orderId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetItem(int index, TUnit item)
        {
            lock (lockObj)
            {
                if (index >= Count)
                {
                    InsertItem(0, item);
                    return;
                }

                RemoveItem(index);
                InsertItem(index, item);
            }
        }

        /// <summary>
        /// Returns the contents as an array of <see cref="TUnit"/>.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TUnit[] ToArray()
        {

            if (Count == 0) return new TUnit[0];
            TUnit[] output;

            lock (lockObj)
            {
                output = new TUnit[Count];
                CopyTo(output, 0);
            }

            return output;
        }

        IEnumerator<KeyValuePair<string, TUnit>> IEnumerable<KeyValuePair<string, TUnit>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, TUnit>>)orderIds).GetEnumerator();
        }

    }

}
