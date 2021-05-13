using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Kucoin.NET.Observable
{
    public class ObservableOrderUnits : KeyedCollection<decimal, OrderUnit>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override decimal GetKeyForItem(OrderUnit item) => item.Price;

        bool descending;

        public ObservableOrderUnits() : this(false)
        {
        }

        public ObservableOrderUnits(bool descending) : base()
        {
            this.descending = descending;
        }

        int GetInsertIndex(OrderUnit unit)
        {
            if (Count == 0) return 0;

            int hi = Count - 1;
            int lo = 0;
            int mid; 
            
            var l = this as IList<OrderUnit>;
            var uprice = unit.Price;
            decimal cprice;

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

        public OrderUnit[] ToArray()
        {
            if (Count == 0) return new OrderUnit[0];
            OrderUnit[] output = new OrderUnit[Count];

            CopyTo(output, 0);
            return output;
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void InsertItem(int index, OrderUnit item)
        {
            index = GetInsertIndex(item);

            base.InsertItem(index, item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            var oldItem = ((IList<OrderUnit>)this)[index];
            base.RemoveItem(index);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
        }

        protected override void SetItem(int index, OrderUnit item)
        {
            if (index >= Count)
            {
                InsertItem(0, item);
                return;
            }

            var oldItem = ((IList<OrderUnit>)this)[index];

            if (Contains(item.Price))
            {
                var orgitem = this[item.Price];

                orgitem.Size = item.Size;
                orgitem.Sequence = item.Sequence;

                //Remove(item.Price);
                //InsertItem(0, item);
                return;
            }
            base.SetItem(index, item);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }

    }
}
