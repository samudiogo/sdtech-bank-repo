
using MongoDB.Bson.Serialization;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.Deposits;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Messaging;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Domain.Transactions.Entities;
using SdtechBank.Domain.Accounts;
using SdtechBank.Domain.OutboxInbox;

namespace SdtechBank.Infrastructure.Shared.Mongo;

internal static class MongoDbClassMap
{
    private static bool _registered;
    private static readonly Lock _lock = new();

    public static void Register()
    {
        if (_registered) return;
        lock (_lock)
        {
            if (_registered) return;

            BsonClassMap.RegisterClassMap<PaymentOrder>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(x => x.Id);
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<PaymentDestination>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(x => x.PixKey);
                cm.MapMember(x => x.BankAccount);
            });
            RegisterEntity<Deposit>();
            RegisterEntity<Transaction>();
            RegisterEntity<LedgerEntry>();
            RegisterEntity<InboxMessage>();
            RegisterEntity<OutboxMessage>();
            RegisterEntity<Account>();        

            _registered = true;
        }
    }

    private static void RegisterEntity<T>() where T : class
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(T))) return;

        BsonClassMap.RegisterClassMap<T>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(typeof(T).GetProperty("Id"));
            cm.SetIgnoreExtraElements(true);
        });
    }

    private static void RegisterValueObject<T>() where T : class
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(T))) return;

        BsonClassMap.RegisterClassMap<T>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
    }
}
