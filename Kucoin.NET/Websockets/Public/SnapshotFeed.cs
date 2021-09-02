using System;
using System.Collections.Generic;
using System.Text;

using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using System.Threading.Tasks;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Websockets.Public
{
    /// <summary>
    /// Implements the symbol snapshot feed (Level 2).
    /// </summary>
    public class SnapshotFeed : SymbolTopicFeedBase<SnapshotItem>
    {
        /// <summary>
        /// Instantiate a new symbol snapshot feed.
        /// </summary>
        public SnapshotFeed() : base(null, null, null)
        {
        }

        protected override string Subject => "trade.snapshot";

        protected override string Topic => "/market/snapshot";

        /// <summary>
        /// Instantiate and connect a new price snapshot feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public SnapshotFeed(IEnumerable<string> symbols) : base(null, null, null)
        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbols(symbols);
            });
        }

        /// <summary>
        /// Instantiate and connect a new price snapshot feed.
        /// </summary>
        /// <param name="symbol">The symbol to watch.</param>
        public SnapshotFeed(string symbol) : base(null, null, null)
        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
            });
        }


    }
}
