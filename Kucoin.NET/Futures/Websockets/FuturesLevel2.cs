using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Futures.Data.Market;
using Kucoin.NET.Futures.Websockets.Observations;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets
{
    public class FuturesLevel2 : Level2Base<FuturesOrderBook, OrderUnit, FuturesLevel2Update, FuturesLevel2Observation>
    {
        public FuturesLevel2(ICredentialsProvider credProvider) : base(credProvider, true)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        public FuturesLevel2(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox, true)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        public override string Subject => "level2";

        public override string AggregateEndpoint => "/api/v1/level2/snapshot";

        public override string Topic => "/contractMarket/level2";
        
        protected override FuturesLevel2Observation CreateNewObservation(string symbol)
        {
            return new FuturesLevel2Observation(this, symbol, defaultPieces);
        }

    }
}
