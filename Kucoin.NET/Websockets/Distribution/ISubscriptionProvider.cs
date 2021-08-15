using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{
    /// <summary>
    /// Provides subscriptions to services that are identified by <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TObservation">The observation type.</typeparam>
    /// <typeparam name="TValue">The type of the content that is expected.</typeparam>
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IUnsubscribableSubscriptionProvider{TKey, TObservation, TValue}"/> may be used in place of <see cref="ISubscriptionProvider{TKey, TObservation, TValue}"/>.
    /// </remarks>
    public interface ISubscriptionProvider<TKey, TObservation, TValue> : IDisposable where TObservation : IDisposable
    {
        /// <summary>
        /// Subscribe to a single service identified by the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the service to subscribe.</param>
        /// <returns>A new instance of <see cref="TObservation"/>.</returns>
        IDisposable SubscribeOne(TKey key);

        /// <summary>
        /// Subscribe to multiple services, identified by the specified keys.
        /// </summary>
        /// <param name="keys">The keys that identify the services to subscribe.</param>
        /// <returns>A list of new instances of <see cref="TObservation"/>.</returns>
        IEnumerable<TObservation> SubscribeMany(IEnumerable<TKey> keys);
    }

    /// <summary>
    /// Provides subscriptions to services that are identified by <typeparamref name="TKey"/> that can be both subscribed and unsubscribed.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TObservation">The observation type.</typeparam>
    /// <typeparam name="TValue">The type of the content that is expected.</typeparam>
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IUnsubscribableSubscriptionProvider{TKey, TObservation, TValue}"/> may be used in place of <see cref="ISubscriptionProvider{TKey, TObservation, TValue}"/>.
    /// </remarks>
    public interface IUnsubscribableSubscriptionProvider<TKey, TObservation, TValue> : ISubscriptionProvider<TKey, TObservation, TValue> where TObservation : IDisposable
    {

        /// <summary>
        /// Unsubscribe from a single service identified by the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the service to unsubscribe.</param>
        void UnsubscribeOne(TKey key);

        /// <summary>
        /// Unsubscribe from multiple services, identified by the specified keys.
        /// </summary>
        /// <param name="keys">The keys that identify the services to unsubscribe.</param>
        void UnsubscribeMany(IEnumerable<TKey> keys);
    }

    /// <summary>
    /// Provides asynchronous functions for subscriptions to services that are identified by <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TObservation">The observation type.</typeparam>
    /// <typeparam name="TValue">The type of the content that is expected.</typeparam>
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IAsyncUnsubscribableSubscriptionProvider{TKey, TObservation, TValue}"/> may be used in place of <see cref="IAsyncSubscriptionProvider{TKey, TObservation, TValue}"/>.
    /// </remarks>
    public interface IAsyncSubscriptionProvider<TKey, TObservation, TValue> : IDisposable where TObservation : IDisposable
    {
        /// <summary>
        /// Subscribe to a single service identified by the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the service to subscribe.</param>
        /// <returns>A new instance of <see cref="TObservation"/>.</returns>
        Task<TObservation> SubscribeOne(TKey key);

        /// <summary>
        /// Subscribe to multiple services, identified by the specified keys.
        /// </summary>
        /// <param name="keys">The keys that identify the services to subscribe.</param>
        /// <returns>A list of new instances of <see cref="TObservation"/>.</returns>
        Task<IDictionary<TKey, TObservation>> SubscribeMany(IEnumerable<TKey> keys);
    }

    /// <summary>
    /// Provides asynchronous functions for subscriptions to services that are identified by <typeparamref name="TKey"/> that can be both subscribed and unsubscribed.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TObservation">The observation type.</typeparam>
    /// <typeparam name="TValue">The type of the content that is expected.</typeparam>
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IAsyncUnsubscribableSubscriptionProvider{TKey, TObservation, TValue}"/> may be used in place of <see cref="IAsyncSubscriptionProvider{TKey, TObservation, TValue}"/>.
    /// </remarks>
    public interface IAsyncUnsubscribableSubscriptionProvider<TKey, TObservation, TValue> : IAsyncSubscriptionProvider<TKey, TObservation, TValue> where TObservation : IDisposable
    {
        /// <summary>
        /// Unsubscribe from a single service identified by the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the service to unsubscribe.</param>
        Task UnsubscribeOne(TKey key);

        /// <summary>
        /// Unsubscribe from multiple services, identified by the specified keys.
        /// </summary>
        /// <param name="keys">The keys that identify the services to unsubscribe.</param>
        Task UnsubscribeMany(IEnumerable<TKey> keys);
    }

}
