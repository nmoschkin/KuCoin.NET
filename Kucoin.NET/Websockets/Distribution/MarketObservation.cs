using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{
    public interface IMarketObservation<TInternal, TObservable, TValue> : IDistributable<string, TValue>, IInitializable<string, TInternal>, ISymbol, IMarketDepth, IObservableCopy<TInternal, TObservable> where TValue : ISymbol
    {
    }

    /// <summary>
    /// Base class for all market observations.
    /// </summary>
    /// <typeparam name="TInternal"></typeparam>
    /// <typeparam name="TObservable"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public abstract class MarketObservation<TInternal, TObservable, TValue> : DistributableObject<string, TValue>, IMarketObservation<TInternal, TObservable, TValue> where TValue : ISymbol 
    {
        public MarketObservation(IDistributor parent, string symbol) : base(parent, symbol)
        {
            Symbol = symbol;
            ObserverService.RegisterService(this);
        }

        public abstract bool IsObservationDisabled { get; set; }

        public abstract TInternal InternalData { get; protected set; }
        public abstract TObservable ObservableData { get; protected set; }
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
        public abstract bool IsCalibrated { get; protected set; }
        public abstract bool IsInitialized { get; protected set; }
        public abstract int ResetCount { get; protected set; }
        public abstract int ResetTimeout { get; set; }
        public abstract int MaxTimeoutRetries { get; set; }

        public abstract event EventHandler Initialized;

        public abstract Task Calibrate();
        public abstract void CopyToObservable();
        public abstract Task Initialize();
        public abstract Task Reset();
        public abstract void SetInitialDataProvider(IInitialDataProvider<string, TInternal> dataProvider);

        protected override void Dispose(bool disposing)
        {
            ObserverService.UnregisterService(this);    
            base.Dispose(disposing);
        }
    }
}
