using Kucoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{
    public class VolumeHelper : IMarketVolume
    {
        public decimal MarketVolume { get; }
        public bool IsVolumeEnabled { get; set; }
    }
}
