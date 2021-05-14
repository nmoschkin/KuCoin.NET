using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kucoin.NET.Data.Interfaces
{
    public interface ICandle
    {
        DateTime Timestamp { get; }

        decimal OpenPrice { get; }

        decimal ClosePrice { get; }

        decimal HighPrice { get; }

        decimal LowPrice { get; }

        decimal Volume { get; }

        decimal Amount { get; }
    }

    public interface IWriteableCandle : ICandle
    {
        new DateTime Timestamp { get; set; }

        new decimal OpenPrice { get; set; }

        new decimal ClosePrice { get; set; }

        new decimal HighPrice { get; set; }

        new decimal LowPrice { get; set; }

        new decimal Volume { get; set; }

        new decimal Amount { get; set; }
    }

    public interface ITypedCandle : ICandle
    {
        KlineType Type { get; }

    }

    public interface IWriteableTypedCandle : IWriteableCandle, ITypedCandle
    {
        new KlineType Type { get; set; }

    }

}
