using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Distribution;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Kucoin.NET.Websockets.Distribution
{
    /// <summary>
    /// Provides push services for objects that present an observable version of internal data at regular intervals.
    /// </summary>
    public static class ObserverService
    {

        private static readonly List<IObservableCopy> feeds = new List<IObservableCopy>();

        private static Thread IntervalThread;

        private static CancellationTokenSource cts;

        static ObserverService()
        {
        }

        /// <summary>
        /// Register a new <see cref="IObservableCopy"/> object.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <returns>True if the object was successfully registered, false if the object was already registered.</returns>
        public static bool RegisterService(IObservableCopy obj)
        {
            if (!feeds.Contains(obj))
            {
                int x = obj.Interval;

                if (x % 10 != 0)
                {
                    x = x - (x % 10);
                }

                obj.Interval = x;
                if (x == 0) throw new ArgumentOutOfRangeException("Interval cannot be 0.");

                feeds.Add(obj);
            }
            else
            {
                return false;
            }

            if (IntervalThread == null)
            {
                cts = new CancellationTokenSource();

                IntervalThread = new Thread(IntervalMethod);
                IntervalThread.IsBackground = true;
                IntervalThread.Start();
            }

            return true;
        }

        /// <summary>
        /// Unregister an <see cref="IObservableCopy"/> object.
        /// </summary>
        /// <param name="obj">The object to unregister.</param>
        /// <returns>True if the object was successfully unregistered, false if the object was not registered.</returns>
        public static bool UnregisterService(IObservableCopy obj)
        {
            if (feeds.Contains(obj))
            {
                feeds.Remove(obj);
            }
            else
            {
                return false;
            }

            if (feeds.Count == 0)
            {
                CancelIntervalThread();
            }

            return true;
        }

        /// <summary>
        /// Cancel the pinger thread.
        /// </summary>
        private static void CancelIntervalThread()
        {
            cts?.Cancel();

            IntervalThread = null;
            cts = null;
        }

        /// <summary>
        /// Pinger method.
        /// </summary>
        private static void IntervalMethod()
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    if (feeds.Count == 0)
                    {
                        return;
                    }

                    long intervalMax = 0;

                    foreach (var feed in feeds)
                    {
                        if (feed.Interval > intervalMax)
                        {
                            intervalMax = feed.Interval;
                        }
                    }

                    for (int i = 0; i < intervalMax; i += 10)
                    {
                        Task.Delay(10, cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                        if (cts.IsCancellationRequested) return;

                        foreach (var feed in feeds)
                        {
                            if (cts.IsCancellationRequested) return;

                            if (i == feed.Interval)
                            {
                                if (feed.PreferDispatcher && !Dispatcher.CurrentThreadIsDispatcher && Dispatcher.Initialized)
                                {
                                    Dispatcher.BeginInvokeOnMainThread((o) =>
                                    {
                                        feed.CopyToObservable();
                                    });
                                }
                                else
                                {
                                    feed.CopyToObservable();
                                }
                            }
                        }
                    }

                    if (cts.IsCancellationRequested) return;
                }
                catch //(Exception ex)
                {

                    return;
                }

            }
        }


    }
}
