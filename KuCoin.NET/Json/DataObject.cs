using System.Collections.Generic;
using System.Reflection;

using KuCoin.NET.Data;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KuCoin.NET.Json
{
    /// <summary>
    /// Base class for data objects.
    /// </summary>
    public abstract class DataObject : IDataObject
    {
        /// <summary>
        /// Returns the JSON-serialized contents of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => JsonConvert.SerializeObject(ToDict());

        /// <summary>
        /// Converts public properties into a <see cref="Dictionary{TKey, TValue}"/> of string, object.
        /// </summary>
        /// <returns>A new dictionary.</returns>
        public virtual Dictionary<string, object> ToDict()
        {
            return ToDict(this);
        }

        /// <summary>
        /// Converts public properties into a <see cref="Dictionary{TKey, TValue}"/> of string, object.
        /// </summary>
        /// <returns>A new dictionary.</returns>
        public static Dictionary<string, object> ToDict(object obj)
        {
            object value;
            var dict = new Dictionary<string, object>();

            var pis = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            string name;

            foreach (var pi in pis)
            {
                value = pi.GetValue(obj);

                var attr = pi.GetCustomAttribute(typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;

                if (attr != null)
                {
                    name = attr.PropertyName;

                }
                else
                {
                    name = pi.Name;
                }

                if (value != null)
                {
                    if (value is IDataObject dobj)
                    {
                        dict.Add(name, dobj.ToDict());
                    }
                    else
                    {
                        dict.Add(name, value);
                    }

                }
            }

            return dict;
        }

    }
}
