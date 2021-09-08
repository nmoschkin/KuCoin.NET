using Kucoin.NET.Futures.Data.Websockets;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Websockets
{
    /// <summary>
    /// Ticker for the 24 hour market snapshot
    /// </summary>
    public class ContractMarketSnapshotTicker : SymbolTopicFeedBase<ContractMarketSnapshot>
    {
        public ContractMarketSnapshotTicker() : base(null, futures: true)
        {

        }

        public override bool IsPublic => true;

        public override string Subject => "snapshot.24h";

        public override string Topic => "/contractMarket/snapshot";
    }
}
