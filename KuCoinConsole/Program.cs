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
        static ILevel3OrderBookProvider observer;

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
                observer = await l3.AddSymbol("ETH-USDT");

                observer.NextObject += Observer_NextObject;

            }).Wait();

            while (l3?.Connected ?? false)
            {
                Task.Delay(5).Wait();
            }

        }

        private static void Observer_NextObject(Kucoin.NET.Data.Websockets.Level3Update data)
        {
            Console.WriteLine(data.Price);
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
