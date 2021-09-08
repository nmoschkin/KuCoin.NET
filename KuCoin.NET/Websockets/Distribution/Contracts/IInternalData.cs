using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{
    public interface IInternalData<T>
    {

        /// <summary>
        /// The internal data.
        /// </summary>
        /// <remarks>
        /// This is data that can change rapidly and is likely not observable.
        /// </remarks>
        T InternalData { get; }

    }
}
