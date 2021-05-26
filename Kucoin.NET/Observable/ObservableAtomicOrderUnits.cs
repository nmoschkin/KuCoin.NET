﻿using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Kucoin.NET.Observable
{
    public class ObservableAtomicOrderUnits<T> : KeyedCollection<decimal, T>, INotifyCollectionChanged where T: IAtomicOrderUnit
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private object lockObj = new object();

        protected override decimal GetKeyForItem(T item) => item.Price;

        bool descending;

        public ObservableAtomicOrderUnits() : this(false)
        {
        }

        public ObservableAtomicOrderUnits(bool descending) : base()
        {
            this.descending = descending;
        }

        int GetInsertIndex(T unit)
        {
            if (Count == 0) return 0;

            int hi = Count - 1;
            int lo = 0;
            int mid;

            var l = this as IList<T>;
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

        public T[] ToArray()
        {

            if (Count == 0) return new T[0];
            T[] output = new T[Count];

            lock (lockObj)
            {
                CopyTo(output, 0);
            }

            return output;
        }

        protected override void ClearItems()
        {
            lock (lockObj)
            {
                base.ClearItems();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected override void InsertItem(int index, T item)
        {
            lock (lockObj)
            {
                index = GetInsertIndex(item);

                base.InsertItem(index, item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (lockObj)
            {
                var oldItem = ((IList<T>)this)[index];
                base.RemoveItem(index);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
            }
        }

        protected override void SetItem(int index, T item)
        {
            lock (lockObj)
            {
                if (index >= Count)
                {
                    InsertItem(0, item);
                    return;
                }
                var oldItem = ((IList<T>)this)[index];
                
                if (Contains(item.Price))
                {
                    var orgitem = this[item.Price];
                    
                    orgitem.Size = item.Size;
                    orgitem.Timestamp = item.Timestamp;
                    orgitem.OrderId = item.OrderId;

                    return;
                }
                base.SetItem(index, item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
            }
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

    }
}
