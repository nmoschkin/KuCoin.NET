﻿using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Futures.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets
{
    /// <summary>
    /// Trade Order Feed
    /// </summary>
    public class TradeOrderFeed : TopicFeedBase<TradeOrder>
    {
        public TradeOrderFeed(ICredentialsProvider cred) : base(cred, futures: true)
        {
        }

        public TradeOrderFeed(string key,
            string secret,
            string passphrase,
            bool isSandbox = false)
            : base(
                  key,
                  secret,
                  passphrase,
                  isSandbox,
                  futures: true)
        {
        }

        public override bool IsPublic => false;

        protected override string Subject => "orderChange";

        protected override string Topic => "/contractMarket/tradeOrders";

    }

    /// <summary>
    /// Symbol-based Trade Order Feed
    /// </summary>
    public class SymbolTradeOrderFeed : SymbolTopicFeedBase<TradeOrder>
    {

        public SymbolTradeOrderFeed(ICredentialsProvider cred) : base(cred, futures: true)
        {
        }

        public SymbolTradeOrderFeed(string key, 
            string secret, 
            string passphrase, 
            bool isSandbox = false) 
            : base(
                  key, 
                  secret, 
                  passphrase, 
                  isSandbox, 
                  true)
        {
        }

        public override bool IsPublic => false;

        protected override string Subject => "symbolOrderChange";

        protected override string Topic => "/contractMarket/tradeOrders";
    }
}
