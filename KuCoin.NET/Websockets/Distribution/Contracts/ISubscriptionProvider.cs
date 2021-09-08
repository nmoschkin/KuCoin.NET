using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Provides subscriptions to services that are identified by keys.
    /// </summary>
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IUnsubscribableSubscriptionProvider"/> may be used in place of <see cref="ISubscriptionProvider"/>.
    /// </remarks>
    public interface ISubscriptionProvider : IDisposable
    {
        /// <summary>
        /// Subscribe to a single service identified by the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the service to subscribe.</param>
        /// <returns>A new subscription.</returns>
        IDisposable SubscribeOne(object key);

        /// <summary>
        /// Subscribe to multiple services, identified by the specified keys.
        /// </summary>
        /// <param name="keys">The keys that identify the services to subscribe.</param>
        /// <returns>New subscriptions.</returns>
        IEnumerable SubscribeMany(IEnumerable keys);
    }

    /// <summary>
    /// Provides subscriptions to services that are identified by keys that can be both subscribed and unsubscribed.
    /// </summary>
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IUnsubscribableSubscriptionProvider"/> may be used in place of <see cref="ISubscriptionProvider"/>.
    /// </remarks>
    public interface IUnsubscribableSubscriptionProvider : ISubscriptionProvider
    {

        /// <summary>
        /// Unsubscribe from a single service identified by the specified key.
        /// </summary>
        void UnsubscribeOne(object key);


        /// <summary>
        /// Unsubscribe from multiple services, identified by the specified keys.
        /// </summary>
        void UnsubscribeMany(IEnumerable keys);

    }

    /// <summary>
    /// Provides subscriptions to services that are identified by keys.
    /// </summary>
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IAsyncUnsubscribableSubscriptionProvider"/> may be used in place of <see cref="IAsyncSubscriptionProvider"/>.
    /// </remarks>
    public interface IAsyncSubscriptionProvider 
    {
        /// <summary>
        /// Subscribe to a single service identified by the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the service to subscribe.</param>
        /// <returns>A new subscription.</returns>
        Task<IDisposable> SubscribeOne(object key);

        /// <summary>
        /// Subscribe to multiple services, identified by the specified keys.
        /// </summary>
        /// <param name="keys">The keys that identify the services to subscribe.</param>
        /// <returns>New subscriptions.</returns>
        Task<IEnumerable> SubscribeMany(IEnumerable keys);

    }


    /// <summary>
    /// Provides asynchronous functions for subscriptions to services that are identified by <typeparamref name="TKey"/> that can be both subscribed and unsubscribed.
    /// </summary>
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IAsyncUnsubscribableSubscriptionProvider"/> may be used in place of <see cref="IAsyncSubscriptionProvider"/>.
    /// </remarks>
    public interface IAsyncUnsubscribableSubscriptionProvider : IAsyncSubscriptionProvider
    {

        /// <summary>
        /// Unsubscribe from a single service identified by the specified key.
        /// </summary>
        Task UnsubscribeOne(object key);


        /// <summary>
        /// Unsubscribe from multiple services, identified by the specified keys.
        /// </summary>
        Task UnsubscribeMany(IEnumerable keys);

    }


    /// <summary>
    /// Provides subscriptions to services that are identified by <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TObservation">The observation type.</typeparam>
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IUnsubscribableSubscriptionProvider{TKey, TObservation, TValue}"/> may be used in place of <see cref="ISubscriptionProvider{TKey, TObservation, TValue}"/>.
    /// </remarks>
    public interface ISubscriptionProvider<TKey, TObservation> : ISubscriptionProvider where TObservation : IDisposable
    {
        /// <summary>
        /// Subscribe to a single service identified by the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the service to subscribe.</param>
        /// <returns>A new instance of <see cref="TObservation"/>.</returns>
        TObservation SubscribeOne(TKey key);

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
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IUnsubscribableSubscriptionProvider{TKey, TObservation, TValue}"/> may be used in place of <see cref="ISubscriptionProvider{TKey, TObservation, TValue}"/>.
    /// </remarks>
    public interface IUnsubscribableSubscriptionProvider<TKey, TObservation> : ISubscriptionProvider<TKey, TObservation>, IUnsubscribableSubscriptionProvider where TObservation : IDisposable
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
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IAsyncUnsubscribableSubscriptionProvider{TKey, TObservation, TValue}"/> may be used in place of <see cref="IAsyncSubscriptionProvider{TKey, TObservation, TValue}"/>.
    /// </remarks>
    public interface IAsyncSubscriptionProvider<TKey, TObservation> : IAsyncSubscriptionProvider where TObservation : IDisposable
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
    /// <remarks>
    /// Normally, a subscription is terminated by calling the <see cref="IDisposable.Dispose"/> method of the returned subscription object.<br /><br />
    /// In instances where it is useful or desirable for the subscription provider to provider unsubscribe functionality, directly, <see cref="IAsyncUnsubscribableSubscriptionProvider{TKey, TObservation, TValue}"/> may be used in place of <see cref="IAsyncSubscriptionProvider{TKey, TObservation, TValue}"/>.
    /// </remarks>
    public interface IAsyncUnsubscribableSubscriptionProvider<TKey, TObservation> : IAsyncSubscriptionProvider<TKey, TObservation>, IAsyncUnsubscribableSubscriptionProvider where TObservation : IDisposable
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
