using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Kucoin.NET.Observable
{

    /// <summary>
    /// Standard observable, keyed collection of orders (asks and bids.)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableOrderUnits<T> : Level2KeyedCollection<T>, INotifyPropertyChanged, INotifyCollectionChanged where T: IOrderUnit
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableOrderUnits() : this(false)
        {
        }

        public ObservableOrderUnits(bool descending) : base()
        {
            this.descending = descending;
        }

        protected override void ClearItems()
        {
            lock(lockObj)
            {
                base.ClearItems();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected override void InsertItem(int index, T item)
        {
            lock(lockObj)
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

                    if (item is ISequencedOrderUnit seq && orgitem is ISequencedOrderUnit orgseq)
                        orgseq.Sequence = seq.Sequence;

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
