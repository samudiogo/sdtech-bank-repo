using SdtechBank.Application.Transactions.UseCases;
using SdtechBank.PixManagerWorker.Consumers;
using SdtechBank.PixManagerWorker.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<PaymentWorker>();
builder.Services.AddScoped<PaymentCreatedConsumer>();
builder.Services.AddScoped<ProcessPaymentCreatedUseCase>();

var host = builder.Build();
host.Run();
