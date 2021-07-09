using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Helpers;
using Kucoin.NET.Data.Order;

namespace Kucoin.NET.Websockets.Public
{
    /// <summary>
    ///  Level 3 Match Engine Feed
    /// </summary>
    public class Level3 : Level3Base<ObservableAtomicOrderBook<ObservableAtomicOrderUnit>, ObservableAtomicOrderUnit, KeyedAtomicOrderBook<AtomicOrderStruct>, AtomicOrderStruct, Level3Update, Level3Observation>
    {
        
        public Level3(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        public Level3(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
        }



        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public Level3(IEnumerable<string> symbols, ICredentialsProvider credProvider) : base(credProvider)
        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbols(symbols);
            });
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbol">The symbol to watch.</param>
        public Level3(string symbol, ICredentialsProvider credProvider) : base(credProvider)

        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
            });
        }


        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public Level3(IEnumerable<string> symbols, string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbols(symbols);
            });
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbol">The symbol to watch.</param>
        public Level3(string symbol, string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)

        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
            });
        }

        public override bool IsPublic => false;

        public override string Subject => throw new NotImplementedException();

        public override string Topic => "/spotMarket/level3";

        public override string OrderBookEndPoint => "/api/v3/market/orderbook/level3";

        protected override Level3Observation CreateNewObservation(string symbol)
        {
            return new Level3Observation(this, symbol, defaultPieces)
            {
                AutoPushEnabled = (UpdateInterval != 0)
            };
        }

    }

}
