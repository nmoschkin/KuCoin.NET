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


            var l3 = new Level3(cred);

            Console.Write("Connecting...");

            l3.Connect().ContinueWith(async (t) =>
            {
                Console.WriteLine("Subscribing to ETH-USDT...");
                Console.WriteLine("");
                Console.WriteLine("");

                observers.Add("ADA-USDT", await l3.AddSymbol("ADA-USDT"));
                observers.Add("BTC-USDT", await l3.AddSymbol("BTC-USDT"));

                foreach (var obs in observers)
                {
                    obs.Value.NextObject += Observer_NextObject;
                }

            }).Wait();

            while (l3?.Connected ?? false)
            {
                Task.Delay(5).Wait();
            }

        }

        static decimal oba = 0M, obb = 0M, ba = 0M, bb = 0M;

        private static void Observer_NextObject(Kucoin.NET.Data.Websockets.Level3Update data)
        {
            ba = observers[data.Symbol].FullDepthOrderBook.Asks[0].Price;
            bb = observers[data.Symbol].FullDepthOrderBook.Bids[0].Price;

            if (ba != oba || bb != obb)
            {
                Console.Write($"Best Ask: {ba:#,###.00} : Best Bid: {bb:#,###.00} {data.Symbol}                        \r");
            }

            oba = ba;
            obb = bb;

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
    }
}
