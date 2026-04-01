using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Infrastructure.Shared.Mongo;

internal sealed class PaymentDestinationSerializer : SerializerBase<PaymentDestination>
{
    public override void Serialize(BsonSerializationContext ctx, BsonSerializationArgs args, PaymentDestination value)
    {
        ctx.Writer.WriteStartDocument();

        ctx.Writer.WriteName("PixKey");
        if (value.PixKey is not null)
            ctx.Writer.WriteString(value.PixKey);
        else
            ctx.Writer.WriteNull();

        ctx.Writer.WriteName("BankAccount");
        if (value.BankAccount is not null)
            BsonSerializer.Serialize(ctx.Writer, value.BankAccount);
        else
            ctx.Writer.WriteNull();

        ctx.Writer.WriteEndDocument();
    }

    public override PaymentDestination Deserialize(BsonDeserializationContext ctx, BsonDeserializationArgs args)
    {
        ctx.Reader.ReadStartDocument();

        string? pixKey = null;
        BankAccount? bankAccount = null;

        while (ctx.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var field = ctx.Reader.ReadName();
            switch (field)
            {
                case "PixKey":
                    pixKey = ctx.Reader.CurrentBsonType == BsonType.Null
                        ? null
                        : ctx.Reader.ReadString();
                    break;
                case "BankAccount":
                    bankAccount = ctx.Reader.CurrentBsonType == BsonType.Null
                        ? null
                        : BsonSerializer.Deserialize<BankAccount>(ctx.Reader);
                    break;
                default:
                    ctx.Reader.SkipValue();
                    break;
            }
        }

        ctx.Reader.ReadEndDocument();

        return pixKey is not null
            ? PaymentDestination.FromPixKey(pixKey)
            : PaymentDestination.FromBankAccount(bankAccount!);
    }
}
