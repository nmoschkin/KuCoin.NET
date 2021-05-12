using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Market
{

    public class TradeHistoryUnitConverter : JsonConverter<TradeHistoryUnit>
    {
        public override TradeHistoryUnit ReadJson(JsonReader reader, Type objectType, TradeHistoryUnit existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var dict = new Dictionary<string, object>();
                reader.Read();

                do
                {
                    var name = reader.Value as string;
                    reader.Read();
                    dict.Add(name, reader.Value);
                    reader.Read();
                } while (reader.TokenType != JsonToken.EndObject);


                if (dict.Count != 5) throw new JsonException();

                var th = new TradeHistoryUnit();

                th.Sequence = long.Parse((string)dict["sequence"]);

                th.Price = decimal.Parse((string)dict["price"]);

                th.Size = decimal.Parse((string)dict["size"]);

                th.Time = (long)dict["time"];

                if ((string)dict["side"] == "buy")
                {
                    th.Side = Side.Buy;
                }
                else
                {
                    th.Side = Side.Sell;
                }


                return th;
            }
            else
            {
                throw new JsonException();
            }
        }

        public override void WriteJson(JsonWriter writer, TradeHistoryUnit value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(TradeHistoryUnitConverter))]
    public struct TradeHistoryUnit
    {
        public long Sequence;

        public decimal Price;

        public decimal Size;

        public Side Side;

        public long Time;


        [JsonIgnore]
        public DateTime Timestamp
        {
            get
            {
                return EpochTime.NanosecondsToDate(Time);
            }
        }

        public override string ToString()
        {
            return $"{Side}: {Price} ({Size}) - {Timestamp}";
        }

    }


}
