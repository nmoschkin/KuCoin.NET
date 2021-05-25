using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Interfaces
{
    public interface IOrderUnit
    {
        decimal Price { get; set; }

        decimal Size { get; set; }
    }

    public interface IAtomicOrderUnit : IOrderUnit
    {

        string OrderId { get; set; }

        DateTime Timestamp { get; set; }

    }

    public interface ISequencedOrderUnit : IOrderUnit
    {
        long Sequence { get; set; }
    }

    public interface IOrderUnitList
    {
        IList<IOrderUnit> Asks { get; }

        IList<IOrderUnit> Bids { get; }

    }

    public interface IAtomicOrderUnitList : IOrderUnitList
    {
        new IList<IAtomicOrderUnit> Asks { get; }

        new IList<IAtomicOrderUnit> Bids { get; }

    }

    public interface IOrderBook : IOrderUnitList
    {
        long Sequence { get; set; }

        DateTime Timestamp { get; set; }
    }

    public interface IAtomicOrderBook : IAtomicOrderUnitList
    {
        long Sequence { get; set; }

        DateTime Timestamp { get; set; }
    }

}
