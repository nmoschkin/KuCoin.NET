using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Websockets.Distribution.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Presents market data to a UI/UX
    /// </summary>
    public interface IMarketPresenter
    : IDistributable,
        IInitializable,
        ISymbol,
        IMarketDepth,
        IPresentable
    {
    }

    /// <summary>
    /// Presents market data to a UI/UX
    /// </summary>
    public interface IMarketPresenter<TKey, TValue>
    : IMarketPresenter, IDistributable<TKey, TValue>
      where TValue : IStreamableObject
    {
    }

    /// <summary>
    /// Presents market data to a UI/UX
    /// </summary>
    public interface IMarketPresenter<TObservable, TKey, TValue> 
    :  IMarketPresenter<TKey, TValue>, IPresentable<TObservable>
       where TValue : IStreamableObject
    {
    }

    /// <summary>
    /// Presents market data to a UI/UX
    /// </summary>
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
    /// <remarks>
    /// <see cref="MarketPresenter{TInternal, TObservable, TValue}"/> is an <see cref="InitializableObject{TKey, TValue, TInternal}"/>.
    /// </remarks>
    public abstract class MarketPresenter<TInternal, TObservable, TValue> : 
        InitializableObject<string, TValue, TInternal>, 
        IMarketPresenter<TInternal, TObservable, string, TValue> 
        where TValue : IStreamableObject
    {
        public MarketPresenter(IDistributor parent, string symbol, bool direct) : base(parent, symbol, direct)
        {
            Symbol = symbol;
            PresentationService.RegisterService(this);
        }

        public abstract bool IsPresentationDisabled { get; set; }
        public abstract TObservable PresentedData { get; protected set; }
        public abstract bool PreferDispatcher { get; }
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

        public abstract void PresentData();
        protected override void Dispose(bool disposing)
        {
            PresentationService.UnregisterService(this);    
            base.Dispose(disposing);
        }
    }
}
