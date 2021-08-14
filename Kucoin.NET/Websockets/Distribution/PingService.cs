using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Kucoin.NET.Websockets.Distributable
{
    public static class PingService
    {

        private static readonly List<KucoinBaseWebsocketFeed> feeds = new List<KucoinBaseWebsocketFeed>();

        private static Thread PingerThread;

        private static CancellationTokenSource cts;

        static PingService()
        {
        }
        
        public static void RegisterService(KucoinBaseWebsocketFeed feed)
        {
            if (!feeds.Contains(feed))
            {
                feeds.Add(feed);    
            }

            if (PingerThread == null)
            {
                cts = new CancellationTokenSource();    

                PingerThread = new Thread(PingerMethod);
                PingerThread.IsBackground = true;   
                PingerThread.Start();   
            }
        }

        public static void CancelPingerThread()
        {
            cts?.Cancel();

            PingerThread = null;
            cts = null;
        }

        public static void UnregisterService(KucoinBaseWebsocketFeed feed)
        {
            if (feeds.Contains(feed))
            {
                feeds.Remove(feed); 
            }

            if (feeds.Count == 0)
            {
                CancelPingerThread();
            }
        }

        private static void PingerMethod()
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
                        if (feed.Token.Data.Servers[0].PingInterval > pingMax)
                        {
                            pingMax = feed.Token.Data.Servers[0].PingInterval;
                        }
                    }

                    for (int i = 0; i < pingMax; i += 100)
                    {
                        Task.Delay(100, cts.Token).ConfigureAwait(false).GetAwaiter().GetResult(); 
                        if (cts.IsCancellationRequested) return;

                        foreach (var feed in feeds)
                        {
                            if (i == feed.Token.Data.Servers[0].PingInterval)
                            {
                                _ = feed.Ping();
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
