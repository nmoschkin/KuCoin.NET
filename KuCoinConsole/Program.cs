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

using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using Kucoin.NET.Websockets.Distribution;
using System.Linq;

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


        public static ICredentialsProvider cred;

        static Kucoin.NET.Rest.Market market;
        
        static bool ready = false;

        static List<object> feeds = new List<object>();

        static int maxSymbolLen = 0;
        static int maxCurrencyLen = 0;
        
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        static DateTime start = DateTime.Now;
        
        public static event EventHandler TickersReady;

        public static void Main(string[] args)
        {

            // Analytics and crash reporting.
            AppCenter.Start("d364ea69-c1fa-4d0d-8c37-debaa05f91bc",
                   typeof(Analytics), typeof(Crashes));
            // Analytics and crash reporting.

            RunProgram();
        }

        public static void RunProgram() 
        {
            AllocConsole();

            Console.WindowWidth = 130;
            Console.WindowHeight = 60;

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
                    Application.Exit();
                }

                /**/

                KuCoin.Credentials.Add(cred);
            }

            var ast = market.GetAllTickers().ConfigureAwait(false). GetAwaiter().GetResult();

            var tickers = new List<AllSymbolsTickerItem>(ast.Ticker);
            tickers.Sort((a, b) =>
            {
                if (a.VolumneValue > b.VolumneValue) return -1;
                if (a.VolumneValue < b.VolumneValue) return 1;
                return 0;

            });

            

            // This changes the number of feeds per distributor:
            ParallelService.MaxTenants = 10;

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
            for (int h = 0; h < 5; h++)
            {
                syms.Add(tickers[h].Symbol);
            }


            Task.Run(async () =>
            {

                // 10 of the most popular trading symbols

                //syms.Sort((a, b) =>
                //{
                //    // sort symbols alphabetically
                //    return string.Compare(a, b);
                //});
             
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
                            curr = serviceFactory.EnableOrAddSymbol(sym, service, (tickerCount == 0 || (tickerCount++ % 10 != 0)));

                            if (curr == null) continue;

                            await curr.EnableLevel3();
                            await Task.Delay(10);

                            if (!services.Contains(curr))
                            {
                                services.Add(curr);
                            }

                            if (curr.Level3Feed != null && curr.Level3Feed.Connected)
                            {
                                if (!feeds.Contains(curr.Level3Feed))
                                {
                                    feeds.Add(curr.Level3Feed);
                                }

                                curr.Level3Feed.MonitorThroughput = true;
                                curr.Level3Observation.DiagnosticsEnabled = true;

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
                
                //if (!ready)
                //{
                //    // wait til all the feeds are calibrated before displaying anything

                //    while (!ready)
                //    {
                //        Task.Delay(delay).ConfigureAwait(false).GetAwaiter().GetResult();
                //    }
                //}

                // loop forever to keep the program alive.
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

                Console.ResetColor();
                Console.Write(headerText);

                foreach (var sitem in itemStrings)
                {
                    var s = sitem.Split("\r\n");

                    int i = s[0].IndexOf("Ask: ");
                    int j = s[0].IndexOf(" ", i + 5);
                    int k = s[0].IndexOf("Bid: ", j);
                    int l = s[0].IndexOf(" ", k + 5);

                    string s1 = s[0].Substring(0, i + 5);

                    Console.Write(s1);

                    Console.ForegroundColor = ConsoleColor.Green;
                    string s2 = s[0].Substring(i + 5, (j - i) - 1);
                    Console.Write(s2);
                    Console.ResetColor();

                    string s3 = s[0].Substring(j + 1, (k + 4) - j);

                    Console.Write(s3);
                    Console.ForegroundColor = ConsoleColor.Red;

                    string s4 = s[0].Substring(k + 5, (l - k) - 1);

                    Console.Write(s4);
                    Console.ResetColor();

                    string s5 = s[0].Substring(l + 1);

                    Console.WriteLine(s5);
                    Console.WriteLine(s[1]);
                    Console.WriteLine(s[2]);


                }

                Console.Write(footerText);

                // write the text to the console.
                //var text = readOut.ToString();

                //Console.Write(text);

                // restore the cursor position.
                if (Console.CursorTop != lpos) Console.CursorTop = lpos;
                if (Console.CursorLeft != 0) Console.CursorLeft = 0;

            }

            //_ = Task.Run(() =>
            //{
            //    Dispatcher.BeginInvokeOnMainThread((o) =>
            //    {
            //        RunProgram();
            //    });
            //});

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

        /// <summary>
        /// Write the current status of the feed to a <see cref="StringBuilder"/> object.
        /// </summary>
        /// <param name="timestamp">The current feed timestamp, or null for local now.</param>
        private static void WriteOut(ref string headerText, ref IEnumerable<string> itemText, ref string footerText, DateTime? timestamp = null)
        {
            Console.CursorVisible = false;
            if (timestamp == null) timestamp = DateTime.Now;
            List<double> pcts = new List<double>();
            List<double> mpcts = new List<double>();

            lock (lockObj)
            {
                decimal ba, bb;
                
                long biggrand = 0;
                long matchgrand = 0;

                foreach (var obs in Observers)
                {
                    var l3 = obs.Value.Level3Observation;
                    biggrand += l3.GrandTotal;
                    matchgrand += l3.MatchTotal;
                }

                pcts.Clear();
                
                int resetting = 0;
                int running = 0;
                int failed = 0;
                double minresettime = 0d;

                foreach (var obs in Observers)
                {
                    var l3 = obs.Value.Level3Observation;
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

                int z = 0;

                readOut.Clear();
                readOut.AppendLine($"Feed Time Stamp:    {timestamp:G}                 ");
                readOut.AppendLine($"Up Time:            {(DateTime.Now - start):G}                 ");
                readOut.AppendLine($"                                   ");
                readOut.AppendLine($"Feeds Running:      {running}                 ");
                readOut.AppendLine($"Feeds Initializing: {resetting}                 ");

                var failtext = $"Feeds Failed:       {failed}  ";
                if (resetting > 0 && minresettime > 0)
                {
                    failtext += $" (Next reset in {(minresettime/1000):#,##0} seconds)";
                }
                failtext += "                            ";

                readOut.AppendLine(failtext);

                readOut.AppendLine($"                                   ");

                foreach (var f in feeds)
                {
                    if (f is Level3 l3a)
                    {
                        readOut.AppendLine($"Throughput:                         {PrintFriendlySpeed((ulong)l3a.Throughput)}                        ");
                        readOut.AppendLine($"Queue Length:                       {l3a.QueueLength}                                                  ");
                        readOut.AppendLine($"Max Queue Length (Last 60 Seconds): {l3a.MaxQueueLengthLast60Seconds}                                  ");
                    }
                }

                readOut.AppendLine($"                                   ");

                headerText = readOut.ToString();

                int count = 0;
                var sortobs = new List<ISymbolDataService>(Observers.Values);
                var itemTexts = new List<string>();

                sortobs.Sort((a, b) =>
                {

                    if (a.Level3Observation.GrandTotal > b.Level3Observation.GrandTotal) return -1;
                    else if (a.Level3Observation.GrandTotal < b.Level3Observation.GrandTotal) return 1;
                    else return 0;
                });

                foreach (var obs in sortobs)
                {
                    var l3 = obs.Level3Observation;

                    if (l3.FullDepthOrderBook is null) continue;
                    if (++count > 10) break;

                    ba = ((IList<AtomicOrderStruct>)l3.FullDepthOrderBook.Asks)[0].Price;
                    bb = ((IList<AtomicOrderStruct>)l3.FullDepthOrderBook.Bids)[0].Price;

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

                    var text = $"{MinChars(obs.Symbol, maxSymbolLen)} - Best Ask: {MinChars(ba.ToString("#,##0.00######"), 12)} Best Bid: {MinChars(bb.ToString("#,##0.00######"), 12)} - {MinChars(currname, maxCurrencyLen)}  Total: {MinChars(l3.GrandTotal.ToString("#,##0"), 14)}";
                    text += $"\r\n{MinChars("", maxSymbolLen)} - Match Share: {MinChars(mpcts[z].ToString("##0") + "%", 4)}   Total Share: {MinChars(pcts[z++].ToString("##0") + "%", 4)}   State: " + MinChars(l3.State.ToString(), 14) + "  Queue Length: " + MinChars(l3.QueueLength.ToString(), 10);
                    text += "\r\n                                                      ";

                    itemTexts.Add(text);
                    readOut.AppendLine(text);
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

                if (Observers.Count - count > 0)
                {
                    ft.AppendLine($"Feeds Not Shown: {Observers.Count - count}             ");
                }

                ft.AppendLine($"                                                           ");
                ft.AppendLine($"Match Total: {matchgrand:#,##0}                            ");
                ft.AppendLine($"Grand Total: {biggrand:#,##0}                              ");
                ft.AppendLine($"                                                           ");
                ft.AppendLine($"Matches Per Second:      ~ {mps:#,###}                   ");
                ft.AppendLine($"Transactions Per Second: ~ {tps:#,###}                   ");

                footerText = ft.ToString();
                readOut.Append(ft);

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
}
