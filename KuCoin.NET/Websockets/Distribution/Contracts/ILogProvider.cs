using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution.Contracts
{
 
    /// <summary>
    /// Implements a system log based on the <see cref="SimpleLog"/> class.
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Gets the current logger for this object.
        /// </summary>
        SimpleLog Logger { get; }  

    }

    /// <summary>
    /// Implements a mutable system log based on the <see cref="SimpleLog"/> class.
    /// </summary>
    /// <remarks>
    /// This interface supports getting and setting the <see cref="SimpleLog"/> object.
    /// </remarks>
    public interface IMutableLogProvider : ILogProvider
    {
        /// <summary>
        /// Gets or sets the current logger for this object.
        /// </summary>
        new SimpleLog Logger { get; set;  }

    }

}
