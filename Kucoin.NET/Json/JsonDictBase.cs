using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;

namespace Kucoin.NET.Json
{
    /// <summary>
    /// Base class for objects that can be serialized to <see cref="Dictionary{string, object}"/> objects.
    /// </summary>
    public abstract class JsonDictBase
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
            object obj;
            var dict = new Dictionary<string, object>();

            var pis = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            string name;

            foreach (var pi in pis)
            {
                obj = pi.GetValue(this);

                var attr = pi.GetCustomAttribute(typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;

                if (attr != null)
                {
                    name = attr.PropertyName;

                }
                else
                {
                    name = pi.Name;
                }

                if (obj != null)
                {
                    dict.Add(name, obj);
                }
            }

            return dict;
        }


    }
}
