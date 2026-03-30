using SdtechBank.Application.Transactions.UseCases.ProcessPayment;
using SdtechBank.PixManagerWorker.Consumers;
using SdtechBank.PixManagerWorker.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<PaymentWorker>();
builder.Services.AddScoped<PaymentCreatedConsumer>();
builder.Services.AddScoped<ProcessPaymentCreatedUseCase>();
builder.Services.AddScoped<TransactionCompletedConsumer>();

var host = builder.Build();
host.Run();
