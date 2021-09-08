using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Json
{
    public class Level3UpdateConverter : JsonConverter<Level3Update>
    {
        public const int buy = -813464969;
        public const int sell = -1684090755;
        public const int filled = -1725622140;
        public const int canceled = -443854079;
        public const int sequence = 1384568619;
        public const int symbol = -322423047;
        public const int orderId = -98339785;
        public const int clientOid = 97372753;
        public const int side = 595663797;
        public const int price = -892853543;
        public const int size = -138402710;
        public const int remainSize = 513984757;
        public const int takerOrderId = -1263072696;
        public const int makerOrderId = 621206821;
        public const int tradeId = -154148381;
        public const int reason = 1001949196;
        public const int orderTime = -1047109151;
        public const int ts = -1014591861;

        public override Level3Update ReadJson(JsonReader reader, Type objectType, Level3Update existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s;
            Level3Update update = new Level3Update();

            reader.Read();

            do
            {
                var hash = Crc32.Hash(reader.Value as string ?? "");

                reader.Read();

                switch ((int)hash)
                {
                    case sequence:
                        update.Sequence = (long)reader.Value;
                        break;

                    case symbol:
                        update.Symbol = (string)reader.Value;
                        break;

                    case orderId:
                        update.OrderId = (string)reader.Value;
                        break;


                    case clientOid:
                        update.ClientOid = (string)reader.Value;
                        break;

                    case side:
                        update.Side = (Side)Crc32.Hash(reader.Value as string ?? "");
                        break;

                    case price:

                        s = (string)reader.Value;
                        if (s == "") break;

                        update.Price = decimal.Parse(s);
                        break;

                    case size:
                        s = (string)reader.Value;
                        if (s == "") break;

                        update.Size = decimal.Parse(s);
                        break;

                    case remainSize:
                        s = (string)reader.Value;
                        if (s == "") break;

                        update.RemainSize = decimal.Parse(s);
                        break;

                    case takerOrderId:
                        update.TakerOrderId = (string)reader.Value;
                        break;

                    case makerOrderId:
                        update.MakerOrderId = (string)reader.Value;
                        break;

                    case tradeId:
                        update.TradeId = (string)reader.Value;
                        break;

                    case reason:
                        update.Reason = (DoneReason)Crc32.Hash(reader.Value as string);
                        break;

                    case orderTime:
                        update.OrderTime = EpochTime.NanosecondsToDate((long)reader.Value);
                        break;

                    case ts:
                        update.Timestamp = EpochTime.NanosecondsToDate((long)reader.Value);
                        break;

                }

                reader.Read();
            } while (reader.TokenType != JsonToken.EndObject);

            return update;
        }

        public override void WriteJson(JsonWriter writer, Level3Update value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }

    //public class Level3UpdateConverter : JsonConverter<Level3Update>
    //{
    //    public override Level3Update ReadJson(JsonReader reader, Type objectType, Level3Update existingValue, bool hasExistingValue, JsonSerializer serializer)
    //    {
    //        string s;
    //        Level3Update update = new Level3Update();

    //        reader.Read();

    //        do
    //        {
    //            var hash = (reader.Value as string ?? "");

    //            reader.Read();

    //            switch (hash)
    //            {
    //                case "sequence":
    //                    update.Sequence = (long)reader.Value;
    //                    break;

    //                case "symbol":
    //                    update.Symbol = (string)reader.Value;
    //                    break;

    //                case "orderId":
    //                    update.OrderId = (string)reader.Value;
    //                    break;


    //                case "clientOid":
    //                    update.ClientOid = (string)reader.Value;
    //                    break;

    //                case "side":
    //                    update.Side = (reader.Value as string == "buy" ? Side.Buy : Side.Sell);
    //                    break;

    //                case "price":

    //                    s = (string)reader.Value;
    //                    if (s == "") break;

    //                    update.Price = decimal.Parse(s);
    //                    break;

    //                case "size":
    //                    s = (string)reader.Value;
    //                    if (s == "") break;

    //                    update.Size = decimal.Parse(s);
    //                    break;

    //                case "remainSize":
    //                    s = (string)reader.Value;
    //                    if (s == "") break;

    //                    update.RemainSize = decimal.Parse(s);
    //                    break;

    //                case "takerOrderId":
    //                    update.TakerOrderId = (string)reader.Value;
    //                    break;

    //                case "makerOrderId":
    //                    update.MakerOrderId = (string)reader.Value;
    //                    break;

    //                case "tradeId":
    //                    update.TradeId = (string)reader.Value;
    //                    break;

    //                case "reason":
    //                    update.Reason = (reader.Value as string == "filled" ? DoneReason.Filled : DoneReason.Canceled);
    //                    break;

    //                case "orderTime":
    //                    update.OrderTime = EpochTime.NanosecondsToDate((long)reader.Value);
    //                    break;

    //                case "ts":
    //                    update.Timestamp = EpochTime.NanosecondsToDate((long)reader.Value);
    //                    break;

    //            }

    //            reader.Read();
    //        } while (reader.TokenType != JsonToken.EndObject);

    //        return update;
    //    }

    //    public override void WriteJson(JsonWriter writer, Level3Update value, JsonSerializer serializer)
    //    {
    //        throw new NotSupportedException();
    //    }
    //}

}
