﻿using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

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


namespace KuCoin.NET.Data.Market
{
    #region Atomic Order Unit Comparer 
    public class AtomicComparer<TUnit> : IComparer<TUnit> where TUnit : IAtomicOrderUnit
    {
        bool descending;
        int m = 1;

        public bool Descending => descending;

        public AtomicComparer(bool descending)
        {
            this.descending = descending;
            if (descending) m = -1;
        }

        public int Compare(TUnit x, TUnit y)
        {
            int r;

            r = decimal.Compare(x.Price, y.Price) * m;
            if (r == 0) r = decimal.Compare(x.Size, y.Size) * -1;
            if (r == 0) r = DateTime.Compare(x.Timestamp, y.Timestamp) * -1;

            return r;
        }
    }
    #endregion Atomic Order Unit Comparer 


    /// <summary>
    /// Keyed Atomic Order Book
    /// </summary>
    /// <typeparam name="TUnit">Any <see cref="IAtomicOrderUnit"/> implementation.</typeparam>
    public class KeyedBook<TUnit> : KeyedRedBlackTree<string, TUnit> where TUnit : IAtomicOrderUnit, new()
    {
        #region Public Constructors
      
        public KeyedBook() : base(new AtomicComparer<TUnit>(false))
        {
        }

        public KeyedBook(bool descending) : base(new AtomicComparer<TUnit>(descending))
        {
        }

        #endregion Public Constructors

        #region Public Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(string key)
        {
            lock (syncRoot)
            {
                if (keyDict.ContainsKey(key))
                {
                    var item = keyDict[key];
                    Remove(item);
                }
            }
        }

        #endregion Public Methods

        #region Protected Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string ProvideKey(TUnit value)
        {
            return value.OrderId;
        }

        #endregion Protected Methods
    }




    #region Old KeyedBook

    //[JsonConverter(typeof(KeyedBookConverter))]
    //public sealed class KeyedBook<TUnit> : Collection<TUnit>, IReadOnlyDictionary<string, TUnit> where TUnit : IAtomicOrderUnit, new()
    //{
    //    private object lockObj = new object();
    //    internal SortedDictionary<string, TUnit> orderIds = new SortedDictionary<string, TUnit>();
    //    private List<TUnit> items;

    //    private bool descending;

    //    public bool Descending => descending;

    //    public KeyedBook() : this(false)
    //    {
    //    }

    //    /// <summary>
    //    /// Gets or sets the maximum capacity of the buffer without reallocating the array.
    //    /// </summary>
    //    public int Capacity
    //    {
    //        get => ((List<TUnit>)Items).Capacity;
    //        set => ((List<TUnit>)Items).Capacity = value;
    //    }

    //    public TUnit this[string orderId]
    //    {
    //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //        get
    //        {
    //            lock (lockObj)
    //            {

    //                if (orderIds.TryGetValue(orderId, out TUnit found))
    //                {
    //                    return found;
    //                }
    //                else
    //                {
    //                    return default;
    //                }
    //            }
    //        }
    //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //        set
    //        {
    //            lock (lockObj)
    //            {
    //                int i = FindItem(value);
    //                if (i == -1) throw new KeyNotFoundException();
    //                SetItem(i, value);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Match Order
    //    /// </summary>
    //    /// <param name="order">Order</param>
    //    /// <param name="match">Match Amount</param>
    //    /// <returns>True if successful.</returns>
    //    public bool Match(TUnit order, decimal match)
    //    {
    //        lock (lockObj)
    //        {
    //            int z = FindItem(order);
    //            if (z == -1) return false;

    //            order.Size -= match;

    //            if (!descending)
    //            {
    //                while (z > 0 && Items[z].Price == Items[z - 1].Price)
    //                {
    //                    if (Items[z].Size < Items[z - 1].Size)
    //                    {
    //                        order = Items[z - 1];
    //                        Items[z - 1] = Items[z];
    //                        Items[z] = order;
    //                    }

    //                    z--;
    //                }
    //            }
    //            else
    //            {
    //                // so, we know that we're always reducing the size, above.  
    //                // there's never a match that adds, so we want to quickly sort out 
    //                // if this price is smaller than the next place the price should be getting smaller
    //                // and in a descending list, that direction is ahead, and not behind.
    //                // so we're still looking for the smaller value, just going in the other direction.

    //                int c = Count - 1;
    //                while (z < c && Items[z].Price == Items[z + 1].Price)
    //                {
    //                    if (Items[z].Size < Items[z + 1].Size)
    //                    {
    //                        order = Items[z + 1];
    //                        Items[z + 1] = Items[z];
    //                        Items[z] = order;
    //                    }

    //                    z++;
    //                }

    //            }

    //            // SetItem(z, order);

    //            return true;
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    private int FindItem(TUnit item)
    //    {
    //        int z = GetInsertIndex(item);
    //        if (z < 0 || z >= Count) return -1;

    //        if (this[z].OrderId == item.OrderId) return z;
    //        int oz = z;

    //        z++;
    //        while (z < Count && this[z].Price == item.Price)
    //        {
    //            if (this[z].OrderId == item.OrderId) return z;
    //            z++;
    //        }

    //        z = oz;

    //        --z;
    //        while (z >= 0 && this[z].Price == item.Price)
    //        {
    //            if (this[z].OrderId == item.OrderId) return z;
    //            --z;
    //        }

    //        return -1;
    //    }

    //    public KeyedBook(bool descending) : base()
    //    {
    //        this.descending = descending;
    //        items = Items as List<TUnit>;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    protected override void InsertItem(int index, TUnit item)
    //    {
    //        lock (lockObj)
    //        {
    //            orderIds.Add(item.OrderId, item);

    //            index = GetInsertIndex(item);

    //            base.InsertItem(index, item);
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    protected override void RemoveItem(int index)
    //    {
    //        lock (lockObj)
    //        {
    //            orderIds.Remove(this[index].OrderId);
    //            base.RemoveItem(index);
    //        }
    //    }


    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    protected override void ClearItems()
    //    {
    //        lock (lockObj)
    //        {
    //            orderIds.Clear();
    //            base.ClearItems();
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void Remove(string orderId)
    //    {
    //        lock (lockObj)
    //        {
    //            if (!orderIds.ContainsKey(orderId)) return;
    //            var item = orderIds[orderId];

    //            orderIds.Remove(orderId);

    //            var i = FindItem(item);

    //            if (i != -1)
    //                base.RemoveItem(i);
    //        }
    //    }

    //    public IEnumerable<string> Keys
    //    {
    //        get
    //        {
    //            lock (lockObj)
    //            {
    //                return orderIds.Keys.ToArray();
    //            }
    //        }
    //    }

    //    public IEnumerable<TUnit> Values
    //    {
    //        get
    //        {
    //            lock (lockObj)
    //            {
    //                return ToArray();
    //            }
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public bool TryGetValue(string orderId, out TUnit value)
    //    {
    //        lock (lockObj)
    //        {
    //            return orderIds.TryGetValue(orderId, out value);
    //        }
    //    }

    //    /// <summary>
    //    /// Get the appropriate insert index for the configured sort direction, based on price.
    //    /// </summary>
    //    /// <param name="unit">The order unit to test.</param>
    //    /// <returns>The calculated insert index based on the sort direction.</returns>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public int GetInsertIndex(TUnit unit)
    //    {
    //        TUnit item;

    //        var count = Count;
    //        if (count == 0) return 0;

    //        int hi = count - 1;
    //        int lo = 0;
    //        int mid;

    //        var uprice = unit.Price;

    //        long utime = unit.Timestamp.Ticks;
    //        var usize = unit.Size;

    //        decimal cprice;
    //        decimal csize;

    //        long ctime;

    //        if (!descending)
    //        {
    //            // ascending

    //            while (true)
    //            {
    //                if (hi < lo)
    //                {
    //                    return lo;
    //                }

    //                mid = (hi + lo) / 2;
    //                item = Items[mid];

    //                cprice = item.Price;
    //                ctime = item.Timestamp.Ticks;
    //                csize = item.Size;

    //                if (uprice > cprice)
    //                {
    //                    lo = mid + 1;
    //                }
    //                else if (uprice < cprice)
    //                {
    //                    hi = mid - 1;
    //                }
    //                else
    //                {
    //                    //return mid;

    //                    if (usize < csize)
    //                    {
    //                        lo = mid + 1;
    //                    }
    //                    else if (usize > csize)
    //                    {
    //                        hi = mid - 1;
    //                    }
    //                    else
    //                    {
    //                        if (utime < ctime)
    //                        {
    //                            lo = mid + 1;
    //                        }
    //                        else if (utime > ctime)
    //                        {
    //                            hi = mid - 1;
    //                        }
    //                        else
    //                        {
    //                            return mid;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            while (true)
    //            {
    //                if (hi < lo)
    //                {
    //                    return lo;
    //                }

    //                mid = (hi + lo) / 2;

    //                item = Items[mid];

    //                cprice = item.Price;
    //                ctime = item.Timestamp.Ticks;
    //                csize = item.Size;

    //                if (uprice < cprice)
    //                {
    //                    lo = mid + 1;
    //                }
    //                else if (uprice > cprice)
    //                {
    //                    hi = mid - 1;
    //                }
    //                else
    //                {
    //                    //return mid;

    //                    if (usize < csize)
    //                    {
    //                        lo = mid + 1;
    //                    }
    //                    else if (usize > csize)
    //                    {
    //                        hi = mid - 1;
    //                    }
    //                    else
    //                    {
    //                        if (utime < ctime)
    //                        {
    //                            lo = mid + 1;
    //                        }
    //                        else if (utime > ctime)
    //                        {
    //                            hi = mid - 1;
    //                        }
    //                        else
    //                        {
    //                            return mid;
    //                        }
    //                    }
    //                }

    //            }

    //        }

    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public bool ContainsKey(string orderId)
    //    {
    //        lock (lockObj)
    //        {
    //            return orderIds.ContainsKey(orderId);
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    protected override void SetItem(int index, TUnit item)
    //    {
    //        lock (lockObj)
    //        {
    //            RemoveItem(index);
    //            InsertItem(index, item);

    //            //var newIdx = GetInsertIndex(item);

    //            //if (newIdx == index)
    //            //{
    //            //    Items[index] = item;
    //            //    return;
    //            //}
    //            //else
    //            //{
    //            //    if (newIdx > index)
    //            //    {
    //            //        for (var i = index + 1; i <= newIdx; i++)
    //            //        {

    //            //            Items[i - 1] = Items[i];
    //            //        }
    //            //    }
    //            //    else if (newIdx < index)
    //            //    {
    //            //        for (var i = index - 1; i >= newIdx; i--)
    //            //        {
    //            //            Items[i + 1] = Items[i];
    //            //        }
    //            //    }

    //            //    Items[newIdx] = item;
    //            //}
    //        }
    //    }

    //    /// <summary>
    //    /// Returns the contents as an array of <see cref="TUnit"/>.
    //    /// </summary>
    //    /// <returns></returns>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public TUnit[] ToArray()
    //    {

    //        if (Count == 0) return new TUnit[0];
    //        TUnit[] output;

    //        lock (lockObj)
    //        {
    //            output = new TUnit[Count];
    //            CopyTo(output, 0);
    //        }

    //        return output;
    //    }

    //    IEnumerator<KeyValuePair<string, TUnit>> IEnumerable<KeyValuePair<string, TUnit>>.GetEnumerator()
    //    {
    //        return ((IEnumerable<KeyValuePair<string, TUnit>>)orderIds).GetEnumerator();
    //    }

    //}

    #endregion Old KeyedBook

}
