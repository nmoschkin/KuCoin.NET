using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Websockets
{
    public class DataReceivedEventArgs : EventArgs
    {
        public string Json { get; protected set; }

        public DataReceivedEventArgs(string json)
        {
            Json = json;
        }
    }

    public class FeedDataReceivedEventArgs<T> : EventArgs where T: class
    {
        public T Data { get; private set; }

        public FeedDataReceivedEventArgs(T obj) 
        {
            Data = obj;
        }
    }
}
