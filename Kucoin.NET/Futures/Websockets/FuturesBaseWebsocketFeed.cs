using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Websockets
{
    /// <summary>
    /// Base class for most futures websocket feeds
    /// </summary>
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


    /// <summary>
    /// Base class for most symbol-specific futures websocket feeds
    /// </summary>
    public abstract class FuturesBaseWebsocketFeed<T> : KucoinBaseWebsocketFeed<T>
        where T: class        
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
