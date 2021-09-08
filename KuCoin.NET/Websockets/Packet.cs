using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets
{
    public struct Packet
    {
        public string Symbol;

        public string Json;

        public Packet(string json, string symbol)
        {
            Json = json;
            Symbol = symbol;
        }
    }

}
