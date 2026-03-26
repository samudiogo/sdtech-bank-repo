using MongoDB.Driver;
using SdtechBank.Domain.Contracts;
using SdtechBank.Domain.Entities;
using SdtechBank.Infrastructure.MongoDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace SdtechBank.Infrastructure.Data
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
