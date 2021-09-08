
using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuCoin.NET.Observable
{

    /// <summary>
    /// Identify a property inside of a class as a default value for the key of an <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class KeyPropertyAttribute : Attribute
    {
        public string PropertyName { get; private set; }

        /// <summary>
        /// The name of the property.
        /// </summary>
        /// <remarks>
        /// If this attribute is used at the class level, the property name must be specified.
        /// </remarks>
        /// <param name="propertyName"></param>
        public KeyPropertyAttribute([CallerMemberName] string propertyName = null)
        {
            PropertyName = propertyName;
        }

        public override string ToString() => PropertyName;

        /// <summary>
        /// Gets the value of the property with the specified name from the specified object.
        /// </summary>
        /// <typeparam name="T">The type of the property value to retrieve.</typeparam>
        /// <param name="obj">The object from which to retrieve the property value.</param>
        /// <returns></returns>
        public T GetValue<T>(object obj)
        {
            return (T)obj.GetType().GetProperty(PropertyName)?.GetValue(obj);
        }

        /// <summary>
        /// Gets the property info for the specified object.
        /// </summary>
        /// <param name="obj">The object from which to retrieve the property.</param>
        /// <returns></returns>
        public PropertyInfo GetProperty(object obj)
        {
            return obj.GetType().GetProperty(PropertyName);
        }
    }



    /// <summary>
    /// A dictionary class that implements <see cref="INotifyCollectionChanged"/>.
    /// </summary>
    /// <typeparam name="TKey">They key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class ObservableDictionary<TKey, TValue> : KeyedCollection<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged where TValue : class
    {
        protected object lockObject = new object();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        protected string id = Guid.NewGuid().ToString("d");

        protected string keyProperty = null;

        protected PropertyInfo keyPropInfo = null;

        protected ListSortDirection direction;

        protected Comparison<TValue> comparer;

        protected bool sorted;

        protected bool noevent;

        /// <summary>
        /// Create a new observable dictionary.
        /// </summary>
        public ObservableDictionary() : base()
        {                
            var attr = typeof(TValue).GetCustomAttribute<KeyPropertyAttribute>();

            if (attr == null || typeof(TValue).GetProperty(attr.PropertyName) == null)
            { 
                var v = typeof(TValue).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var p in v)
                {
                    attr = p.GetCustomAttribute<KeyPropertyAttribute>();
                    if (attr != null)
                    {
                        break;
                    }
                }
            }
            
            if (attr != null)
            {
                var pi = typeof(TValue).GetProperty(attr.PropertyName);

                if (!typeof(TKey).IsAssignableFrom(pi.PropertyType)) throw new InvalidCastException($"{pi.PropertyType.FullName} is an invalid type for key.");

                keyProperty = attr.PropertyName;
                keyPropInfo = pi;
            }
            else
            {
                throw new MissingMemberException("Cannot determine the key property for this collection of objects.  Try calling a different constructor, or use the KeyPropertyAttribute.");
            }

            // default comparer
            if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey))) 
            {
                comparer = new Comparison<TValue>((obja, objb) =>
                {
                    TKey a = (TKey)keyPropInfo.GetValue(obja);
                    TKey b = (TKey)keyPropInfo.GetValue(objb);

                    if (!(a is object) && !(b is object))
                    {
                        return 0;
                    }
                    else if (!(a is object))
                    {
                        return 1;
                    }
                    else if (!(b is object))
                    {
                        return -1;
                    }

                    return ((IComparable<TKey>)a).CompareTo(b);
                });

                sorted = true;
                direction = ListSortDirection.Ascending;
            }
            else
            {
                direction = ListSortDirection.Ascending;
                comparer = MakeComparison();

                sorted = (comparer != null);
            }

        }

        /// <summary>
        /// Create a new dictionary with the specified sort configuration.
        /// </summary>
        /// <param name="valueComparer">The value comparer</param>
        /// <param name="direction">The sort direction</param>
        public ObservableDictionary(IComparer<TValue> valueComparer, ListSortDirection direction) : this()
        {
            ChangeSort(valueComparer, direction);
        }

        /// <summary>
        /// Create a new dictionary with the specified sort configuration.
        /// </summary>
        /// <param name="valueComparison">The value comparison</param>
        /// <param name="direction">The sort direction</param>
        public ObservableDictionary(Comparison<TValue> valueComparison, ListSortDirection direction) : this()
        {
            ChangeSort(valueComparison, direction);
        }

        /// <summary>
        /// Create a new dictionary with the specified sort configuration.
        /// </summary>
        /// <param name="sortPropertyName">The name of the property to sort on</param>
        /// <param name="direction">The sort direction</param>
        public ObservableDictionary(string sortPropertyName, ListSortDirection direction) : this()
        {
            ChangeSort(sortPropertyName, direction);
        }

        /// <summary>
        /// Create a new dictionary with the specified sort configuration.
        /// </summary>
        /// <param name="valueComparer">The value comparer</param>
        /// <param name="direction">The sort direction</param>
        /// <param name="values">Items to initialize the collection with</param>
        public ObservableDictionary(IComparer<TValue> valueComparer, ListSortDirection direction, IEnumerable<TValue> values) : this(valueComparer, direction)
        {
            AddRange(values);
        }

        /// <summary>
        /// Create a new dictionary with the specified sort configuration.
        /// </summary>
        /// <param name="valueComparison">The value comparison</param>
        /// <param name="direction">The sort direction</param>
        /// <param name="values">Items to initialize the collection with</param>
        public ObservableDictionary(Comparison<TValue> valueComparison, ListSortDirection direction, IEnumerable<TValue> values) : this(valueComparison, direction)
        {
            AddRange(values);
        }

        public ObservableDictionary(string sortPropertyName, ListSortDirection direction, IEnumerable<TValue> values) : this(sortPropertyName, direction)
        {
            AddRange(values);
        }

        /// <summary>
        /// Creates a new observable dictionary from the specified items.
        /// </summary>
        /// <remarks>
        /// The items must be of a class type, and must be decorated with the <see cref="KeyPropertyAttribute"/> attribute.
        /// </remarks>
        /// <param name="items"></param>
        public ObservableDictionary(IEnumerable<TValue> items) : this()
        {
            AddRange(items);
        }

        /// <summary>
        /// Create a new observable dictionary of class objects using the specified property as the key.
        /// </summary>
        /// <param name="keyPropertyName">The name of the key property inside the <see cref="TKey"/> class type.</param>
        public ObservableDictionary(string keyPropertyName) : base()
        {
            var prop = typeof(TValue).GetProperty(keyPropertyName);

            if (prop == null) throw new MissingMemberException("Cannot determine the key property for this collection of objects.  Try calling a different constructor, or use the KeyPropertyAttribute.");
            if (!typeof(TKey).IsAssignableFrom(prop.PropertyType)) throw new InvalidCastException($"{prop.PropertyType.FullName} is an invalid type for key.");

            keyPropInfo = prop;
            keyProperty = prop.Name;
        }

        /// <summary>
        /// Create a new observable dictionary of class objects using the specified property as the key.
        /// </summary>
        /// <param name="keyPropertyName">The name of the key property inside the <see cref="TKey"/> class type.</param>
        /// <param name="values">A sequence of items of type <see cref="TValue"/>.</param>
        public ObservableDictionary(string keyPropertyName, IEnumerable<TValue> values) : this(keyPropertyName)
        {
            AddRange(values);
        }

        public ObservableDictionary(string keyPropertyName, string sortPropertyName, ListSortDirection direction) : this(keyPropertyName)
        {
            ChangeSort(sortPropertyName, direction);
        }

        public ObservableDictionary(string keyPropertyName, IComparer<TValue> valueComparer, ListSortDirection direction) : this(keyPropertyName)
        {
            ChangeSort(valueComparer, direction);
        }

        public ObservableDictionary(string keyPropertyName, Comparison<TValue> valueComparison, ListSortDirection direction) : this(keyPropertyName)
        {
            ChangeSort(valueComparison, direction);
        }
        public ObservableDictionary(string keyPropertyName, string sortPropertyName, ListSortDirection direction, IEnumerable<TValue> values) : this(keyPropertyName)
        {
            ChangeSort(sortPropertyName, direction);
            AddRange(values);
        }

        public ObservableDictionary(string keyPropertyName, IComparer<TValue> valueComparer, ListSortDirection direction, IEnumerable<TValue> values) : this(keyPropertyName)
        {
            ChangeSort(valueComparer, direction);
            AddRange(values);
        }

        public ObservableDictionary(string keyPropertyName, Comparison<TValue> valueComparison, ListSortDirection direction, IEnumerable<TValue> values) : this(keyPropertyName)
        {
            ChangeSort(valueComparison, direction);
            AddRange(values);
        }


        /// <summary>
        /// Gets a value indicating whether this collection sorted.
        /// </summary>
        [JsonProperty("sorted")]
        public bool Sorted
        {
            get => sorted;

            // Only used for JSON Deserialization
            internal set
            {
                SetProperty(ref sorted, value);
            }
        }

        /// <summary>
        /// The name of the KeyProperty.
        /// </summary>
        [JsonIgnore]
        public string KeyProperty
        {
            get => keyProperty;

            // Only used for JSON deserialization
            internal set
            {
                SetProperty(ref keyProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a unique id for this ObservableDictionary.
        /// </summary>
        [JsonProperty("id")]
        public string Id
        {
            get => id;
            set
            {
                SetProperty(ref id, value);
            }
        }

        /// <summary>
        /// Add a range of items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<TValue> items)
        {
            lock (lockObject)
            {
                var oldno = noevent;
                noevent = true;

                foreach (var item in items)
                {
                    Add(item);
                }

                noevent = oldno;
                if (!noevent) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Try to get the value with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="value">Receives the item that was found, or null/default.</param>
        /// <returns>True if the item was found.</returns>
#if DOTNETSTD
        public bool TryGetValue(TKey key, out TValue value)
#else
        new public bool TryGetValue(TKey key, out TValue value)
#endif
        {
            if (!Contains(key))
            {
                value = default;
                return false;
            }
            else
            {
                value = ((IDictionary<TKey, TValue>)this)[key];
                return true;
            }

        }

        /// <summary>
        /// Return the contents of this collection as an array of <see cref="T"/>.
        /// </summary>
        /// <returns></returns>
        public TValue[] ToArray()
        {
            lock (lockObject)
            {
                if (Count == 0) return new TValue[0];
                TValue[] output = new TValue[Count];

                CopyTo(output, 0);
                return output;
            }
        }

        /// <summary>
        /// Perform a binary search for the specified value
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>The index of the value or -1.</returns>
        public int BinarySearchBySortProperty(TValue value)
        {
            return GetInsertIndex(value, true);
        }

#region KeyedCollection overrides

        protected override TKey GetKeyForItem(TValue item)
        {
            return (TKey)keyPropInfo.GetValue(item);
        }

        protected override void ClearItems()
        {
            lock(lockObject)
            {
                base.ClearItems();
                if (!noevent) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

        }

        protected override void InsertItem(int index, TValue item)
        {
            lock(lockObject)
            {
                if (sorted) index = GetInsertIndex(item);
                base.InsertItem(index, item);

                if (!noevent) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }

        }

        protected override void RemoveItem(int index)
        {
            TValue oldItem;

            lock(lockObject)
            {
                oldItem = ((IList<TValue>)this)[index];
                base.RemoveItem(index);

                if (!noevent) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
            }

        }

        protected override void SetItem(int index, TValue item)
        {
            TValue oldItem;
            TKey keyVal;

            lock(lockObject)
            {
                if (index >= Count)
                {
                    InsertItem(0, item);
                    return;
                }

                oldItem = ((IList<TValue>)this)[index];
                keyVal = (TKey)keyPropInfo.GetValue(item);

                if (Contains(keyVal))
                {
                    Remove(keyVal);
                    InsertItem(Count, item);

                    return;
                }

                base.SetItem(index, item);

                if (!noevent) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
            }

        }

#endregion

#region INotifyCollectionChanged

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!noevent) CollectionChanged?.Invoke(this, e);
        }

#endregion


#region INotifyPropertyChanged

        /// <summary>
        /// Set a property value if the current value is not equal to the new value and raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="backingStore">The value to compare and set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns></returns>
        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            bool pass;
            if (typeof(T).IsValueType)
            {
                pass = !backingStore.Equals(value);
            }
            else
            {
                if (!(backingStore is object) && !(value is object))
                {
                    pass = false;
                }
                else if (backingStore is object && !(value is object))
                {
                    pass = true;
                }
                else if (!(backingStore is object) && value is object)
                {
                    pass = true;
                }
                else
                {
                    pass = !backingStore.Equals(value);
                }
            }

            if (pass)
            {
                backingStore = value;
                OnPropertyChanged(propertyName);
            }

            return pass;
        }

        /// <summary>
        /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

#endregion



#region Sorting


        /// <summary>
        /// Change the way items are sorted in this collection
        /// </summary>
        /// <param name="valueComparer">The new value comparer</param>
        /// <param name="direction">The new sort direction</param>
        public virtual void ChangeSort(IComparer<TValue> valueComparer, ListSortDirection direction)
        {
            lock (lockObject)
            {
                sorted = true;
                this.direction = direction;
                comparer = MakeComparison(valueComparer);

                if (Count > 0) Sort();
            }
        }

        /// <summary>
        /// Change the way items are sorted in this collection
        /// </summary>
        /// <param name="valueComparison">The new value comparison</param>
        /// <param name="direction">The new sort direction</param>
        public virtual void ChangeSort(Comparison<TValue> valueComparison, ListSortDirection direction)
        {
            lock (lockObject)
            {
                sorted = true;
                this.direction = direction;
                comparer = valueComparison;

                if (Count > 0) Sort();
            }
        }

        /// <summary>
        /// Change the way items are sorted in this collection
        /// </summary>
        /// <param name="sortPropertyName">The new sort property name</param>
        /// <param name="direction">The new sort direction</param>
        public virtual void ChangeSort(string sortPropertyName, ListSortDirection direction)
        {
            lock (lockObject)
            {
                sorted = true;
                this.direction = direction;

                var p = typeof(TValue).GetProperty(sortPropertyName);
                if (p == null) throw new KeyNotFoundException("sortPropertyName not found in " + typeof(TValue).FullName);

                comparer = MakeComparison(p);
                if (comparer == null) throw new NotSupportedException("Cannot sort on a key that is not IComparable.");

                if (Count > 0) Sort();
            }
        }

        /// <summary>
        /// Clear and reset the sorting enforcement
        /// </summary>
        public virtual void ClearSort()
        {
            sorted = false;
            comparer = null;
            direction = ListSortDirection.Ascending;
        }

        protected virtual void Sort()
        {
            if (!sorted) return;

            lock (lockObject)
            {
                var oldno = noevent;
                noevent = true;

                var values = ToArray();

                Clear();
                AddRange(values);

                noevent = oldno;
                if (!noevent) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

#endregion

#region Comparison 

        protected virtual Comparison<TValue> MakeComparison()
        {
            if (typeof(IComparable<TValue>).IsAssignableFrom(typeof(TValue)))
            {
                if (direction == ListSortDirection.Ascending)
                {
                    return new Comparison<TValue>((a, b) =>
                    {
                        if (!(a is object) && !(b is object))
                        {
                            return 0;
                        }
                        else if (!(a is object))
                        {
                            return 1;
                        }
                        else if (!(b is object))
                        {
                            return -1;
                        }

                        return ((IComparable<TValue>)a).CompareTo(b);
                    });
                }
                else
                {
                    return new Comparison<TValue>((a, b) =>
                    {
                        if (!(a is object) && !(b is object))
                        {
                            return 0;
                        }
                        else if (!(a is object))
                        {
                            return -1;
                        }
                        else if (!(b is object))
                        {
                            return 1;
                        }

                        return -((IComparable<TValue>)a).CompareTo(b);
                    });
                }
            }
            else if (typeof(IComparable).IsAssignableFrom(typeof(TValue)))
            {

                if (direction == ListSortDirection.Ascending)
                {
                    return new Comparison<TValue>((a, b) =>
                    {
                        if (!(a is object) && !(b is object))
                        {
                            return 0;
                        }
                        else if (!(a is object))
                        {
                            return 1;
                        }
                        else if (!(b is object))
                        {
                            return -1;
                        }

                        return ((IComparable)a).CompareTo(b);
                    });

                }
                else
                {
                    return new Comparison<TValue>((a, b) =>
                    {
                        if (!(a is object) && !(b is object))
                        {
                            return 0;
                        }
                        else if (!(a is object))
                        {
                            return -1;
                        }
                        else if (!(b is object))
                        {
                            return 1;
                        }

                        return -((IComparable)a).CompareTo(b);
                    });

                }
            }
            else
            {
                return null;
            }
        }

        protected virtual Comparison<TValue> MakeComparison(PropertyInfo destProp)
        {
            var gt = typeof(IComparable).MakeGenericType(destProp.PropertyType);

            if (gt.IsAssignableFrom(destProp.PropertyType))
            {
                if (direction == ListSortDirection.Ascending)
                {
                    return new Comparison<TValue>((obja, objb) =>
                    {
                        IComparable a, b;

                        a = (IComparable)destProp.GetValue(obja);
                        b = (IComparable)destProp.GetValue(objb);

                        if (!(a is object) && !(b is object))
                        {
                            return 0;
                        }
                        else if (!(a is object))
                        {
                            return 1;
                        }
                        else if (!(b is object))
                        {
                            return -1;
                        }

                        return a.CompareTo(b);

                    });
                }
                else
                {
                    return new Comparison<TValue>((obja, objb) =>
                    {
                        IComparable a, b;

                        a = (IComparable)destProp.GetValue(obja);
                        b = (IComparable)destProp.GetValue(objb);

                        if (!(a is object) && !(b is object))
                        {
                            return 0;
                        }
                        else if (!(a is object))
                        {
                            return -1;
                        }
                        else if (!(b is object))
                        {
                            return 1;
                        }

                        return -a.CompareTo(b);

                    });
                }
            }
            else
            {
                return null;
            }

        }

        protected virtual Comparison<TValue> MakeComparison(IComparer<TValue> comp)
        {
            if (direction == ListSortDirection.Ascending)
            {
                return new Comparison<TValue>((a, b) =>
                {
                    return comp.Compare(a, b);
                });

            }
            else
            {
                return new Comparison<TValue>((a, b) =>
                {
                    return -comp.Compare(a, b);
                });

            }
            
        }

        protected virtual int GetInsertIndex(TValue item, bool mustExist = false)
        {
            if (Count == 0) return 0;

            int hi = Count - 1;
            int lo = 0;
            int mid;

            var l = this as IList<TValue>;
            var origitem = item;

            TValue curritem;

            if (direction == ListSortDirection.Ascending)
            {
                while (true)
                {
                    if (hi < lo)
                    {
                        return mustExist ? -1 : lo;
                    }

                    mid = (hi + lo) / 2;
                    curritem = l[mid];

                    if (comparer(origitem, curritem) > 0)
                    {
                        lo = mid + 1;
                    }
                    else if (comparer(origitem, curritem) < 0)
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
                        return mustExist ? -1 : lo;
                    }

                    mid = (hi + lo) / 2;
                    curritem = l[mid];

                    if (comparer(origitem, curritem) < 0)
                    {
                        lo = mid + 1;
                    }
                    else if (comparer(origitem, curritem) > 0)
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

#endregion
    }


}
