using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace KuCoinConsole
{
    public static class Program
    {
        static Dictionary<string, Level3Observation> observers = new Dictionary<string, Level3Observation>();

        static Level3 l3conn;
        static void Main(string[] args)
        {

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

            string[] syms = new string[] { "ETH-USDT", "XLM-USDT", "BTC-USDT", "ADA-USDT", "DOGE-USDT" };

            l3conn.Connect().ContinueWith(async (t) =>
            {
                foreach (var sym in syms)
                {
                    Console.WriteLine($"Subscribing to {sym} ...");

                    try
                    {
                        var obs = await l3conn.AddSymbol(sym);
                        observers.Add(sym, obs);
                    }
                    catch { }
                }

                foreach (var obs in observers)
                {
                    obs.Value.NextObject += Observer_NextObject;
                }

                // clear console to display data.
                Console.Clear();
                Console.CursorVisible = false;

            }).Wait();

            while (l3conn?.Connected ?? false)
            {
                // loop forever to keep the program alive.
                Task.Delay(5).Wait();
            }

        }

        // keep track of time
        static long? updTime = null;

        private static void Observer_NextObject(Kucoin.NET.Data.Websockets.Level3Update data)
        {

            // We don't want to update the screen every single time we get new data.
            // If we do that, the data buffer will grow larger and we will not get to the end.
            // We need to give the thread time to catch up with the buffering thread.
            // So we will do every 100 milliseconds update.
            if (updTime != null)
            {
                if (DateTime.UtcNow.Ticks - updTime < (100 * 10_000)) return;
            }

            updTime = DateTime.UtcNow.Ticks;
            // 

            decimal ba, bb;
            
            // remember cursor position
            var x = Console.CursorTop;

            Console.WriteLine($"{data.Timestamp:G}                 ");
            Console.WriteLine($"                                   ");

            foreach (var obs in observers)
            {
                if (obs.Value.FullDepthOrderBook is null) continue;

                ba = obs.Value.FullDepthOrderBook.Asks[0].Price;
                bb = obs.Value.FullDepthOrderBook.Bids[0].Price;

                var text = $"{MinChars(obs.Value.Symbol, 9)} - Best Ask: {MinChars(ba.ToString("#,##0.00######"), 16)} : Best Bid: {MinChars(bb.ToString("#,##0.00######"), 16)}                         ";

                Console.WriteLine(text);
            }

            Console.WriteLine("                                                           ");
            Console.WriteLine($"Throughput: {PrintFriendlySpeed((ulong)l3conn.Throughput)}                           ");

            // restore cursor position
            Console.CursorTop = x;
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
