using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Observable;
using Kucoin.NET.Rest;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// All Symbols Ticker list.
    /// </summary>
    public class TickerList : ObservableBase
    {
        private ObservableDictionary<string, TickerListItem> ticker = new ObservableDictionary<string, TickerListItem>();

        [JsonProperty("ticker")]
        [JsonConverter(typeof(ListToDictionaryConverter<string, TickerListItem>), new object[] { "Symbol" })]
        public ObservableDictionary<string, TickerListItem> Ticker
        {
            get => ticker;
            set
            {
                SetProperty(ref ticker, value);
            }
        }

        private long time;

        [JsonProperty("time")]
        public long Time
        {
            get => time;
            set
            {
                if (SetProperty(ref time, value))
                {
                    OnPropertyChanged(nameof(Timestamp));
                }
            }
        }

        [JsonIgnore]
        public DateTime Timestamp
        {
            get
            {
                return EpochTime.MillisecondsToDate(Time);
            }
        }

    }

}
