using Kucoin.NET.Data;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Websockets.Distribution
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
