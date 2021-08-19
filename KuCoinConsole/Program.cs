using Kucoin.NET.Websockets.Observations;
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

        static Level2 l2conn;
        static ISymbolDataService service;
        static StringBuilder readOut = new StringBuilder();
        static object lockObj = new object();

        static int maxRows;

        public static ICredentialsProvider cred;

        static Kucoin.NET.Rest.Market market;
        
        static bool ready = false;
        
        static int sortmode = 0;
        static int sortorder = -1;

        static List<object> feeds = new List<object>();

        static int maxSymbolLen = 0;
        static int maxCurrencyLen = 0;
        
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        static DateTime start = DateTime.Now;
        
        public static event EventHandler TickersReady;

        static int scrollIndex = 0;

        static int maxScrollIndex = 0;
        static List<string> usersymbols = null;
        static int feednum;

        //[STAThread]
        public static void Main(string[] args)
        {
           
            // Analytics and crash reporting.
            //AppCenter.Start("d364ea69-c1fa-4d0d-8c37-debaa05f91bc",
            //       typeof(Analytics), typeof(Crashes));
            // Analytics and crash reporting.

            RunProgram();
        }

        public static void RunProgram() 
        {
            //if (GetConsoleWindow() == IntPtr.Zero)
            //{
            //    AllocConsole();
            //}

            Console.WindowWidth = 130;
            Console.WindowHeight = 60;
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

            // This changes the number of feeds per distributor:
            ParallelService.MaxTenants = Environment.ProcessorCount;
            List<AllSymbolsTickerItem> l2;

            for (int xx = 0; xx < tickers.Count; xx += ParallelService.MaxTenants)
            {

                for (int xy = 0; xy < ParallelService.MaxTenants; xy++)
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

            var syms = new List<string>();

            if (usersymbols != null)
            {
                syms = usersymbols;
            }
            else
            {
                for (int h = 0; h < feednum; h++)
                {
                    syms.Add(tickers[h].Symbol);
                }
            }

            maxScrollIndex = syms.Count - maxRows;

            Task.Run(async () =>
            {

                int c = 0;
              
                ISymbolDataService curr = service;

                foreach (var sym in syms)
                {
                    if (!market.Symbols.Contains(sym))
                    {
                        continue;
                        //throw new KeyNotFoundException($"The trading symbol '{sym}' does not exist on the KuCoin exchange.");
                    }

                    Console.WriteLine($"Subscribing to {sym} ...");

                    try
                    {
                        if (!Observers.ContainsKey(sym))
                        {
                            curr = serviceFactory.EnableOrAddSymbol(sym, service, (tickerCount == 0 || (tickerCount % ParallelService.MaxTenants != 0)));
                            tickerCount++;
                            if (curr == null) continue;

                            await curr.EnableLevel3();
                            await Task.Delay(10);

                            if (!services.Contains(curr))
                            {
                                services.Add(curr);
                            }

                            if (curr.Level3Feed != null)
                            {
                                curr.Level3Feed.DistributionStrategy = DistributionStrategy.Link;

                                if (!feeds.Contains(curr.Level3Feed))
                                {
                                    feeds.Add(curr.Level3Feed);
                                }

                                curr.Level3Feed.MonitorThroughput = true;
                                curr.Level3Observation.DiagnosticsEnabled = true;
                                curr.Level3Observation.IsVolumeEnabled = true;

                                Observers.Add(sym, curr);
                            }
                            else
                            {
                                Console.WriteLine("Something's wrong.  We seem to be unable to connect. Aborting...");
                                return;
                            }

                        }
                    }
                    catch { }
                }

                // clear console to display data.
                Console.Clear();
                Console.CursorVisible = false;
                ready = true;

                TickersReady?.Invoke(services, new EventArgs());

            }).ConfigureAwait(false).GetAwaiter().GetResult();

            // loop until the connection is broken or the program is exited.
            while (service?.Level3Feed?.Connected ?? false)
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
                        if (key.Key == ConsoleKey.DownArrow)
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

                    }

                }

                Task.Delay(delay).ConfigureAwait(false).GetAwaiter().GetResult();


                // remember the cursor position on the screen
                int lpos = Console.CursorTop;
                Console.CursorTop = 0;
                Console.CursorLeft = 0;

                // let's find the most current update date/time from all feeds

                DateTime ts = DateTime.MinValue;

                foreach (var obs in Observers)
                {
                    if (obs.Value?.Level3Observation?.FullDepthOrderBook == null) continue;

                    if (obs.Value.Level3Observation.FullDepthOrderBook.Timestamp > ts)
                    {
                        ts = obs.Value.Level3Observation.FullDepthOrderBook.Timestamp;
                    }
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

            _ = Task.Run(() =>
            {
                Dispatcher.BeginInvokeOnMainThread((o) =>
                {
                    RunTickers();
                });
            });

        }

        private static void Ticker_FeedDataReceived(object sender, Kucoin.NET.Websockets.FeedDataReceivedEventArgs<Kucoin.NET.Futures.Data.Websockets.ContractMarketSnapshot> e)
        {

            // Console.Write($"{e.Data.Symbol}: Mark Price - {e.Data.MarkPrice:#,##0.00########}: Index Price - {e.Data.IndexPrice:#,##0.00########}                       \r");

            Console.Write($"{e.Data}          \r");
        }

        private static void Ticker_DataReceived(object sender, Kucoin.NET.Websockets.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Json);
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

            lock (lockObj)
            {
                decimal ba, bb;
                
                long biggrand = 0;
                long matchgrand = 0;

                foreach (var obs in Observers)
                {
                    var l3 = obs.Value.Level3Observation;
                    if (l3 != null)
                    {
                        biggrand += l3.GrandTotal;
                        matchgrand += l3.MatchTotal;
                    }
                }

                pcts.Clear();
                
                int resetting = 0;
                int running = 0;
                int failed = 0;
                double minresettime = 0d;
                int obscount = Observers.Count;

                foreach (var obs in Observers)
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
                int queue = 0;
                long maxqueue = 0;
                int linkstr = 0;

                foreach (var f in feeds)
                {
                    if (f is Level3 l3a)
                    {
                        if (!l3a.Connected)
                        {
                            foreach (var f2 in feeds)
                            {
                                if (f2 is Level3 l3b)
                                {
                                    l3b.Dispose();
                                }
                            }
                            headerText = "DISCONNECTED";
                            return;
                        }

                        through += l3a.Throughput;
                        queue += l3a.QueueLength;
                    
                        if (l3a.DistributionStrategy == DistributionStrategy.Link)
                        {
                            linkstr++;
                        }

                        if (l3a.MaxQueueLengthLast60Seconds > maxqueue)
                            maxqueue += l3a.MaxQueueLengthLast60Seconds;

                    }
                }

                if (through != 0d)
                {
                    readOut.WriteToEdgeLine($"Throughput:                         {{Green}}{PrintFriendlySpeed((ulong)through)}{{Reset}}");
                 
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

                readOut.WriteToEdgeLine($"");
                readOut.Append("Sort Order: ");
                switch (sortmode)
                {
                    case 0:
                        readOut.Append($"Volume ");
                        break;
                    case 1:
                        readOut.Append($"Price ");
                        break;
                    case 2:
                        readOut.Append($"Alphabetically ");
                        break;
                }

                if (sortorder > 0) 
                    readOut.WriteToEdgeLine($"Ascending");
                else
                    readOut.WriteToEdgeLine($"Descending");

                readOut.WriteToEdgeLine($"");

                headerText = readOut.ToString();

                int count = 0;
                var sortobs = new List<ISymbolDataService>(Observers.Values);
                var itemTexts = new List<string>();

                sortobs.Sort((a, b) =>
                {
                    switch(sortmode)
                    {
                        case 0:

                            if (a.Level3Observation.SortingVolume > b.Level3Observation.SortingVolume) return 1 * sortorder;
                            else if (a.Level3Observation.SortingVolume < b.Level3Observation.SortingVolume) return -1 * sortorder;
                            else break;

                        case 1:

                            if (a.Level3Observation.FullDepthOrderBook.Bids[0].Price > b.Level3Observation.FullDepthOrderBook.Bids[0].Price) return 1 * sortorder;
                            else if (a.Level3Observation.FullDepthOrderBook.Bids[0].Price < b.Level3Observation.FullDepthOrderBook.Bids[0].Price) return -1 * sortorder;
                            else break;
                    }

                    return string.Compare(a.Symbol, b.Symbol) * sortorder;

                });

                int idx = scrollIndex;
                
                if (idx > obscount - maxRows) idx = obscount - maxRows;
                if (idx < 0) idx = 0;

                for (int vc = idx; vc < idx + maxRows; vc++)
                {
                    if (vc >= obscount) break;

                    var obs = sortobs[vc];
                    var l3 = obs.Level3Observation;

                    if (l3.FullDepthOrderBook is object)
                    {
                        ba = ((IList<AtomicOrderStruct>)l3.FullDepthOrderBook.Asks)[0].Price;
                        bb = ((IList<AtomicOrderStruct>)l3.FullDepthOrderBook.Bids)[0].Price;
                    }
                    else
                    {
                        ba = bb = 0;
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


                    itsb.WriteToEdgeLine($"{MinChars(obs.Symbol, maxSymbolLen)} - Best Ask: {{Red}}{MinChars(ba.ToString("#,##0.00######"), 12)}{{Reset}} Best Bid: {{Green}}{MinChars(bb.ToString("#,##0.00######"), 12)}{{Reset}} - {{Yellow}}{MinChars(currname, maxCurrencyLen)}{{Reset}}  Volume: {{Cyan}}{MinChars(l3.SortingVolume.ToString("#,##0.00"), 14)}{{Reset}}");
                    
                    if (ba == 0)
                    {
                        itsb.WriteToEdgeLine($"\r\n{MinChars($"{{White}}{vc + 1} {{Yellow}}Initializing{{Reset}}", maxSymbolLen + 22)}");
                    }
                    else
                    {
                        itsb.WriteToEdgeLine($"\r\n{MinChars($"{{White}}{vc + 1} ", maxSymbolLen + 7)} - Match Share: {MinChars(mpcts[z].ToString("##0") + "%", 4)}   Total Share: {MinChars(pcts[z++].ToString("##0") + "%", 4)}   State: " + MinChars(l3.State.ToString(), 14) + "  Queue Length: " + MinChars(l3.QueueLength.ToString(), 10));
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

                    foreach (var obs in Observers)
                    {
                        var l3 = obs.Value.Level3Observation;
                        mps += l3.MatchesPerSecond;
                        tps += l3.TransactionsPerSecond;

                    }
                }

                itemText = itemTexts;

                var ft = new StringBuilder();

                ft.WriteToEdgeLine("");

                if (Observers.Count - count > 0)
                {
                    ft.WriteToEdgeLine($"Feeds Not Shown: {{Magenta}}{Observers.Count - maxRows}{{Reset}}");
                }
                
                ft.WriteToEdgeLine($"");
                ft.WriteToEdgeLine($"Match Total: {{White}}{matchgrand:#,##0}{{Reset}}");
                ft.WriteToEdgeLine($"Grand Total: {{White}}{biggrand:#,##0}{{Reset}}");
                ft.WriteToEdgeLine($"");
                ft.WriteToEdgeLine($"Matches Per Second:      ~ {{Cyan}}{mps:#,###}{{Reset}}");
                ft.WriteToEdgeLine($"Transactions Per Second: ~ {{Cyan}}{tps:#,###}{{Reset}}");
                ft.WriteToEdgeLine($"");
                ft.WriteToEdgeLine($"{{White}}Use Arrow Up/Arrow Down, Page Up/Page Down, Home/End to navigate the feed list.{{Reset}}");
                ft.WriteToEdgeLine($"{{White}}Press: (A) Sort Alphabetically, (P) Price, (V) Volume. Press again to reverse order.");

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
