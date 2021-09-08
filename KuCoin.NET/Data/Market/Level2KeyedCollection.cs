﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kucoin.NET.Data.Market
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
    public class Level2KeyedCollection<TUnit> : KeyedCollection<decimal, TUnit> where TUnit : IOrderUnit
    {
        protected object lockObj = new object();

        protected override decimal GetKeyForItem(TUnit item) => item.Price;

        protected bool descending;

        public Level2KeyedCollection() : this(false)
        {
        }

        public Level2KeyedCollection(bool descending) : base(null, 0)
        {
            this.descending = descending;
        }

        protected override void InsertItem(int index, TUnit item)
        {
            lock (lockObj)
            {
                index = GetInsertIndex(item);
                base.InsertItem(index, item);
            }
        }

        /// <summary>
        /// Get the appropriate insert index for the configured sort direction, based on price.
        /// </summary>
        /// <param name="unit">The order unit to test.</param>
        /// <returns>The calculated insert index based on the sort direction.</returns>
        protected int GetInsertIndex(TUnit unit)
        {
            if (Count == 0) return 0;

            int hi = Count - 1;
            int lo = 0;
            int mid;

            var l = this as IList<TUnit>;
            var uprice = unit.Price;
            decimal cprice;

            // the code is quicker if we don't check the ascending/descending variable every time.
            // so we implement the loop twice.  
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
                        return mid;
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
                        return mid;
                    }

                }

            }

        }
        protected override void SetItem(int index, TUnit item)
        {
            lock (lockObj)
            {
                if (index >= Count)
                {
                    InsertItem(0, item);
                    return;
                }
                var oldItem = ((IList<TUnit>)this)[index];

                if (Contains(item.Price))
                {
                    //var orgitem = this[item.Price];
                    //orgitem.Size = item.Size;

                    //if (item is ISequencedOrderUnit seq && orgitem is ISequencedOrderUnit orgseq)
                    //    orgseq.Sequence = seq.Sequence;

                    RemoveItem(index);
                    InsertItem(index, item);

                    return;
                }

                base.SetItem(index, item);
            }
        }

        /// <summary>
        /// Returns the contents as an array of <see cref="TUnit"/>.
        /// </summary>
        /// <returns></returns>
        public TUnit[] ToArray()
        {

            if (Count == 0) return new TUnit[0];
            TUnit[] output = new TUnit[Count];

            lock (lockObj)
            {
                CopyTo(output, 0);
            }

            return output;
        }


    }
}
