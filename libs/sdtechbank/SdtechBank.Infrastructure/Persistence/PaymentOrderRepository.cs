using MongoDB.Driver;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Infrastructure.MongoDB;

namespace SdtechBank.Infrastructure.Persistence
{
    public class PaymentOrderRepository(MongoDbContext context) : IPaymentOrderRepository
    {
        private readonly IMongoCollection<PaymentOrder> _collection = context.GetCollection<PaymentOrder>("payment-orders");

        public async Task SaveAsync(PaymentOrder paymentOrder)
        {
            await _collection.InsertOneAsync(paymentOrder);
        }
    }
}
