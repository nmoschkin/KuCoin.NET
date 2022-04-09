using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Order;
using KuCoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace KuCoin.NET.Json
{


    public class KeyedBookConverter<TBook, TItem> : JsonConverter<TBook> 
        where TItem: class, IAtomicOrderUnit, new() 
        where TBook: AtomicOrderCollection<TItem>, new()
    {
        public override TBook ReadJson(JsonReader reader, Type objectType, TBook existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var alldata = (object[][])serializer.Deserialize(reader, typeof(object[][]));

                var d = objectType.GetGenericArguments()[0];
                var mtd = d.GetConstructor(new Type[] { typeof(object[]) });
                bool gconst = mtd != null;

                var output = (existingValue ?? new TBook());
                TItem dest;

                output.Capacity = alldata.Length * 2;

                if (gconst)
                {
                    foreach (var input in alldata)
                    {
                        dest = (TItem)mtd.Invoke(new object[] { input });
                        output.Add(dest);
                    }
                }
                else
                {
                    foreach (var input in alldata)
                    {
                        dest = new TItem();
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

        public override void WriteJson(JsonWriter writer, TBook value, JsonSerializer serializer)
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
