using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Kucoin.NET.Websockets.Distribution
{
    public abstract class MarketObservation<TInternal, TObservable, TValue> : DistributableObject<string, TValue>, ISymbol, IObservableCopy<TInternal, TObservable> where TValue : IReadOnlySymbol 
    {
        public MarketObservation(IDistributor parent, string symbol) : base(parent, symbol)
        {
        }

        public abstract TInternal InternalData { get; }
        public abstract TObservable ObservableData { get; }
        public abstract bool PreferDispatcher { get; }
        public abstract int Interval { get; set; }

        string ISymbol.Symbol
        {
            get => key;
            set
            {
                SetProperty(ref key, value);    
            }
        }

        string IReadOnlySymbol.Symbol => key;

        public abstract void CopyToObservable();
    }
}
