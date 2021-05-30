using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Data.Helpers;

namespace Kucoin.NET.Websockets.Private
{
    public class Level3Feed<T> : SymbolTopicFeedBase<Level2Update> where T: IAtomicOrderUnit
    {
        public override bool IsPublic => throw new NotImplementedException();

        protected override string Subject => throw new NotImplementedException();

        protected override string Topic => throw new NotImplementedException();

        protected override Task HandleMessage(FeedMessage msg)
        {
            throw new NotImplementedException();
        }


        public Level3Feed(
          string key,
          string secret,
          string passphrase,
          bool isSandbox = false)
          : base(key, secret, passphrase, isSandbox)
        {
        }

        internal Level3Feed() : base(null)
        {
        }

        public Level3Feed(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public Level3Feed(IEnumerable<string> symbols,
          string key,
          string secret,
          string passphrase,
          bool isSandbox = false)
            : base(key, secret, passphrase, isSandbox)
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
        public Level3Feed(string symbol,
          string key,
          string secret,
          string passphrase,
          bool isSandbox = false)
            : base(key, secret, passphrase, isSandbox)

        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
            });
        }


        protected async Task<OrderBook<T>> GetAtomicOrder(string symbol)
        {
            var curl = $"/api/v3/market/orderbook/level3?symbol={symbol}";

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false);
            return jobj.ToObject<OrderBook<T>>();
        }

    }
}
