using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Websockets.Distribution;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Observations
{
    public class KlineChart<TCandle, TCustom, TCol> : DistributableObject<SymbolKline, KlineFeedMessage<TCandle>>
          where TCandle : IFullCandle, TCustom, new()
          where TCol : IList<TCustom>, new()        
    {

        protected TCol candles;

        /// <summary>
        /// Gets the candles data.
        /// </summary>
        public virtual TCol Candles
        {
            get => candles;
            protected set
            {
                SetProperty(ref candles, value);
            }
        }

        public KlineChart(KlineFeedNew<TCandle, TCustom, TCol> parent, SymbolKline key) : base(parent, key)
        {
        }
        
        public override void DoWork()
        {
            lock (lockObj)
            {
                foreach (var b in buffer)
                {

                }
            }
        }

        public override bool ProcessObject(KlineFeedMessage<TCandle> obj)
        {
            throw new NotImplementedException();
        }
    }
}
