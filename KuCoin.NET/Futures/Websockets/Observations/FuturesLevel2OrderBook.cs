using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Futures.Data.Market;
using KuCoin.NET.Websockets.Distribution;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Futures.Websockets.Observations
{
    public class FuturesLevel2OrderBook : OrderBookDistributable<AggregatedOrderBook<OrderUnitStruct>, ObservableOrderBook<ObservableOrderUnit>, FuturesLevel2Update, FuturesLevel2>
    {

        public FuturesLevel2OrderBook(FuturesLevel2 parent, string symbol) : base(parent, symbol, false, false)
        {
            this.parent = parent;
            base.parent = parent;
            this.dataProvider = parent;
            this.IsPresentationDisabled = true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void PresentData()
        {
            lock (lockObj)
            {
                if (fullDepth == null) return;
                if (this.marketDepth <= 0) return;

                var asks = fullDepth.Asks as IList<OrderUnitStruct>;
                var bids = fullDepth.Asks as IList<OrderUnitStruct>;

                if (orderBook == null)
                {
                    OrderBook = new ObservableOrderBook<ObservableOrderUnit>();
                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = fullDepth.Timestamp;
                }

                int marketDepth = this.marketDepth;

                if (asks.Count < marketDepth) marketDepth = asks.Count;

                if (orderBook.Asks.Count != marketDepth)
                {
                    orderBook.Asks.Clear();
                    for (int i = 0; i < marketDepth; i++)
                    {
                        orderBook.Asks.Add(asks[i].Clone<ObservableOrderUnit>());
                    }
                }
                else
                {
                    for (int i = 0; i < marketDepth; i++)
                    {
                        if (orderBook.Asks[i].Price == asks[i].Price)
                        {
                            orderBook.Asks[i].Price = asks[i].Price;
                            orderBook.Asks[i].Size = asks[i].Size;
                        }
                        else
                        {
                            orderBook.Asks[i] = asks[i].Clone<ObservableOrderUnit>();
                        }
                    }
                }

                if (bids.Count < marketDepth) marketDepth = bids.Count;

                if (orderBook.Bids.Count != marketDepth)
                {
                    orderBook.Bids.Clear();
                    for (int i = 0; i < marketDepth; i++)
                    {
                        orderBook.Bids.Add(bids[i].Clone<ObservableOrderUnit>());
                    }
                }
                else
                {
                    for (int i = 0; i < marketDepth; i++)
                    {
                        if (orderBook.Bids[i].Price == bids[i].Price)
                        {
                            orderBook.Bids[i].Price = bids[i].Price;
                            orderBook.Bids[i].Size = bids[i].Size;
                        }
                        else
                        {
                            orderBook.Bids[i] = bids[i].Clone<ObservableOrderUnit>();
                        }
                    }
                }

            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ProcessObject(FuturesLevel2Update obj)
        {
            if (disposedValue || fullDepth == null) return false;

            lock (lockObj)
            {
                try
                {

                    if (obj.Change.Side == Side.Buy)
                    {

                        SequencePieces(obj, fullDepth.Bids);

                    }
                    else
                    {
                        SequencePieces(obj, fullDepth.Asks);

                    }

                    fullDepth.Sequence = obj.Sequence;
                }
                catch (Exception ex)
                {
                    string e = ex.Message;
                }
            }

            if (updVol)
            {
                decimal price = (decimal)fullDepth.Bids[0].Price;

                if (!Candle.IsTimeInCandle(candle, fullDepth.Timestamp.ToUniversalTime()))
                {
                    candle.ClosePrice = price;
                    LastCandles.Add(candle);

                    Candle = new Candle();
                    Candle.Volume = 0;

                    Candle.OpenPrice = Candle.ClosePrice = Candle.HighPrice = Candle.LowPrice = price;
                    KlineTime = Candle.Timestamp = klineType.GetCurrentKlineStartTime();
                }
                else
                {
                    Candle.ClosePrice = price;

                    if (price > Candle.HighPrice)
                    {
                        Candle.HighPrice = price;
                    }
                    else if (price < Candle.LowPrice)
                    {
                        Candle.LowPrice = price;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="change">The change to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        private void SequencePieces(FuturesLevel2Update change, OrderUnitKeyedCollection<OrderUnitStruct> pieces)
        {
            decimal cp = change.Change.Price;

            if (change.Change.Size == 0.0M)
            {
                if (updVol)
                {
                    Candle.Volume += pieces[cp].Price * pieces[cp].Size;
                }
                pieces.Remove(cp);
            }
            else
            {
                if (pieces.ContainsKey(cp))
                {
                    var piece = pieces[cp];

                    piece.Size = change.Change.Size;

                    if (piece is ISequencedOrderUnit seqpiece)
                        seqpiece.Sequence = change.Sequence;
                }
                else
                {
                    var ou = new OrderUnitStruct();

                    ou.Price = change.Change.Price;
                    ou.Size = change.Change.Size;

                    if (ou is ISequencedOrderUnit seqpiece)
                        seqpiece.Sequence = change.Sequence;

                    pieces.Add(ou);
                }
            }
        }
    }

}
