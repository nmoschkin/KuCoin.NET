
using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kucoin.NET.Observable
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
    public class ObservableDictionary<TKey, TValue> : ObservableBase, INotifyCollectionChanged, IDictionary<TKey, TValue>, IList<TValue>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected ObservableCollection<TValue> innerCollection = new ObservableCollection<TValue>();

        protected List<TKey> innerKeys = new List<TKey>();

        protected string id = Guid.NewGuid().ToString("d");

        protected string keyProperty = null;

        protected PropertyInfo keyPropInfo = null;

        protected DateTime lastChanged = DateTime.Now;

        /// <summary>
        /// True if there is a KeyProperty.
        /// </summary>
        [JsonIgnore]
        public bool HasKeyProperty
        {
            get => keyProperty != null;
        }

        /// <summary>
        /// The name of the KeyProperty.
        /// </summary>
        [JsonIgnore]
        public string KeyProperty => keyProperty;

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
        /// Gets the timestamp for the last change of the values in this dictionary.
        /// </summary>
        [JsonProperty("lastChanged")]
        public DateTime LastChanged
        {
            get => lastChanged;
            protected set
            {
                SetProperty(ref lastChanged, value);
            }
        }

        /// <summary>
        /// Create a new observable dictionary.
        /// </summary>
        public ObservableDictionary()
        {
            innerCollection.CollectionChanged += InnerCollection_CollectionChanged;
        }

        /// <summary>
        /// Create a new observable dictionary.
        /// </summary>
        /// <param name="items">A sequence of <see cref="KeyValuePair{TKey, TValue}"/> objects.</param>
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items) : this()
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Creates a new observable dictionary from the specified items.
        /// </summary>
        /// <remarks>
        /// The items must be of a class type, and must be decorated with the <see cref="KeyPropertyAttribute"/> attribute.
        /// </remarks>
        /// <param name="items"></param>
        public ObservableDictionary(IEnumerable<TValue> items)
        {
            var sample = items.FirstOrDefault();

            if (sample.GetType().IsClass == false)
            {
                throw new InvalidCastException("Values in collection must be class objects.");
            }

            var attr = FindKeyPropertyAttr(sample.GetType());

            if (attr != null)
            {
                var pi = attr.GetProperty(sample);

                if (!typeof(TKey).IsAssignableFrom(pi.PropertyType)) throw new InvalidCastException($"{pi.PropertyType.FullName} is an invalid type for key.");

                keyProperty = attr.PropertyName;
                keyPropInfo = pi;

                foreach (var item in items)
                {
                    Add((TKey)pi.GetValue(item), item);
                }
            }
            else
            {
                throw new MissingMemberException("Cannot determine the key property for this collection of objects.  Try calling a different constructor, or use the KeyPropertyAttribute.");
            }

        }

        /// <summary>
        /// Create a new observable dictionary of class objects using the specified property as the key.
        /// </summary>
        /// <param name="keyPropertyName">The name of the key property inside the <see cref="TKey"/> class type.</param>
        /// <param name="values">A sequence of items of type <see cref="TValue"/>.</param>
        public ObservableDictionary(string keyPropertyName, IEnumerable<TValue> values) : this()
        {
            var sample = values.FirstOrDefault();
            if (sample == null) throw new NullReferenceException($"{nameof(values)} is empty or the first element is null.");

            var t = sample.GetType();
            if (t.IsValueType) throw new InvalidCastException(t.FullName + " is not a class.");

            PropertyInfo pi = t.GetProperty(keyPropertyName);
            if (pi == null) throw new KeyNotFoundException(keyPropertyName + " does not exist in " + t.FullName);

            if (!typeof(TKey).IsAssignableFrom(pi.PropertyType)) throw new InvalidCastException($"{pi.PropertyType.FullName} is an invalid type for key.");

            keyPropInfo = pi;
            keyProperty = keyPropertyName;

            foreach (var val in values)
            {
                TKey key = (TKey)pi.GetValue(val);
                Add(key, val);
            }

        }

        /// <summary>
        /// Sorts the entire collection by key.  
        /// This will trigger the <see cref="INotifyCollectionChanged.CollectionChanged"/> event.
        /// 
        /// If the comparer is not specified, the default comparer for the type will be used.
        /// If no such comparer exists, an exception will be thrown.
        /// 
        /// This method is thread-safe.
        /// </summary>
        /// <param name="comparer">The <see cref="IComparer{TKey}"/> instance to use to compare items.</param>
        public void SortByKey(IComparer<TKey> comparer)
        {
            SortByKey(new Comparison<TKey>((a, b) => comparer.Compare(a, b)));
        }

        /// <summary>
        /// Sorts the entire collection by key.  
        /// This will trigger the <see cref="INotifyCollectionChanged.CollectionChanged"/> event.
        /// 
        /// If the comparison is not specified, the default comparer for the type will be used.
        /// If no such comparer exists, an exception will be thrown.
        /// 
        /// This method is thread-safe.
        /// </summary>
        /// <param name="comparison">The <see cref="Comparison{TKey}"/> instance to use to compare items.</param>
        public void SortByKey(Comparison<TKey> comparison = null)
        {
            lock (innerCollection)
            {
                lock (innerKeys)
                {
                    Dictionary<TKey, int> map = new Dictionary<TKey, int>();
                    List<TValue> items = new List<TValue>();

                    int i = 0;
                    foreach (var k in innerKeys)
                    {
                        map.Add(k, i++);
                    }

                    if (comparison != null)
                    {
                        innerKeys.Sort(comparison);
                    }
                    else
                    {
                        innerKeys.Sort();
                    }

                    foreach (var k in innerKeys)
                    {
                        items.Add(innerCollection[map[k]]);
                    }

                    innerCollection.CollectionChanged -= InnerCollection_CollectionChanged;

                    innerCollection = new ObservableCollection<TValue>(items);
                    innerCollection.CollectionChanged += InnerCollection_CollectionChanged;
                }
            }

            LastChanged = DateTime.Now;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public TValue this[TKey key]
        {
            get => innerCollection[innerKeys.IndexOf(key)];
            set
            {
                var idx = innerKeys.IndexOf(key);
                TValue oldVal = innerCollection[idx];

                if (SetProperty(ref oldVal, value))
                {
                    innerCollection[idx] = value;
                    LastChanged = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Returns an item by its absolute position in the list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        TValue IList<TValue>.this[int index]
        {
            get => innerCollection[index];
            set
            {
                innerCollection[index] = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="KeyValuePair{TKey, TValue}"/> item at the specified absolute index in the collection.
        /// </summary>
        /// <param name="index">The index of the item to retrieve.</param>
        /// <returns></returns>
        public KeyValuePair<TKey, TValue> GetKeyValuePairAt(int index)
        {
            return new KeyValuePair<TKey, TValue>(innerKeys[index], innerCollection[index]);
        }

        public ICollection<TKey> Keys => innerKeys.ToArray();

        public ICollection<TValue> Values => innerCollection.ToArray();

        public int Count => innerCollection.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            if (innerKeys.Contains(key))
            {
                throw new DuplicateNameException(nameof(KeyValuePair<TKey, TValue>.Key));
            }

            LastChanged = DateTime.Now;
            innerKeys.Add(key);
            innerCollection.Add(value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(TValue item)
        {
            if (!HasKeyProperty)
            {

                var attr = FindKeyPropertyAttr(item.GetType());
                if (attr != null)
                {
                    keyProperty = attr.PropertyName;
                    keyPropInfo = attr.GetProperty(item);
                }
                else
                {
                    throw new InvalidOperationException("No Key Property.");
                }
            }

            Add((TKey)keyPropInfo.GetValue(item), item);
        }

        public void Clear()
        {
            LastChanged = DateTime.Now;
            innerKeys.Clear();
            innerCollection.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var i = innerKeys.IndexOf(item.Key);
            if (i == -1) return false;

            return innerCollection[i].Equals(item.Value);

        }

        public bool ContainsKey(TKey key)
        {
            return innerKeys.Contains(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int i, c = innerCollection.Count;
            int idx = arrayIndex;

            for (i = 0; i < c; i++)
            {
                array[idx++] = new KeyValuePair<TKey, TValue>(innerKeys[i], innerCollection[i]);
            }

        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new ObservableDictionaryEnumerator<TKey, TValue>(this);
        }

        /// <summary>
        /// Returns the absolute index position of the specified item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns></returns>
        public int IndexOf(TValue item)
        {
            return ((IList<TValue>)innerCollection).IndexOf(item);
        }

        public void Insert(int index, TValue item)
        {
            if (!HasKeyProperty)
            {
                throw new InvalidOperationException("No Key Property.");
            }

            TKey key = (TKey)keyPropInfo.GetValue(item);

            innerCollection.Insert(index, item);
            innerKeys.Insert(index, key);
        }

        public bool Remove(TKey key)
        {
            if (!ContainsKey(key)) return false;

            var i = innerKeys.IndexOf(key);

            innerCollection.RemoveAt(i);
            innerKeys.RemoveAt(i);

            LastChanged = DateTime.Now;
            return true;
        }

        public void RemoveAt(int index)
        {
            innerCollection.RemoveAt(index);
            innerKeys.RemoveAt(index);
        }

        public bool Remove(TValue item)
        {
            if (!HasKeyProperty)
            {
                throw new InvalidOperationException("No Key Property.");
            }

            TKey key = (TKey)keyPropInfo.GetValue(item);

            if (ContainsKey(key))
            {
                return Remove(key);
            }
            else
            {
                return false;
            }

        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!ContainsKey(item.Key)) return false;

            var i = innerKeys.IndexOf(item.Key);

            if (innerCollection[i].Equals(item.Value))
            {

                innerCollection.RemoveAt(i);
                innerKeys.RemoveAt(i);

                LastChanged = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!ContainsKey(key))
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
        /// Finds the key property attribute for the specified type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected static KeyPropertyAttribute FindKeyPropertyAttr(Type t)
        {
            var attr = t.GetCustomAttribute<KeyPropertyAttribute>();

            if (attr != null && t.GetProperty(attr.PropertyName) != null)
            {
                return attr;
            }
            else
            {

                var v = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var p in v)
                {
                    attr = p.GetCustomAttribute<KeyPropertyAttribute>();
                    if (attr != null)
                    {
                        return attr;
                    }
                }
            }

            return null;
        }

        private void InnerCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TValue>)innerCollection).GetEnumerator();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return ((IEnumerable<TValue>)innerCollection).GetEnumerator();
        }

        public bool Contains(TValue item)
        {
            return ((ICollection<TValue>)innerCollection).Contains(item);
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            ((ICollection<TValue>)innerCollection).CopyTo(array, arrayIndex);
        }


    }

    public class ObservableDictionaryEnumerator<T, U> : IEnumerator<KeyValuePair<T, U>>
    {
        private ObservableDictionary<T, U> source;
        private int idx = -1;
        private int count = 0;

        public ObservableDictionaryEnumerator(ObservableDictionary<T, U> source)
        {
            this.source = source;
            count = source.Count;
        }

        public KeyValuePair<T, U> Current => source.GetKeyValuePairAt(idx);

        object IEnumerator.Current => source.GetKeyValuePairAt(idx);

        public void Dispose()
        {
            source = null;
            idx = -1;
        }

        public bool MoveNext()
        {
            return ++idx < count;
        }

        public void Reset()
        {
            idx = -1;
        }
    }


}
