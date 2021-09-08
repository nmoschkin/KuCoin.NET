﻿using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace KuCoin.NET.Observable
{
    /// <summary>
    /// Standard implementation of a keyed, observable collection for Level 3 atomic order units
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableAtomicOrderUnits<T> : Level2KeyedCollection<T>, INotifyPropertyChanged, INotifyCollectionChanged where T: IAtomicOrderUnit
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableAtomicOrderUnits() : this(false)
        {
        }

        public ObservableAtomicOrderUnits(bool descending) : base()
        {
            this.descending = descending;
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

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                    break;
            }
        }

    }
}