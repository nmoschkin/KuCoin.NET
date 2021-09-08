using System;
using System.Collections.Generic;
using System.Text;

using KuCoin.NET.Helpers;

using Newtonsoft.Json;

namespace KuCoin.NET.Json
{

    /// <summary>
    /// Automatically convert timestamps from the REST API.
    /// </summary>
    public class AutoTimeConverter : JsonConverter
    {
        public TimeTypes? Type { get; set; } = null;

        public AutoTimeConverter()
        {
        }

        public AutoTimeConverter(TimeTypes type)
        {
            Type = type;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is long l)
            {
                return GetTime(l);
            }
            else if (reader.Value is string s)
            {
                return GetTime(long.Parse(s));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime t)
            {
                if (Type == TimeTypes.InMilliseconds)
                {
                    serializer.Serialize(writer, EpochTime.DateToMilliseconds(t));
                }
                else if (Type == TimeTypes.InNanoseconds)
                {
                    serializer.Serialize(writer, EpochTime.DateToNanoseconds(t));
                }
                else
                {
                    serializer.Serialize(writer, EpochTime.DateToSeconds(t));
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private DateTime GetTime(long value)
        {
            switch (Type)
            {
                case TimeTypes.InSeconds:
                    return EpochTime.SecondsToDate(value);

                case TimeTypes.InNanoseconds:
                    return EpochTime.NanosecondsToDate(value);

                case TimeTypes.InMilliseconds:
                    return EpochTime.MillisecondsToDate(value);

                case null:
                default:
                    // some autodetect?

                    DateTime t = EpochTime.Epoch;
                    DateTime t2;

                    try
                    {
                        t2 = t.AddMilliseconds(value);
                    }
                    catch
                    {
                        t2 = t;
                    }
                    var now = DateTime.Now;

                    if (now.Year - t2.Year < 5)
                    {
                        return t2.ToLocalTime();
                    }

                    try
                    {
                        t2 = t.AddSeconds(value);
                    }
                    catch
                    {
                        t2 = t;
                    }

                    if (now.Year - t2.Year < 5)
                    {
                        return t2;
                    }

                    try
                    {
                        t2 = t.AddTicks(value / 100);
                    }
                    catch
                    {
                        return t.ToLocalTime();
                    }

                    if (now.Year - t2.Year < 5)
                    {
                        return t2.ToLocalTime();
                    }

                    return t.ToLocalTime();

            }

        }

    }
}
