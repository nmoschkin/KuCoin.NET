﻿using Kucoin.NET.Futures.Data.Websockets;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Websockets.Observations
{
    /// <summary>
    /// Funding event observation
    /// </summary>
    public class FundingObservation : FeedObject<FundingSettlement>
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
