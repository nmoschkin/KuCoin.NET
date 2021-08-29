using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Provides an interface for an object that has a distributor parent.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IParent
    {
        /// <summary>
        /// Gets the parent distributor
        /// </summary>
        IDistributor Parent { get; }
    }

}
