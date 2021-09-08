using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kucoin.NET.Json
{


    public class KeyedBookConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.FullName.Contains("Kucoin.NET.Data.Order.KeyedBook");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var alldata = (object[][])serializer.Deserialize(reader, typeof(object[][]));

                var d = objectType.GetGenericArguments()[0];
                var mtd = d.GetConstructor(new Type[] { typeof(object[]) });
                bool gconst = mtd != null;

                var output = (existingValue ?? Activator.CreateInstance(objectType)) as IList;
                IAtomicOrderUnit dest;

                if (gconst)
                {
                    foreach (var input in alldata)
                    {
                        dest = (IAtomicOrderUnit)mtd.Invoke(new object[] { input });
                        output.Add(dest);
                    }
                }
                else
                {
                    foreach (var input in alldata)
                    {
                        dest = (IAtomicOrderUnit)Activator.CreateInstance(d);
                        dest.OrderId = (string)reader.Value;
                        dest.Price = decimal.Parse((string)reader.Value);
                        dest.Size = decimal.Parse((string)reader.Value);
                        dest.Timestamp = EpochTime.NanosecondsToDate((long)reader.Value);

                        output.Add(dest);
                    }
                }

                return output;
            }
            else throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = value as IList;
            var l = new List<object[]>();

            foreach (IAtomicOrderUnit unit in list)
            {
                var output = new object[] { unit.OrderId, unit.Price.ToString(), unit.Size.ToString(), EpochTime.DateToNanoseconds(unit.Timestamp) };
                l.Add(output);
            }

            serializer.Serialize(writer, l.ToArray());
        }
    }

}
