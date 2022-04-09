using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Data.Market
{
    public interface IKeyProvider<T>
    {

        T Key { get; }
    }
}
