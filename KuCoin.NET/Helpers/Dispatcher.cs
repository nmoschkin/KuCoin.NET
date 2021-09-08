using System.Data;
using System.Threading;

namespace KuCoin.NET.Helpers
{
    /// <summary>
    /// Synchronization dispatcher (for UI data-binding)
    /// </summary>
    public static class Dispatcher
    {
        private static bool init = false;
        private static bool nosend = false;
        private static bool nopost = false;
        

        /// <summary>
        /// True if the current thread is the dispatcher thread.
        /// </summary>
        public static bool CurrentThreadIsDispatcher
        {
            get => SynchronizationContext.Current is object;
        }

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
        /// This method will also return true if the dispatcher was previously successfully initialized.
        /// </remarks>
        public static bool Initialize(SynchronizationContext context)
        {
            if (init) return true;

            Context = context;
            init = Context != null;
            if (init) TestPlatform();

            return init;
        }


        /// <summary>
        /// Initialize the dispatcher.
        /// </summary>
        /// <returns>True if dispatcher was successfully initialized.</returns>
        /// <remars>
        /// This method must be called from the main/UI thread of your application.
        /// This method will also return true if the dispatcher was previously successfully initialized.
        /// </remars>
        public static bool Initialize()
        {
            if (init) return true;

            Context = SynchronizationContext.Current;

            init = Context != null;
            if (init) TestPlatform();

            return init;
        }

        private static void TestPlatform()
        {
            try
            {
                Context.Send((o) =>
                {
                    return;
                }, null);
            }
            catch
            {
                nosend = true;
            }

            try
            {
                Context.Post((o) =>
                {
                    return;
                }, null);
            }
            catch
            {
                nopost = true;
            }

            // no supported invoke!
            if (nosend && nopost) init = false;

        }

        /// <summary>
        /// Synchronously execute the code inside of the callback on the main application thread.
        /// </summary>
        /// <param name="callback">Code to execute.</param>
        /// <param name="param">Optional parameter to pass to the function.</param>
        public static void InvokeOnMainThread(SendOrPostCallback callback, object param = null)
        {
            if (nosend)
            {
                Context.Post(callback, param);
            }
            else
            {
                Context.Send(callback, param);
            }
        }

        /// <summary>
        /// Asynchronously execute the code inside of the callback on the main application thread.
        /// </summary>
        /// <param name="callback">Code to execute.</param>
        /// <param name="param">Optional parameter to pass to the function.</param>
        public static void BeginInvokeOnMainThread(SendOrPostCallback callback, object param = null)
        {
            if (nopost)
            {
                Context.Send(callback, param);
            }
            else
            {
                Context.Post(callback, param);
            }
        }

    }
}
