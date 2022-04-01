

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

//using DataTools.Text;

namespace KuCoin.NET.Data.Market 
{ 
    public enum SortOrder
    {
        Ascending,
        Descending,
    }

    public enum RebalanceResult
    {
        NotPerformed,
        Unchanged,
        Changed
    }

    /// <summary>
    /// A sorted, spatially buffered collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection (must be a class)</typeparam>
    /// <remarks>
    /// Items cannot be <see cref="null"/>.  Null is reserved for buffer space.
    /// </remarks>
    public class RedBlackTree<T> : ICollection<T>
    {
        protected SortOrder sortOrder;
        protected int count = 0;
        protected Comparison<T> comp;
        protected IComparer<T> comparer;
        protected List<T> items;
        protected object syncRoot = new object();
        protected T[] arrspace;
        protected int m;

        /// <summary>
        /// Gets the sort order for the current instance.
        /// </summary>
        public SortOrder SortOrder => sortOrder;

        /// <summary>
        /// Gets the total number of actual elements.
        /// </summary>
        public int Count => count;

        public bool IsReadOnly { get; } = false;

        public T First
        {
            get => count == 0 ? default : items[0];
        }

        public T Last
        {
            get
            {
                if (count == 0) return default;
                if (items[count - 1] is object)
                {
                    return items[count - 1];
                }
                else
                {
                    return items[count - 2];
                }
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <param name="space">The number of total new elements to insert for each single new element inserted.</param>
        /// <param name="comparer">The comparer class.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RedBlackTree(IComparer<T> comparer, SortOrder sortOrder) : base()
        {
            items = new List<T>();

            arrspace = new T[2];
            this.sortOrder = sortOrder;
            if (sortOrder == SortOrder.Ascending)
            {
                m = 1;
            }
            else
            {
                m = -1;
            }

            if (comparer == null)
            {
                typeof(T).GetInterfaceMap(typeof(IComparable<T>));

                comp = new Comparison<T>((x, y) =>
                {
                    if (x is IComparable<T> a && y is T b)
                    {
                        return a.CompareTo(b);
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                });
            }
            else
            {
                this.comparer = comparer;

                comp = new Comparison<T>((x, y) =>
                {
                    if (x is object && y is object)
                    {
                        return this.comparer.Compare(x, y);
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                });

            }

        }

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RedBlackTree() : this(SortOrder.Ascending) { }

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <param name="sortOrder">The sort order.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RedBlackTree(SortOrder sortOrder) : this((IComparer<T>)null, sortOrder)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <param name="comparer">The comparer class.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RedBlackTree(IComparer<T> comparer) : this(comparer, SortOrder.Ascending)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <param name="space">The number of total new elements to insert for each single new element inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RedBlackTree(byte space) : this((IComparer<T>)null, SortOrder.Ascending)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <param name="initialItems">The initial items used to populate the collection.</param>
        /// <param name="comparer">The comparer class.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RedBlackTree(IEnumerable<T> initialItems, IComparer<T> comparer, SortOrder sortOrder) : this(comparer, sortOrder)
        {
            AddRange(initialItems);
        }

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <param name="initialItems">The initial items used to populate the collection.</param>
        /// <param name="comparer">The comparer class.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RedBlackTree(IEnumerable<T> initialItems, IComparer<T> comparer) : this(comparer, SortOrder.Ascending)
        {
            AddRange(initialItems);
        }

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <param name="initialItems">The initial items used to populate the collection.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RedBlackTree(IEnumerable<T> initialItems, SortOrder sortOrder) : this((IComparer<T>)null, sortOrder)
        {
            AddRange(initialItems);
        }


        /// <summary>
        /// Adds multiple items to the <see cref="RedBlackTree{T}"/> at once.
        /// </summary>
        /// <param name="newItems"></param>
        public void AddRange(IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
            {
                Add(item);
            }
        }
        public bool Locate(T item)
        {
            int idx = Walk(item, TreeWalkMode.Locate);

            if (idx >= items.Count || idx < 0) return false;
            if (!items[idx].Equals(item)) return false;

            return true;
        }

        public void AlterItem(T item, Func<T, T> alteration)
        {
            lock (syncRoot)
            {
                int idx = Walk(item, TreeWalkMode.Locate);

                if (idx >= items.Count || idx < 0) throw new KeyNotFoundException();
                if (!items[idx].Equals(item)) throw new KeyNotFoundException();

                RemoveItem(idx);

                var newitem = alteration(item);
                InsertItem(newitem);
                //int idx2 = Walk(newitem);





                //if (idx == idx2) return;
                //if (idx2 >= count)
                //{
                //    RemoveItem(idx);
                //    Add(newitem);

                //    return;
                //}

                //bool black1 = (idx & 1) == 0;
                //bool black2 = (idx2 & 1) == 0;

                //if (black1 && items[idx + 1] is object)
                //{
                //    items[idx] = items[idx + 1];
                //    items[idx + 1] = default;
                //}
                //else
                //{
                //    items[idx] = default;
                //    walker.BalanceTree(idx);
                //}

                //if (!black2 && !(items[idx2] is object))
                //{
                //    items[idx2] = newitem;
                //}
                //else
                //{
                //    InsertItem(newitem);
                //}

            }
        }

        /// <summary>
        /// Clear the collection.
        /// </summary>
        protected virtual void ClearItems()
        {
            lock (syncRoot)
            {
                items.Clear();
                count = 0;
            }
        }


        public void Add(T item)
        {
            InsertItem(item);
        }

        public void Clear()
        {
            ClearItems();
        }

        public bool Contains(T item)
        {
            lock (syncRoot)
            {
                return items.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (syncRoot)
            {
                foreach (var item in items)
                {
                    if (!(item is object)) continue;
                    array[arrayIndex++] = item;
                }
            }
        }

        /// <summary>
        /// Copies <paramref name="count"/> elements of the <see cref="RedBlackTree{T}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <param name="count"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            lock (syncRoot)
            {
                if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));

                int c = 0;

                foreach (var item in items)
                {
                    if (!(item is object)) continue;

                    array[arrayIndex++] = item;
                    c++;

                    if (c == count) return;
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="Array"/> of the items in this <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <returns>A new <see cref="Array"/> with all the actual items.</returns>
        public T[] ToArray()
        {
            lock (syncRoot)
            {
                var l = new List<T>();

                foreach (var item in items)
                {
                    if (item != null)
                    {
                        l.Add(item);
                    }
                }

                return l.ToArray();
            }
        }

        public T[] ToArray(int elementCount)
        {
            lock (syncRoot)
            {
                var l = new List<T>();
                int x = 0;

                foreach (var item in items)
                {
                    if (item != null)
                    {
                        l.Add(item);

                        x++;
                        if (x == elementCount) break;
                    }
                }

                return l.ToArray();
            }
        }


        public bool Remove(T item)
        {
            lock (syncRoot)
            {
                var idx = Walk(item, TreeWalkMode.Locate);
                if (idx >= count || idx < 0) return false;

                if (items[idx] is object && items[idx].Equals(item))
                {
                    RemoveItem(idx);
                    return true;
                }
                return false;
            }
        }

        #region Tree


        int hardInserts = 0;
        int softInserts = 0;

        int hardRemoves = 0;
        int softRemoves = 0;

        int sixteened = 0;

        int changedRebalances = 0;

        int unchangedRebalances = 0;

        bool metrics = true;

        public int ChangedRebalances => changedRebalances;
        public int UnchangedRebalances => unchangedRebalances;

        public int SixteenOpt => sixteened;

        public int HardRemoves => hardRemoves;

        public int SoftRemoves => softRemoves;

        public int HardInserts => hardInserts;

        public int SoftInserts => softInserts;

        public int TreeSize => items.Count;

        public bool EnableMetrics
        {
            get => metrics;
            set
            {
                if (metrics == value) return;

                lock (syncRoot)
                {
                    metrics = value;
                    ResetMetrics();
                }
            }
        }

        public void ResetMetrics()
        {
            lock (syncRoot)
            {
                hardInserts = 0;
                softInserts = 0;

                hardRemoves = 0;
                softRemoves = 0;

                sixteened = 0;
                changedRebalances = 0;
                unchangedRebalances = 0;

            }
        }

        /// <summary>
        /// Insert an item into the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <exception cref="ArgumentNullException" />
        protected virtual void InsertItem(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            lock (syncRoot)
            {
                var index = Walk(item);
                int rc = items.Count;

                if (index < rc && items[index] == null)
                {
                    items[index] = item;
                    if (metrics) softInserts++;
                }
                else if (index > 0 && items[index - 1] == null)
                {
                    items[index - 1] = item;
                    if (metrics) softInserts++;
                }
                else if (index < rc - 2 && items[index + 2] == null)
                {
                    items[index + 2] = items[index + 1];
                    items[index + 1] = items[index];
                    items[index] = item;

                    if (metrics) softInserts++;
                }
                else
                {
                    if ((index & 1) == 0)
                    {
                        arrspace[0] = item;
                        arrspace[1] = default;

                    }
                    else
                    {
                        arrspace[0] = default;
                        arrspace[1] = item;
                    }

                    items.InsertRange(index, arrspace);
                    if (metrics) hardInserts++;
                }

                count++;
            }
        }

        /// <summary>
        /// Remove an item from the collection.
        /// </summary>
        /// <param name="index"></param>
        protected virtual void RemoveItem(int index)
        {
            lock (syncRoot)
            {
                items[index] = default;
                count--;

                if ((index & 1) == 0)
                {
                    if (items[index + 1] is object)
                    {
                        items[index] = items[index + 1];
                        items[index + 1] = default;

                        if (metrics) softRemoves++;
                    }
                    else if (index < items.Count - 3 && items[index + 2] is object && items[index + 3] is object)
                    {
                        items[index] = items[index + 2];
                        items[index + 2] = items[index + 3];
                        items[index + 3] = default;

                        if (metrics) softRemoves++;
                    }
                    else
                    {
                        items.RemoveRange(index, 2);
                        if (metrics) hardRemoves++;
                    }
                }
                else
                {
                    if (metrics) softRemoves++;

                    if (Rebalance() == RebalanceResult.NotPerformed)
                    {
                        CheckThem(index);
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to Rebalance The Tree
        /// </summary>
        /// <param name="threshold">The ratio of the tree size over the logical count at which a rebalance should be performed. Default is 1.2 : 1</param>
        /// <returns>A <see cref="RebalanceResult"/> of <see cref="RebalanceResult.NotPerformed"/>, <see cref="RebalanceResult.Unchanged"/>, or <see cref="RebalanceResult.Changed"/>.</returns>
        public RebalanceResult Rebalance(float threshold = 1.2f)
        {
            if (count > 1024 && ((float)items.Count / count) >= threshold)
            {
                bool b = false;

                for (int i = items.Count - 2; i >= 2; i -= 2)
                {
                    b = b | CheckThem(i, 4, true);
                }

                if (b && metrics)
                {
                    changedRebalances++;
                    return RebalanceResult.Changed;
                }
                else
                {
                    unchangedRebalances++;
                    return RebalanceResult.Unchanged;
                }
            }

            return RebalanceResult.NotPerformed;
        }

        protected bool CheckThem(int index, int cadence = 16, bool rebalancing = false)
        {
            if (cadence == 16)
            {
                if ((index & 1) == 1) index--;

                if (index + 8 > items.Count) return false;
                if (index - 8 < 0) return false;

                index -= 8;

                if (
                    items[index] is object && !(items[index + 1] is object)
                    && items[index + 2] is object && !(items[index + 3] is object)
                    && items[index + 4] is object && !(items[index + 5] is object)
                    && items[index + 6] is object && !(items[index + 7] is object)
                    && items[index + 8] is object && !(items[index + 9] is object)
                    && items[index + 10] is object && !(items[index + 11] is object)
                    && items[index + 12] is object && !(items[index + 13] is object)
                    && items[index + 14] is object && !(items[index + 15] is object)
                    )
                {
                    items[index + 1] = items[index + 2];
                    items[index + 2] = items[index + 4];
                    items[index + 3] = items[index + 6];
                    items[index + 4] = items[index + 8];
                    items[index + 5] = items[index + 10];
                    items[index + 6] = items[index + 12];
                    items[index + 7] = items[index + 14];

                    items.RemoveRange(index + 8, 8);

                    if (metrics && !rebalancing)
                    {
                        softRemoves--;
                        sixteened++;
                        hardRemoves++;
                    }

                    return true;
                }
            }
            else if (cadence == 8)
            {
                if ((index & 1) == 1) index--;

                if (index + 4 > items.Count) return false;
                if (index - 4 < 0) return false;

                index -= 4;

                if (
                    items[index] is object && !(items[index + 1] is object)
                    && items[index + 2] is object && !(items[index + 3] is object)
                    && items[index + 4] is object && !(items[index + 5] is object)
                    && items[index + 6] is object && !(items[index + 7] is object)
                    )
                {
                    items[index + 1] = items[index + 2];
                    items[index + 2] = items[index + 4];
                    items[index + 3] = items[index + 6];

                    items.RemoveRange(index + 4, 4);

                    if (metrics && !rebalancing)
                    {
                        softRemoves--;
                        sixteened++;
                        hardRemoves++;
                    }

                    return true;
                }

            }
            else if (cadence == 4)
            {
                if ((index & 1) == 1) index--;

                if (index + 2 > items.Count) return false;
                if (index - 2 < 0) return false;

                index -= 2;

                if (
                    items[index] is object && !(items[index + 1] is object)
                    && items[index + 2] is object && !(items[index + 3] is object)
                    )
                {
                    items[index + 1] = items[index + 2];
                    items.RemoveRange(index + 2, 2);

                    if (metrics && !rebalancing)
                    {
                        softRemoves--;
                        sixteened++;
                        hardRemoves++;
                    }

                    return true;
                }
            }

            return false;
        }

        protected virtual int Walk(T item1, TreeWalkMode walkMode = TreeWalkMode.InsertIndex)
        {
            int count = items.Count;
            int lo = 0;
            int hi = count - 1;
            int mid = 0;

            T item2, item3;
            int r;

            while (true)
            {
                if (hi < lo)
                {
                    if (walkMode == TreeWalkMode.InsertIndex && (lo & 1) == 0)
                    {
                        if (lo < count - 1 && !(items[lo + 1] is object))
                        {
                            r = comp(item1, items[lo]) * m;
                            if (r >= 0) lo++;
                        }

                        else if (lo > 0 && !(items[lo - 1] is object))
                        {
                            if (lo < count)
                            {
                                r = comp(item1, items[lo]) * m;
                                if (r <= 0) lo--;
                            }
                            else
                            {
                                lo--;
                            }
                        }
                    }

                    return lo;
                }

                mid = (hi + lo) / 2;

                if (((mid & 1)) == 1) mid--;

                item2 = items[mid];
                item3 = items[mid + 1];

                r = comp(item1, item2) * m;

                if (r > 0)
                {
                    if (item3 is object)
                    {
                        r = comp(item1, item3) * m;

                        if (r <= 0)
                        {
                            return mid + 1;
                        }
                    }

                    lo = mid + 2;
                }
                else if (r < 0)
                {
                    hi = mid - 2;
                }
                else
                {
                    lo = mid;
                    hi = lo - 2;
                }
            }

        }

        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            return new RedBlackTreeEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RedBlackTreeEnumerator(this);
        }

        /// <summary>
        /// <see cref="RedBlackTree{T}"/> enumerator.
        /// </summary>
        public class RedBlackTreeEnumerator : IEnumerator<T>
        {
            RedBlackTree<T> collection;
            T current = default;

            int idx = -1;
            int count = 0;
            private object syncRoot;

            public RedBlackTreeEnumerator(RedBlackTree<T> collection)
            {
                this.collection = collection;
                count = collection.items.Count;
                syncRoot = collection.syncRoot;
            }

            public T Current => current;
            object IEnumerator.Current => current;

            public void Dispose()
            {
                Reset();

                collection = null;
                syncRoot = null;
            }

            public bool MoveNext()
            {
                lock (syncRoot)
                {
                    idx++;
                    while (idx < count)
                    {
                        current = collection.items[idx];
                        if (current is object)
                        {
                            break;
                        }
                        idx++;
                    }

                    return idx < count;
                }
            }

            public void Reset()
            {
                lock (syncRoot)
                {
                    idx = -1;
                    current = default;
                }
            }
        }
    }
    public enum TreeWalkMode
    {
        InsertIndex,
        Locate
    }

    public class TreeUnbalancedException : Exception
    {
        public TreeUnbalancedException() : base()
        {

        }
        public TreeUnbalancedException(string message) : base(message)
        {
        }

    }

    public abstract class KeyedRedBlackTree<TKey, TValue> : RedBlackTree<TValue> //, IReadOnlyDictionary<TKey, TValue>
    {
        protected SortedDictionary<TKey, TValue> keyDict = new SortedDictionary<TKey, TValue>();

        public TValue this[TKey key] => keyDict[key];

        public IEnumerable<TKey> Keys => keyDict.Keys;

        public IEnumerable<TValue> Values => keyDict.Values;

        protected abstract TKey ProvideKey(TValue value);

        public KeyedRedBlackTree(SortOrder sortOrder) : base(sortOrder)
        {
        }

        public KeyedRedBlackTree(IComparer<TValue> comparer, SortOrder sortOrder) : base(comparer, sortOrder)
        {
        }

        protected override void RemoveItem(int index)
        {
            lock (syncRoot)
            {
                var item = items[index];
                keyDict.Remove(ProvideKey(item));
                base.RemoveItem(index);
            }
        }

        protected override void InsertItem(TValue item)
        {
            lock (syncRoot)
            {
                keyDict.Add(ProvideKey(item), item);
                base.InsertItem(item);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (syncRoot)
            {
                return keyDict.ContainsKey(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (syncRoot)
            {
                return keyDict.TryGetValue(key, out value);
            }
        }
    }
    public class KeyedBook<TUnit> : KeyedRedBlackTree<string, TUnit> where TUnit : IAtomicOrderUnit, new()
    {
        protected override string ProvideKey(TUnit value)
        {
            return value.OrderId;
        }

        public void Remove(string key)
        {
            lock (syncRoot)
            {
                if (keyDict.ContainsKey(key))
                {
                    var item = keyDict[key];

                    keyDict.Remove(key);
                    Remove(item);
                }
            }
        }

        public KeyedBook(SortOrder sortOrder) : base(new AtomicComparer<TUnit>(sortOrder == SortOrder.Descending), sortOrder)
        {
        }

        public KeyedBook() : base(new AtomicComparer<TUnit>(false), SortOrder.Ascending)
        {
        }

        public KeyedBook(bool descending) : base(new AtomicComparer<TUnit>(false), descending ? SortOrder.Descending : SortOrder.Ascending)
        {
        }

    }

}
