using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Websockets
{
    public abstract class FuturesBaseWebsocketFeed : KucoinBaseWebsocketFeed
    {

        public FuturesBaseWebsocketFeed(ICredentialsProvider credProvider) : base(credProvider, futures: true)
        {
        }

        public FuturesBaseWebsocketFeed(
            string key, 
            string secret, 
            string passphrase, 
            bool isSandbox = false) 
            : base(
                  key, 
                  secret, 
                  passphrase, 
                  isSandbox: isSandbox, 
                  futures: true)
        {
        }

    }
}
