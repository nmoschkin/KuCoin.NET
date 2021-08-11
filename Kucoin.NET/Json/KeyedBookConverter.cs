using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

//namespace Kucoin.NET.Json
//{
//    public class KeyedBookConverter<TUnit> : JsonConverter<KeyedBook<TUnit>> where TUnit : IAtomicOrderUnit
//    {
//        public override KeyedBook<TUnit> ReadJson(JsonReader reader, Type objectType, KeyedBook<TUnit> existingValue, bool hasExistingValue, JsonSerializer serializer)
//        {
//            if (existingValue != null)
//            {
//                serializer.Deserialize(reader, typeof())
//            }
//        }

//        public override void WriteJson(JsonWriter writer, KeyedBook<TUnit> value, JsonSerializer serializer)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
