using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Kucoin.NET.Observable;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Data.Websockets
{
    public class KlineFeedMessage 
    {
        [JsonProperty("time")]
        public long? Time { get; set; }

        [JsonIgnore]
        public DateTime Timestamp
        {
            get
            {
                if (Time is long t)
                {
                    return EpochTime.NanosecondsToDate(t);
                }
                else
                {
                    return DateTime.Now;
                }
            }
        }

        [JsonProperty("candles")]
        public Candle Candles { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }


    }
}
