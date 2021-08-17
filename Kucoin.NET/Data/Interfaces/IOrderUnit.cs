using Kucoin.NET.Data.Order;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        ///// <summary>
        ///// Clone to another order unit.
        ///// </summary>
        ///// <typeparam name="T">The type to clone to.</typeparam>
        ///// <returns></returns>
        //T Clone<T>() where T : IOrderUnit, new();
    }

    /// <summary>
    /// Level 3 Atomic Order Unit Interface
    /// </summary>
    public interface IAtomicOrderUnit : IOrderUnit //, IEquatable<IAtomicOrderUnit>
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
    /// <typeparam name="TUnit"></typeparam>
    public interface IKeyedOrderUnitList<TCol, TKey, TUnit> 
        : IOrderUnitList<TUnit>
        where TCol : KeyedCollection<TKey, TUnit>
        where TUnit : IOrderUnit
    {
        /// <summary>
        /// Asks (sell)
        /// </summary>
        new TCol Asks { get; }

        /// <summary>
        /// Bids (buy)
        /// </summary>
        new TCol Bids { get; }

    }

    /// <summary>
    /// Interface for a class that contains keyed collections of asks and bids for level 3.
    /// </summary>
    /// <typeparam name="TUnit"></typeparam>
    public interface ILevel3OrderUnitList<TCol, TUnit>
        : IOrderUnitList<TUnit>
        where TCol : KeyedBook<TUnit>
        where TUnit : IAtomicOrderUnit, new()
    {
        /// <summary>
        /// Asks (sell)
        /// </summary>
        new TCol Asks { get; }

        /// <summary>
        /// Bids (buy)
        /// </summary>
        new TCol Bids { get; }

    }


    /// <summary>
    /// Interface for any class that implements the properties of a full order book.
    /// </summary>
    /// <typeparam name="TUnit">A type that implements <see cref="IOrderUnit"/>.</typeparam>
    public interface IOrderBook<TUnit> : IOrderUnitList<TUnit> 
        where TUnit : IOrderUnit
    {
        long Sequence { get; set; }

        DateTime Timestamp { get; set; }

    }

    public interface IAtomicOrderBook<TUnit> : IOrderBook<TUnit> 
        where TUnit: IAtomicOrderUnit
    {
    }

    public interface IKeyedOrderBook<TCol, TUnit> : IOrderBook<TUnit>, IKeyedOrderUnitList<TCol, decimal, TUnit>
        where TCol : Level2KeyedCollection<TUnit>
        where TUnit : IOrderUnit
    {
    }


    public interface IKeyedAtomicOrderBook<TCol, TUnit> : IAtomicOrderBook<TUnit>, ILevel3OrderUnitList<TCol, TUnit>
        where TCol : KeyedBook<TUnit>
        where TUnit : IAtomicOrderUnit, new()
    {
    }

    public interface ILevel3Update : ISymbol
    {

        string Subject { get; set; }
    }

}
