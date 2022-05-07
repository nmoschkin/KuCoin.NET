using KuCoin.NET.Data.Market;
using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace KuCoin.NET.Observable
{

    /// <summary>
    /// Standard observable, keyed collection of orders (asks and bids.)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableOrderUnits<T> : OrderUnitKeyedCollection<T>, INotifyPropertyChanged, INotifyCollectionChanged where T: IOrderUnit, new()
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableOrderUnits() : this(false)
        {
        }

        public ObservableOrderUnits(bool descending) : base(descending)
        {
        }

        protected internal override void ClearItems()
        {
            lock(syncRoot)
            {
                base.ClearItems();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected internal override void InsertItem(T item)
        {
            lock(syncRoot)
            {

                base.InsertItem(item);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Walk(item, TreeWalkMode.Locate)));
            }
        }

        protected internal override void RemoveItem(int index)
        {
            lock (syncRoot)
            {
                var oldItem = ((IList<T>)this)[index];
                base.RemoveItem(index);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
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
