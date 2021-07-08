using Kucoin.NET.Futures.Data.Websockets;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Websockets.Observations
{
    public class FundingObservation : FeedObservation<FundingSettlement>
    {
        private new PositionChangeFeed feed;

        public FundingObservation(PositionChangeFeed feed, IObserver<FundingSettlement> observer) : base(null, observer)
        {
            this.feed = feed;
        }

        protected override void Dispose(bool disposing)
        {
            feed?.RemoveFundingObservation(this);
            feed = null;

            base.Dispose(disposing);
        }
    }

}
