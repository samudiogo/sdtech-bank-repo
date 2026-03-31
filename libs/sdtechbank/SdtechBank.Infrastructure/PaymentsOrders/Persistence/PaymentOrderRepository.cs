using MongoDB.Driver;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.PaymentsOrders.Persistence
{
    public class PaymentOrderRepository(MongoDbContext context) : IPaymentOrderRepository
    {
        private readonly IMongoCollection<PaymentOrder> _collection = context.GetCollection<PaymentOrder>("payment-orders");

        public async Task<PaymentOrder?> GetByIdAsync(Guid paymentId)
        {
            var filter = Builders<PaymentOrder>.Filter.Eq(o => o.Id, paymentId);

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task SaveAsync(PaymentOrder paymentOrder)
        {
            await _collection.InsertOneAsync(paymentOrder);
        }
    }
}
