using Kucoin.NET.Websockets.Distribution;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Kucoin.NET.Websockets.Distribution
{
    public static class ObserverService
    {

        private static readonly List<IObservableCopy> feeds = new List<IObservableCopy>();

        private static Thread PushThread;

        private static CancellationTokenSource cts;

        static ObserverService()
        {
        }

        public static void RegisterService(IObservableCopy feed)
        {
            if (!feeds.Contains(feed))
            {
                int x = feed.Interval;

                if (x % 10 != 0)
                {
                    x = x - (x % 10);
                }

                feed.Interval = x;
                if (x == 0) throw new ArgumentOutOfRangeException("Interval cannot be 0.");
                feeds.Add(feed);
            }

            if (PushThread == null)
            {
                cts = new CancellationTokenSource();

                PushThread = new Thread(PushMethod);
                PushThread.IsBackground = true;
                PushThread.Start();
            }
        }

        public static void CancelPushThread()
        {
            cts?.Cancel();

            PushThread = null;
            cts = null;
        }

        public static void UnregisterService(IObservableCopy feed)
        {
            if (feeds.Contains(feed))
            {
                feeds.Remove(feed);
            }

            if (feeds.Count == 0)
            {
                CancelPushThread();
            }
        }

        private static void PushMethod()
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
                                feed.CopyToObservable();
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
