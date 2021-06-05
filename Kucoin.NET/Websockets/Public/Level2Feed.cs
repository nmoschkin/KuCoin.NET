using Kucoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Public
{
    public class Level2Feed : KucoinBaseWebsocketFeed<Level2Update>
    {
        public Level2Feed() : base(null)
        {

        }

        public override bool IsPublic => true;

        protected override string Subject => throw new NotImplementedException();

        protected override string Topic => throw new NotImplementedException();

        protected override Task HandleMessage(FeedMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
