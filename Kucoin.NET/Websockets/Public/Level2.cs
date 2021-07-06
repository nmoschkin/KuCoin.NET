using Kucoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kucoin.NET.Data.Market;
using System.Net.Http;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Websockets.Public
{


    /// <summary>
    /// Standard Spot Market Level 2 feed implementation with observable and UI data binding support.
    /// </summary>
    public class Level2 : Level2Base<ObservableOrderBook<ObservableOrderUnit>, ObservableOrderUnit, KeyedOrderBook<OrderUnitStruct>, OrderUnitStruct, Level2Update, Level2Observation>
    {

        /// <summary>
        /// Create a new Level 2 feed.
        /// </summary>
        public Level2() : base()
        {
        }

        public override string AggregateEndpoint => "/api/v2/market/orderbook/level2";

        public override string Subject => "trade.l2update";

        public override string Topic => "/market/level2";

        public override async Task<KeyedOrderBook<OrderUnitStruct>> GetAggregatedOrder(string symbol)
        {
            return await GetPartList(symbol, 0);
        }


        /// <summary>
        /// Get the Level 2 Data Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <param name="pieces">The number of pieces.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Settings the number of pieces to 0 returns the full market depth. 
        /// Use 0 to calibrate a full level 2 feed.
        /// </remarks>
        public async Task<KeyedOrderBook<OrderUnitStruct>> GetPartList(string symbol, int pieces)
        {
            var curl = pieces > 0 ? $"{AggregateEndpoint}_{pieces}" : AggregateEndpoint;
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            var result = jobj.ToObject<KeyedOrderBook<OrderUnitStruct>>();

            foreach (var ask in result.Asks)
            {
                if (ask is ISequencedOrderUnit seq)
                    seq.Sequence = result.Sequence;
            }

            foreach (var bid in result.Bids)
            {
                if (bid is ISequencedOrderUnit seq)
                    seq.Sequence = result.Sequence;
            }

            return result;
        }

        protected override Level2Observation CreateNewObservation(string symbol)
        {
            return new Level2Observation(this, symbol, defaultPieces);
        }


    }


    /// <summary>
    /// Spot Market Level 2 Feed Base Class for custom implementations.
    /// </summary>
    /// <typeparam name="TBook">The type of your custom order book.</typeparam>
    /// <typeparam name="TUnit">The type of your custom order pieces.</typeparam>
    public abstract class Level2StandardBase<TBook, TUnit> : Level2Base<TBook, TUnit, KeyedOrderBook<OrderUnitStruct>, OrderUnitStruct, Level2Update, CustomLevel2Observation<TBook, TUnit>>
        where TBook : IOrderBook<TUnit>, new()
        where TUnit : IOrderUnit, new()
    {
        /// <summary>
        /// Create a new Level 2 feed.
        /// </summary>
        public Level2StandardBase() : base()
        {
        }

        public override string AggregateEndpoint => "/api/v2/market/orderbook/level2";

        public override string Subject => "trade.l2update";

        public override string Topic => "/market/level2";
    }

}
