
using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// Ask or Bid Order Unit Interface
    /// </summary>
    public interface IOrderUnit : ICloneable, IDataObject
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
    /// Ask or Bid Order Unit with Sequence Number Interface
    /// </summary>
    public interface ISequencedOrderUnit : IOrderUnit, IComparable<ISequencedOrderUnit>
    {
        /// <summary>
        /// The sequence number of the order
        /// </summary>
        long Sequence { get; set; }
    }

    /// <summary>
    /// Lists of Asks and Bids Interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOrderUnitCollection<T> : IDataSeries<T, T, ICollection<T>, ICollection<T>> where T : IOrderUnit
    {
        /// <summary>
        /// Asks (sell)
        /// </summary>
        ICollection<T> Asks { get; }

        /// <summary>
        /// Bids (buy)
        /// </summary>
        ICollection<T> Bids { get; }
    }

    /// <summary>
    /// Interface for a class that contains an order unit list.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IOrderUnit"/>.</typeparam>
    public interface IOrderUnitListProvider<T> where T : IOrderUnit
    {
        IOrderUnitCollection<T> OrderList { get; set; }
    }


    /// <summary>
    /// Interface for a class that contains keyed collections of asks and bids.
    /// </summary>
    /// <typeparam name="TUnit"></typeparam>
    public interface IKeyedOrderUnitCollection<TCol, TKey, TUnit> 
        : IOrderUnitCollection<TUnit>
        where TCol : IEnumerable<TUnit>
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
    /// Interface for any class that implements the properties of a full order book.
    /// </summary>
    /// <typeparam name="TUnit">A type that implements <see cref="IOrderUnit"/>.</typeparam>
    public interface IOrderBook<TUnit> : IOrderUnitCollection<TUnit> 
        where TUnit : IOrderUnit
    {
        long Sequence { get; set; }

        DateTime Timestamp { get; set; }

    }

    public interface IKeyedOrderBook<TCol, TUnit> : IOrderBook<TUnit>, IKeyedOrderUnitCollection<TCol, decimal, TUnit>
        where TCol : OrderUnitKeyedCollection<TUnit>
        where TUnit : IOrderUnit, new()
    {
    }
    public interface ILevel2Update : ISymbol, IOrderUnitListProvider<IOrderUnit>, IStreamableObject
    {
    }

    public interface ILevel3Update : ISymbol, IStreamableObject
    {

        string Subject { get; set; }
    }

    /// <summary>
    /// Futures Level 2 Order Update Interface
    /// </summary>
    public interface IFuturesOrderUpdate : IOrderUnit, IStreamableObject
    {

        /// <summary>
        /// The side of the order (buy or sell)
        /// </summary>
        Side Side { get; set; }
    }


}
