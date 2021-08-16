using Kucoin.NET.Data.Market;
using Kucoin.NET.Rest;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoinApp.ViewModels
{
    public class Level3TickerViewModel : SymbolViewModel
    {
        private Level3Observation observation;

        private decimal bestAsk;

        private decimal bestBid;

        private int queueLen;

        private DateTime? ts;

        private FeedState state;

        public void Update()
        {
            BestAsk = observation?.FullDepthOrderBook?.Asks[0].Price ?? 0M;
            BestBid = observation?.FullDepthOrderBook?.Bids[0].Price ?? 0M;
            Timestamp = observation?.FullDepthOrderBook?.Timestamp;
            QueueLength = observation?.QueueLength ?? 0;
            State = observation?.State ?? FeedState.Disconnected;
        }

        public Level3TickerViewModel(TradingSymbol symbol, Level3Observation observation) : base(symbol)
        {
            this.observation = observation;
        }

        public int QueueLength
        {
            get => queueLen;
            set
            {
                SetProperty(ref queueLen, value);
            }
        }

        public decimal BestAsk
        {
            get => bestAsk;
            set
            {
                SetProperty(ref bestAsk, value);
            }
        }

        public decimal BestBid
        {
            get => bestBid;
            set
            {
                SetProperty(ref bestBid, value);
            }
        }

        public Level3Observation Observation
        {
            get => observation;
            set
            {
                SetProperty(ref observation, value);
            }
        }

        public DateTime? Timestamp
        {
            get => ts;
            set
            {
                SetProperty(ref ts, value);
            }
        }

        public FeedState State
        {
            get => state;
            set
            {
                SetProperty(ref state, value);
            }
        }

    }
}
