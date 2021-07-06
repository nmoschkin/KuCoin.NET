using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace KuCoinConsole
{
    public static class Program
    {
        static Dictionary<string, Level3Observation> observers = new Dictionary<string, Level3Observation>();

        static Level3 l3conn;

        static StringBuilder readOut = new StringBuilder();
        static object lockObj = new object();
        static Kucoin.NET.Rest.Market market;
        static bool ready = false;

        static void Main(string[] args)
        {

            Console.WriteLine("Loading Symbols and Currencies...");

            market = new Kucoin.NET.Rest.Market();
            market.RefreshSymbolsAsync().Wait();
            market.RefreshCurrenciesAsync().Wait();

            // You must configure your pin and credentials via the WPF app, first!
            Console.WriteLine("Type your pin and press enter: ");

            var pin = Console.ReadLine();
            var cred = CryptoCredentials.LoadFromStorage(Seed, pin);

            if (cred == null)
            {
                Console.WriteLine("Invalid credentials!");
            }

            l3conn = new Level3(cred);

            // We want to monitor how fast.
            l3conn.MonitorThroughput = true;
            
            // Disable Observable book updating for console app.
            l3conn.UpdateInterval = 0;

            // clear the pin off the screen
            Console.Clear();

            Console.WriteLine("Connecting...");

            l3conn.Connect().ContinueWith(async (t) =>
            {
                var syms = new List<string>(new string[] { "ETH-USDT", "XLM-USDT", "BTC-USDT", "ADA-USDT", "DOGE-USDT", "DOT-USDT", "UNI-USDT", "LTC-USDT", "LINK-USDT", "MATIC-USDT" });

                syms.Sort((a, b) =>
                {
                    return string.Compare(a, b);
                });

                foreach (var sym in syms)
                {
                    Console.WriteLine($"Subscribing to {sym} ...");

                    try
                    {
                        var obs = await l3conn.AddSymbol(sym);

                        while (obs.Calibrated == false)
                        {
                            await Task.Delay(10);
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


            // milliseconds
            int delay = 100;

            while (l3conn?.Connected ?? false)
            {
                if (!ready)
                {
                    while (!ready)
                    {
                        Task.Delay(delay).Wait();
                    }
                }

                // loop forever to keep the program alive.
                Task.Delay(delay).Wait();

                int lpos = Console.CursorTop;
                DateTime ts = DateTime.MinValue;

                foreach (var obs in observers)
                {
                    if (obs.Value.FullDepthOrderBook.Timestamp > ts)
                    {
                        ts = obs.Value.FullDepthOrderBook.Timestamp;
                    }
                }

                WriteOut(ts);
                Console.Write(readOut.ToString());

                Console.CursorTop = lpos;
                Console.CursorLeft = 0;
            }

        }

        private static void WriteOut(DateTime? timestamp = null)
        {
            if (timestamp == null) timestamp = DateTime.Now;

            lock (lockObj)
            {
                decimal ba, bb;

                readOut.Clear();
                readOut.AppendLine($"{timestamp:G}                 ");
                readOut.AppendLine($"                                   ");

                foreach (var obs in observers)
                {
                    if (obs.Value.FullDepthOrderBook is null) continue;

                    ba = obs.Value.FullDepthOrderBook.Asks[0].Price;
                    bb = obs.Value.FullDepthOrderBook.Bids[0].Price;

                    var curr = "";

                    curr = market.Currencies[market.Symbols[obs.Value.Symbol].BaseCurrency].FullName;

                    var text = $"{MinChars(obs.Value.Symbol, 12)} - Best Ask: {MinChars(ba.ToString("#,##0.00######"), 16)} : Best Bid: {MinChars(bb.ToString("#,##0.00######"), 16)}  {curr}                       ";

                    readOut.AppendLine(text);
                }

                readOut.AppendLine("                                                           ");
                readOut.AppendLine($"Throughput: {PrintFriendlySpeed((ulong)l3conn.Throughput)}                           ");
                readOut.AppendLine("                                                           ");
                readOut.AppendLine($"Queue Length: {l3conn.QueueLength}                                                           ");
            }
        }

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
