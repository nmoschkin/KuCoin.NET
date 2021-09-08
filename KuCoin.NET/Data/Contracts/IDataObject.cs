using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Data
{
    public interface IDataSeries
    {
    }

    public interface IDataSeries<T, TCol> : IDataSeries 
        where TCol: IList<T> 
        where T: IDataObject
    {
        TCol Data { get; }
    }

    public interface IDataSeries<T1, T2, TCol1, TCol2> : IDataSeries 
        where TCol1 : IList<T1> 
        where T1 : IDataObject
        where TCol2 : IList<T1>
        where T2 : IDataObject
    {
        TCol1 Data1 { get; }

        TCol2 Data2 { get; }
    }

    /// <summary>
    /// A data object that is recognized as valid by the framework.
    /// </summary>
    public interface IDataObject
    {
        /// <summary>
        /// Writes the properties of this object ito a dictionary of string/object pairs.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> ToDict();
    }

    /// <summary>
    /// A data object that is recognized as streamable by the framework.
    /// </summary>
    public interface IStreamableObject : IDataObject
    {
        // There's nothing here.  This is just to identify objects as objects we can expect to stream.
    }


}
