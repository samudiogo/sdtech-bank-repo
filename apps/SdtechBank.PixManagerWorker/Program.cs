using SdtechBank.Application.Common.DI;
using SdtechBank.Application.Payments.Consumers;
using SdtechBank.Application.Transactions.Consumers;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;
using SdtechBank.Infrastructure.DI;
using SdtechBank.Infrastructure.Shared.Mongo;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<PaymentCreatedConsumer>();
builder.Services.AddScoped<ProcessPaymentCreatedUseCase>();
builder.Services.AddScoped<TransactionCompletedConsumer>();
builder.Services.AddWorkerInfrastructureCore(builder.Configuration);
builder.Services.AddWorkerApplication(builder.Configuration);

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<MongoDbIndexInitializer>();
    await initializer.InitializeAsync();
}
host.Run();
