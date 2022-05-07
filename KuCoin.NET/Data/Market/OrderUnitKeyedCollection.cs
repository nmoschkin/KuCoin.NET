
using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KuCoin.NET.Data.Market
{

    /// <summary>
    /// Keyed, sorted Level 2 order book.
    /// </summary>
    /// <typeparam name="TUnit">The type of the order unit.</typeparam>
    /// <remarks>
    /// <see cref="TUnit"/> must implement <see cref="IOrderUnit"/>.
    /// Classes derived from this class maintain a price-sorted, keyed list.
    /// Sorting is vital to the function of Level 2 and Level 3 websocket feeds.
    /// Index for insert is ignored.  The insert index is calculated by the sort order using binary search.
    /// </remarks>
    public class OrderUnitKeyedCollection<TUnit> : RedBlackCollection<TUnit> where TUnit : IOrderUnit, new()
    {

        bool descending;
        TUnit wunit = new TUnit();

        class Comparer : IComparer<TUnit>
        {
            int m;
            public Comparer(bool descending)
            {
                m = descending ? -1 : 1;
            }
            public int Compare(TUnit a, TUnit b)
            {
                return decimal.Compare(a.Price, b.Price) * m;
            }
        }

        public bool Descending => descending;

        public OrderUnitKeyedCollection() : this(false)
        {
        }

        public OrderUnitKeyedCollection(bool descending) : base(new Comparer(descending))
        {
            this.descending = descending;            
        }

        public TUnit this[decimal key]
        {
            get
            {
                lock (syncRoot)
                {
                    wunit.Price = key;
                    return Items[Walk(wunit, TreeWalkMode.Locate)];
                }
            }
        }

        public bool ContainsKey(decimal key)
        {
            lock (syncRoot)
            {
                wunit.Price = key;
                return Walk(wunit, TreeWalkMode.Locate) >= 0;
            }
        }

        public bool TryGetValue(decimal key, [MaybeNullWhen(false)] out TUnit value)
        {
            lock (syncRoot)
            {
                wunit.Price = key;
                int i = Walk(wunit, TreeWalkMode.Locate);

                if (i >= 0)
                {
                    value = Items[i];
                    return true;
                }

                value = default;
                return false;
            }
        }

        public bool Remove(decimal key)
        {
            lock(syncRoot)
            {
                wunit.Price = key;
                int i = Walk(wunit, TreeWalkMode.Locate);

                if (i < 0) return false;

                base.RemoveItem(i);
                return true;
            }
        }
    }
}
