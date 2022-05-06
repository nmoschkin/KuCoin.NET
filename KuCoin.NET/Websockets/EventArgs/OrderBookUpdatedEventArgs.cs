using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Websockets
{
    /// <summary>
    /// Event information for when the order book is updated from the full-depth order book.
    /// </summary>
    /// <typeparam name="TBook">The type of book.</typeparam>
    /// <typeparam name="TUnit">The type of the order unit.</typeparam>
    public class OrderBookUpdatedEventArgs<TBook, TUnit> : EventArgs
        where TBook : IOrderBook<TUnit>
        where TUnit : IOrderUnit
    {

        public string Symbol { get; private set; }

        public TBook OrderBook { get; private set; }

        public OrderBookUpdatedEventArgs(string symbol, TBook book)
        {
            Symbol = symbol;
            OrderBook = book;
        }

    }


}
