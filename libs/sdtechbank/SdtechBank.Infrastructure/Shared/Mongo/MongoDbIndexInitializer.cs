using MongoDB.Driver;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.OutboxInbox;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.Shared.Messaging;
using SdtechBank.Domain.Transactions.Entities;

namespace SdtechBank.Infrastructure.Shared.Mongo;

public sealed class MongoDbIndexInitializer
{
    private readonly MongoDbContext _context;

    public MongoDbIndexInitializer(MongoDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await CreatePaymentOrderIndexesAsync(ct);
        await CreateTransactionIndexesAsync(ct);
        await CreateLedgerIndexesAsync(ct);
        await CreateInboxIndexesAsync(ct);
        await CreateOutboxIndexesAsync(ct);
    }

    private async Task CreatePaymentOrderIndexesAsync(CancellationToken ct)
    {
        var col = _context.GetCollection<PaymentOrder>("payment_orders");
        var indexes = new[]
        {
        // Busca por conta + status (query mais comum)
        new CreateIndexModel<PaymentOrder>(
            Builders<PaymentOrder>.IndexKeys
                .Ascending(x => x.PayerId)
                .Ascending(x => x.PaymentStatus)),

        // Idempotência: ExternalId único por conta
        new CreateIndexModel<PaymentOrder>(
            Builders<PaymentOrder>.IndexKeys
                .Ascending(x => x.IdempotencyKey),
            new CreateIndexOptions { Unique = true, Sparse = true })
    };

        await col.Indexes.CreateManyAsync(indexes, ct);
    }

    private async Task CreateTransactionIndexesAsync(CancellationToken ct)
    {
        var col = _context.GetCollection<Transaction>("transactions");
        var indexes = new[]
        {
        new CreateIndexModel<Transaction>(
            Builders<Transaction>.IndexKeys
                .Ascending(x => x.PaymentOrderId)),

        new CreateIndexModel<Transaction>(
            Builders<Transaction>.IndexKeys
                .Ascending(x => x.IdempotencyKey)
                .Descending(x => x.CreatedAt))
    };

        await col.Indexes.CreateManyAsync(indexes, ct);
    }

    private async Task CreateLedgerIndexesAsync(CancellationToken ct)
    {
        var col = _context.GetCollection<LedgerEntry>("ledger_entries");

        // Reconstrução de saldo: sempre por conta + data
        var index = new CreateIndexModel<LedgerEntry>(
            Builders<LedgerEntry>.IndexKeys
                .Ascending(x => x.AccountId)
                .Descending(x => x.CreatedAt));

        await col.Indexes.CreateOneAsync(index, cancellationToken: ct);
    }

    private async Task CreateInboxIndexesAsync(CancellationToken ct)
    {
        var col = _context.GetCollection<InboxMessage>("inbox_messages");
        var indexes = new[]
        {
        // Idempotência: MessageId único
        new CreateIndexModel<InboxMessage>(
            Builders<InboxMessage>.IndexKeys
                .Ascending(x => x.MessageId),
            new CreateIndexOptions { Unique = true }),

        // Polling de mensagens não processadas
        new CreateIndexModel<InboxMessage>(
            Builders<InboxMessage>.IndexKeys
                .Ascending(x => x.CreatedAt)                )
    };

        await col.Indexes.CreateManyAsync(indexes, ct);
    }

    private async Task CreateOutboxIndexesAsync(CancellationToken ct)
    {
        var col = _context.GetCollection<OutboxMessage>("outbox_messages");
        var indexes = new[]
        {
        // Polling de mensagens não publicadas
        new CreateIndexModel<OutboxMessage>(
            Builders<OutboxMessage>.IndexKeys
                .Ascending(x => x.ProcessedAt)
                .Ascending(x => x.OccurredAt)),

        // Correlação de rastreamento
        //TODO: adicionar CorrelationId na classe OutboxMessage
        //new CreateIndexModel<OutboxMessage>(
        //    Builders<OutboxMessage>.IndexKeys
        //        .Ascending(x => x.CorrelationId),
        //    new CreateIndexOptions { Sparse = true })
    };

        await col.Indexes.CreateManyAsync(indexes, ct);
    }
}
