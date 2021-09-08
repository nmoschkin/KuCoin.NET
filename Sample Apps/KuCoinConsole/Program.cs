﻿using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Helpers;
using Kucoin.NET.Futures.Rest;
using Kucoin.NET.Futures.Websockets;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Services;
using System.ComponentModel;


using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Runtime.InteropServices;
using Kucoin.NET;
using System.Security.Cryptography.X509Certificates;
using Kucoin.NET.Websockets;

using System.Reflection;
using System.Threading;
using Kucoin.NET.Websockets.Distribution;
using System.Linq;
using System.Security.AccessControl;
using Newtonsoft.Json;
using Kucoin.NET.Data.Websockets;
using System.IO;
using Kucoin.NET.Websockets.Distribution.Services;
using Kucoin.NET.Rest;

namespace KuCoinConsole
{

    /// <summary>
    /// A simple credentials provider for you to test the program.
    /// </summary>
    public class SimpleCredentials : ICredentialsProvider
    {
        public static SimpleCredentials Instance { get; private set; }


        static SimpleCredentials()
        {
            // create instance 
            Instance = new SimpleCredentials();
        }


        private SimpleCredentials()
        {
            // only need 1 instance of this object.
        }

        public ICredentialsProvider AttachedAccount => throw new NotImplementedException();

        public bool GetFutures() => false;

        public bool GetSandbox() => false;

        /*  You must use a key to connect to the new Level 2 / Level 3 feeds. */

        // Enter your Kucoin Credentials Here
        public string GetKey() => "yourkey";

        // Enter your Kucoin Credentials Here
        public string GetSecret() => "yoursecret";

        // Enter your Kucoin Credentials Here
        public string GetPassphrase() => "yourpassphrase";

    }


    public static class Program
    {
        public static Dictionary<string, ISymbolDataService> Observers { get; set; } = new Dictionary<string, ISymbolDataService>();

        private static FileStream fslog;

        static object lockObj = new object();
        public static ICredentialsProvider cred;

        static ISymbolDataService service;
        static StringBuilder readOut = new StringBuilder();

        static int maxRows;
        static int msgidx = 0;
        static int sortmode = 0;
        static int sortorder = -1;

        static int scrollIndex = 0;

        static int maxScrollIndex = 0;
        static int currentConn = -1;

        static string subscribing = null;

        static List<string> messages = new List<string>();

        static Kucoin.NET.Rest.Market market;

        static List<object> feeds = new List<object>();

        static int maxSymbolLen = 0;
        static int maxCurrencyLen = 0;
        
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        static DateTime start = DateTime.Now;
        
        public static event EventHandler TickersReady;
        static List<string> usersymbols = null;
        static int feednum;
        static List<string> activeSymbols = new List<string>();

        static SnapshotFeed sf;

        //[STAThread]
        public static void Main(string[] args)
        {
            //Oid o, nn = new Oid();

            //o = "012345678901234567890123";
            //nn = o;
            //string p = o;
            //string z7 = o.ToString();
            //var bt = p == o;
            //bt = o == nn;
            //int pipi = o;

            //o.OrderId = "987654321098765432109876";
            //z7 = o;
            //z7 = o.OrderId;
            //pipi = (int)o.Hash;

            // Analytics and crash reporting.
            //AppCenter.Start("d364ea69-c1fa-4d0d-8c37-debaa05f91bc",
            //       typeof(Analytics), typeof(Crashes));
            // Analytics and crash reporting.

            Console.ResetColor();
            
            KuCoin.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            int x, y, z;

            x = 4;
            y = 24;
            z = 144;


            var r = FindGCF(-156, 32);

            RunProgram();
        }

        private static void Sf_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Json);
        }

        public static int FindGCF(params int[] values)
        {
            if (values == null || values.Length == 1) throw new ArgumentOutOfRangeException("Function takes 2 or more numbers.");
            int found = values[0];

            int i, c = values.Length;
            int min, max, mod;
            
            int lastfound = -1;

            for (i = 1; i < c; i++)
            {
                min = found;
                max = values[i];

                if (min > max)
                {
                    min = max;
                    max = values[i - 1];
                }

                mod = max % min;

                while (mod != 0)
                {
                    max = min;
                    min = mod;
                    mod = max % min;
                }

                found = min;

                if (lastfound == -1 || lastfound > found) lastfound = found;
                if (found == 1) break;
            }

            return lastfound;
        }

        public static void RunProgram() 
        {
            //if (GetConsoleWindow() == IntPtr.Zero)
            //{
            //    AllocConsole();
            //}

            try
            {
                Console.WindowWidth = 150;
                Console.WindowHeight = 60;
            }
            catch
            {

            }


            maxRows = 11;

            var em = new ConsoleEmulator();

            Console.Clear();

            Console.WriteLine("Initializing KuCoin Library (Loading Symbols and Currencies)...");
            KuCoin.Initialize();

            market = KuCoin.Market;

            foreach (var curr in market.Currencies)
            {
                if ((curr?.FullName ?? curr.Name).Length > maxCurrencyLen)
                {
                    maxCurrencyLen = (curr?.FullName ?? curr.Name).Length;
                }
            }

            foreach (var sym in market.Symbols)
            {
                if (sym.Name.Length > maxSymbolLen)
                {
                    maxSymbolLen = sym.Name.Length;
                }
            }

            cred = KuCoin.Credentials.FirstOrDefault();

            if (cred == null)
            {
                // Use simple credentials:
                cred = SimpleCredentials.Instance;

                // If you want to run the WPF app, and set up your credentials from there, 
                // you can uncomment this code and use the same pin you use in the WPF app.
                // You must configure your pin and credentials via the WPF app, first!

                /**/

                Console.WriteLine("Type your pin and press enter: ");

                var pin = Console.ReadLine();
                cred = CryptoCredentials.LoadFromStorage(Seed, pin);

                if (cred == null || !((CryptoCredentials)cred).IsFilled)
                {
                    Console.WriteLine("Invalid credentials!");
                    return;
                }

                /**/

                KuCoin.Credentials.Add(cred);
            }

            //var l3 = new Level3(cred);

            //var test = l3.ProvideInitialData("ETH-USDT").ConfigureAwait(false).GetAwaiter().GetResult();
            //return;

            Console.WriteLine("Number of feeds, or list of feeds separated by commas: ");

            var txtfeednum = Console.ReadLine();

            if (!int.TryParse(txtfeednum, out feednum))
            {
                string[] switches = txtfeednum.Split(',');

                usersymbols = new List<string>();

                foreach (var us in switches)
                {
                    var z = us.Trim().ToUpper();
                    if (market.Symbols.Contains(z))
                    {
                        usersymbols.Add(z);
                    }
                    else if (market.Symbols.Contains(z + "-USDT"))
                    {
                        usersymbols.Add(z + "-USDT");
                    }

                }

                if (usersymbols.Count == 0) 
                {
                    usersymbols = null;
                    Console.WriteLine("Couldn't figure that out, defaulting to 10.");
                    feednum = 10;
                }
            }


            RunTickers();
        }

        private static void RunTickers()
        {

            int maxTenants = Environment.ProcessorCount / 2;
            int maxSubscriptions = 0;
            int maxSharedConn = maxTenants * 2;

            ParallelService.MaxTenants = maxTenants;
            ParallelService.SleepDivisor = 4;
            ParallelService.WorkRepeat = 2;
            ParallelService.WorkIdleSleepTime = 1;

            var ast = market.GetAllTickers().ConfigureAwait(false). GetAwaiter().GetResult();

            var tickers = new List<AllSymbolsTickerItem>(ast.Ticker);
            Dictionary<string, AllSymbolsTickerItem> tickerdict = new Dictionary<string, AllSymbolsTickerItem>();
            
            tickers.Sort((a, b) =>
            {
                if (a.VolumeValue > b.VolumeValue) return -1;
                if (a.VolumeValue < b.VolumeValue) return 1;
                return 0;

            });
            if (feednum > tickers.Count) feednum = tickers.Count;

            tickers.RemoveRange(feednum, tickers.Count - feednum);

            List<List<AllSymbolsTickerItem>> buckets = new List<List<AllSymbolsTickerItem>>();

            List<AllSymbolsTickerItem> l2;

            // 
            maxSubscriptions = (int)Math.Ceiling((double)tickers.Count / maxSharedConn);

            for (int xx = 0; xx < tickers.Count; xx += maxSubscriptions)
            {
                for (int xy = 0; xy < maxSubscriptions; xy++)
                {
                    if (tickers.Count < (xy + xx + 1)) break;

                    if (buckets.Count < xy + 1)
                    {
                        l2 = new List<AllSymbolsTickerItem>();
                        buckets.Add(l2);

                    }
                    else
                    {
                        l2 = buckets[xy];
                    }

                    l2.Add(tickers[xx + xy]);
                }
            }

            tickers.Clear();
            foreach (var bucket in buckets)
            {
                foreach (var item in bucket)
                {
                    tickerdict.Add(item.Symbol, item);
                }
            }

            tickers.AddRange(tickerdict.Values);

            var serviceFactory = KuCoin.GetServiceFactory();
            List<ISymbolDataService> services = new List<ISymbolDataService>();
#if DEBUG
            int delay = 100;
#else
            int delay = 50;
#endif
            service = serviceFactory.CreateConnected(cred);
            services.Add(service);

            //var syms = new List<string>(new string[] { "XLM-ETH" });
            //var syms = new List<string>(new string[] { "DOT-USDT", "UNI-USDT", "SOL-USDT", "LINK-USDT", "ETC-USDT", "BCH-USDT", "WBTC-USDT", "FIL-USDT", "DAI-USDT", "AAVE-USDT", "ICP-USDT", "MATIC-USDT", "XRP-USDT", "DOGE-USDT", "KCS-USDT", "ETH-USDT", "XLM-USDT", "BTC-USDT", "ADA-USDT", "LTC-USDT" });

            int tickerCount = 0;
                        
            if (usersymbols != null)
            {
                activeSymbols = usersymbols;
            }
            else
            {
                for (int h = 0; h < feednum; h++)
                {
                    activeSymbols.Add(tickers[h].Symbol);
                }
            }

            maxScrollIndex = activeSymbols.Count - maxRows;

            if (fslog != null)
            {
                fslog.Dispose();
                fslog = null;
            }

            fslog = new FileStream(".\\message_log.txt", FileMode.Append, FileAccess.Write);

            _ = Task.Run(async () =>
            {
                ISymbolDataService curr = service;
                var ass = activeSymbols.ToArray();
                int c = ass.Length;

                string sym;

                for (int i = 0; i < c; i++)
                {
                    sym = ass[i];
                    if (!market.Symbols.Contains(sym))
                    {
                        lock (fslog)
                        {
                            fslog?.Write(Encoding.UTF8.GetBytes($"Skipping Unknown Symbol '{sym}' : {DateTime.Now:G}"));
                            fslog?.Flush();
                        }

                        continue;
                    }

                    lock (lockObj)
                    {
                        subscribing = ($"{sym}");
                    }

                    lock (fslog)
                    {
                        fslog?.Write(Encoding.UTF8.GetBytes($"Subscribing to {sym} ... {DateTime.Now:G}\r\n"));
                        fslog?.Flush();
                    }

                    try
                    {
                        bool pass;
                        lock (lockObj)
                        {
                            pass = Observers.ContainsKey(sym);
                        }
                        if (!pass)
                        {
                            try
                            {
                                var precurr = curr;
                                curr = serviceFactory.EnableOrAddSymbol(sym, curr, (tickerCount == 0 || (tickerCount % maxSharedConn != 0)));

                                if (curr == null)
                                {
                                    curr = precurr;
                                    curr = serviceFactory.EnableOrAddSymbol(sym, curr, (tickerCount == 0 || (tickerCount % maxSharedConn != 0)));

                                    if (curr == null)
                                    {
                                        curr = precurr;
                                        continue;
                                    }
                                }
                                tickerCount++;

                                if (!services.Contains(curr))
                                {
                                    services.Add(curr);
                                }

                                await curr.EnableLevel3Direct();
                                await Task.Delay(10);

                                if (curr.Level3Feed == null) 
                                {
                                    await curr.EnableLevel3Direct();
                                    await Task.Delay(10);
                                }

                            }
                            catch (Exception ex2)
                            {
                                var s = ex2.Message;
                                lock (fslog)
                                {
                                    fslog?.Write(Encoding.UTF8.GetBytes($"{ex2.GetType().Name}: {s} : {DateTime.Now:G}"));
                                    fslog?.Flush();
                                }

                            }


                            if (curr.Level3Feed != null)
                            {
                                lock (lockObj)
                                {
                                    if (!feeds.Contains(curr.Level3Feed))
                                    {
                                        curr.Level3Feed.DataReceived += Level3Feed_DataReceived;

                                        //curr.Level3Feed.DistributionStrategy = DistributionStrategy.Link;
                                        curr.Level3Feed.MonitorThroughput = true;
                                        lock (feeds)
                                        {
                                            feeds.Add(curr.Level3Feed);
                                        }
                                    }

                                    curr.Level3Observation.DiagnosticsEnabled = true;
                                    curr.Level3Observation.IsVolumeEnabled = true;

                                    Observers.Add(sym, curr);
                                }
                            }
                            else
                            {
                                lock (fslog)
                                {
                                    Console.WriteLine("Something's wrong.  We seem to be unable to connect. Aborting...");
                                    fslog?.Write(Encoding.UTF8.GetBytes($"Something's wrong.  We seem to be unable to connect. Aborting... {DateTime.Now:G}"));
                                    fslog?.Flush();
                                    fslog.Dispose();
                                    //fslog = null;
                                    Environment.Exit(-352);
                                }
                            }

                        }
                        else
                        {
                            lock (fslog)
                            {
                                fslog?.Write(Encoding.UTF8.GetBytes($"Skipping Duplicate '{sym}' : {DateTime.Now:G}"));
                                fslog?.Flush();
                            }

                        }
                    }
                    catch (Exception ex) 
                    {

                        var s = ex.Message;
                        lock (fslog)
                        {
                            fslog?.Write(Encoding.UTF8.GetBytes($"{ex.GetType().Name}: {s} : {DateTime.Now:G}"));
                            fslog?.Flush();
                        }
                    }
                }

                subscribing = null;

                // clear console to display data.
                Console.Clear();
                Console.CursorVisible = false;

                TickersReady?.Invoke(services, new EventArgs());

            });

            // loop until the connection is broken or the program is exited.
            while (true)
            {
                Console.CursorVisible = false;

                if (lwidth != Console.WindowWidth || lheight != Console.WindowHeight)
                {
                    lwidth = Console.WindowWidth;
                    lheight = Console.WindowHeight;

                    Console.BufferHeight = Console.WindowHeight;
                    Console.Clear();

                    continue;
                }

                if (Console.KeyAvailable)
                {
                    while(Console.KeyAvailable)
                    {

                        var key = Console.ReadKey();

                        if (key.Modifiers == ConsoleModifiers.Control)
                        {
                            if (key.Key == ConsoleKey.DownArrow)
                            {
                                if (msgidx < messages.Count - 5)
                                {
                                    ++msgidx;
                                }
                            }
                            else if (key.Key == ConsoleKey.UpArrow)
                            {
                                if (msgidx > 0)
                                {
                                    --msgidx;
                                }
                            }
                            if (key.Key == ConsoleKey.LeftArrow)
                            {
                                KlineLeft();
                            }
                            else if (key.Key == ConsoleKey.RightArrow)
                            {
                                KlineRight();
                            }

                        }
                        else
                        {
                            if (key.Key == ConsoleKey.RightArrow)
                            {
                                currentConn++;
                                if (currentConn >= feeds.Count)
                                {
                                    currentConn = -1;
                                }
                                scrollIndex = 0;
                                Console.Clear();
                                continue;
                            }
                            else if (key.Key == ConsoleKey.LeftArrow)
                            {
                                currentConn--;
                                if (currentConn < -1) currentConn = feeds.Count - 1;
                                scrollIndex = 0;
                                Console.Clear();
                                continue;
                            }
                            else if (key.Key == ConsoleKey.DownArrow)
                            {
                                if (scrollIndex < maxScrollIndex)
                                {
                                    ++scrollIndex;
                                }
                            }
                            else if (key.Key == ConsoleKey.UpArrow)
                            {
                                if (scrollIndex > 0)
                                {
                                    --scrollIndex;
                                }
                            }
                            else if (key.Key == ConsoleKey.PageDown)
                            {
                                if (scrollIndex < maxScrollIndex)
                                {
                                    scrollIndex += 10;
                                    if (scrollIndex > maxScrollIndex) scrollIndex = maxScrollIndex;
                                }
                            }
                            else if (key.Key == ConsoleKey.PageUp)
                            {
                                if (scrollIndex > 0)
                                {
                                    scrollIndex -= 10;
                                    if (scrollIndex < 0) scrollIndex = 0;
                                }
                            }
                            else if (key.Key == ConsoleKey.Home)
                            {
                                scrollIndex = 0;
                            }
                            else if (key.Key == ConsoleKey.End)
                            {
                                scrollIndex = maxScrollIndex;
                            }
                            else if (key.Key == ConsoleKey.V)
                            {
                                if (sortmode == 0) sortorder = sortorder * -1;
                                else sortmode = 0;
                            }
                            else if (key.Key == ConsoleKey.P)
                            {
                                if (sortmode == 1) sortorder = sortorder * -1;
                                else sortmode = 1;
                            }
                            else if (key.Key == ConsoleKey.A)
                            {
                                if (sortmode == 2) sortorder = sortorder * -1;
                                else sortmode = 2;
                            }
                            else if (key.Key == ConsoleKey.Q)
                            {
                                Console.CursorTop = Console.WindowHeight - 1;
                                Console.CursorVisible = true;
                                Environment.Exit(0);
                            }

                        }

                    }

                }

                Task.Delay(delay).ConfigureAwait(false).GetAwaiter().GetResult();


                // remember the cursor position on the screen
                int lpos = Console.CursorTop;
                Console.CursorTop = 0;
                Console.CursorLeft = 0;

                // let's find the most current update date/time from all feeds

                DateTime ts = DateTime.MinValue;

                try
                {
                    foreach (var obs in Observers)
                    {
                        if (obs.Value?.Level3Observation?.FullDepthOrderBook == null) continue;

                        if (obs.Value.Level3Observation.FullDepthOrderBook.Timestamp > ts)
                        {
                            ts = obs.Value.Level3Observation.FullDepthOrderBook.Timestamp;
                        }
                    }
                }
                catch
                {
                    continue;
                }

                string headerText = null;
                string footerText = null;
                IEnumerable<string> itemStrings = null;
                // create the text.

                WriteOut(ref headerText, ref itemStrings, ref footerText, ts);

                if (headerText == "DISCONNECTED") break;

                var headlines = headerText?.Split("\r\n")?.Length ?? 0;
                var footerlines = footerText?.Split("\r\n")?.Length ?? 0;

                var itemlines = itemStrings?.FirstOrDefault()?.Split("\r\n")?.Length ?? 3;

                int conh = Console.WindowHeight - (headlines + footerlines + 2);
                int maxitems = conh / itemlines;
                if (maxitems > Observers.Count) maxitems = Observers.Count;

                if (maxRows != maxitems)
                {
                    maxRows = maxitems;
                    maxScrollIndex = Observers.Count - maxRows;

                    if (scrollIndex > maxScrollIndex) scrollIndex = maxScrollIndex;
                    
                    WriteOut(ref headerText, ref itemStrings, ref footerText, ts);
                }

                Console.ResetColor();
                ColorConsole.Write(headerText);

                foreach (var sitem in itemStrings)
                {
                    ColorConsole.WriteLine(sitem);
                }

                ColorConsole.WriteLine(footerText);
                conh = Console.CursorTop;
                if (conh < Console.WindowHeight)
                {
                    for (int jz = conh; jz < Console.WindowHeight; jz++)
                    {
                        if (jz < Console.WindowHeight - 1) 
                            Console.WriteLine(MinChars("", Console.WindowWidth - 2));
                        else
                            Console.Write(MinChars("", Console.WindowWidth - 2));
                    }
                }
                // write the text to the console.
                //var text = readOut.ToString();

                //Console.Write(text);

                // restore the cursor position.
                if (Console.CursorTop != lpos) Console.CursorTop = lpos;
                if (Console.CursorLeft != 0) Console.CursorLeft = 0;

            }

            fslog?.Dispose();
            fslog = null;
            
            _ = Task.Run(() =>
            {
                Dispatcher.BeginInvokeOnMainThread((o) =>
                {
                    RunTickers();
                });
            });

        }

        private static void Level3Feed_DataReceived(object sender, DataReceivedEventArgs e)
        {
            var msg = JsonConvert.DeserializeObject<FeedMessage>(e.Json);
            int x = 1;

            lock (feeds)
            {
                foreach (var feed in feeds)
                {
                    if (sender == feed && feed is Level3 l3a)
                    {
                        lock (messages)
                        {
                            messages.Add($"{{Reset}}Feed {{White}}{x}{{Reset}}: {{Yellow}}{msg.Type} {{Cyan}}{msg.Subject} {msg.Topic} {{Blue}}({DateTime.Now:G}){{Reset}}");
                            msgidx = messages.Count >= 4 ? messages.Count - 5 : messages.Count - 1;


                            lock (fslog)
                            {
                                var tstr = ($"Feed {x}: {msg.Type} {msg.Subject} {msg.Topic} ({DateTime.Now:G})\r\n");
                                fslog?.Write(Encoding.UTF8.GetBytes(tstr));
                                fslog?.Flush();
                            }
                        }
                    }

                    x++;
                }
            }
        }

        private static void KlineRight()
        {
            lock (lockObj)
            {
                var obs1 = Observers.FirstOrDefault().Value ?? null;
                if (obs1 == null) return;

                IKlineType kt = obs1.Level3Observation.KlineType;

                IKlineType ktnext = KlineType.AllTypes.Where((x) => x.TimeSpan > kt.TimeSpan)?.FirstOrDefault() ?? default;

                if (ktnext == default)
                {
                    ktnext = KlineType.Min1;
                }

                foreach(var obs in Observers)
                {
                    obs.Value.Level3Observation.KlineType = ktnext;
                }
            }
        }

        private static void KlineLeft()
        {
            lock (lockObj)
            {
                var obs1 = Observers.FirstOrDefault().Value ?? null;
                if (obs1 == null) return;

                IKlineType kt = obs1.Level3Observation.KlineType;

                IKlineType ktnext = KlineType.AllTypes.Reverse().Where((x) => x.TimeSpan < kt.TimeSpan)?.FirstOrDefault() ?? default;

                if (ktnext == default)
                {
                    ktnext = KlineType.Week1;
                }

                foreach (var obs in Observers)
                {
                    obs.Value.Level3Observation.KlineType = ktnext;
                }
            }
        }

        private static void Ticker_FeedDataReceived(object sender, Kucoin.NET.Websockets.FeedDataReceivedEventArgs<Kucoin.NET.Futures.Data.Websockets.ContractMarketSnapshot> e)
        {

            // Console.Write($"{e.Data.Symbol}: Mark Price - {e.Data.MarkPrice:#,##0.00########}: Index Price - {e.Data.IndexPrice:#,##0.00########}                       \r");

            Console.Write($"{e.Data}          \r");
        }

        private static void Ticker_DataReceived(object sender, Kucoin.NET.Websockets.DataReceivedEventArgs e)
        {
        }
        
        static DateTime resetCounter = DateTime.UtcNow;
        static long tps = 0;
        static long mps = 0;

        static int lwidth = 0;
        static int lheight = 0;

        /// <summary>
        /// Write the current status of the feed to a <see cref="StringBuilder"/> object.
        /// </summary>
        /// <param name="timestamp">The current feed timestamp, or null for local now.</param>
        private static void WriteOut(ref string headerText, ref IEnumerable<string> itemText, ref string footerText, DateTime? timestamp = null)
        {
          
            if (timestamp == null) timestamp = DateTime.Now;
            List<double> pcts = new List<double>();
            List<double> mpcts = new List<double>();
            var itsb = new StringBuilder();
            int cwid = Console.WindowWidth;

            if (activeSymbols != null && maxRows > 0)
            {
                if (maxScrollIndex != activeSymbols.Count - maxRows)
                {
                    maxScrollIndex = activeSymbols.Count - maxRows;
                }
            }


            lock (lockObj)
            {
                decimal ba, bb, op, cp;
                
                long biggrand = 0;
                long matchgrand = 0;
                int ccount = -1;
                Level3 current = null;



                try
                {
                    foreach (Level3 feed in feeds)
                    {
                        ++ccount;
                        if (ccount == currentConn)
                        {
                            current = feed;
                            break;
                        }

                    }

                    foreach (var obs in Observers)
                    {
                        if (currentConn == -1 || obs.Value.Level3Feed == current)
                        {
                            var l3 = obs.Value.Level3Observation;
                            if (l3 != null)
                            {
                                biggrand += l3.GrandTotal;
                                matchgrand += l3.MatchTotal;
                            }
                        }
                    }
                }
                catch { }

                pcts.Clear();
                
                int resetting = 0;
                int running = 0;
                int failed = 0;
                double minresettime = 0d;
                ccount = -1;

                foreach (var obs in Observers)
                {
                    if (currentConn == -1 || obs.Value.Level3Feed == current)
                    {
                        var l3 = obs.Value.Level3Observation;

                        if (l3 == null)
                        {
                            continue;
                        }
                        if (l3.State == FeedState.Running)
                        {
                            running++;
                        }
                        else if (l3.Failure)
                        {
                            failed++;
                            if (l3.TimeUntilNextRetry is double t)
                            {
                                if (t < minresettime || minresettime == 0)
                                    minresettime = t;
                            }
                        }
                        else
                        {
                            resetting++;
                        }
                        pcts.Add(((double)l3.GrandTotal / (double)biggrand) * 100d);
                        mpcts.Add(((double)l3.MatchTotal / (double)matchgrand) * 100d);
                    }
                }

                int z = scrollIndex;

                readOut.Clear();
                readOut.WriteToEdgeLine($"Feed Time Stamp:    {{Green}}{timestamp:G}{{Reset}}");
                readOut.WriteToEdgeLine($"Up Time:            {{Blue}}{(DateTime.Now - start):G}{{Reset}}");
                readOut.WriteToEdgeLine($"");
                readOut.WriteToEdgeLine($"Feeds Running:      {{Green}}{running}{{Reset}}");
                readOut.WriteToEdgeLine($"Feeds Initializing: {{Yellow}}{resetting}{{Reset}}");

                var failtext = $"Feeds Failed:       {{Red}}{failed}{{Reset}}";
                if (minresettime > 0)
                {
                    failtext += $" (Next reset in {(minresettime/1000):#,##0} seconds)";
                }
                failtext += "";

                readOut.WriteToEdgeLine(failtext);
                readOut.WriteToEdgeLine($"");

                double through = 0d;
                int queue = -1;
                long maxqueue = 0;
                int linkstr = 0;
                ccount = -1;

                if (current == null)
                {
                    foreach (var f in feeds)
                    {
                        if (f is Level3 l3a)
                        {
                            if (!l3a.Connected)
                            {
                                continue;
                            }

                            through += l3a.Throughput;
                            if (!(f is Level3Direct))
                            {
                                queue += l3a.QueueLength;
                                if (l3a.MaxQueueLengthLast60Seconds > maxqueue)
                                    maxqueue += l3a.MaxQueueLengthLast60Seconds;

                            }
                        }
                    }
                }
                else
                {
                    through += current.Throughput;
                    if (!(current is Level3Direct))
                    {
                        queue += current.QueueLength;
                        if (current.MaxQueueLengthLast60Seconds > maxqueue)
                            maxqueue += current.MaxQueueLengthLast60Seconds;

                    }

                }

                lock (lockObj)
                {
                    if (subscribing != null)
                    {
                        readOut.WriteToEdgeLine($"Subscribing:                        {{White}}{subscribing} ({activeSymbols.IndexOf(subscribing)} / {activeSymbols.Count}){{Reset}}");
                    }
                }

                if (currentConn != -1)
                {
                    readOut.WriteToEdgeLine($"Total Connections:                  {{White}}{MinChars(feeds.Count.ToString(), 4)}{{Reset}}       ({{White}}Showing Connection: {{Blue}}@{currentConn+1}{{Reset}}{{Reset}})");
                }
                else
                {
                    readOut.WriteToEdgeLine($"Total Connections:                  {{White}}{MinChars(feeds.Count.ToString(), 4)}{{Reset}}");
                }
                readOut.WriteToEdgeLine($"Throughput:                         {{Green}}{PrintFriendlySpeed((ulong)through)}{{Reset}}");

                    
                if (queue != -1)
                {
                    if (currentConn == -1)
                    {
                        if (linkstr == 0)
                        {
                            readOut.WriteToEdgeLine($"Combined Queue Length:              {{Yellow}}{MinChars(queue.ToString(), 8)}{{Reset}}");
                            readOut.WriteToEdgeLine($"Max Queue Length (Last 60 Seconds): {{Red}}{maxqueue}{{Reset}}");
                        }
                        else if (linkstr == feeds.Count)
                        {
                            readOut.WriteToEdgeLine($"Combined Queue Length:              {{Green}}Link Distribution Strategy (No Main Queue){{Reset}}");
                            readOut.WriteToEdgeLine($"Max Queue Length (Last 60 Seconds): {{Red}}{maxqueue}{{Reset}}");
                        }
                        else
                        {
                            readOut.WriteToEdgeLine($"Combined Queue Length:              {{Yellow}}{MinChars(queue.ToString(), 8)} {{Green}}({linkstr} using Link Dist.) {{Reset}}");
                            readOut.WriteToEdgeLine($"Max Queue Length (Last 60 Seconds): {{Red}}{maxqueue}{{Reset}}");
                        }
                    }
                    else 
                    {
                        if (linkstr == 0)
                        {
                            readOut.WriteToEdgeLine($"Queue Length:                       {{Yellow}}{MinChars(queue.ToString(), 8)}{{Reset}}");
                            readOut.WriteToEdgeLine($"Max Queue Length (Last 60 Seconds): {{Red}}{maxqueue}{{Reset}}");
                        }
                        else if (linkstr == feeds.Count)
                        {
                            readOut.WriteToEdgeLine($"Queue Length:                       {{Green}}Link Distribution Strategy (No Main Queue){{Reset}}");
                            readOut.WriteToEdgeLine($"Max Queue Length (Last 60 Seconds): {{Red}}{maxqueue}{{Reset}}");
                        }
                        else
                        {
                            readOut.WriteToEdgeLine($"Queue Length:                       {{Yellow}}{MinChars(queue.ToString(), 8)} {{Green}}({linkstr} using Link Dist.) {{Reset}}");
                            readOut.WriteToEdgeLine($"Max Queue Length (Last 60 Seconds): {{Red}}{maxqueue}{{Reset}}");
                        }

                    }

                }

                readOut.WriteToEdgeLine($"");
                readOut.Append("Sort Order: ");

                string ordering = null;

                var sortobs = new List<ISymbolDataService>(Observers.Values.Where((item) =>
                {
                    return currentConn == -1 || item.Level3Feed == current;
                }));

                sortobs.Sort((a, b) =>
                {
                    try
                    {
                        switch (sortmode)
                        {
                            case 0:

                                if (a.Level3Observation.MarketVolume > b.Level3Observation.MarketVolume) return 1 * sortorder;
                                else if (a.Level3Observation.MarketVolume < b.Level3Observation.MarketVolume) return -1 * sortorder;
                                else break;

                            case 1:

                                if (a.Level3Observation.FullDepthOrderBook.Bids[0].Price > b.Level3Observation.FullDepthOrderBook.Bids[0].Price) return 1 * sortorder;
                                else if (a.Level3Observation.FullDepthOrderBook.Bids[0].Price < b.Level3Observation.FullDepthOrderBook.Bids[0].Price) return -1 * sortorder;
                                else break;
                        }

                        return string.Compare(a.Symbol, b.Symbol) * sortorder;
                    }
                    catch
                    {
                        return 0;
                    }

                });

                switch (sortmode)
                {
                    case 0:
                        ordering = ($"{{White}}Volume ");
                        break;
                    case 1:
                        ordering = ($"{{White}}Price ");
                        break;
                    case 2:
                        ordering = ($"{{White}}Alphabetically ");
                        break;
                }

                if (sortorder > 0)
                    ordering += ($"{{Green}}▲ {{Yellow}}Ascending{{Reset}}");
                else
                    ordering += ($"{{Red}}▼ {{Yellow}}Descending{{Reset}}");

                readOut.Append(MinChars(ordering, 127));

                if (sortobs.FirstOrDefault() is SymbolDataService firstData)
                {
                    var klineStr = firstData.Level3Observation.KlineType.ToString("G");
                    readOut.WriteToEdgeLine($"{{Reset}}Current K-Line: {{Cyan}}{klineStr}");
                }
                else
                {
                    readOut.WriteToEdgeLine($"");
                }

                readOut.WriteToEdgeLine($"");

                headerText = readOut.ToString();

                int count = 0;

                var itemTexts = new List<string>();

                int idx = scrollIndex;
                int obscount = sortobs.Count;

                if (idx > obscount - maxRows) idx = obscount - maxRows;
                if (idx < 0) idx = 0;
                z = idx;
                for (int vc = idx; vc < idx + maxRows; vc++)
                {
                    if (vc >= obscount) break;

                    var obs = sortobs[vc];
                    var l3 = obs.Level3Observation;
                    var ts = DateTime.MinValue;

                    int fidx = 0;
                    int cidx = 0;

                    if (currentConn == -1)
                    {
                        foreach (var feed in feeds)
                        {
                            if (feed is Level3 l3b)
                            {
                                if (l3b.ActiveFeeds.ContainsKey(obs.Symbol))
                                {
                                    fidx = cidx + 1;
                                    break;
                                }
                            }
                            cidx++;
                        }

                    }
                    else
                    {
                        fidx = currentConn + 1;
                    }

                    if (l3.FullDepthOrderBook is object)
                    {
                        ba = ((IList<AtomicOrderStruct>)l3.FullDepthOrderBook.Asks)[0].Price;
                        bb = ((IList<AtomicOrderStruct>)l3.FullDepthOrderBook.Bids)[0].Price;
                        ts = l3.FullDepthOrderBook.Timestamp;

                        op = l3.Candle.OpenPrice;
                        cp = l3.Candle.ClosePrice;

                    }
                    else
                    {
                        op = cp = ba = bb = 0;
                    }

                    var currname = "";
                    var bc = market.Symbols[obs.Symbol].BaseCurrency;

                    if (market.Currencies.Contains(bc))
                    {
                        currname = market.Currencies[bc].FullName;
                    }
                    else
                    {
                        currname = bc;
                    }

                    itsb.Clear();

                    var zt = "{Reset}-";
                    var t = "▲▼";

                    if (op > cp)
                    {
                        zt = "{Red}▼{Reset}";
                    }
                    else if (cp > op)
                    {
                        zt = "{Green}▲{Reset}";
                    }

                    itsb.WriteToEdgeLine($"{MinChars(obs.Symbol, maxSymbolLen)} {zt} Best Ask: {{Red}}{MinChars(ba.ToString("#,##0.00######"), 12)}{{Reset}} Best Bid: {{Green}}{MinChars(bb.ToString("#,##0.00######"), 12)}{{Reset}} {{Yellow}}{MinChars(currname, maxCurrencyLen)}{{Reset}}          Volume: {{Cyan}}{MinChars(l3.MarketVolume.ToString("#,##0.00##"), 14)}{{Reset}}");
                    
                    
                    if (l3.Parent.Connected == false)
                    {
                        itsb.WriteToEdgeLine($"{MinChars($"{{White}}{vc + 1} {{Red}}Feed Disconnected{{Reset}}", maxSymbolLen + 22)}");
                    }
                    else if (ba == 0)
                    {
                        itsb.WriteToEdgeLine($"{MinChars($"{{White}}{vc + 1} {{Yellow}}Initializing{{Reset}}", maxSymbolLen + 22)}");
                    }
                    else
                    {
                        if (queue == -1)
                        {
                            itsb.WriteToEdgeLine($"{MinChars($"{{White}}{vc + 1} {{Blue}}@{fidx}{{Reset}}", maxSymbolLen + 20)}   Match Share: {MinChars(mpcts[z].ToString("##0.##") + "%", 7)}   Total Share: {MinChars(pcts[z++].ToString("##0.##") + "%", 7)}   State: " + MinChars(l3.State.ToString(), 10) + "  Throughput: " + MinChars(PrintFriendlySpeed((ulong)l3.Throughput), 16) + $"{{Reset}} Timestamp: {{Blue}}{ts:G}{{Reset}}");
                        }
                        else
                        {
                            itsb.WriteToEdgeLine($"{MinChars($"{{White}}{vc + 1} {{Blue}}@{fidx}{{Reset}}", maxSymbolLen + 20)}   Match Share: {MinChars(mpcts[z].ToString("##0.##") + "%", 7)}   Total Share: {MinChars(pcts[z++].ToString("##0.##") + "%", 7)}   State: " + MinChars(l3.State.ToString(), 14) + "  Queue Length: " + MinChars(l3.QueueLength.ToString(), 10) + $"{{Reset}} Timestamp: {{Blue}}{ts:G}{{Reset}}");
                        }
                        //itsb.WriteToEdgeLine($"{MinChars($"{{White}}{vc + 1} ", maxSymbolLen + 7)} - Match Share: {MinChars(mpcts[z].ToString("##0") + "%", 4)}   Total Share: {MinChars(pcts[z++].ToString("##0") + "%", 4)}   State: " + MinChars(l3.State.ToString(), 14) + "  Queue Length: " + MinChars(l3.QueueLength.ToString(), 10) + $" {{Reset}}Feed: {{White}}{MinChars((fidx == 0) ? "N/A" : fidx.ToString(), 4)}");
                    }

                    itsb.WriteToEdge("");
                    itemTexts.Add(itsb.ToString());
                }

                // trades per second:

                if ((DateTime.UtcNow - resetCounter).TotalSeconds >= 1)
                {
                    mps = 0;
                    tps = 0;

                    resetCounter = DateTime.UtcNow;

                    foreach (var obs in sortobs)
                    {
                        var l3 = obs.Level3Observation;
                        mps += l3.MatchesPerSecond;
                        tps += l3.TransactionsPerSecond;

                    }
                }

                itemText = itemTexts;

                var ft = new StringBuilder();

                ft.WriteToEdgeLine("");

                if (obscount - count > 0)
                {
                    ft.WriteToEdgeLine($"Feeds Not Shown: {{Magenta}}{obscount - maxRows}{{Reset}}");
                }
                
                ft.WriteToEdgeLine($"");
                ft.WriteToEdgeLine($"Match Total: {{White}}{matchgrand:#,##0}{{Reset}}      ");
                ft.WriteToEdgeLine($"Grand Total: {{White}}{biggrand:#,##0}{{Reset}}        ");
                ft.WriteToEdgeLine($"                                                       ");
                ft.WriteToEdgeLine($"Matches Per Second:      ~ {{Cyan}}{mps:#,###}{{Reset}}");
                ft.WriteToEdgeLine($"Transactions Per Second: ~ {{Cyan}}{tps:#,###}{{Reset}}");
                ft.WriteToEdgeLine($"");
                ft.WriteToEdgeLine($"{{White}}Use Arrow Up/Arrow Down, Page Up/Page Down, Home/End to navigate the feed list. Ctrl+Arrow Up/Down scrolls the message log, below.{{Reset}}");
                ft.WriteToEdgeLine($"{{White}}Use Arrow Left/Arrow Right to switch between different connections.  Use Ctrl + Arrow Left/Arrow Right to change the K-Line.{{Reset}}");
                ft.WriteToEdgeLine($"{{White}}Press: (A) Sort Alphabetically, (P) Price, (V) Volume. Press again to reverse order. (Q) To Quit.");

                lock(messages)
                {
                    if (messages.Count > 0)
                    {
                        if (msgidx < 0) msgidx = 0;
                        int mc = messages.Count, mi, mg;
                        mg = msgidx;

                        ft.WriteToEdgeLine("");

                        for (mi = mg; mi < mc; mi++)
                        {
                            ft.WriteToEdgeLine(messages[mi]);
                            if (mi - mg >= 4) break;
                        }
                    }
                }

                footerText = ft.ToString();

            }
        }

        /// <summary>
        /// Print a padded string.
        /// </summary>
        /// <param name="text">The string to print.</param>
        /// <param name="minChars">The minimum string length.</param>
        /// <returns>A padding string.</returns>
        public static string MinChars(string text, int minChars)
        {
            var o = text;

            if (text.Length >= minChars) return o;
            o += new string(' ', minChars - text.Length);

            return o;
        }

        public static string Hvcyp
        {
            get
            {
                return CryptoCredentials.GetHvcyp().ToString("d");
            }
            set
            {
                var g = Guid.Parse(value);
                CryptoCredentials.GetHvcyp(g);
            }
        }

        public static Guid Seed
        {
            get => Guid.Parse(Hvcyp);
        }

        /// <summary>
        /// Prints a number value as a friendly byte speed in TiB, GiB, MiB, KiB or B.
        /// </summary>
        /// <param name="speed">The speed to format.</param>
        /// <param name="format">Optional numeric format for the resulting value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string PrintFriendlySpeed(ulong speed, string format = null, bool binary = false)
        {
            double fs;
            double spd = speed;
            string nom;

            if (binary)
            {
                if ((spd >= (1024L * 1024 * 1024 * 1024 * 1024 * 1024)))
                {
                    fs = (spd / (1024L * 1024 * 1024 * 1024 * 1024 * 1024));
                    nom = "Eb/s";
                    //wow
                }
                else if ((spd >= (1024L * 1024 * 1024 * 1024 * 1024)))
                {
                    fs = (spd / (1024L * 1024 * 1024 * 1024 * 1024));
                    nom = "Pb/s";
                    //wow
                }
                else if ((spd >= (1024L * 1024 * 1024 * 1024)))
                {
                    fs = (spd / (1024L * 1024 * 1024 * 1024));
                    nom = "Tb/s";
                    //wow
                }
                else if ((spd >= (1024 * 1024 * 1024)))
                {
                    fs = (spd / (1024 * 1024 * 1024));
                    nom = "Gb/s";
                    // still wow
                }
                else if ((spd >= (1024 * 1024)))
                {
                    fs = (spd / (1024 * 1024));
                    nom = "Mb/s";
                    // okay
                }
                else if ((spd >= (1024)))
                {
                    fs = (spd / (1024));
                    nom = "Kb/s";
                    // fine.
                }
                else
                {
                    fs = spd;
                    nom = "b/s";
                    // wow.
                }

            }
            else
            {
                if ((spd >= (1000L * 1000 * 1000 * 1000 * 1000 * 1000)))
                {
                    fs = (spd / (1000L * 1000 * 1000 * 1000 * 1000 * 1000));
                    nom = "Eb/s";
                    //wow
                }
                else if ((spd >= (1000L * 1000 * 1000 * 1000 * 1000)))
                {
                    fs = (spd / (1000L * 1000 * 1000 * 1000 * 1000));
                    nom = "Pb/s";
                    //wow
                }
                else if ((spd >= (1000L * 1000 * 1000 * 1000)))
                {
                    fs = (spd / (1000L * 1000 * 1000 * 1000));
                    nom = "Tb/s";
                    //wow
                }
                else if ((spd >= (1000 * 1000 * 1000)))
                {
                    fs = (spd / (1000 * 1000 * 1000));
                    nom = "Gb/s";
                    // still wow
                }
                else if ((spd >= (1000 * 1000)))
                {
                    fs = (spd / (1000 * 1000));
                    nom = "Mb/s";
                    // okay
                }
                else if ((spd >= (1000)))
                {
                    fs = (spd / (1000));
                    nom = "Kb/s";
                    // fine.
                }
                else
                {
                    fs = spd;
                    nom = "b/s";
                    // wow.
                }

            }

            if (format != null)
            {
                return System.Math.Round(fs, 2).ToString(format) + " " + nom;
            }
            else
            {
                return System.Math.Round(fs, 2) + " " + nom;
            }

        }
    }

    public static class ColorConsole
    {

        public static void WriteColor(string text, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = oldColor;
        }

        public static void WriteColorLine(string text, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }


        public static void Write(string text)
        {

            var stext = new StringBuilder();
            var sb = new StringBuilder();

            int i, c = text.Length;

            var oldColor = Console.ForegroundColor;

            for (i = 0; i < c; i++)
            {
                if (text[i] == '{')
                {
                    if (stext.Length != 0)
                    {
                        Console.Write(stext.ToString());
                        stext.Clear();
                    }

                    i++;
                    while (text[i] != '}')
                    {
                        sb.Append(text[i++]);
                    }

                    var color = sb.ToString();
                    if (color == "Reset")
                    {
                        Console.ResetColor();
                    }
                    else
                    {
                        var val = (ConsoleColor?)typeof(ConsoleColor).GetField(color)?.GetValue(null);

                        if (val is ConsoleColor cc)
                        {
                            Console.ForegroundColor = cc;
                        }
                    }
                    sb.Clear();
                }
                else
                {
                    stext.Append(text[i]);
                }
            }

            if (stext.Length > 0) Console.Write(stext);
            Console.ForegroundColor = oldColor;
        }

        public static void WriteLine(string text)
        {
            Write(text + "\r\n");
        }


        public static void WriteToEdge(this StringBuilder sb, string text)
        {
            int c = RealCount(text);
            int d = Console.WindowWidth - 2;

            if (c >= d)
            {
                if (c > d) text = text.Substring(0, d);
                sb.Append(text);

                return;
            }

            sb.Append(text);
            sb.Append(new string(' ', d - c));
        }

        public static void WriteToEdgeLine(this StringBuilder sb, string text)
        {
            int c = RealCount(text);
            int d = Console.WindowWidth - 2;

            if (c >= d)
            {
                if (c > d) text = text.Substring(0, d);
                sb.Append(text);

                return;
            }

            sb.Append(text);
            sb.AppendLine(new string(' ', d - c));
        }

        private static int RealCount(string str)
        {
            int i, c = str.Length;
            int d = 0;

            for (i = 0; i < c; i++)
            {
                if (str[i] == '{')
                {
                    while (str[i] != '}')
                    {
                        i++;
                    }
                }
                else
                {
                    d++;
                }

                
            }

            return d;
        }
    }

}
