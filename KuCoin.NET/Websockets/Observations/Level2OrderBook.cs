using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Websockets.Distribution;
using KuCoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Observations
{
    public class Level2OrderBook : OrderBookDistributable<KeyedOrderBook<OrderUnitStruct>, ObservableOrderBook<ObservableOrderUnit>, Level2Update, Level2>
    {

        public Level2OrderBook(Level2 parent, string symbol) : base(parent, symbol, false)
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
        public override bool ProcessObject(Level2Update obj)
        {
            if (disposedValue) return false;
    
            lock (lockObj)
            {
                if (fullDepth == null)
                {
                    if (!failure) Failure = true;
                    return false;
                }
                else if (obj.SequenceEnd <= fullDepth.Sequence)
                {
                    return false;
                }
                else if (obj.SequenceStart - fullDepth.Sequence > 1)
                {
                    _ = Reset();
                    return false;
                }

                if (diagEnable)
                {
                    var tt = DateTime.UtcNow.Ticks;
                    if (tt - matchTime >= 10_000_000)
                    {
                        matchTime = tt;

                        TransactionsPerSecond = transactSec;
                        MatchesPerSecond = matchSec;
                        transactSec = 0;
                        matchSec = 0;
                    }

                    GrandTotal++;
                    transactSec++;
                }


                SequencePieces(obj.Changes.Asks, fullDepth.Asks);
                SequencePieces(obj.Changes.Bids, fullDepth.Bids);

                fullDepth.Sequence = obj.SequenceEnd;
                fullDepth.Timestamp = DateTime.Now;

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
        }

        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="changes">The changes to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SequencePieces(IList<OrderUnitStruct> changes, Level2KeyedCollection<OrderUnitStruct> pieces)
        {
            foreach (var change in changes)
            {
                decimal cp = change.Price;

                if (change.Size == 0.0M)
                {
                    if (updVol)
                    {
                        Candle.Volume += pieces[cp].Price * pieces[cp].Size;
                    }
                    pieces.Remove(cp);
                }
                else
                {
                    if (pieces.Contains(cp))
                    {
                        var piece = pieces[cp];

                        piece.Size = change.Size;
                        piece.Sequence = change.Sequence;
                    }
                    else
                    {
                        var newPiece = new OrderUnitStruct
                        {
                            Price = change.Price,
                            Size = change.Size,
                            Sequence = change.Sequence
                        };

                        pieces.Add(newPiece);
                    }
                }
            }
        }
    }

}
