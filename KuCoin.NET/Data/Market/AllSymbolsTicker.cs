using KuCoin.NET.Helpers;
using KuCoin.NET.Json;
using KuCoin.NET.Observable;
using KuCoin.NET.Rest;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// All Symbols Ticker list.
    /// </summary>
    public class AllSymbolsTicker : ObservableBase, IDataSeries<AllSymbolsTickerItem, ObservableDictionary<string, AllSymbolsTickerItem>>
    {
        private ObservableDictionary<string, AllSymbolsTickerItem> ticker = new ObservableDictionary<string, AllSymbolsTickerItem>();

        /// <summary>
        /// A dictionary of ticker items keyed on trading symbol.
        /// </summary>
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



        private DateTime time;

        /// <summary>
        /// The time stamp of the current observation.
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp
        {
            get => time;
            set
            {
                SetProperty(ref time, value);
            }
        }

        ObservableDictionary<string, AllSymbolsTickerItem> IDataSeries<AllSymbolsTickerItem, ObservableDictionary<string, AllSymbolsTickerItem>>.Data => Ticker;
    }

}
