using KuCoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Public
{
    /// <summary>
    /// Market symbol price ticker feed.
    /// </summary>
    public class MatchFeed : SymbolTopicFeedBase<MatchExecution>
    {
        public override bool IsPublic => true;

        public override string Subject => "trade.l3match";

        public override string Topic => "/market/match";

        /// <summary>
        /// Creates a new symbol ticker feed.
        /// </summary>
        public MatchFeed() : base(null, null, null)
        {
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public MatchFeed(IEnumerable<string> symbols) : base(null, null, null)
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
        public MatchFeed(string symbol) : base(null, null, null)
        {
            Connect().ContinueWith(async (t) =>
            {
                await SubscribeOne(symbol);
            });
        }
    }
}
