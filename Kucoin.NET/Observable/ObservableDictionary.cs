
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
    public class ObservableDictionary<TKey, TValue> : KeyedCollection<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected string id = Guid.NewGuid().ToString("d");

        protected string keyProperty = null;

        protected PropertyInfo keyPropInfo = null;

        protected DateTime lastChanged = DateTime.Now;

        protected ListSortDirection direction;

        protected Comparer<TValue> comparer;

        protected bool sorted;
        
        int GetInsertIndex(TValue item)
        {
            if (Count == 0) return 0;

            int hi = Count - 1;
            int lo = 0;
            int mid; 
            
            var l = this as IList<TValue>;
            var uprice = item;
            
            TValue cprice;

            if (direction == ListSortDirection.Ascending)
            {
                while (true)
                {
                    if (hi < lo)
                    {
                        return lo;
                    }

                    mid = (hi + lo) / 2;
                    cprice = l[mid];

                    if (comparer.Compare(uprice, cprice) > 0)
                    {
                        lo = mid + 1;
                    }
                    else if (comparer.Compare(uprice, cprice) < 0)
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
                    cprice = l[mid];

                    if (comparer.Compare(uprice, cprice) < 0)
                    {
                        lo = mid + 1;
                    }
                    else if (comparer.Compare(uprice, cprice) > 0)
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
        public ObservableDictionary() : base()
        {
            var attr = FindKeyPropertyAttr(typeof(TValue));

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
            var sample = items.FirstOrDefault();

            if (sample.GetType().IsClass == false)
            {
                throw new InvalidCastException("Values in collection must be class objects.");
            }

            AddRange(items);
        }

        /// <summary>
        /// Create a new observable dictionary of class objects using the specified property as the key.
        /// </summary>
        /// <param name="keyPropertyName">The name of the key property inside the <see cref="TKey"/> class type.</param>
        /// <param name="values">A sequence of items of type <see cref="TValue"/>.</param>
        public ObservableDictionary(string keyPropertyName, IEnumerable<TValue> values) : base()
        {
            var prop = typeof(TValue).GetProperty(keyPropertyName);

            if (prop == null) throw new MissingMemberException("Cannot determine the key property for this collection of objects.  Try calling a different constructor, or use the KeyPropertyAttribute.");
            if (!typeof(TKey).IsAssignableFrom(prop.PropertyType)) throw new InvalidCastException($"{prop.PropertyType.FullName} is an invalid type for key.");

            keyPropInfo = prop;
            keyProperty = prop.Name;

            AddRange(values);
        }

        public void AddRange(IEnumerable<TValue> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
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

        protected Comparison<TValue> MakeComparison(IComparer<TValue> comp = null)
        {
            if (comp != null)
            {
                return new Comparison<TValue>((a, b) =>
                {
                    return comp.Compare(a, b);
                });
            }
            else if (typeof(IComparable<TValue>).IsAssignableFrom(typeof(TValue)))
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
            else if (typeof(IComparable).IsAssignableFrom(typeof(TValue))) 
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
                return null;
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

        protected override TKey GetKeyForItem(TValue item)
        {
            return (TKey)keyPropInfo.GetValue(item);
        }


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

    }


}
