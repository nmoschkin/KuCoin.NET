using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kucoin.NET.Data.Order
{


    public class KeyedBook<TUnit> : Collection<TUnit> where TUnit : IAtomicOrderUnit
    {
        protected object lockObj = new object();
        protected Dictionary<string, TUnit> orderIds = new Dictionary<string, TUnit>();

        protected bool descending;

        public KeyedBook() : this(false)
        {
        }
        public TUnit this[string key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return orderIds[key];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                int i = FindItem(value);

                if (i != -1)
                {
                    SetItem(i, value);
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
            index = GetInsertIndex(item);

            orderIds.Add(item.OrderId, item);
            base.InsertItem(index, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void RemoveItem(int index)
        {
            orderIds.Remove(this[index].OrderId);
            base.RemoveItem(index);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ClearItems()
        {
            orderIds.Clear();
            base.ClearItems();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(string orderId)
        {
            //if (!orderIds.ContainsKey(orderId)) return;
            var item = orderIds[orderId];

            orderIds.Remove(orderId);

            var i = FindItem(item);
            if (i != -1)
                base.RemoveItem(i);
        }

        /// <summary>
        /// Get the appropriate insert index for the configured sort direction, based on price.
        /// </summary>
        /// <param name="unit">The order unit to test.</param>
        /// <returns>The calculated insert index based on the sort direction.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetInsertIndex(TUnit unit)
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
        public bool Contains(string orderId)
        {
            return orderIds.ContainsKey(orderId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetItem(int index, TUnit item)
        {
            if (index >= Count)
            {
                InsertItem(0, item);
                return;
            }

            RemoveItem(index);
            InsertItem(index, item);
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

    }

}
