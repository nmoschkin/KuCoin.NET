using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kucoin.NET.Helpers
{
    /// <summary>
    /// Synchronization dispatcher (for UI data-binding)
    /// </summary>
    public static class Dispatcher
    {
        private static bool init;

        /// <summary>
        /// The synchronization context for the current application.
        /// </summary>
        public static SynchronizationContext Context { get; private set; }

        /// <summary>
        /// Gets a value indicating that the dispatcher has been initialized.
        /// </summary>
        public static bool Initialized => init;

        /// <summary>
        /// Call this method with a new <see cref="SynchronizationContext"/> created from the 
        /// default dispatcher for your application.
        /// </summary>
        /// <param name="context">The synchronization context.</param>
        public static void Initialize(SynchronizationContext context)
        {
            Context = context;
            init = true;
        }
        
        /// <summary>
        /// Synchronously execute the code inside of the callback on the main application thread.
        /// </summary>
        /// <param name="callback">Code to execute.</param>
        public static void InvokeOnMainThread(SendOrPostCallback callback)
        {
            Context.Send(callback, null);            
        }

        /// <summary>
        /// Asynchronously execute the code inside of the callback on the main application thread.
        /// </summary>
        /// <param name="callback">Code to execute.</param>
        public static void BeginInvokeOnMainThread(SendOrPostCallback callback)
        {
            Context.Post(callback, null);
        }

    }
}
