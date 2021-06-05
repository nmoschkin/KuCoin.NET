using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Observable;
using Kucoin.NET.Websockets.Private;

using Newtonsoft.Json;

namespace Kucoin.NET.Data.Websockets
{
    /// <summary>
    /// Level 2 New Sequence Changes
    /// </summary>
    public class Changes : IOrderUnitList<IOrderUnit>
    {
        IList<IOrderUnit> IOrderUnitList<IOrderUnit>.Asks => (IList<IOrderUnit>)Asks;

        IList<IOrderUnit> IOrderUnitList<IOrderUnit>.Bids => (IList<IOrderUnit>)Bids;

        /// <summary>
        /// Asks (from sellers)
        /// </summary>
        [JsonProperty("asks")]
        public List<OrderUnit> Asks { get; set; }

        /// <summary>
        /// Bids (from buyers)
        /// </summary>
        [JsonProperty("bids")]
        public List<OrderUnit> Bids { get; set; }
    }


    /// <summary>
    /// Level 2 Full Market Depth Data Update
    /// </summary>
    public class Level2Update : ISymbol, IOrderUnitListProvider<IOrderUnit>
    {
        /// <summary>
        /// Update sequence start
        /// </summary>
        [JsonProperty("sequenceStart")]
        public long SequenceStart { get; set; }

        /// <summary>
        /// Update sequence end
        /// </summary>
        [JsonProperty("sequenceEnd")]
        public long SequenceEnd { get; set; }

        /// <summary>
        /// The symbol for this update
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// The changes for this sequence
        /// </summary>
        [JsonProperty("changes")]
        public Changes Changes { get; set; }

        IOrderUnitList<IOrderUnit> IOrderUnitListProvider<IOrderUnit>.OrderList
        {
            get => Changes;
            set => Changes = (Changes)value;
        }
    }
}
