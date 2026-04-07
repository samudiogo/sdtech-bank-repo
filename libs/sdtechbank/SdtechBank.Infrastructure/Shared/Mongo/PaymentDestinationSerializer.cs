using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Infrastructure.Shared.Mongo;

internal sealed class PaymentDestinationSerializer : SerializerBase<PaymentDestination>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, PaymentDestination value)
    {
        context.Writer.WriteStartDocument();

        context.Writer.WriteName("PixKey");
        if (value.PixKey is not null)
            context.Writer.WriteString(value.PixKey);
        else
            context.Writer.WriteNull();

        context.Writer.WriteName("BankAccount");
        if (value.BankAccount is not null)
            BsonSerializer.Serialize(context.Writer, value.BankAccount);
        else
            context.Writer.WriteNull();

        context.Writer.WriteEndDocument();
    }

    public override PaymentDestination Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;
        
        if (reader.GetCurrentBsonType() == BsonType.Null)
        {
            reader.ReadNull();
            throw new InvalidOperationException("PaymentDestination não pode ser nulo.");
        }

        
        if (reader.GetCurrentBsonType() == BsonType.String)
        {
            var legacyPixKey  = reader.ReadString();
            return PaymentDestination.FromPixKey(legacyPixKey );
        }

        reader.ReadStartDocument();

        string? pixKey = null;
        BankAccount? bankAccount = null;

        while (reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var field = reader.ReadName();

            switch (field)
            {
                case "PixKey":
                    if (reader.CurrentBsonType == BsonType.Null)
                    {
                        reader.ReadNull();
                        pixKey = null;
                    }
                    else
                    {
                        pixKey = reader.ReadString();
                    }
                    break;

                case "BankAccount":
                    if (reader.CurrentBsonType == BsonType.Null)
                    {
                        reader.ReadNull();
                        bankAccount = null;
                    }
                    else
                    {
                        bankAccount = BsonSerializer.Deserialize<BankAccount>(reader);
                    }
                    break;

                default:
                    reader.SkipValue();
                    break;
            }
        }

        reader.ReadEndDocument();

        
        if (pixKey is not null)
            return PaymentDestination.FromPixKey(pixKey);

        if (bankAccount is not null)
            return PaymentDestination.FromBankAccount(bankAccount);

        throw new InvalidOperationException("PaymentDestination inválido: nenhum valor preenchido.");
    }
}
