using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Websockets.Distribution
{
    public interface IPingable
    {
        void Ping();        

        int Interval { get; set; }

    }

    public interface IAsyncPingable : IPingable
    {
        new Task Ping();
    }
}
