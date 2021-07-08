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

        public string GetKey() => "yourkey";

        public string GetSecret() => "yoursecret";

        public string GetPassphrase() => "yourpassphrase";

    }


    public static class Program
    {
        static Dictionary<string, Level2Observation> observers = new Dictionary<string, Level2Observation>();

        static Level2 l2conn;

        static StringBuilder readOut = new StringBuilder();
        static object lockObj = new object();
        
        static Kucoin.NET.Rest.Market market;
        
        static bool ready = false;

        static SymbolTradeOrderFeed ticker;

        static void Main(string[] args)
        {

            Console.WriteLine("Loading Symbols and Currencies...");

            market = new Kucoin.NET.Rest.Market();
            market.RefreshSymbolsAsync().Wait();
            market.RefreshCurrenciesAsync().Wait();

            ICredentialsProvider cred;

            // Use simple credentials:
            cred = SimpleCredentials.Instance;


            // If you want to run the WPF app, and set up your credentials from there, 
            // you can uncomment this code and use the same pin you use in the WPF app.
            // You must configure your pin and credentials via the WPF app, first!

            /**/

            Console.WriteLine("Type your pin and press enter: ");

            var pin = Console.ReadLine();
            cred = CryptoCredentials.LoadFromStorage(Seed, pin);

            if (cred == null)
            {
                Console.WriteLine("Invalid credentials!");
            }

            /**/


            /* Testing Futures Ticker */
            /* Uncomment To Use */

            //ticker = new SymbolTradeOrderFeed(cred.AttachedAccount);

            //ticker.Connect().Wait();

            //ticker.DataReceived += Ticker_DataReceived;

            //ticker.AddSymbol("XBTUSDM").Wait();

            //while (ticker.Connected)
            //{
            //    Task.Delay(100).Wait();
            //}

            //return;
            /* Uncomment To Use */
            /* Testing Futures Ticker */



            // Create the new websocket client.
            l2conn = new Level2(cred);

            // We want to monitor how much data is coming through the feed.
            l2conn.MonitorThroughput = true;
            
            // Disable Observable/UI book updating for console app.
            l2conn.UpdateInterval = 0;

            // clear the console (if you use a pin, this will get it off the screen)
            Console.Clear();

            Console.WriteLine("Connecting...");

            // default update delay in milliseconds.
            // do not set this number too low.
            int delay = 100;

            l2conn.Connect().ContinueWith(async (t) =>
            {

                // 10 of the most popular trading symbols
                var syms = new List<string>(new string[] { "ETH-USDT", "XLM-USDT", "BTC-USDT", "ADA-USDT", "DOGE-USDT", "DOT-USDT", "UNI-USDT", "LTC-USDT", "LINK-USDT", "MATIC-USDT" });

                syms.Sort((a, b) =>
                {
                    // sort symbols alphabetically
                    return string.Compare(a, b);
                });

                foreach (var sym in syms)
                {
                    Console.WriteLine($"Subscribing to {sym} ...");

                    try
                    {
                        var obs = await l2conn.AddSymbol(sym);

                        while (obs.Calibrated == false)
                        {
                            await Task.Delay(delay);
                        }

                        observers.Add(sym, obs);
                    }
                    catch { }
                }

                // clear console to display data.
                Console.Clear();
                Console.CursorVisible = false;
                ready = true; 

            }).Wait();

            while (l2conn?.Connected ?? false)
            {
                if (!ready)
                {
                    // wait til all the feeds are calibrated before displaying anything

                    while (!ready)
                    {
                        Task.Delay(delay).Wait();
                    }
                }

                // loop forever to keep the program alive.
                Task.Delay(delay).Wait();

                // remember the cursor position on the screen
                int lpos = Console.CursorTop;
                
                // let's find the most current update date/time from all feeds

                DateTime ts = DateTime.MinValue;

                foreach (var obs in observers)
                {
                    if (obs.Value.FullDepthOrderBook.Timestamp > ts)
                    {
                        ts = obs.Value.FullDepthOrderBook.Timestamp;
                    }
                }

                // create the text.
                WriteOut(ts);

                // write the text to the console.
                Console.Write(readOut.ToString());


                // restore the cursor position.
                Console.CursorTop = lpos;
                Console.CursorLeft = 0;
            }

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

        /// <summary>
        /// Write the current status of the feed to a <see cref="StringBuilder"/> object.
        /// </summary>
        /// <param name="timestamp">The current feed timestamp, or null for local now.</param>
        private static void WriteOut(DateTime? timestamp = null)
        {
            if (timestamp == null) timestamp = DateTime.Now;

            lock (lockObj)
            {
                decimal ba, bb, vol;

                readOut.Clear();
                readOut.AppendLine($"Feed Time Stamp: {timestamp:G}                 ");
                readOut.AppendLine($"                                   ");

                foreach (var obs in observers)
                {
                    if (obs.Value.FullDepthOrderBook is null) continue;

                    //ba = ((IList<AtomicOrderStruct>)obs.Value.FullDepthOrderBook.Asks)[0].Price;
                    //bb = ((IList<AtomicOrderStruct>)obs.Value.FullDepthOrderBook.Bids)[0].Price;
                    ba = ((IList<OrderUnitStruct>)obs.Value.FullDepthOrderBook.Asks)[0].Price;
                    bb = ((IList<OrderUnitStruct>)obs.Value.FullDepthOrderBook.Bids)[0].Price;

                    var curr = "";

                    curr = market.Currencies[market.Symbols[obs.Value.Symbol].BaseCurrency].FullName;

                    var text = $"{MinChars(obs.Value.Symbol, 12)} - Best Ask: {MinChars(ba.ToString("#,##0.00######"), 12)} Best Bid: {MinChars(bb.ToString("#,##0.00######"), 12)} - {curr}            ";

                    readOut.AppendLine(text);
                }

                readOut.AppendLine("                                                           ");
                readOut.AppendLine($"Throughput: {PrintFriendlySpeed((ulong)l2conn.Throughput)}                           ");
                readOut.AppendLine("                                                           ");
                readOut.AppendLine($"Queue Length: {l2conn.QueueLength}                                                           ");
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
