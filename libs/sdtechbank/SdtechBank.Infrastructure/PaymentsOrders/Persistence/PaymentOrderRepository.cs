using MongoDB.Driver;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.PaymentsOrders.Persistence
{
    public class PaymentOrderRepository(MongoDbContext context) : IPaymentOrderRepository
    {
        private readonly IMongoCollection<PaymentOrder> _collection = context.GetCollection<PaymentOrder>("payment_orders");

        public async Task<PaymentOrder?> GetByIdAsync(Guid paymentId)
        {
            var filter = Builders<PaymentOrder>.Filter.Eq(o => o.Id, paymentId);

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<PaymentOrder?> GetByIdempotencyKeyAsync(IdempotencyKey key, CancellationToken cancellationToken)
        {
            var filter = Builders<PaymentOrder>.Filter.Eq(o => o.IdempotencyKey.Value, key.Value);

            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task SaveAsync(PaymentOrder paymentOrder)
        {
            var filter = Builders<PaymentOrder>.Filter.Eq(o => o.Id, paymentOrder.Id);
            var options = new ReplaceOptions { IsUpsert = true };
            await _collection.ReplaceOneAsync(filter, paymentOrder, options);
        }

        public async Task<bool> ExistsRecentSimilarAsync(Guid payerId, PaymentDestination destination, Money amount, TimeSpan window, CancellationToken cancellationToken)
        {
            var builder = Builders<PaymentOrder>.Filter;
            var filterDefinition = builder.Empty;
            var cutoff = DateTime.UtcNow.Subtract(window);

            filterDefinition &= builder.And(
                builder.Eq(a => a.PayerId, payerId),
                builder.Eq(a => a.Amount.Value, amount.Value),
                builder.Gte(a => a.CreatedAt, cutoff));

            if (destination.IsPix())
                filterDefinition &= builder.Eq(d => d.Destination.PixKey, destination.PixKey);
                
            if (destination.HasBankAccount())
                filterDefinition &= builder.And(
                    builder.Eq(a => a.Destination.BankAccount!.Cpf, destination.BankAccount!.Cpf),
                    builder.Eq(a => a.Destination.BankAccount!.BankCode, destination.BankAccount!.BankCode),
                    builder.Eq(a => a.Destination.BankAccount!.Branch, destination.BankAccount!.Branch),
                    builder.Eq(a => a.Destination.BankAccount!.Account, destination.BankAccount!.Account));

            return await _collection.Find(filterDefinition).AnyAsync(cancellationToken);
        }
    }
}
