using KuCoin.NET.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    public interface IWebsocketListener : IParent, INotifyPropertyChanged, IDisposable
    {
    }

    public interface IWebsocketListener<T> : IWebsocketListener where T: IStreamableObject
    {

    }
}
