using Kucoin.NET.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Data.Market
{

    public class OrderBook : ObservableBase
    {
        private ObservableCollection<OrderUnit> asks;

        private ObservableCollection<OrderUnit> bids;

        private long sequence;

        [JsonProperty("sequence")]
        [KeyProperty]
        public long Sequence
        {
            get => sequence;
            set
            {
                SetProperty(ref sequence, value);
            }
        }

        [JsonProperty("asks")]
        public ObservableCollection<OrderUnit> Asks
        {
            get
            {
                if (asks == null)
                {
                    Asks = new ObservableCollection<OrderUnit>();
                }

                return asks;
            }
            set
            {
                SetProperty(ref asks, value);
            }
        }

        [JsonProperty("bids")]
        public ObservableCollection<OrderUnit> Bids
        {
            get
            {
                if (bids == null)
                {
                    Bids = new ObservableCollection<OrderUnit>();
                }

                return bids;
            }
            set
            {
                SetProperty(ref bids, value);
            }
        }

        private long time;

        [JsonProperty("time")]
        public long Time
        {
            get => time;
            set
            {
                SetProperty(ref time, value);
            }
        }


        [JsonIgnore]
        public DateTime Timestamp
        {
            get
            {
                return EpochTime.NanosecondsToDate(time);
            }
        }

    }


}
