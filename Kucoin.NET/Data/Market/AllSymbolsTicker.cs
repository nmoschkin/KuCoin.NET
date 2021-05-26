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
    public class AllSymbolsTicker : ObservableBase
    {
        private ObservableDictionary<string, AllSymbolsTickerItem> ticker = new ObservableDictionary<string, AllSymbolsTickerItem>();

        [JsonProperty("ticker")]
        [JsonConverter(typeof(ListToDictionaryConverter<string, AllSymbolsTickerItem>), new object[] { "Symbol" })]
        public ObservableDictionary<string, AllSymbolsTickerItem> Ticker
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
