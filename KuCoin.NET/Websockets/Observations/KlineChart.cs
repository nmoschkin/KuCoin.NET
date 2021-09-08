using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Websockets.Distribution;
using KuCoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Observations
{
    public class KlineChart<TCandle, TCustom, TCol> : InitializableObject<SymbolKline, KlineFeedMessage<TCandle>, TCol>, IParent<KlineFeedNew<TCandle, TCustom, TCol>>
          where TCandle : IFullCandle, TCustom, new()
          where TCol : IList<TCustom>, new()        
    {

        protected TCol candles;

        new public KlineFeedNew<TCandle, TCustom, TCol> Parent
        {
            get => (KlineFeedNew<TCandle, TCustom, TCol>)parent;
        }

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

        public KlineChart(KlineFeedNew<TCandle, TCustom, TCol> parent, SymbolKline key) : base(parent, key, false)
        {
        }
        
        public override void DoWork()
        {
            lock (lockObj)
            {
                foreach (var b in buffer)
                {
                    ProcessObject(b);
                }
            }
        }

        public override bool ProcessObject(KlineFeedMessage<TCandle> obj)
        {





            return true;
        }
    }
}
