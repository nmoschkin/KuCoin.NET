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

    public interface IOrderUnitList
    {

        IList<IOrderUnit> Asks { get; }

        IList<IOrderUnit> Bids { get; }

    }
}
