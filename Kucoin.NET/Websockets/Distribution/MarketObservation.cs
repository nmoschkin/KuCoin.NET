using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Kucoin.NET.Websockets.Distribution
{
    public abstract class MarketObservation<TBookIn, TBookOut, TValue> : DistributableObject<string, TValue>, ISymbol, IObservableCopy<TBookIn, TBookOut> where TValue : IReadOnlySymbol where TBookOut: INotifyPropertyChanged
    {
    }
}
