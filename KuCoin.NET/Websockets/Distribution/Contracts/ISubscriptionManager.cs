using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Websockets.Distribution
{
    /// <summary>
    /// Subscription event args
    /// </summary>
    public class SubscriptionEventArgs : EventArgs
    {
        public IInitializable Subscriber { get; protected set; }

        public ISubscriptionProvider Provider { get; protected set; }

        public SubscriptionEventArgs(IInitializable subscriber, ISubscriptionProvider provider)
        {
            Subscriber = subscriber;
            Provider = provider;    
        }
        
    }

    /// <summary>
    /// Handles subscribing to multitudinous workloads in an orderly fashion.
    /// </summary>
    public interface ISubscriptionManager
    {
        
        /// <summary>
        /// The maximum number of initializations to run concurrently.
        /// </summary>
        int MaxParallelTasks { get; set; }

        /// <summary>
        /// The minimum time, in milliseconds, between REST requests.
        /// </summary>
        int MinimumRequestDelay { get; set; }
        
        /// <summary>
        /// Subscribe to many workloads in an orderly fashion.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TObservation">The type of observation to expect.</typeparam>
        /// <param name="subscriber">The <see cref="ISubscriptionProvider{TKey, TObservation}"/>.</param>
        /// <param name="keys">The list of keys to subscribe.</param>
        /// <param name="onCompleted">The function to call when subscribing is complete.</param>
        /// <returns>The new subscriptions.</returns>
        IDictionary<TKey, TObservation> SubscribeMany<TKey, TObservation>(ISubscriptionProvider<TKey, TObservation> subscriber, IEnumerable<TKey> keys, Action<IEnumerable<TObservation>> onCompleted) where TObservation : IDisposable, IInitializable;

        /// <summary>
        /// Register a method to be called after the specified object has completed its initialization process.
        /// </summary>
        /// <param name="subscriber">The subscriber to observe.</param>
        /// <param name="afterInitialized">The code to execute after the subscribe is initialized.</param>
        /// <remarks>
        /// The <paramref name="subscriber"/> must have been previously returned by a call to <see cref="SubscribeMany{TKey, TObservation}(ISubscriptionProvider{TKey, TObservation}, IEnumerable{TKey}, Action{IEnumerable{TObservation}})"/>.
        /// </remarks>
        void RegisterAfterInitializedHandler(IInitializable subscriber, Action afterInitialized);

    }
}
