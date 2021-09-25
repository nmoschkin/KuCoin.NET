using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    public enum FailReason
    {
        None = 0,
        OrderBookTimeout = 1,
        Other = 2
    }

}
