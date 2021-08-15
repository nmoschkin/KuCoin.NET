using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{           
    public interface ISubscriptionProvider<TKey, TObservation, TValue> : IDisposable where TObservation : IDisposable
    {
        IDisposable SubscribeOne(TKey key);

        IEnumerable<TObservation> SubscribeMany(IEnumerable<TKey> keys);
    }

    public interface IUnsubscribableSubscriptionProvider<TKey, TObservation, TValue> : ISubscriptionProvider<TKey, TObservation, TValue> where TObservation : IDisposable
    {
        void UnsubscribeOne(TKey key);

        void UnsubscribeMany(IEnumerable<TKey> keys);
    }

    public interface IAsyncSubscriptionProvider<TKey, TObservation, TValue> : IDisposable where TObservation : IDisposable
    {
        Task<TObservation> SubscribeOne(TKey key);

        Task<IEnumerable<TObservation>> SubscribeMany(IEnumerable<TKey> keys);
    }

    public interface IAsyncUnsubscribableSubscriptionProvider<TKey, TObservation, TValue> : IAsyncSubscriptionProvider<TKey, TObservation, TValue> where TObservation : IDisposable
    {
        Task UnsubscribeOne(TKey key);

        Task UnsubscribeMany(IEnumerable<TKey> keys);
    }

}
