using Kucoin.NET.Data.Order;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Ask or Bid Order Unit Interface
    /// </summary>
    public interface IOrderUnit : ICloneable
    {
        /// <summary>
        /// The price of the order in quote currency.
        /// </summary>
        decimal Price { get; set; }

        /// <summary>
        /// The size of the order in applicable units.
        /// </summary>
        decimal Size { get; set; }
    }

    /// <summary>
    /// Level 3 Atomic Order Unit Interface
    /// </summary>
    public interface IAtomicOrderUnit : IOrderUnit
    {
        /// <summary>
        /// The Order Id
        /// </summary>
        string OrderId { get; set; }

        /// <summary>
        /// The time stamp of the order
        /// </summary>
        DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Ask or Bid Order Unit with Sequence Number Interface
    /// </summary>
    public interface ISequencedOrderUnit : IOrderUnit
    {
        /// <summary>
        /// The sequence number of the order
        /// </summary>
        long Sequence { get; set; }
    }

    /// <summary>
    /// Futures Level 2 Order Update Interface
    /// </summary>
    public interface IFuturesOrderUpdate : IOrderUnit
    {

        /// <summary>
        /// The side of the order (buy or sell)
        /// </summary>
        Side Side { get; set; }
    }

    /// <summary>
    /// Lists of Asks and Bids Interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOrderUnitList<T> where T : IOrderUnit
    {
        /// <summary>
        /// Asks (sell)
        /// </summary>
        IList<T> Asks { get; }

        /// <summary>
        /// Bids (buy)
        /// </summary>
        IList<T> Bids { get; }
    }

    /// <summary>
    /// Interface for a class that contains an order unit list.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IOrderUnit"/>.</typeparam>
    public interface IOrderUnitListProvider<T> where T : IOrderUnit
    {
        IOrderUnitList<T> OrderList { get; set; }
    }


    /// <summary>
    /// Interface for a class that contains keyed collections of asks and bids.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IKeyedOrderUnitList<T> where T : IOrderUnit
    {
        /// <summary>
        /// Asks (sell)
        /// </summary>
        SortedKeyedOrderUnitBase<T> Asks { get; }

        /// <summary>
        /// Bids (buy)
        /// </summary>
        SortedKeyedOrderUnitBase<T> Bids { get; }

    }

    /// <summary>
    /// Interface for any class that implements the properties of a full order book.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IOrderUnit"/>.</typeparam>
    public interface IOrderBook<T> : IKeyedOrderUnitList<T> where T : IOrderUnit
    {
        long Sequence { get; set; }

        DateTime Timestamp { get; set; }
    }

    public interface IAtomicOrderBook<T> where T: IAtomicOrderUnit
    {
        long Sequence { get; set; }

        DateTime Timestamp { get; set; }

        Level3KeyedCollectionBase<T> Asks { get; }

        Level3KeyedCollectionBase<T> Bids { get; }

    }

    public interface ILevel3Update
    {

        string Subject { get; set; }
    }

}
