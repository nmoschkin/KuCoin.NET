using System;
using System.Collections.Generic;
using System.Text;

using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using System.Threading.Tasks;
using KuCoin.NET.Helpers;

namespace KuCoin.NET.Websockets.Public
{
    /// <summary>
    /// Implements the symbol snapshot feed (Level 2).
    /// </summary>
    public class SnapshotFeed : SymbolTopicFeedBase<SnapshotItem>
    {
        /// <summary>
        /// Instantiate a new symbol snapshot feed.
        /// </summary>
        public SnapshotFeed(ICredentialsProvider cred) : base(cred)
        {
        }
        public override bool IsPublic => false;
        public override string Subject => "trade.snapshot";

        public override string Topic => "/market/snapshot";

        /// <summary>
        /// Instantiate and connect a new price snapshot feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public SnapshotFeed(IEnumerable<string> symbols) : base(null, null, null)
        {
            Connect().ContinueWith(async (t) =>
            {
                await SubscribeMany(symbols);
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
                await SubscribeOne(symbol);
            });
        }


    }
}
