using Kucoin.NET.Observable;
using Kucoin.NET.Websockets.Distributable;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Websockets.Distribution
{
    public abstract class DistributionObject<TKey, TValue> : ObservableBase, IDistributor<TKey, TValue> where TValue : IDistributable<TKey, TValue>
    {
        public abstract IReadOnlyDictionary<TKey, TValue> ActiveFeeds { get; }

        public abstract IEnumerable<IDistributable> GetActiveFeeds();
        
        public abstract void Release(IDistributable obj);
    }

}
