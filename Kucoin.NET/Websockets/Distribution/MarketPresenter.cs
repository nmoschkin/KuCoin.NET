using Kucoin.NET.Data;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Websockets.Distribution.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{

    public interface IMarketPresenter
    : IDistributable,
        IInitializable,
        ISymbol,
        IMarketDepth,
        IPresentable
    {
    }

    public interface IMarketPresenter<TKey, TValue>
    : IMarketPresenter, IDistributable<TKey, TValue>
      where TValue : IStreamableObject
    {
    }

    public interface IMarketPresenter<TObservable, TKey, TValue> 
    :  IMarketPresenter<TKey, TValue>, IPresentable<TObservable>
       where TValue : IStreamableObject
    {
    }

    public interface IMarketPresenter<TInternal, TObservable, TKey, TValue> 
        : IDistributable<TKey, TValue>, 
        IInitializable<TKey, TInternal>, 
        ISymbol, 
        IMarketDepth, 
        IPresentable<TInternal, TObservable> 
        where TValue : IStreamableObject
    {
    }



    /// <summary>
    /// Base class for all market presentation where the raw data and the presented data are two different objects, with the presented data being data prepared from the raw data at regular intervals.
    /// </summary>
    /// <typeparam name="TInternal"></typeparam>
    /// <typeparam name="TObservable"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public abstract class MarketPresenter<TInternal, TObservable, TValue> : 
        DistributableObject<string, TValue>, 
        IMarketPresenter<TInternal, TObservable, string, TValue> 
        where TValue : IStreamableObject
    {
        public MarketPresenter(IDistributor parent, string symbol) : base(parent, symbol)
        {
            Symbol = symbol;
            ObserverService.RegisterService(this);
        }

        public abstract bool IsPresentationDisabled { get; set; }

        public abstract TInternal InternalData { get; protected set; }
        public abstract TObservable PresentedData { get; protected set; }
        public abstract bool PreferDispatcher { get; }

        public abstract bool Failure { get; protected set; }
        public abstract int Interval { get; set; }
        public abstract int MarketDepth { get; set; }

        public string Symbol
        {
            get => key;
            set
            {
                SetProperty(ref key, value);    
            }
        }

        public abstract IInitialDataProvider<string, TInternal> DataProvider { get; protected set; }
        public abstract bool IsDataProviderAvailable { get; protected set; }
        public abstract bool IsInitialized { get; protected set; }
        public abstract int ResetCount { get; protected set; }
        public abstract int ResetTimeout { get; set; }
        public abstract int MaxTimeoutRetries { get; set; }

        public abstract event EventHandler Initialized;

        public abstract void CopyToPresentation();
        public abstract Task<bool> Initialize();
        public abstract Task Reset();
        public abstract void SetInitialDataProvider(IInitialDataProvider<string, TInternal> dataProvider);

        protected override void Dispose(bool disposing)
        {
            ObserverService.UnregisterService(this);    
            base.Dispose(disposing);
        }
    }
}
