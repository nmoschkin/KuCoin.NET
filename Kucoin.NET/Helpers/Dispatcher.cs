﻿using System;
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
        public static void Initialize(SynchronizationContext context)
        {
            Context = context;
            init = true;
        }


        /// <summary>
        /// Initialize the dispatcher.
        /// </summary>
        /// <remars>
        /// This method must be called from the main/UI thread of your application.
        /// </remars>
        public static void Initialize()
        {
            Context = SynchronizationContext.Current;
            
            if (Context == null)
            {
                throw new InvalidOperationException("Dispatcher.Initialize() must be called from the UI thread when called without parameters.");
            }

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
