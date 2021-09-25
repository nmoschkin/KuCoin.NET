using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Websockets.Distribution
{

    /// <summary>
    /// An object that implements all of the necessary interfaces for a full market feed.
    /// </summary>
    /// <typeparam name="TDistributable">The type of the distribable object.</typeparam>
    /// <typeparam name="TValue">The type of data being consumed.</typeparam>
    /// <typeparam name="TInitial">The type of the initial data source.</typeparam>
    /// <typeparam name="TObservable">The type of the user-observable data.</typeparam>
    public interface IMarketFeed<TDistributable, TValue, TInitial, TObservable> :
                        IInitialDataProvider<string, TInitial>,
                        IAsyncUnsubscribableSubscriptionProvider<string, TDistributable>,
                        IDistributor<TDistributable, TValue>        
                        where TDistributable : DistributableObject<string, TValue>
                        where TValue : ISymbol, IStreamableObject
    {
     
    }

}
