using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Websockets.Distribution
{

    public interface IMarketFeed<TDistributable, TValue, TInitial, TObservable> :
                        IInitialDataProvider<string, TInitial>,
                        IAsyncUnsubscribableSubscriptionProvider<string, TDistributable>,
                        IDistributor<TDistributable, TValue>        
                        where TDistributable : DistributableObject<string, TValue>
                        where TValue : ISymbol, IStreamableObject
    {
     
    }

}
