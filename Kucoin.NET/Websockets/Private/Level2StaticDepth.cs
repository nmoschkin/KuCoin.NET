using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Private
{

    /// <summary>
    /// The depth of the market for the feed.
    /// </summary>
    public enum Level2Depth
    {
        /// <summary>
        /// 5 Best Asks/Bids
        /// </summary>
        Depth5 = 5,

        /// <summary>
        /// 50 Best Asks/Bids
        /// </summary>
        Depth50 = 50
    }

    /// <summary>
    /// Level 2 5 best asks/bids.
    /// </summary>
    public class Level2Depth5 : GranularFeedBase<StaticMarketDepthUpdate>
    {               
        public override bool IsPublic => false;

        protected override string Subject => "level2";

        protected override string Topic => "/spotMarket/level2Depth5";

        internal Level2Depth5() : base(null)
        {
        }

        public Level2Depth5(
          string key,
          string secret,
          string passphrase,
          bool isSandbox = false)
          : base(key, secret, passphrase, isSandbox)
        {
        }

        public Level2Depth5(ICredentialsProvider credProvider, bool isSandbox = false) : base(credProvider, isSandbox)
        {
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public Level2Depth5(IEnumerable<string> symbols, 
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
        public Level2Depth5(string symbol,
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


        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbols">The symbols to watch.</param>
        public Level2Depth5(IEnumerable<string> symbols,
          ICredentialsProvider credProvider,
          bool isSandbox = false)
            : base(credProvider, isSandbox)
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
        public Level2Depth5(string symbol,
          ICredentialsProvider credProvider,
          bool isSandbox = false)
            : base(credProvider, isSandbox)

        {
            Connect().ContinueWith(async (t) =>
            {
                await AddSymbol(symbol);
            });
        }

        /// <summary>
        /// The depth of this level 2 feed.
        /// </summary>
        public virtual Level2Depth Depth => Level2Depth.Depth5;

    }


    /// <summary>
    /// Level 2 50 best asks/bids
    /// </summary>
    public class Level2Depth50 : Level2Depth5
    {
        protected override string Topic => "/spotMarket/level2Depth5";

        /// <summary>
        /// The depth of this level 2 feed.
        /// </summary>
        public override Level2Depth Depth => Level2Depth.Depth50;

    }
}
