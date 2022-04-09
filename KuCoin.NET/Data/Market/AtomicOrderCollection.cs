using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;


namespace KuCoin.NET.Data.Market
{
    #region Atomic Order Unit Comparer 
    public class AtomicComparer<TUnit> : IComparer<TUnit> where TUnit : IAtomicOrderUnit
    {
        bool descending;
        int m = 1;

        public bool Descending => descending;

        public AtomicComparer(bool descending)
        {
            this.descending = descending;
            if (descending) m = -1;
        }

        public int Compare(TUnit x, TUnit y)
        {
            int r;

            r = decimal.Compare(x.Price, y.Price) * m;
            if (r == 0) r = decimal.Compare(x.Size, y.Size) * -1;
            if (r == 0) r = DateTime.Compare(x.Timestamp, y.Timestamp) * -1;

            return r;
        }
    }
    #endregion Atomic Order Unit Comparer 


    /// <summary>
    /// Keyed Atomic Order Book
    /// </summary>
    /// <typeparam name="TUnit">Any <see cref="IAtomicOrderUnit"/> implementation.</typeparam>
    public class AtomicOrderCollection<TUnit> : KeyedBufferedCollection<string, TUnit> where TUnit : IAtomicOrderUnit, new()
    {
        #region Public Constructors
      
        public AtomicOrderCollection() : base(new AtomicComparer<TUnit>(false))
        {
        }

        public AtomicOrderCollection(bool descending) : base(new AtomicComparer<TUnit>(descending))
        {
        }

        #endregion Public Constructors

        #region Public Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(string key)
        {
            lock (syncRoot)
            {
                if (keyDict.TryGetValue(key, out var value))
                {
                    keyDict.Remove(key);
                    Remove(value);
                }

            }
        }

        #endregion Public Methods

        #region Protected Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string ProvideKey(TUnit value)
        {
            return value.OrderId;
        }

        #endregion Protected Methods
    }

}
