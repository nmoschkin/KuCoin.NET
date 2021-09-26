using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    /// <summary>
    /// An object that contains data that is not meant for the UI/UX.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
