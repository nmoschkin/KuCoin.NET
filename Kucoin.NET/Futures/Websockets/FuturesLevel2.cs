using Kucoin.NET.Data.Market;
using Kucoin.NET.Futures.Data.Market;
using Kucoin.NET.Futures.Websockets.Observations;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Websockets.Public;

using System;

namespace Kucoin.NET.Futures.Websockets
{
    /// <summary>
    /// Standard Futures Market Level 2 feed implementation with observable and UI data binding support.
    /// </summary>
    public class FuturesLevel2 : Level2Base<FuturesOrderBook, ObservableOrderUnit, KeyedOrderBook<OrderUnitStruct>, OrderUnitStruct, FuturesLevel2Update, FuturesLevel2Observation>
    {
        /// <summary>
        /// Create a new futures Level 2 feed.
        /// </summary>
        public FuturesLevel2() : base(futures: true)
        {
            if (!Dispatcher.Initialized && !Dispatcher.Initialize())
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        public override string Subject => "level2";

        public override string AggregateEndpoint => "/api/v1/level2/snapshot";

        public override string Topic => "/contractMarket/level2";
        
        protected override FuturesLevel2Observation CreateNewObservation(string symbol)
        {
            return new FuturesLevel2Observation(this, symbol, defaultPieces);
        }

    }

    /// <summary>
    /// Futures Market Level 2 Feed Base Class for custom implementations.
    /// </summary>
    /// <typeparam name="TBook">The type of your custom order book.</typeparam>
    /// <typeparam name="TUnit">The type of your custom order pieces.</typeparam>
    public abstract class Level2FuturesBase<TBook, TUnit> : Level2Base<TBook, TUnit, KeyedOrderBook<OrderUnitStruct>, OrderUnitStruct, FuturesLevel2Update, CustomFuturesLevel2Observation<TBook, TUnit>>
        where TBook : IOrderBook<TUnit>, new()
        where TUnit : IOrderUnit, new()
    {
        /// <summary>
        /// Create a new futures Level 2 feed.
        /// </summary>
        public Level2FuturesBase() : base(futures: true)
        {
        }

        public override string Subject => "level2";

        public override string AggregateEndpoint => "/api/v1/level2/snapshot";

        public override string Topic => "/contractMarket/level2";

    }

}
