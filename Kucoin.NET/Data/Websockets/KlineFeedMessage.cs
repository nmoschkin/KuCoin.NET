using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Kucoin.NET.Observable;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;
using Kucoin.NET.Data.Interfaces;

namespace Kucoin.NET.Data.Websockets
{
    public class KlineFeedMessage<T> where T: IWriteableTypedCandle, new()
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
        public T Candles { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }


    }
}
