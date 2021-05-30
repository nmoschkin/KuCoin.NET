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
        /// Initialize the dispatcher with a <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="context">The synchronization context.</param>
        /// <remarks>
        /// You must create a synchronization context from the default dispatcher of your application.
        /// </remarks>
        public static bool Initialize(SynchronizationContext context)
        {
            Context = context;
            init = Context != null;

            return init;
        }


        /// <summary>
        /// Initialize the dispatcher.
        /// </summary>
        /// <remars>
        /// This method must be called from the main/UI thread of your application.
        /// </remars>
        public static bool Initialize()
        {
            Context = SynchronizationContext.Current;

            init = Context != null;

            return init;
        }

        /// <summary>
        /// Synchronously execute the code inside of the callback on the main application thread.
        /// </summary>
        /// <param name="callback">Code to execute.</param>
        /// <param name="param">Optional parameter to pass to the function.</param>
        public static void InvokeOnMainThread(SendOrPostCallback callback, object param = null)
        {
            Context.Send(callback, param);            
        }

        /// <summary>
        /// Asynchronously execute the code inside of the callback on the main application thread.
        /// </summary>
        /// <param name="callback">Code to execute.</param>
        /// <param name="param">Optional parameter to pass to the function.</param>
        public static void BeginInvokeOnMainThread(SendOrPostCallback callback, object param = null)
        {
            Context.Post(callback, param);
        }

    }
}
