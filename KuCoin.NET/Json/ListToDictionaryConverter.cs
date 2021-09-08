using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using KuCoin.NET.Observable;
using System.Reflection;

namespace KuCoin.NET.Json
{
    public class ListToDictionaryConverter<TKey, TValue> : JsonConverter where TValue: class
    {
        public string KeyPropertyName { get; private set; }

        public ListToDictionaryConverter(string keyPropertyName)
        {
            KeyPropertyName = keyPropertyName;
        }

        public ListToDictionaryConverter()
        {

            Attribute attr;
            var valType = typeof(TValue);
            PropertyInfo keyProp = null;

            attr = valType.GetCustomAttribute(typeof(KeyPropertyAttribute));

            if (attr is KeyPropertyAttribute kpa && !string.IsNullOrEmpty(kpa.PropertyName))
            {
                keyProp = valType.GetProperty(kpa.PropertyName);
            }

            if (keyProp == null)
            {
                var props = valType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in props)
                {
                    attr = prop.GetCustomAttribute(typeof(KeyPropertyAttribute));
                    if (attr != null)
                    {
                        keyProp = prop;
                        break;
                    }
                }
            }

            KeyPropertyName = keyProp?.Name ?? throw new KeyNotFoundException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObservableDictionary<TKey, TValue>) || objectType == typeof(TValue[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var dict = new ObservableDictionary<TKey, TValue>(KeyPropertyName, (TValue[])serializer.Deserialize(reader, typeof(TValue[])));
                return dict;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ObservableDictionary<string, TValue> dict)
            {
                writer.WriteValue(dict);
            }
            else
            {
                throw new NotImplementedException();
            }
        }


    }
}
