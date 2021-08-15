
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Kucoin.NET.Websockets.Distribution
{
    /// <summary>
    /// Provides automatic pinging services for objects that must ping servers at regular intervals.
    /// </summary>
    public static class PingService
    {

        private static readonly List<IPingable> feeds = new List<IPingable>();

        private static Thread IntervalThread;

        private static CancellationTokenSource cts;

        static PingService()
        {
        }

        /// <summary>
        /// Register a new pingable object.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <returns>True if the object was successfully registered, false if the object was already registered.</returns>
        public static bool RegisterService(IPingable obj)
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
        /// Unregister a pingable object.
        /// </summary>
        /// <param name="obj">The object to unregister.</param>
        /// <returns>True if the object was successfully unregistered, false if the object was not registered.</returns>
        public static bool UnregisterService(IPingable obj)
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

                    long pingMax = 0;

                    foreach (var feed in feeds)
                    {
                        if (feed.Interval > pingMax)
                        {
                            pingMax = feed.Interval;
                        }
                    }

                    for (int i = 0; i < pingMax; i += 10)
                    {
                        Task.Delay(10, cts.Token).ConfigureAwait(false).GetAwaiter().GetResult(); 
                        if (cts.IsCancellationRequested) return;

                        foreach (var feed in feeds)
                        {
                            if (cts.IsCancellationRequested) return;

                            if (i == feed.Interval)
                            {
                                if (feed is IAsyncPingable aping)
                                {
                                    _ = aping.Ping();
                                }
                                else
                                {
                                    feed.Ping();
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
