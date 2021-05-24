using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Rest;
using Kucoin.NET.Websockets.Public;
using Kucoin.NET.Websockets;
using Kucoin.NET.Data.User;
using Kucoin.NET.Websockets.Private;
using Kucoin.NET.Observable;
using System.Windows;
using System.Text;
using System.Reflection;

namespace KuCoinApp
{
    public static class Program
    {
        private static KucoinBaseWebsocketFeed feed;
        public static KucoinBaseWebsocketFeed Feed
        {
            get => feed;
            set
            {
                if (feed != value)
                {
                    if (feed != null)
                        feed.DataReceived -= Feed_DataReceived;

                    feed = value;

                    if (feed != null)
                        feed.DataReceived += Feed_DataReceived;

                }

            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        static string PrintProperty(PropertyInfo pi, bool observable = true)
        {

            var osb = new StringBuilder();

            var attrs = pi.CustomAttributes;

            string json;
            string propName;
            string summary;

            var chars = pi.Name.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            summary = propName = new string(chars);
            
            chars[0] = char.ToLower(chars[0]);
            json = new string(chars);

            foreach (var attr in attrs)
            {

            }


            osb.AppendLine($"/// <summary>");
            osb.AppendLine($"/// {summary}");
            osb.AppendLine($"/// </summary>");







            return osb.ToString();
        }

        static void CreateTheThing()
        {
            string s = "";

            StringBuilder sb = new StringBuilder();








            Clipboard.SetText(s);



        }

        [STAThread]
        public static async Task TestMain(Kucoin.NET.Data.Interfaces.ICredentialsProvider cred = null)
        {
            //var bb = new ObservableOrderUnits();

            //bb.Add(new OrderUnit() { Price = 102660, Size = 20 });
            //bb.Add(new OrderUnit() { Price = 526360, Size = 201 });
            //bb.Add(new OrderUnit() { Price = 524562, Size = 202 });
            //bb.Add(new OrderUnit() { Price = 24542, Size = 203 });
            //bb.Add(new OrderUnit() { Price = 845670, Size = 204 });
            //bb.Add(new OrderUnit() { Price = 305240, Size = 205 });
            //bb.Add(new OrderUnit() { Price = 126640, Size = 206 });
            //bb.Add(new OrderUnit() { Price = 37260, Size = 207 });
            //bb.Add(new OrderUnit() { Price = 2600, Size = 208 });
            //bb.Add(new OrderUnit() { Price = 848670, Size = 204 });
            //bb.Add(new OrderUnit() { Price = 308690, Size = 205 });
            //bb.Add(new OrderUnit() { Price = 135640, Size = 206 });
            //bb.Add(new OrderUnit() { Price = 37060, Size = 207 });
            //bb.Add(new OrderUnit() { Price = 2050, Size = 208 });
            //bb.Add(new OrderUnit() { Price = 300940, Size = 205 });
            //bb.Add(new OrderUnit() { Price = 139940, Size = 206 });
            //bb.Add(new OrderUnit() { Price = 39330, Size = 207 });
            //bb.Add(new OrderUnit() { Price = 2640, Size = 208 });
            //bb.Add(new OrderUnit() { Price = 899670, Size = 204 });
            //bb.Add(new OrderUnit() { Price = 309540, Size = 205 });
            //bb.Add(new OrderUnit() { Price = 135740, Size = 206 });
            //bb.Add(new OrderUnit() { Price = 37460, Size = 207 });
            //bb.Add(new OrderUnit() { Price = 2630, Size = 208 });
            //bb.Add(new OrderUnit() { Price = 842560, Size = 204 });
            //bb.Add(new OrderUnit() { Price = 304740, Size = 205 });
            //bb.Add(new OrderUnit() { Price = 135780, Size = 206 });
            //bb.Add(new OrderUnit() { Price = 35860, Size = 207 });
            //bb.Add(new OrderUnit() { Price = 2660, Size = 208 });
            
            //var bb2 = bb.ToArray();

            if (cred == null)
            {
                var pin = await PinWindow.GetPin(App.Current.MainWindow);

                AllocConsole();

                cred = CryptoCredentials.LoadFromStorage(App.Current.Seed, pin, false);
            }
            else
            {
                AllocConsole();
            }

            if (cred == null)
            {
                Console.Clear();
                Console.WriteLine("Not Authorized");
                return;
            }

            //var ltest1 = new DataSet<string>()
            //{
            //    "Horse",
            //    "Tropical",
            //    "Lift",
            //    "Snow",
            //    "Baffle",
            //    "Coin",
            //    "Shoe"
            //};


            //var ltest2 = new DataSet<string>()
            //{
            //    "Horse",
            //    "Smell",
            //    "Anxiety",
            //    "Snow",
            //    "Baffle",
            //    "Drench",
            //    "Coin"
            //};

            var feed = new KlineFeed<KlineCandle>();

            Feed = feed;

            //feed.Pong += Feed_Pong;
            //feed.DataReceived += Feed_DataReceived;

            //await feed.Connect();

            //await feed.AddSymbol("ETH-USDT", KlineType.Min1);


            //var feed2 = new TickerFeed();

            ////feed2.DataReceived += Feed_DataReceived;

            //await feed2.MultiplexInit(feed, "tunnel2");

            //await feed.AddSymbols(new TickerSymbol[] { "BTC-USDT", "ETH-USDT", "XLM-USDT", "XLM-ETH" });

            //await feed2.AddSymbol("ORBS-USDT");



            //Console.WriteLine("Listening to XLM-USDT: ");

            var market = new Market();
            //var markets = await market.GetMarketList();

            //Console.Write(string.Join("\r\n", markets));

            //var ml = new SnapshotFeed();

            //ml.DataReceived += Feed_DataReceived;
            //await ml.Connect();


            //var klines = await market.GetKline("ETH-USDT", KlineType.Min1, startTime: DateTime.Now - new TimeSpan(1, 0, 0));

            await market.RefreshSymbolsAsync();

            await market.GetAllTickers();

            await market.RefreshAllTickers();

            await market.RefreshCurrenciesAsync();

            var stats = await market.Get24HourStats("ETH-USDT");

            var markets = await market.GetMarketList();

            var symbols = market.FilterSymbolsByCurrency("USDT");

            //foreach (var symbol in symbols.Values)
            //{
            //    Console.WriteLine(symbol.Symbol + " " + symbol.Name);
            //    if (symbol.EnableTrading)
            //    {
            //        Console.WriteLine("Tradable");
            //    }
            //    if (symbol.IsMarginEnabled)
            //    {
            //        Console.WriteLine("Leveraged Trading");
            //    }
            //    Console.WriteLine("Base Currency: " + symbol.BaseCurrency);
            //    Console.WriteLine("Fee Currency:  " + symbol.FeeCurrency);
            //    Console.WriteLine("------------------------------------------------------------");
            //}

            var sym = symbols["XLM-USDT"];

            //Console.WriteLine();
            //Console.WriteLine($"Now Getting Request for {sym.Name}: ");
            //Console.WriteLine();

            var ticker = await market.GetTicker((string)sym);

            //Console.WriteLine("Current Price: " + string.Format("${0:#,##0.00########}", ticker.Price));

            //Console.WriteLine();

            //Console.WriteLine("Now going to attempt API key log in to get user data: ");





            //var l2 = new Level2(cred);
            //var pl = await l2.GetPartList("ETH-USDT", 20);

            var u = new User(cred);

            var ledgeAcct = await u.GetAccountList("XLM", AccountType.Trading);

            var ledger = await u.GetAccountLedger("XLM");

            var ledgeProps = typeof(AccountLedgerItem).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var ctxProps = typeof(LedgerContext).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            foreach (var ledgerItem in ledger)
            {

                foreach (var prop1 in ledgeProps)
                {
                    if (prop1.PropertyType == typeof(LedgerContext))
                    {
                        var ctx = (LedgerContext)prop1.GetValue(ledgerItem);

                        if (ctx != null)
                        {
                            Console.WriteLine("Context");
                            Console.WriteLine("-------");

                            foreach (var prop2 in ctxProps)
                            {
                                var propVal2 = prop2.GetValue(ctx);

                                if (propVal2 != null)
                                {
                                    Console.WriteLine($"    {prop2.Name}: " + propVal2.ToString());
                                }


                            }
                            Console.WriteLine("-------");
                        }
                    }
                    else
                    {
                        var propVal1 = prop1.GetValue(ledgerItem);

                        if (propVal1 != null)
                        {
                            Console.WriteLine($"{prop1.Name}: " + propVal1.ToString());
                        }
                    }

                }

                //Console.WriteLine("Id: " + l.Id);

                //Console.WriteLine("Timestamp: " + l.Timestamp.ToString("G"));

                //Console.WriteLine("Business Type: " + l.BizType);
                //Console.WriteLine("Amount: " + l.Amount);

                //Console.WriteLine("Fee: " + l.Fee);

                Console.WriteLine("------------------------------------------");


            }
            //return;
            try
            {
                var accts = await u.GetAccountList();

                Console.WriteLine();
                Console.WriteLine("Login Succeeded!");
                Console.WriteLine();

                foreach (var acct in accts)
                {
                    await PrintAccount(acct, market, true);
                }

                var acc2 = await u.GetAccount(accts[0].Id);
                await PrintAccount(acc2, market, true);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Login Failed!");
                Console.WriteLine("Error Message: " + ex.Message);
            }

            Console.WriteLine();




            //Console.ReadKey();
        }

        private static async Task PrintAccount(Account acct, Market market = null, bool ifZero = false)
        {
            if (acct.Balance == 0 && !ifZero) return;

            decimal fiatval = 0.0M;
            if (market == null) market = new Market();
            Ticker ticker;

            if (acct.Currency != "USDT")
            {
                TradingSymbol sym = (TradingSymbol)(acct.Currency + "-USDT");

                Console.WriteLine($"Now fetching data for {sym.Name}: ");
                Console.WriteLine();

                ticker = null;

                try
                {
                    ticker = await market.GetTicker((string)sym);
                    Console.WriteLine("Current Price: " + string.Format("${0:#,##0.00########}", ticker.Price));
                    fiatval = acct.Balance * ticker.Price;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }

            Console.WriteLine("Account Id:            " + acct.Id);
            Console.WriteLine("Account Currency:      " + acct.Currency);
            Console.WriteLine("Account Balance:       " + acct.Balance.ToString("#,##0.00########"));

            if (fiatval != 0.0m)
                Console.WriteLine("Account Value (USDT):  " + string.Format("${0:#,##0.00########}", fiatval));

            Console.WriteLine("Account Type:          " + acct.TypeDescription);
            Console.WriteLine("------------------------------------------------------------");

        }

        private static void Feed_DataReceived(object sender, Kucoin.NET.Websockets.DataReceivedEventArgs e)
        {
            //return;
            while (!Monitor.TryEnter(printLock))
            {
                Task.Delay(10);
            }

            Console.WriteLine(e.Json);
            Console.WriteLine("--------------------------");

            Monitor.Exit(printLock);
        }

        static string printLock = "";

        //private static void Feed_Ticker(Ticker ticker)
        //{

        //    while (!Monitor.TryEnter(printLock))
        //    {
        //        Task.Delay(10);
        //    }

        //    Console.Write(ticker.Symbol + ": " + ticker.Price.ToString("#,##0.00000000") + "          \r");

        //    Monitor.Exit(printLock);
        //}

        private static void Feed_Pong(object sender, EventArgs e)
        {
            //return;
            while (!Monitor.TryEnter(printLock))
            {
                Task.Delay(10);
            }

            Console.WriteLine("");
            Console.WriteLine("Pong!");

            Monitor.Exit(printLock);
        }

        //private static void Feed_ReceiveJson(string jsonData)
        //{

        //    while (!Monitor.TryEnter(printLock))
        //    {
        //        Task.Delay(10);
        //    }

        //    Console.WriteLine(jsonData);
        //    Console.WriteLine("--------------------------");

        //    Monitor.Exit(printLock);
        //}

        public static async System.Threading.Tasks.Task<string> RequestConsent(string userMessage)
        {
            string returnMessage;

            if (String.IsNullOrEmpty(userMessage))
            {
                userMessage = "Please provide fingerprint verification.";
            }

            try
            {
                // Request the logged on user's consent via fingerprint swipe.
                var consentResult = await Windows.Security.Credentials.UI.UserConsentVerifier.RequestVerificationAsync(userMessage);
                
                switch (consentResult)
                {
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.Verified:
                        returnMessage = "Fingerprint verified.";
                        break;
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.DeviceBusy:
                        returnMessage = "Biometric device is busy.";
                        break;
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.DeviceNotPresent:
                        returnMessage = "No biometric device found.";
                        break;
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.DisabledByPolicy:
                        returnMessage = "Biometric verification is disabled by policy.";
                        break;
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.NotConfiguredForUser:
                        returnMessage = "The user has no fingerprints registered. Please add a fingerprint to the " +
                                        "fingerprint database and try again.";
                        break;
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.RetriesExhausted:
                        returnMessage = "There have been too many failed attempts. Fingerprint authentication canceled.";
                        break;
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.Canceled:
                        returnMessage = "Fingerprint authentication canceled.";
                        break;
                    default:
                        returnMessage = "Fingerprint authentication is currently unavailable.";
                        break;
                }
            }
            catch (Exception ex)
            {
                returnMessage = "Fingerprint authentication failed: " + ex.ToString();
            }

            return returnMessage;
        }
    }

}
