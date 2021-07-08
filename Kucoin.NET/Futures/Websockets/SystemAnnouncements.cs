using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Futures.Data.Websockets;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets
{
    public class SystemAnnouncements : KucoinBaseWebsocketFeed<FundingPoint>
    {
        public SystemAnnouncements() : base(null, futures: true)
        {
        }

        public override bool IsPublic => true;

        protected override string Subject => throw new NotImplementedException();

        protected override string Topic => "/contract/announcement";

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Topic == Topic)
            {
                var newObj = msg.Data.ToObject<FundingPoint>();

                if (msg.Subject == "funding.begin") 
                {
                    newObj.Type = FundingPointType.Begin;
                }
                else if (msg.Subject == "funding.end")
                {
                    newObj.Type = FundingPointType.End;
                }

                await PushNext(newObj);
            }
        }
    }
}
