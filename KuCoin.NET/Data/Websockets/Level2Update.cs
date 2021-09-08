using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using KuCoin.NET.Data.Market;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;
using KuCoin.NET.Observable;
using KuCoin.NET.Websockets.Private;

using Newtonsoft.Json;

namespace KuCoin.NET.Data.Websockets
{
    /// <summary>
    /// Level 2 New Sequence Changes
    /// </summary>
    public struct Changes : IDataObject, IOrderUnitList<IOrderUnit>
    {
        IList<IOrderUnit> IOrderUnitList<IOrderUnit>.Asks => (IList<IOrderUnit>)Asks;

        IList<IOrderUnit> IOrderUnitList<IOrderUnit>.Bids => (IList<IOrderUnit>)Bids;

        IList<IOrderUnit> IDataSeries<IOrderUnit, IOrderUnit, IList<IOrderUnit>, IList<IOrderUnit>>.Data1 => (IList<IOrderUnit>)Asks;

        IList<IOrderUnit> IDataSeries<IOrderUnit, IOrderUnit, IList<IOrderUnit>, IList<IOrderUnit>>.Data2 => (IList<IOrderUnit>)Bids;


        /// <summary>
        /// Asks (from sellers)
        /// </summary>
        [JsonProperty("asks")]
        public List<OrderUnitStruct> Asks { get; set; }

        /// <summary>
        /// Bids (from buyers)
        /// </summary>
        [JsonProperty("bids")]
        public List<OrderUnitStruct> Bids { get; set; }

        public Dictionary<string, object> ToDict()
        {

            var l1 = new List<object>();
            var l2 = new List<object>();

            foreach (var ask in Asks)
            {
                l1.Add(ask.ToDict());
            }

            foreach (var ask in Bids)
            {
                l2.Add(ask.ToDict());
            }

            return new Dictionary<string, object>()
            {
                { "asks", l1.ToArray() },
                { "bids", l2.ToArray() }
            };
        }


    }


    /// <summary>
    /// Level 2 Full Market Depth Data Update
    /// </summary>
    public struct Level2Update : ILevel2Update
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

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                { "sequenceStart", SequenceStart },
                { "sequenceEnd", SequenceEnd },
                { "symbol", Symbol },
                { "changes", Changes.ToDict() },
            };
        }

    }
}
