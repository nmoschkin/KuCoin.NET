using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Data.Market
{

    public class LargeActiveList<T> : IList<T>
    {
        public readonly int SegmentSize;
        private object lockObj = new object();

        List<List<T>> segments = new List<List<T>>();

        List<(int, int)> indices = new List<(int, int)>();

        int count = 0;
        
        public LargeActiveList(int segmentSize)
        {
            this.SegmentSize = segmentSize;
        }

        public LargeActiveList()
        {

            var ps = Environment.SystemPageSize;

            int ts;

            if (typeof(T).IsValueType)
            {
                ts = Marshal.SizeOf<T>();
            }
            else
            {
                ts = IntPtr.Size;
            }

            this.SegmentSize = (ps / ts) * 16;
        }

        public T this[int index]
        {
            get
            {
                if (index >= count || index < 0) throw new IndexOutOfRangeException();
                int i = GetSegment(index);
                return (segments[i][index - indices[i].Item1]);
            }
            set
            {
                if (index >= count || index < 0) throw new IndexOutOfRangeException();
                int i = GetSegment(index);
                segments[i][index - indices[i].Item1] = value;
            }
        }

        public int Count
        {
            get
            {
                lock(lockObj)
                {
                    return count;
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            lock (lockObj)
            {
                Insert(count, item);
            }
        }

        public void Clear()
        {

            lock (lockObj)
            {

            }
            segments.Clear();
            indices.Clear();

            count = 0;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int idx = arrayIndex;

            foreach(var item in this)
            {
                array[idx++] = item;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new MyEnumer(this);
        }

        public virtual int IndexOf(T item)
        {
            lock (lockObj)
            {
                int c = segments.Count;
                for (int i = 0; i < c; i++)
                {
                    int cidx = indices[i].Item1;
                    int d = segments[i].Count;

                    for (int j = 0; j < d; j++)
                    {
                        if (segments[i][j].Equals(item))
                        {
                            return cidx + j;
                        }
                    }
                }
            }

            return -1;
        }

        public virtual void Insert(int index, T item)
        {
            lock (lockObj)
            {
                if (index >= count)
                {
                    EnsureSegments(index);
                }

                int i = GetSegment(index);
                if (i >= indices.Count - 1)
                {
                    i = indices.Count - 1;
                }

                segments[i].Insert(index - indices[i].Item1, item);
                Reindex(i);
                count++;
            }
        }

        protected void Reindex(int start = 0)
        {
            lock (lockObj)
            {
                int cd = 0;
                int c = segments.Count;
                int cidx = indices[start].Item1;
                int nc = 0;

                for (int i = start; i < c; i++)
                {
                    cd = segments[i].Count;
                    nc = cidx + cd;

                    indices[i] = (cidx, nc - 1);
                    cidx += cd;
                }

            }
        }

        protected void EnsureSegments(int index)
        {
            int newidx = index;
            int cidx = 0;
            
            if (index >= count && indices.Count > 0)
            {
                int cs = indices.Count - 1;
                int ad = (indices[cs].Item2 - indices[cs].Item1) + 1;

                if (ad >= SegmentSize)
                {
                    segments.Add(new List<T>());
                    indices.Add((indices[cs].Item2 + 1, indices[cs].Item2 + SegmentSize));

                    return;
                }
            }

            newidx /= SegmentSize;

            int cc = segments.Count;

            if (newidx >= cc)
            {
                int mis = (newidx + 1) - cc;

                if (segments.Count > 0)
                {
                    cidx = indices[indices.Count - 1].Item2 + 1;
                }

                for (int i = 0; i < mis; i++)
                {
                    segments.Add(new List<T>());
                    indices.Add((cidx, cidx + SegmentSize - 1));
                    cidx += SegmentSize;
                }
            }
        }

        public async Task Realign(int start = 0)
        {
            await Task.Run(() =>
            {
                lock (lockObj)
                {
                    int c = segments.Count;

                    for (int i = start; i < c; i++)
                    {
                        if (i + 1 < c)
                        {
                            Balance(segments[i], segments[i + 1]);
                        }
                    }

                    Reindex();
                }

            });
        }

        protected void Balance(List<T> first, List<T> second)
        {
            int x, c, d, n;

            c = first.Count;
            d = second.Count;

            x = SegmentSize;



        }

        public virtual bool Remove(T item)
        {
            lock (lockObj)
            {
                int c = segments.Count;
                for (int i = 0; i < c; i++)
                {
                    int cidx = indices[i].Item1;
                    int d = segments[i].Count;
                    
                    for (int j = 0; j < d; j++)
                    {
                        if (segments[i][j].Equals(item))
                        {
                            RemoveAt(cidx + j);
                            count--;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual void RemoveAt(int index)
        {
            lock (lockObj)
            {
                int i = GetSegment(index);

                index -= indices[i].Item1;
                segments[i].RemoveAt(index);
                
                if (segments[i].Count == 0)
                {
                    segments.RemoveAt(i);
                    indices.RemoveAt(i);
                }

                count--;
                Reindex(i);
            }
        }

        protected int GetSegment(int index)
        {
            if (indices.Count == 0) return 0;

            int hi = indices.Count - 1;
            int lo = 0;
            int mid;

            (int, int) valley;

            while (true)
            {
                if (hi < lo)
                {
                    return lo;
                }

                mid = (hi + lo) / 2;

                valley = indices[mid];

                if (index > valley.Item2)
                {
                    lo = mid + 1;
                }
                else if (index < valley.Item1)
                {
                    hi = mid - 1;
                }
                else
                {
                    //return mid;
                    return mid;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MyEnumer(this);
        }

        private class MyEnumer : IEnumerator<T>
        {
            private LargeActiveList<T> src;

            int idx = -1;
            int c = 0;

            public T Current
            {
                get
                {
                    if (c != src.count)
                    {
                        throw new AggregateException("Collection was modified.  Enumeration cannot continue.");
                    }

                    return src[idx];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (c != src.count)
                    {
                        throw new AggregateException("Collection was modified.  Enumeration cannot continue.");
                    }

                    return src[idx];
                }
            }

            public MyEnumer(LargeActiveList<T> src)
            {
                this.src = src;
                c = src.count;
            }

            public void Dispose()
            {
                idx = -1;
                c = 0;
                src = null;
            }

            public bool MoveNext()
            {
                ++idx;
                return idx < c;
            }

            public void Reset()
            {
                idx = -1;
                c = src.count;
            }
        }
    }
}
