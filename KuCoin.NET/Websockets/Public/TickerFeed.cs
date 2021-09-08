using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Websockets.Public
{
    /// <summary>
    /// Market symbol price ticker feed.
    /// </summary>
    public class TickerFeed : SymbolTopicFeedBase<Ticker>
    {
        public override bool IsPublic => true;

        public override string Subject => "trade.ticker";

        public override string Topic => "/market/ticker";

        /// <summary>
        /// Creates a new symbol ticker feed.
        /// </summary>
        public TickerFeed() : base(null, null, null)
        {
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public TickerFeed(IEnumerable<string> symbols) : base(null, null, null)
        {
            Connect().ContinueWith(async (t) =>
            {
                await SubscribeMany(symbols);
            });
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbol">The symbol to watch.</param>
        public TickerFeed(string symbol) : base(null, null, null)
        {
            Connect().ContinueWith(async (t) =>
            {
                await SubscribeOne(symbol);
            });
        }
    }
}
