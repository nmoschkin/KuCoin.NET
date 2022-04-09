
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

namespace KuCoin.NET.Helpers
{
    public interface IKeyProvider<T>
    {
        T Key { get; }
    }

    public class KeyProvidedRedBlackTree<TKey, TValue> : ExperimentalKeyedRedBlackTree<TKey, TValue> where TValue : IKeyProvider<TKey>
    {
        protected override TKey ProvideKey(TValue item)
        {
            return item.Key;
        }
    
    } 

    public abstract class ExperimentalKeyedRedBlackTree<TKey, TValue> : ComparingBase<TKey>, ICollection<TValue>
    {
        #region Protected Fields

        protected TValue[] arrspace;
        protected TKey[] keyspace;

        protected RebalanceStrategy globalStrategy = RebalanceStrategy.Cadance4;
        protected RebalanceStrategy localStrategy = RebalanceStrategy.Cadence16;
        protected float rebalanceThreshold = 1.2f;
        protected object syncRoot = new object();

        #endregion Protected Fields

        #region Private Fields

        private List<TValue> items;
        private List<TKey> keys;


        private int count = 0;
        private int treeSize = 0;

        int changedRebalances = 0;

        int hardInserts = 0;

        int hardRemoves = 0;

        bool metrics = false;

        int localRebalances = 0;

        int softInserts = 0;

        int softRemoves = 0;

        int unchangedRebalances = 0;

        float averageInsertIndex = 0f;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <param name="space">The number of total new elements to insert for each single new element inserted.</param>
        /// <param name="comparer">The comparer class.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ExperimentalKeyedRedBlackTree(IComparer<TKey> comparer, float threshold = 1.2f, RebalanceStrategy globStrategy = RebalanceStrategy.Cadance4, RebalanceStrategy locStrategy = RebalanceStrategy.Cadence16) : base(comparer)
        {
            rebalanceThreshold = threshold;
            globalStrategy = globStrategy;
            localStrategy = locStrategy;

            items = new List<TValue>();

            arrspace = new TValue[2];
            keyspace = new TKey[2];

        }

        /// <summary>
        /// Creates a new instance of <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <param name="sortOrder">The sort order.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ExperimentalKeyedRedBlackTree(float threshold = 1.2f, RebalanceStrategy globStrategy = RebalanceStrategy.Cadance4, RebalanceStrategy locStrategy = RebalanceStrategy.Cadence16) : this((IComparer<TKey>)null, threshold, globStrategy, locStrategy)
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
        public ExperimentalKeyedRedBlackTree(IEnumerable<TValue> initialItems, IComparer<TKey> comparer, float threshold = 1.2f, RebalanceStrategy globStrategy = RebalanceStrategy.Cadance4, RebalanceStrategy locStrategy = RebalanceStrategy.Cadence16) : this(comparer, threshold, globStrategy, locStrategy)
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
        public ExperimentalKeyedRedBlackTree(IEnumerable<TValue> initialItems, float threshold = 1.2f, RebalanceStrategy globStrategy = RebalanceStrategy.Cadance4, RebalanceStrategy locStrategy = RebalanceStrategy.Cadence16) : this((IComparer<TKey>)null, threshold, globStrategy, locStrategy)
        {
            AddRange(initialItems);
        }

        #endregion Public Constructors

        #region Public Properties


        /// <summary>
        /// Gets or sets the capacity of the internal list.
        /// </summary>
        /// <remarks>
        /// If you know you are about to import a large amount of data at once, it might be a good idea to set this to a high value.
        /// </remarks>
        public int Capacity
        {
            get => items.Capacity;
            set => items.Capacity = value;
        }

        /// <summary>
        /// Gets or sets a value that determines whether metrics are recorded.
        /// </summary>
        /// <remarks>
        /// Setting this value will reset all metrics to 0.
        /// </remarks>
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

        /// <summary>
        /// (Metrics) The average insert index.
        /// </summary>
        public float AverageInsertIndex => averageInsertIndex;

        /// <summary>
        /// (Metrics) Number of inserts performed by resizing the tree.
        /// </summary>
        public int HardInserts => hardInserts;

        /// <summary>
        /// (Metrics) Number of removes performed by resizing the tree.
        /// </summary>
        public int HardRemoves => hardRemoves;

        /// <summary>
        /// (Metrics) Number of local rebalances.
        /// </summary>
        public int LocalRebalances => localRebalances;

        /// <summary>
        /// (Metrics) Number of inserts performed without resizing the tree.
        /// </summary>
        public int SoftInserts => softInserts;

        /// <summary>
        /// (Metrics) Number of removes performed without resizing the tree.
        /// </summary>
        public int SoftRemoves => softRemoves;

        /// <summary>
        /// (Metrics) Number of global rebalances that resulted in changes to the tree.
        /// </summary>
        public int ChangedRebalances => changedRebalances;

        /// <summary>
        /// (Metrics) Number of global rebalances that were attempted but resulted in no changes to the tree.
        /// </summary>
        public int UnchangedRebalances => unchangedRebalances;


        /// <summary>
        /// Gets the total number of actual elements.
        /// </summary>
        public int Count => count;

        /// <summary>
        /// Gets the actual size of the tree.
        /// </summary>
        public int TreeSize => treeSize;

        public bool IsReadOnly { get; } = false;

        /// <summary>
        /// Gets the first key element in the sorted collection.
        /// </summary>
        public TKey FirstKey
        {
            get => count == 0 ? default : keys[0];
        }

        public TValue this[TKey key]
        {
            get
            {
                var x = Walk(key, TreeWalkMode.Locate);
                if (x != -1)
                {
                    return items[x];
                }

                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Gets the last key element in the sorted collection.
        /// </summary>
        public TKey LastKey
        {
            get
            {
                var ic = treeSize - 1;
                if (ic == -1) return default;
                if (keys[ic] is object) return keys[ic];
                else return keys[ic - 1];
            }
        }

        /// <summary>
        /// Gets the first item element in the sorted collection.
        /// </summary>
        public TValue FirstItem
        {
            get => count == 0 ? default : items[0];
        }

        /// <summary>
        /// Gets the last item element in the sorted collection.
        /// </summary>
        public TValue LastItem
        {
            get
            {
                var ic = treeSize - 1;
                if (ic == -1) return default;
                if (items[ic] is object) return items[ic];
                else return items[ic - 1];
            }
        }

        /// <summary>
        /// The rebalance strategy to use when performing a global rebalance.
        /// </summary>
        public virtual RebalanceStrategy GlobalStrategy
        {
            get => globalStrategy;
            set
            {
                lock (syncRoot)
                {
                    globalStrategy = value;
                }
            }
        }

        /// <summary>
        /// The rebalance strategy to use when performing a local rebalance.
        /// </summary>
        public virtual RebalanceStrategy LocalStrategy
        {
            get => localStrategy;
            set
            {
                lock (syncRoot)
                {
                    localStrategy = value;
                }
            }
        }

        /// <summary>
        /// The size tolerance difference between the logical size and the tree size before a global rebalance is triggered.
        /// </summary>
        /// <remarks>
        /// Must be a value between 1 ans 2.<br/><br/>
        /// The default value is 1.2f.<br/><br/>
        /// Setting this value below 1.2f is not recommended.
        /// </remarks>
        public virtual float RebalanceThreshold
        {
            get => rebalanceThreshold;
            set
            {
                if (value < 1f || value > 2f) throw new ArgumentOutOfRangeException();

                lock (syncRoot)
                {
                    rebalanceThreshold = value;
                }
            }
        }


        #endregion Public Properties

        #region Public Methods


        public void Add(TValue item)
        {
            InsertItem(item);
        }

        /// <summary>
        /// Adds multiple items to the <see cref="RedBlackTree{T}"/> at once.
        /// </summary>
        /// <param name="newItems"></param>
        public void AddRange(IEnumerable<TValue> newItems)
        {
            foreach (var item in newItems)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Alter an item.
        /// </summary>
        /// <param name="item">The item to alter.</param>
        /// <param name="alteration">The alteration function that returns the changed item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlterItem(TValue item, Func<TValue, TValue> alteration)
        {
            lock (syncRoot)
            {
                int idx = Walk(ProvideKey(item), TreeWalkMode.Locate);
                if (idx == -1)
                {
                    int c = treeSize;
                    string err = $"{idx} for {item} Is Incorrect!";

                    Console.WriteLine(err);

                    for (int i = 0; i < c; i++)
                    {
                        if (items[i] is object && items[i].Equals(item))
                        {
                            err += $"\r\n{i} is the correct index for {item}!";
                            Console.WriteLine($"{i} is the correct index for {item}!");
                            break;
                        }
                    }

                    if (err == null) err += "\r\nKey Not Found!";
                    throw new KeyNotFoundException(err);
                }

                AlterItem(item, alteration, idx);
            }
        }

        public void Clear()
        {
            ClearItems();
        }

        public bool ContainsKey(TKey key)
        {
            lock (syncRoot)
            {
                return Walk(key, TreeWalkMode.Locate) != -1;
            }
        }

        public bool Contains(TValue key)
        {
            lock (syncRoot)
            {
                return Walk(ProvideKey(key), TreeWalkMode.Locate) != -1;
            }
        }

        public void CopyTo(TValue[] array, int arrayIndex)
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
        public void CopyTo(TValue[] array, int arrayIndex, int count)
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

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var item in items)
            {
                if (item is object) yield return item;
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(TValue item)
        {
            lock (syncRoot)
            {
                var idx = Walk(ProvideKey(item), TreeWalkMode.Locate);
                if (idx == -1) return false;

                RemoveItem(idx);
                return true;
            }
        }

        /// <summary>
        /// Reset all metrics to 0.
        /// </summary>
        public void ResetMetrics()
        {
            lock (syncRoot)
            {
                hardInserts = 0;
                softInserts = 0;

                hardRemoves = 0;
                softRemoves = 0;

                localRebalances = 0;

                changedRebalances = 0;
                unchangedRebalances = 0;

                averageInsertIndex = 0f;
            }
        }

        /// <summary>
        /// Return a new <see cref="Array"/> of the items in this <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <returns>A new <see cref="Array"/>.</returns>
        public TValue[] ToArray()
        {
            lock (syncRoot)
            {
                var l = new List<TValue>();

                foreach (var item in this)
                {
                    l.Add(item);
                }

                return l.ToArray();
            }
        }

        /// <summary>
        /// Return a new <see cref="Array"/> of at most <paramref name="elementCount"/> items in this <see cref="RedBlackTree{T}"/>.
        /// </summary>
        /// <returns>A new <see cref="Array"/> with at most <paramref name="elementCount"/> items.</returns>
        public TValue[] ToArray(int elementCount)
        {
            if (elementCount < 1) throw new ArgumentOutOfRangeException();

            lock (syncRoot)
            {
                var l = new List<TValue>();
                int x = 0;

                foreach (var item in this)
                {
                    l.Add(item);

                    x++;
                    if (x >= elementCount) break;
                }

                return l.ToArray();
            }
        }

        /// <summary>
        /// Try to alter an item.
        /// </summary>
        /// <param name="item">The item to alter.</param>
        /// <param name="alteration">The alteration function that returns the changed item.</param>
        /// <returns>True if successful.</returns>
        public bool TryAlterItem(TValue item, Func<TValue, TValue> alteration)
        {
            var idx = Walk(ProvideKey(item), TreeWalkMode.Locate);

            if (idx != -1)
            {
                AlterItem(item, alteration, idx);
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue result)
        {
            lock (syncRoot)
            {
                int x = Walk(key, TreeWalkMode.Locate);

                if (x != -1)
                {
                    result = items[x];
                    return true;
                }

                result = default;
                return false;
            }
        }

        /// <summary>
        /// Attempt to Rebalance The Tree
        /// </summary>
        /// <param name="threshold">The ratio of the tree size over the logical count at which a rebalance should be performed. Default is 1.2 : 1</param>
        /// <returns>A <see cref="RebalanceResult"/> of <see cref="RebalanceResult.NotPerformed"/>, <see cref="RebalanceResult.Unchanged"/>, or <see cref="RebalanceResult.Changed"/>.</returns>
        public RebalanceResult TryRebalance()
        {
            lock (syncRoot)
            {
                if (count > 1024 && (float)treeSize / count >= rebalanceThreshold)
                {
                    bool b = false;

                    for (int i = treeSize - 2; i >= 2; i -= 2)
                    {
                        b = b | LocalRebalance(i, globalStrategy, true);
                    }

                    if (b)
                    {
                        if (metrics) changedRebalances++;
                        return RebalanceResult.Changed;
                    }
                    else
                    {
                        if (metrics) unchangedRebalances++;
                        return RebalanceResult.Unchanged;
                    }
                }

                return RebalanceResult.NotPerformed;
            }
        }

        #endregion Public Methods

        #region Protected Properties

        protected internal IList<TValue> Items => items;

        #endregion Protected Properties

        #region Protected Methods

        /// <summary>
        /// Provide the key for the specified value.
        /// </summary>
        /// <param name="item">The value.</param>
        /// <returns></returns>
        protected abstract TKey ProvideKey(TValue item);

        /// <summary>
        /// Alter an item.
        /// </summary>
        /// <param name="item">The item to alter.</param>
        /// <param name="alteration">The alteration function that returns the changed item.</param>
        /// <param name="idx">The index of the item.</param>
        /// <remarks>
        /// This function should only be used in conjunction with a call to either <see cref="Locate(TValue, out int)"/> or <see cref="Walk(TValue, TreeWalkMode)"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual void AlterItem(TValue item, Func<TValue, TValue> alteration, int idx)
        {
            lock (syncRoot)
            {

                // TODO See about soft moving the item.
                RemoveItem(idx);
                var newitem = alteration(item);
                InsertItem(newitem);
            }
        }

        /// <summary>
        /// Clear the collection.
        /// </summary>
        protected internal virtual void ClearItems()
        {
            lock (syncRoot)
            {
                items.Clear();
                count = 0;
            }
        }

        /// <summary>
        /// Insert an item into the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <exception cref="ArgumentNullException" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual void InsertItem(TValue item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            lock (syncRoot)
            {
                var key = ProvideKey(item);
                var index = Walk(key);

                if (index < treeSize && items[index] == null)
                {
                    keys[index] = key;
                    items[index] = item;
                    if (metrics) softInserts++;
                }
                else if (index > 0 && items[index - 1] == null)
                {
                    keys[index - 1] = key;
                    items[index - 1] = item;
                    if (metrics) softInserts++;
                }
                else if (index < treeSize - 2 && items[index + 2] == null)
                {
                    keys[index + 2] = keys[index + 1];
                    keys[index + 1] = keys[index];
                    keys[index] = key;

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

                        keyspace[0] = key;
                        keyspace[1] = default;

                    }
                    else
                    {
                        arrspace[0] = default;
                        arrspace[1] = item;

                        keyspace[0] = default;
                        keyspace[1] = key;
                    }

                    items.InsertRange(index, arrspace);
                    keys.InsertRange(index, keyspace);

                    this.treeSize += 2;

                    if (metrics) hardInserts++;
                }

                if (metrics)
                {
                    var ins = softInserts + hardInserts;
                    averageInsertIndex = (averageInsertIndex * (ins - 1) + index) / ins;
                }

                count++;
            }
        }

        /// <summary>
        /// Rebalance the tree locally. This usually happens after item removal, but can be performed at any time.
        /// </summary>
        /// <param name="index">The index in the tree that defines the locality.</param>
        /// <param name="strategy">The rebalance strategy to use.</param>
        /// <param name="globalRebalanceOperation">True to indicate this function is being called by <see cref="TryRebalance"/>.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal bool LocalRebalance(int index, RebalanceStrategy strategy, bool globalRebalanceOperation)
        {
            lock (syncRoot)
            {
                if (strategy == RebalanceStrategy.Cadence16)
                {
                    if ((index & 1) == 1) index--;

                    if (index + 8 > treeSize) return false;
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

                        keys[index + 1] = keys[index + 2];
                        keys[index + 2] = keys[index + 4];
                        keys[index + 3] = keys[index + 6];
                        keys[index + 4] = keys[index + 8];
                        keys[index + 5] = keys[index + 10];
                        keys[index + 6] = keys[index + 12];
                        keys[index + 7] = keys[index + 14];


                        items.RemoveRange(index + 8, 8);
                        keys.RemoveRange(index + 8, 8);

                        treeSize -= 8;

                        if (metrics && !globalRebalanceOperation)
                        {
                            softRemoves--;
                            localRebalances++;
                            hardRemoves++;
                        }

                        return true;
                    }
                }
                else if (strategy == RebalanceStrategy.Cadence8)
                {
                    if ((index & 1) == 1) index--;

                    if (index + 4 > treeSize) return false;
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

                        keys[index + 1] = keys[index + 2];
                        keys[index + 2] = keys[index + 4];
                        keys[index + 3] = keys[index + 6];

                        items.RemoveRange(index + 4, 4);
                        keys.RemoveRange(index + 4, 4);

                        treeSize -= 4;

                        if (metrics && !globalRebalanceOperation)
                        {
                            softRemoves--;
                            localRebalances++;
                            hardRemoves++;
                        }

                        return true;
                    }

                }
                else if (strategy == RebalanceStrategy.Cadance4)
                {
                    if ((index & 1) == 1) index--;

                    if (index + 2 > treeSize) return false;
                    if (index - 2 < 0) return false;

                    index -= 2;

                    if (
                        items[index] is object && !(items[index + 1] is object)
                        && items[index + 2] is object && !(items[index + 3] is object)
                        )
                    {
                        items[index + 1] = items[index + 2];
                        keys[index + 1] = keys[index + 2];

                        items.RemoveRange(index + 2, 2);
                        keys.RemoveRange(index + 2, 2);

                        treeSize -= 2;

                        if (metrics && !globalRebalanceOperation)
                        {
                            softRemoves--;
                            localRebalances++;
                            hardRemoves++;
                        }

                        return true;
                    }
                }

                return false;

            }
        }

        /// <summary>
        /// Returns true if the item exists in the collection, and provides the current index for that item.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <param name="index">The current index in the tree.</param>
        /// <returns>True if the item exists.</returns>
        protected internal virtual bool Locate(TValue item, out int index)
        {
            index = Walk(ProvideKey(item), TreeWalkMode.Locate);
            return index != -1;
        }

        /// <summary>
        /// Remove an item from the collection.
        /// </summary>
        /// <param name="index"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual void RemoveItem(int index)
        {
            lock (syncRoot)
            {
                items[index] = default;
                keys[index] = default;

                count--;

                if ((index & 1) == 0)
                {
                    if (items[index + 1] is object)
                    {
                        items[index] = items[index + 1];
                        items[index + 1] = default;

                        keys[index] = keys[index + 1];
                        keys[index + 1] = default;

                        if (metrics) softRemoves++;
                    }
                    else if (index < treeSize - 3 && items[index + 2] is object && items[index + 3] is object)
                    {
                        items[index] = items[index + 2];
                        items[index + 2] = items[index + 3];
                        items[index + 3] = default;

                        keys[index] = keys[index + 2];
                        keys[index + 2] = keys[index + 3];
                        keys[index + 3] = default;

                        if (metrics) softRemoves++;
                    }
                    else
                    {
                        items.RemoveRange(index, 2);
                        keys.RemoveRange(index, 2);

                        treeSize -= 2;
                        if (metrics) hardRemoves++;
                    }
                }
                else
                {
                    if (metrics) softRemoves++;

                    if (TryRebalance() == RebalanceResult.NotPerformed)
                    {
                        LocalRebalance(index, localStrategy, false);
                    }
                }
            }
        }

        /// <summary>
        /// Walk the tree and look for the appropriate index for the specified item.
        /// </summary>
        /// <param name="item1">The item to look for.</param>
        /// <param name="walkMode">The type of walk (either for insert or locate)</param>
        /// <returns>The index where the item is or should be.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual int Walk(TKey item1, TreeWalkMode walkMode = TreeWalkMode.InsertIndex)
        {
            int lo = 0;
            int hi = treeSize - 1;
            int mid = 0;

            TKey item2, item3;
            int r = 0;

            while (true)
            {
                if (hi < lo)
                {
                    if (walkMode == TreeWalkMode.InsertIndex)
                    {
                        if ((lo & 1) == 0)
                        {
                            if (lo < treeSize - 1 && !(keys[lo + 1] is object))
                            {
                                r = comp(item1, keys[lo]);
                                if (r >= 0) lo++;
                            }

                            else if (lo > 0 && !(keys[lo - 1] is object))
                            {
                                if (lo < treeSize)
                                {
                                    r = comp(item1, keys[lo]);
                                    if (r <= 0) lo--;
                                }
                                else
                                {
                                    lo--;
                                }
                            }
                        }
                    }
                    else if (r == 0)
                    {
                        return lo;
                    }
                    else
                    {
                        if (lo < 0 || lo >= treeSize) return -1;
                        else if (!(keys[lo] is object)) return -1;
                        else if (comp(item1, keys[lo]) != 0) return -1;
                    }

                    return lo;
                }

                mid = (hi + lo) / 2;

                if ((mid & 1) == 1) mid--;

                item2 = keys[mid];
                item3 = keys[mid + 1];

                r = comp(item1, item2);

                if (r > 0)
                {
                    if (item3 is object)
                    {
                        r = comp(item1, item3);

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

        #endregion Protected Methods
    }
}
